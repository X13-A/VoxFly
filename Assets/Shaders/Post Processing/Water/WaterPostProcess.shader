// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'

Shader"Custom/WaterPostProcess"
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
                
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                o.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0)); 
                return o;
            }

            sampler2D _MainTex;
            sampler2D _DepthTexture;
            sampler2D _PositionTexture;
            sampler3D _WorldTexture;
            uint3 _WorldTextureSize;
            float3 _CameraPos;

            float _WaterLevel;
            float _WaterDensity;
            float4 _WaterColor;

            bool isInWorld(float3 pos)
            {
                return pos.y < (float) _WorldTextureSize.y && pos.y >= 0;
            }

            uint sampleWorld(float3 pos)
            {
                if (!isInWorld(pos))
                {
                    return 0;
                }
                float4 res = tex3D(_WorldTexture, pos / _WorldTextureSize);
                if (res.a == 0) return 0;

                int blockID = round(res.r * 255); // Scale and round to nearest whole number
                return blockID;
            }

            // Projects a rayPos on a plane along rayDir
            float RayToPlaneDistance(float3 rayPos, float3 rayDir, float3 planeNormal, float planeD)
            {
                float denom = dot(planeNormal, rayDir);

                // Ray parallel to plane
                if (abs(denom) < 1e-6)
                {
                    return -1.0f;
                }

                float num = planeD - dot(planeNormal, rayPos);
                float t = num / denom;
                return t;
            }

            struct rayWaterInfo
            {
                float dstToWater;
                float dstInsideWater;
            };

            rayWaterInfo rayWaterDst(float3 rayPos, float3 rayDir, float depth)
            {
                rayWaterInfo res;
                float dstToWater = RayToPlaneDistance(rayPos, rayDir, float3(0,1,0), _WaterLevel);
                res.dstToWater = dstToWater;

                // If above water
                if (rayPos.y > _WaterLevel && dstToWater >= 0 && dstToWater < depth)
                {
                    res.dstToWater = dstToWater;
                    res.dstInsideWater = depth - dstToWater;
                    return res;
                }

                // If underwater
                if (rayPos.y <= _WaterLevel)
                {
                    float dstInsideWater;
                    if (dstToWater >= 0 && dstToWater < depth)
                    {
                        dstInsideWater = dstToWater;
                    }
                    else if (dstToWater >= 0 && dstToWater > depth)
                    {
                        dstInsideWater = depth;
                    }
                    else if (dstToWater < 0)
                    {
                        dstInsideWater = depth;
                    }
                    res.dstInsideWater = dstInsideWater;
                    return res;
                } 

                res.dstToWater = 0;
                res.dstInsideWater = 0;
                return res;
            }

            float getFoam(float3 surfacePos, float depth, float foamThickness = 1)
            {
                static const float3 offsets[8] = 
                {
                    float3(-1, 0, 0),
                    float3(0, 0, -1),
                    float3(0, 0, 1),
                    float3(1, 0, 0),
                    float3(1, 0, 1),
                    float3(-1, 0, -1),
                    float3(1, 0, -1),
                    float3(-1, 0, 1),
                };

                [unroll(8)]
                for (int i = 0; i < 8; i++)
                {
                    float3 offsetPos = surfacePos + offsets[i] * foamThickness;
                    if (sampleWorld(offsetPos) > 0)
                    {
                        return 1;
                    }
                }
                return 0;
            }

            float getLighting(float3 pos, float3 rayDir, float3 lightDir, bool useSpecular)
            {
                float3 L = -lightDir; // Light direction, from surface to light
                float3 N = float3(0, 1, 0);
                float diffuse = saturate(dot(L, N));
    
                float specular = 0;
                
                if (useSpecular)
                {
                    float3 V = -rayDir; // View direction, from surface to camera
                    float3 H = normalize(L + V); // Halfway vector

                    float specularStrength = 2;
                    float shininess = 64;
                    specular = pow(max(dot(N, H), 0.0), shininess) * specularStrength;
                }

                return (0.0125 + diffuse + specular);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float depth = tex2D(_DepthTexture, i.uv);
                float3 pos = tex2D(_PositionTexture, i.uv);
                
                float3 rayPos = _CameraPos;
                float viewLength = length(i.viewVector);
                float3 rayDir = i.viewVector.xyz;
                float3 lightDir = -_WorldSpaceLightPos0;

                rayWaterInfo waterInfo = rayWaterDst(rayPos, rayDir, depth);
                float waterDensity = waterInfo.dstInsideWater * _WaterDensity;
                float dstToWater = waterInfo.dstToWater;

                if (waterDensity == 0)
                {
                    return tex2D(_MainTex, i.uv);
                }

                float transmittance = 1 * exp(-waterDensity);

                // Fog
                float fog = saturate(depth / 5000) * 1;
                float4 fogColor = saturate(dot(_WorldSpaceLightPos0, float3(0, 1, 0)));
                if (rayPos.y < _WaterLevel) fog = 0;
                float3 surfacePos = 0;
                float foam = 0;
                float light = 0;

                bool surfaceVisible = true;
                if (dstToWater < 0) surfaceVisible = false;
                if (depth < dstToWater) surfaceVisible = false;
                
                if (surfaceVisible)
                {
                    surfacePos = rayPos + rayDir * dstToWater; 
                    
                    // Foam
                    foam = getFoam(surfacePos, depth, 0.05 + (sin(_Time.y) / 2 + 0.5) * 0.2);
                    foam *= transmittance;
                    foam *= 100 / dstToWater;

                    light = getLighting(surfacePos, rayDir, lightDir, true);
                }
                else
                {
                    light = getLighting(rayPos, rayDir, lightDir, true);
                }

                // Distord
                float distortionScale = 2.0;
                float2 distortedUV = sin(pos.xz * distortionScale + float2(_Time.y, _Time.y)) * waterDensity; 
                float distortionStrength = 0.1 * (0.2 / depth);
                if (rayPos.y > _WaterLevel) 
                {
                    distortionScale *= 4;
                    distortionStrength *= 4;
                }

                float4 background = tex2D(_MainTex, i.uv + distortedUV * distortionStrength);

                float4 finalWaterColor = _WaterColor * light * (1 - transmittance) * (1 - foam);
                return background * (transmittance) + finalWaterColor + foam * light + fogColor * fog;
            }
            ENDCG
        }
    }
}
