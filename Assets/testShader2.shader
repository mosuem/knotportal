Shader "testEffectShader2" {
    Properties{
        // _RenderPaintTexture ("Render Texture", 2D) = "white" {}
    	_MaskReplace("Mask Replace Texture", 2D) = "white" {}
		_ColorTest ("Tint", Color) = (0, 0, 0, 1)
    }
SubShader {
        Tags { "RenderType1"="RenderType1" }
        Pass {
            Lighting Off Fog { Mode Off }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#include "UnityCG.cginc"
            sampler2D _MaskReplace;
			float4 _MaskReplace_ST;
            //tint of the texture
			fixed4 _ColorTest;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            uniform int _regionCount;
            uniform float4 _regions[1000];

            float isLeftOfLine (float2 test)
            {
                int c = 0;
                int j = _regionCount-1;
                for (int i = 0; i < _regionCount; j = i++) {
                if (((_regions[i].y>test.y) != (_regions[j].y>test.y)) &&
                    (test.x < (_regions[j].x-_regions[i].x) * (test.y-_regions[i].y) / (_regions[j].y-_regions[i].y) + _regions[i].x)){
                        c = !c;
                    }
                }
                return c;
            }
           
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = v.uv;
                return o;
            }
           
            half4 frag (v2f i) : SV_Target
            {
                float2 screenPos = (i.vertex.xy / _ScreenParams.xy);
                #if UNITY_UV_STARTS_AT_TOP
                screenPos.y *= -_ProjectionParams.x;
                #endif
                clip(-1+isLeftOfLine(screenPos));
                // return half4(screenPos.x, 1.0, 1.0, 1.0);

                // float2 screenPos = i.screenPosition.xy / i.screenPosition.w;
                // float aspect = _ScreenParams.x / _ScreenParams.y;
                // screenPos.x = screenPos.x * aspect;
                screenPos = TRANSFORM_TEX(screenPos, _MaskReplace);
                float4 col = tex2D(_MaskReplace, screenPos);
                col *= _ColorTest;
                return float4(col.rgb,1.0);
            }
            ENDCG
        }
    }
    SubShader {
        Tags { "RenderType"="RenderType2" }
        Pass {
            Lighting Off Fog { Mode Off }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
    
            struct v2f {
                float4 pos : POSITION;
                float3 Z : TEXCOORD0;
            };
    
            v2f vert (float4 vertex : POSITION) {
                v2f o;
                float4 oPos = UnityObjectToClipPos(vertex);
                o.pos = oPos;
                o.Z = oPos.zzz;
                return o;
            }
            half4 frag( v2f i ) : COLOR {
                return i.Z.xxxx;
            }
            ENDCG
        }
    }
}