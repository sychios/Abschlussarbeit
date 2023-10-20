Shader "Hidden/Blinders"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BgColor("Background Color", Color) = (0,0,0,1)
        _blindersActive("Set Active Blinder", Vector) = (0,0,0,0) // Which Blinders are to be active? (LeftDown, LeftUp, RightDown, RightUp)
        _a_radius_right("a Radius Right", Range(.5, .75)) = .5
        _b_radius_right("b Radius Right", Range(.7, 2)) = .72
        _a_radius_left("a Radius Left", Range(.5, .75)) = .5
        _b_radius_left("b Radius Left", Range(.7, 2)) = .72
    }
        SubShader
    {
        Cull Off ZWrite Off ZTest Always

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

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
                
                UNITY_VERTEX_OUTPUT_STEREO
            };

            

            float _border;

            fixed4 _BoundColor;
            fixed4 _BgColor;
            float _circleSizePercent;

            //sampler2D _MainTex;

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
            half4 _MainTex_ST;


            vector _blindersActive;
            float _a_radius_right;
            float _b_radius_right;
            float _a_radius_left;
            float _b_radius_left;

            
            // vertex function
            // take shape of model, potentially modify it
            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = v.uv;
                return o;
            }

            float2 antialiase(float radius, float borderSize, float dist)
            {
                float t = smoothstep(radius + borderSize, radius - borderSize, dist);
                return t;
            }
            
            // fragment function
            // Colors, Textures, Values set by user
            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                fixed4 c = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);
                
                //fixed4 c = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
                //fixed4 c = tex2D(_MainTex, i.uv);
                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                
                float x = i.uv.x;
                float y = i.uv.y;

                float d = 0;
                
                //    float aliasVal = antialiase(radius, _border, dis);
                //    col = lerp(_BgColor, tex2D(_MainTex, i.uv), aliasVal);
                //    return col;
                
                
                if(_blindersActive.x == 1) // Left
                {
                    if(screenUV.x < 0.5f)
                    {
                        d = pow(0.5 - x, 2) / pow(_a_radius_left, 2) + pow(0.5 - y, 2) / pow(_b_radius_left, 2);
                        
                        if(d > .5)
                        {
                            return _BgColor;
                        }
                        return c;
                    }
                    
                }
                if(_blindersActive.y == 1) // Right
                {
                    if(screenUV.x >= 0.5)
                    {
                        d = pow(0.5 - x, 2) / pow(_a_radius_right, 2) + pow(0.5 - y, 2) / pow(_b_radius_right, 2);
                        
                        if(d > .5)
                        {
                            return _BgColor;
                        }
                        return c;
                    }
                }

                return c;
                //if (dis > radius && screenUV.x < 0.5f) {
                //    float aliasVal = antialiase(radius, _border, dis);
                //    col = lerp(_BgColor, tex2D(_MainTex, i.uv), aliasVal);
                //    return col;
                //}
            }
            
            ENDCG
        }
    }
}