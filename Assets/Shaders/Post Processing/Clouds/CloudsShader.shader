// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'

Shader"Custom/CloudsPostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _OverlayColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewVector : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                // Camera space matches OpenGL convention where cam forward is -z. In unity forward is positive z.
                // (https://docs.unity3d.com/ScriptReference/Camera-cameraToWorldMatrix.html)
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                o.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0));
                return o;
            }

            sampler2D _CameraDepthTexture; // Unity depth
            sampler2D _MainTex; // Background

            // G-Buffer
            int _UseGBuffer;
            sampler2D _DepthTexture;
            sampler2D _PositionTexture;
            sampler2D _HeightMap;
            float2 _HeightMapSpeed;

            float4 _BoundsMin;
            float4 _BoundsMax;
            
            fixed4 _LightColor0;
            int _LightSteps;
            float _CustomTime;
            float _GlobalBrightness;
            float _GlobalDensity;
            float _SunLightAbsorption;
            float _HeightVariation;
            float _HeightScale;
            float4 _PhaseParams;
            float _ShadowIntensity;
            int _ShadowSteps;
            float _ShadowDist;

            sampler2D _CoverageMap;
            sampler2D _BlueNoise;
            float _OffsetNoiseIntensity;

            float3 _CoverageOffset;
            float3 _CoverageSpeed;
            float _CoverageScale;
            float _Coverage;
            float _StepSize; 

            float min4(float a, float b, float c, float d)
            {
	            return min(min(a, b), min(c, d));
            }

            float sampleCoverage(float3 pos)
            {
                float res = tex2D(_CoverageMap, (pos.xz + _CoverageOffset.xz + _CustomTime * _CoverageSpeed) / _CoverageScale).r;
                res = saturate(res - (1 - _Coverage));
                return res;
            }

            float sampleDensity(float3 pos)
            {
                float top = max(_BoundsMin.y, _BoundsMax.y);
                float bottom = min(_BoundsMin.y, _BoundsMax.y);

                float boxHeight = abs(top - bottom);
                float maxHeight = top - tex2D(_HeightMap, (pos.xz + _CustomTime * _HeightMapSpeed) / _HeightScale).r * _HeightVariation;
	            float minHeight = bottom + tex2D(_HeightMap, (pos.xz + _CustomTime * _HeightMapSpeed) / _HeightScale) * _HeightVariation;
	            float centerHeight = (minHeight + maxHeight) / 2;

                float t1 = smoothstep(minHeight, centerHeight, pos.y); 
                float t2 = smoothstep(maxHeight, centerHeight, pos.y);
                float fade = pow(min(t1, t2), 3);

                float res = 1;
                res *= sampleCoverage(pos);
                res *= fade;
                
                // Disables clouds at night
                float densityFactor = saturate(dot(float3(0, 1, 0), _WorldSpaceLightPos0));
                res *= densityFactor;

                return saturate(res);
            }

            // g = 0 causes uniform scattering while g = 1 causes directional scattering, in the direction of the photons
            float henyeyGreenstein(float angle, float g)
            {
	            float g2 = g * g;
	            return (1 - g2) / (4 * 3.1415 * pow(1 + g2 - 2 * g * (angle), 1.5));
            }

            float phaseHG(float3 rayDir, float3 lightDir)
            {
	            float angleBottom = dot(rayDir, lightDir);
	            float angleTop = dot(rayDir, -lightDir);
	            float hgBottom = henyeyGreenstein(angleBottom, _PhaseParams.x);
	            float hgTop = henyeyGreenstein(angleTop, _PhaseParams.x);
                float hg = hgBottom + hgTop;
	            return _PhaseParams.z + hg;
            }

            // Not used for now
            float phaseMie(float3 rayDir, float3 lightDir)
            {
                float cosTheta = dot(rayDir, lightDir);
                float mieG = _PhaseParams.x; // Asymmetry parameter
                float phaseMie = (1.0 + mieG*mieG - 2.0*mieG*cosTheta) /
                                 pow(1.0 + mieG*mieG - 2.0*mieG*cosTheta, 1.5);
                return phaseMie;
            }

            float2 rayBoxDst(float3 rayOrigin, float3 rayDir)
            {
	            float3 invRayDir = 1 / rayDir;

	            // Calculate ray intersections with box
	            float3 t0 = (_BoundsMin - rayOrigin) * invRayDir;
	            float3 t1 = (_BoundsMax - rayOrigin) * invRayDir;
	            float3 tmin = min(t0, t1);
	            float3 tmax = max(t0, t1);

	            // Calculate distances
	            float dstA = max(max(tmin.x, tmin.y), tmin.z); // A is the closest point
	            float dstB = min(tmax.x, min(tmax.y, tmax.z)); // B is the furthest point

	            float dstToBox = max(0, dstA);
	            float dstInsideBox = max(0, dstB - dstToBox);
	            return float2(dstToBox, dstInsideBox);
            }

            float lightMarch(float3 samplePos, float3 lightDir, float sampleDist = -1)
            {
	            float3 rayDir = lightDir;
	            float dstTravelled = 0;

	            // Evaluate distance inside box from samplePos to LightDir for computing stepSize
	            float dstInsideBox = rayBoxDst(samplePos, rayDir).y;

                // Considerable optimization : reduce lighting quality with distance
                // TODO: make 300 a parameter
                int steps = _LightSteps;
                if (sampleDist >= 0)
                {
                    steps *= (1 - saturate(sampleDist / 2000.0));
                    steps = max(steps, 2);
                }
	            float stepSize = dstInsideBox / steps;
	            float dstLimit = dstInsideBox;

	            float totalDensity = 0;
                [loop]
	            while (dstTravelled < dstLimit)
	            {
		            float3 rayPos = samplePos + (rayDir * dstTravelled);
		            float sampledDensity = stepSize * sampleDensity(rayPos) * _GlobalDensity;
		            totalDensity += sampledDensity;
		            dstTravelled += stepSize;
	            }

	            return max(0, exp(-totalDensity * _SunLightAbsorption));
            }

            float2 sampleCloud(float3 rayOrigin, float3 rayDir, float3 lightDir, float dstToBox, float dstInsideBox, float depthDist, float offset)
            {
	            float dstTravelled = offset;
	            float dstLimit = min(depthDist - dstToBox, dstInsideBox); // TODO: Improve far distances
                dstLimit = min(dstLimit, 2000);
	            float transmittance = 1;
	            float lightEnergy = 0;
	            float totalDensity = 0;
	            float phaseVal = phaseHG(rayDir, lightDir);

                [loop]
	            while (dstTravelled < dstLimit)
	            {
                    // TODO: increase step size the further we go ?
                    float stepSize = max(_StepSize, _StepSize * dstTravelled / 300);
		            stepSize = min(stepSize, dstLimit - dstTravelled); // TODO: Improve performance on far distances
		            if (transmittance < 0.01)
		            {
			            break;
		            }

		            float3 rayPos = rayOrigin + rayDir * (dstToBox + dstTravelled);
		            float density = sampleDensity(rayPos) * stepSize * _GlobalDensity;
		            totalDensity += density;

                    float sampleDist = dstTravelled + dstToBox;
		            float lightTransmittance = lightMarch(rayPos, lightDir, sampleDist);
		            lightEnergy += density * transmittance * lightTransmittance;
		            transmittance *= exp(-density);

		            dstTravelled += stepSize;
	            }
	            return float2(transmittance, lightEnergy * phaseVal);
            }

            float sampleShadow(float3 pos, float3 lightDir)
            {
	            float2 rayBoxInfo = rayBoxDst(pos, lightDir);
	            float dstToBox = rayBoxInfo.x;
	            float dstInsideBox = rayBoxInfo.y;

                float3 startPos = pos + lightDir * dstToBox;
                float3 rayPos = startPos;

                float stepSize = dstInsideBox / _ShadowSteps;

                float shadow = 0;
                [loop]
                for (int i = 0; i < _ShadowSteps; i++)
                {
                    rayPos = startPos + lightDir * stepSize * i;
                    float density = sampleDensity(rayPos) * stepSize;
                    shadow += density;
                }
                return shadow;
            }

            float sampleLightShaft(float3 rayOrigin, float3 rayDir, float3 lightDir, float dstInsideBox, float depthDist)
            {
                float dstTravelled = 0;
	            float dstLimit = min(depthDist, 10000); // TODO: Improve far distances
                float stepSize = 1;

                float totalLight = 0;
                
                [loop]
	            while (dstTravelled < dstLimit)
	            {
                    float3 rayPos = rayOrigin + rayDir * dstTravelled;
                    if (rayPos.y > _BoundsMax.y) break;

                    float lightComingIn = pow(lightMarch(rayPos, lightDir), 32);
                    totalLight += lightComingIn * stepSize;
		            dstTravelled += stepSize;
	            }

                return totalLight/1000;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Create ray
                float3 rayPos = _WorldSpaceCameraPos;
                float viewLength = length(i.viewVector);
                float3 rayDir = i.viewVector.xyz / viewLength;
                float3 lightDir = _WorldSpaceLightPos0;

                // Compute depth
                // Depth and cloud container intersection info:
                float depthDist;
                if (_UseGBuffer == 1)
                {
                    depthDist = tex2D(_DepthTexture, i.uv);
                }
                else
                {
                    depthDist = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                    depthDist = LinearEyeDepth(depthDist) * viewLength;
                }

                // Calculate dist inside box	
	            float2 rayBoxInfo = rayBoxDst(rayPos, rayDir);
	            float dstToBox = rayBoxInfo.x;
	            float dstInsideBox = rayBoxInfo.y;

	            // Sample cloud density
                float offset = length(tex2D(_BlueNoise, i.uv).rgb) / 3 * _OffsetNoiseIntensity;
	            float2 sampleData = sampleCloud(rayPos, rayDir, lightDir, dstToBox, dstInsideBox, depthDist, offset);
	            float transmittance = sampleData.x;
	            float lightEnergy = sampleData.y;
                
                // Sample shadow
                float shadow = 1;
                if (depthDist < _ShadowDist)
                {
    	            float3 worldPos = tex2D(_PositionTexture, i.uv);
                    shadow = sampleShadow(worldPos, lightDir);
                    shadow /= 50;
                    shadow *= _ShadowIntensity;
                    shadow = clamp(1 - shadow, 0.2, 1);
                }

                // Sample background
                float4 backgroundColor = tex2D(_MainTex, i.uv);
                backgroundColor *= shadow;

                // Add clouds
	            if (dstInsideBox > 0)
	            {
		            float4 cloudColor = float4(_LightColor0.rgb, 0);
		            cloudColor *= lightEnergy * _GlobalBrightness;
		            return float4(backgroundColor.rgb * transmittance + cloudColor.rgb, 1);
	            }

                return backgroundColor;
            }
            ENDCG
        }
    }
}
