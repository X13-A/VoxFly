Shader "Custom/WorldPostProcess"
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
            ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma require 2darray
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

            // G-Buffer
            sampler2D _DepthTexture;
            sampler2D _BlockTexture;
            sampler2D _NormalTexture;
            sampler2D _PositionTexture;

            // World
            sampler2D _MainTex;
            sampler3D _WorldTexture;
            float3 _WorldTextureSize;

            // Textures
            sampler2D _BlockTextureAtlas;
            sampler2D _NoiseTexture;

            // Params
            float3 _CameraPos;
            float3 _PlayerLightPos;
            float3 _PlayerLightDir;
            float _PlayerLightIntensity;
            float _PlayerLightVolumetricIntensity;
            float _PlayerLightRange;
            float _PlayerLightAngle;
            float _VoxelRenderDistance; 
            int _BlocksCount;
            int _DebugToggle;
            int _LightShaftSampleCount;
            float _LightShaftRenderDistance;
            float _LightShaftFadeStart;
            float _LightShaftIntensity;
            float _LightShaftMaximumValue;

            // Shadow map
            float3 _LightDir;
            float3 _LightUp;
            float3 _LightRight;

            sampler2D _ShadowMap;
            uint2 _ShadowMapResolution;
            float2 _ShadowMapCoverage;
            float3 _ShadowMapOrigin;

            bool isInWorld(float3 pos)
            {
                return pos.y < _WorldTextureSize.y && pos.y >= 0;
            }
            
            float dstToPlane(float3 rayOrigin, float3 rayDir, float planeY)
            {
                // Check if the plane is parallel to the ray
                if (rayDir.y == 0)
                {
                    return -1.0f;
                }

                float t = (planeY - rayOrigin.y) / rayDir.y;

                // Check if the plane is behind the ray's origin
                if (t < 0)
                {
                    return -1.0f;
                }

                return t;
            }

            // Returns float3 (dist to world, dist inside world, intersection pos)
            float2 rayWorldHit(float3 rayOrigin, float3 rayDir)
            {
                float dstTop = dstToPlane(rayOrigin, rayDir, _WorldTextureSize.y);
                float dstBottom = dstToPlane(rayOrigin, rayDir, 0);

                float dstToWorld;
                float dstInsideWorld;

                // If inside the world
                if (isInWorld(rayOrigin))
                {
                    dstToWorld = 0;
                    // Check if direction is parallel to the planes
                    if (rayDir.y == 0) return float2(dstToWorld, 1e10); 
                    // Return dist inside world
                    return float2(dstToWorld, max(dstTop, dstBottom));
                }

                // If above the world
                if (rayOrigin.y > _WorldTextureSize.y)
                {
                    // Check if looking at world
                    if (dstTop < 0) return float2(-1, -1);

                    dstInsideWorld = dstBottom - dstTop;
                    return float2(dstTop, dstInsideWorld);
                }
                // If under the world
                else
                {
                    // Check if looking at world
                    if (dstBottom < 0) return float2(-1, -1);

                    dstInsideWorld = dstTop - dstBottom;
                    return float2(dstBottom, dstInsideWorld);
                }
            }

            int3 voxelPos(float3 pos)
            {
                return int3(floor(pos.x), floor(pos.y), floor(pos.z));
            }

            float getOutline(float3 pos)
            {
                if (saturate(distance (pos, _CameraPos) / 100) == 1)
                {
                    return 1;
                }

                float x = (pos.x - floor(pos.x));
                x = min(x, 1.0f - x);
                float y = (pos.y - floor(pos.y));
                y = min(y, 1.0f - y);
                float z = (pos.z - floor(pos.z));
                z = min(z, 1.0f - z);

                float edgeThreshold = 2;

                float nearXEdge = x ;
                float nearYEdge = y ;
                float nearZEdge = z ;

                return saturate(0.7 + min(min(nearXEdge + nearYEdge, nearXEdge + nearZEdge), nearYEdge + nearZEdge) * edgeThreshold);
            }

            float3 getNormal(float3 tMax, float3 step)
            {
                // Determine the normal based on the axis of intersection
                if (tMax.x < tMax.y && tMax.x < tMax.z)
                {
                    return step.x * float3(1, 0, 0);
                }
                else if (tMax.y < tMax.z)
                {
                    return step.y * float3(0, 1, 0);
                }
                else
                {
                    return step.z * float3(0, 0, 1);
                }
            }

            float4 getBlockColor(int blockID, float3 pos, float3 normal)
            {
                float2 uv;

                if (normal.y != 0)
                {
                    uv = pos.xz;
                }
                else if (normal.x != 0)
                {
                    uv = pos.zy;
                } 
                else if (normal.z != 0)
                {
                    uv = pos.xy;
                }
                else 
                {
                    return float4(1, 0, 1, 1);
                }

                uv = frac(uv);
                float blockWidth = 1.0 / _BlocksCount; // Width of one block in normalized texture coordinates
                uv.x = uv.x * blockWidth + blockWidth * (blockID - 1); // Offset uv to the correct block
                return tex2D(_BlockTextureAtlas, uv);
            }

            // Sample world using world space coordinates
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

            // Cmputes ray directions based on a surface normal, used for occlusion
            float3 getSampleDirection(float3 normal, int sampleIndex, int numSamples) {
                float phi = 2.61803398875 * sampleIndex; // Use the golden ratio angle increment for better distribution
                float cosTheta = 1.0 - (float(sampleIndex) / numSamples);
                float sinTheta = sqrt(1.0 - cosTheta * cosTheta);

                // Spherical to Cartesian coordinates
                float x = cos(phi) * sinTheta;
                float y = sin(phi) * sinTheta;
                float z = cosTheta;

                // Align the z-axis with the normal using a simple rotation
                float3 up = abs(normal.z) < 0.999 ? float3(0, 0, 1) : float3(0, 1, 0);
                float3 right = normalize(cross(up, normal));
                up = cross(normal, right);

                return normalize(right * x + up * y + normal * z);
            }

            float calculateOcclusion(float3 pos, float3 normal, int numSamples, float radius) 
            {
                float occlusion = 0.0;
                for (int i = 0; i < numSamples; ++i) 
                {
                    float3 rayDir = getSampleDirection(normal, i, numSamples);
                    float3 samplePos = pos + rayDir * radius;
                    if (sampleWorld(samplePos) != 0) 
                    { 
                        occlusion += 1.0;
                    }
                }
                return 1.0 - (occlusion / numSamples) * 0.5;
            }

            struct rayMarchInfo
            {
                int complexity;
                float3 pos;
                float4 color;
                float depth;
                float3 normal;
            };

            // DEPRECATED
            bool isInShadow_rayMarched(float3 pos, float3 normal, float3 lightDir)
            {
                float3 startPos = pos;
                float3 rayDir = lightDir;
                startPos += normal * 0.002;

                float2 rayWorldInfo = rayWorldHit(startPos, rayDir);
                float dstToWorld = rayWorldInfo.x;
                float dstInsideWorld = rayWorldInfo.y;

                // EXIT EARLY
                if (dstInsideWorld <= 0) 
                {
                    return false;
                }
                if (dstToWorld > 0)
                {
                    startPos = startPos + rayDir * dstToWorld; // Start at intersection point
                }
                int3 voxelIndex = voxelPos(startPos);


                float3 step = sign(rayDir);
                float3 tMax;  // Distance to next voxel boundary
                float3 tMax_old; // Used to get the normals
                float3 tDelta;  // How far we travel to cross a voxel

                // Calculate initial tMax and tDelta for each axis
                for (int i = 0; i < 3; i++)
                { 
                    if (rayDir[i] == 0)
                    {
                        tMax[i] = 1e10;
                        tDelta[i] = 1e10;
                    }
                    else
                    {
                        float voxelBoundary = voxelIndex[i] + (step[i] > 0 ? 1 : 0);
                        tMax[i] = (voxelBoundary - startPos[i]) / rayDir[i]; 
                        tDelta[i] = abs(1.0 / rayDir[i]);
                    }
                }

                float dstLimit = min(_VoxelRenderDistance - dstToWorld, dstInsideWorld);
                float dstTravelled = 0;
                int loopCount = 0;
                int hardLoopLimit = (int) dstLimit * 2; // Hack that prevents convergence caused by precision issues or some dark mathematical magic

                [loop]
                while (loopCount < hardLoopLimit && dstTravelled < dstLimit)
                {
                    // Check the position for a voxel
                    float3 rayPos = startPos + rayDir * (dstTravelled + 0.001);

                    int blockID = sampleWorld(rayPos);
                    
                    // Return the voxel
                    if (blockID != 0)
                    {
                        return true;
                    }

                    // Move to the next voxel
                    tMax_old = tMax;
                    if (tMax.x < tMax.y && tMax.x < tMax.z)
                    {
                        dstTravelled = tMax.x;
                        tMax.x += tDelta.x;
                        voxelIndex.x += step.x;
                    }
                    else if (tMax.y < tMax.z)
                    {
                        dstTravelled = tMax.y;
                        tMax.y += tDelta.y;
                        voxelIndex.y += step.y;
                    }
                    else
                    {
                        dstTravelled = tMax.z;
                        tMax.z += tDelta.z;
                        voxelIndex.z += step.z;
                    }
                    loopCount++;
                }
                return false;

            }
            
            // DEPRECATED
            float getLightShaft_old(float3 rayStart, float3 rayDir, float3 lightDir, float depth, float offset)
            {
                    float n = _LightShaftSampleCount;
                    float dstLimit = min(_LightShaftRenderDistance, depth);
                    float dstTravelled = offset;
                    float stepSize = dstLimit / n;
                    
                    float lightScattered = 0;
                    [loop]
                    while (dstTravelled < dstLimit)
                    {
                        float3 rayPos = rayStart + rayDir * dstTravelled;
                        if (!isInShadow_rayMarched(rayPos, float3(0, 0, 0),lightDir))
                        {
                            lightScattered += 0.01 * stepSize / 2;
                        }
                        dstTravelled += stepSize;
                    }
                    return lightScattered;
            }

            // This function projects a point onto a plane defined by a normal and a point on the plane
            float3 getIntersectionWithPlane(float3 pos, float3 normal, float3 planePoint)
            {
                float3 v = pos - planePoint;
                float d = dot(v, normal) / dot(normal, normal);
                return pos - d * normal;
            }

            float2 getShadowMapUV(float3 worldPosition)
            {
                // Calculate the intersection of the worldPosition with the plane defined by startPos and lightDir
                float3 intersection = getIntersectionWithPlane(worldPosition, _LightDir, _ShadowMapOrigin);

                // Convert the world position to coordinates on the plane using the right and up vectors
                float2 planeCoords;
                planeCoords.x = dot(intersection - _ShadowMapOrigin, _LightRight);
                planeCoords.y = dot(intersection - _ShadowMapOrigin, _LightUp);

                // Normalize these coordinates based on the shadow map's coverage
                float2 normalizedPosition = float2(
                    (planeCoords.x) / _ShadowMapCoverage.x,
                    (planeCoords.y) / _ShadowMapCoverage.y
                );

                // Convert normalized position to UV coordinates by shifting to [0, 1] range
                float2 uv = normalizedPosition + float2(0.5, 0.5);
                return uv;
            }

            bool isInShadow(float3 pos, float3 normal, float3 lightDir)
            {
                float3 startPos = pos;
                float3 rayDir = lightDir;
                startPos += normal * 0.002;

                float3 shadowMapPos = getIntersectionWithPlane(startPos, lightDir, _ShadowMapOrigin);
                float2 shadowMapUV = getShadowMapUV(startPos);
                float shadowMapDepth = tex2D(_ShadowMap, shadowMapUV).r;

                if (distance(startPos, shadowMapPos) < shadowMapDepth)
                { 
                    return false;
                }
                return true;
            }

            float getPlayerSpotLight(float3 pos, float3 normal)
            {
                float cosAngle = dot(normalize(_PlayerLightDir), normalize(pos - _PlayerLightPos));
                float angle = acos(cosAngle) * (180.0 / 3.14159265);
             
                float dstToPlayer = distance(pos, _PlayerLightPos);
                float light = 1;
                light *= pow(saturate((_PlayerLightAngle - angle) / _PlayerLightAngle), 2);
                return light * (1 - saturate(dstToPlayer / _PlayerLightRange)) * _PlayerLightIntensity;
            }

            float computeLighting(float3 pos, float3 normal, float3 lightDir, bool useShadowMap)
            {
                float light = dot(lightDir, normal); 
                if (!useShadowMap && isInShadow_rayMarched(pos, normal, lightDir) == true) light = 0; 
                if (useShadowMap && isInShadow(pos, normal, lightDir) == true) light = 0; 

                float playerSpotLight = getPlayerSpotLight(pos, normal);
                light += playerSpotLight;

                float occlusion = calculateOcclusion(pos, normal, 9, 0.3);
                float ambientLight = 0.025 + 0.075 * saturate(pos.y / _WorldTextureSize.y);
                return max(ambientLight, saturate(light)) * occlusion;
            }

            float getLightShaft(float3 rayStart, float3 rayDir, float3 lightDir, float depth, float offset)
            {
                    float n = _LightShaftSampleCount;
                    float dstLimit = min(_LightShaftRenderDistance, depth);
                    float dstTravelled = offset;
                    float stepSize = dstLimit / n;
                    float blendStart = _LightShaftFadeStart * _LightShaftRenderDistance;
                     
                    float lightScattered = 0;
                    [loop]
                    while (dstTravelled < dstLimit)
                    {
                        float blendFactor = 1 - saturate((dstTravelled - blendStart) / (_LightShaftRenderDistance - blendStart));
                        float3 rayPos = rayStart + rayDir * dstTravelled;
                        
                        // Volumetric sun light
                        if (rayPos.y >= _WorldTextureSize.y || isInShadow(rayPos, float3(0, 0, 0), lightDir) == false)
                        {
                            lightScattered += 0.01 * stepSize * blendFactor * _LightShaftIntensity;
                        }

                        // Volumetric spotlight
                        float playerSpotLight = getPlayerSpotLight(rayPos, float3(0, 0, 0));
                        lightScattered += 0.05 * playerSpotLight * blendFactor * stepSize * _PlayerLightVolumetricIntensity;
                        dstTravelled += stepSize;
                    }
                    return min(lightScattered, _LightShaftMaximumValue);
            }


            fixed4 frag (v2f i : SV_Depth) : SV_Target
            {
                // Create ray
                float3 rayPos = _CameraPos;
                float viewLength = length(i.viewVector);
                float3 rayDir = i.viewVector.xyz;
                float3 lightDir = _WorldSpaceLightPos0;

                // Sample background
                float4 backgroundColor = tex2D(_MainTex, i.uv);

                // Read G-Buffer map
                float3 pos = tex2D(_PositionTexture, i.uv).xyz;
                float depth = tex2D(_DepthTexture, i.uv).r;
                float3 normal = tex2D(_NormalTexture, i.uv);
                uint block = round(tex2D(_BlockTexture, i.uv).r);

                bool isBackground = block == 0;
                float4 worldColor = getBlockColor(block, pos, normal);// * getOutline(pos);

                // Compute fog
                float fog = saturate(depth / 5000) * 1;
                float4 fogColor = saturate(dot(lightDir, float3(0, 1, 0)));
                
                // Compute lighting
                float offset = tex2D(_NoiseTexture, i.uv/1) * 2;
                float lightShaft = getLightShaft(rayPos, rayDir, lightDir, depth, offset);
                float lightShaftTimeMultiplier = saturate(dot(float3(0, 1, 0), lightDir)) * 2;
                lightShaft *= lightShaftTimeMultiplier;

                if (isBackground)
                {
                    if (depth < 100)
                    {
                        float3 objectPos = rayPos + rayDir * depth;
                        float3 light = 1;
                        if (isInShadow_rayMarched(objectPos, float3(0, 0, 0), lightDir))
                        {
                            light = 0.2;
                        }
                        return float4(backgroundColor.rgb * light, 1);
                    }
                    return backgroundColor;
                }

                float lightIntensity = computeLighting(pos, normal, lightDir, false);
                return float4(worldColor.rgb * lightIntensity + lightShaft, worldColor.a) + fogColor * fog;
            }

            ENDCG
        }
    }
}
