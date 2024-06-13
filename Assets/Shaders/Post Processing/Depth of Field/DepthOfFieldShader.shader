// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'

Shader"Custom/DepthOfFieldPostProcess"
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
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _DepthTexture;
            int2 _ScreenSize;
            float _BlurStart;
            float _BlurEnd;
            float _BlurScale;

            fixed4 frag(v2f i) : SV_Target
            {
                float depth = tex2D(_DepthTexture, i.uv);
                if (depth >= 1e10) return tex2D(_MainTex, i.uv);

                float normalizedDepth = (depth - _BlurStart) / (_BlurEnd - _BlurStart);
                normalizedDepth = saturate(normalizedDepth);
                float blurAmount = normalizedDepth * _BlurScale / _ScreenSize.x * 1920.0; 

                float offsetSize = blurAmount * 0.002;
                
                float2 offsets[4] = {
                    float2(-offsetSize, -offsetSize),
                    float2(-offsetSize, offsetSize),
                    float2(offsetSize, -offsetSize),
                    float2(offsetSize, offsetSize)
                };

                fixed4 color = fixed4(0,0,0,0);

                // Sum all samples
                for (int j = 0; j < 4; j++)
                {
                    color += tex2D(_MainTex, i.uv + offsets[j]);
                }

                // Average the colors
                color /= 4.0; 

                //return float4(i.uv, 1, 1);
                return color;
            }
            ENDCG
        }
    }
}
