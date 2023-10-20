Shader "Hidden/DynamicBlinder"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"


            sampler2D _CameraDepthTexture;

            float4x4 _LeftEyeToWorld;
            float4x4 _RightEyeToWorld;
            float4x4 _LeftEyeProjection;
            float4x4 _RightEyeProjection;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = v.vertex * float4(2,2,1,1) + float4(-1,-1,0,0);
                o.uv = v.texcoord;
                o.uv.y = 1.0f - o.uv.y;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(i.uv));

                float4x4 proj, eyeToWorld;
				if (unity_StereoEyeIndex == 0)
				{
					proj = _LeftEyeProjection;
					eyeToWorld = _LeftEyeToWorld;
				}
				else
				{
					proj = _RightEyeProjection;
					eyeToWorld = _RightEyeToWorld;
				}

                float2 uvClip = i.uv * 2.0 - 1.0;
				float4 clipPos = float4(uvClip, d, 1.0);
                float4 viewPos = mul(proj, clipPos);
                viewPos /= viewPos.w;
                float3 worldPos = mul(eyeToWorld, viewPos).xyz;

                fixed3 color = pow(abs(cos(worldPos * UNITY_PI * 4)), 20);
                return fixed4(color, 1);
            }
            ENDCG
        }
    }
}
