Shader "Hidden/BWDiffuse" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma fragment frag
			#pragma vertex vert
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _OtherTex;
			uniform sampler2D _CameraTex0;
			uniform sampler2D _CameraTex1;
			uniform sampler2D _CameraTex2;
			uniform sampler2D _CameraTex3;
			uniform sampler2D _CameraTex4;
			uniform sampler2D _CameraTex5;
			uniform sampler2D _CameraTex6;
            half4 _MainTex_ST;
            half4 _OtherTex_ST;
            half4 _CameraTex0_ST;
            half4 _CameraTex1_ST;
            half4 _CameraTex2_ST;
            half4 _CameraTex3_ST;
            half4 _CameraTex4_ST;
            half4 _CameraTex5_ST;
            half4 _CameraTex6_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };
            uniform float4 _regions[1000];
            uniform float4 _boundingBoxes[1000];
            uniform int _startsEndsCount;
            uniform int _defaultGenerator;
            uniform float4 _startsEnds[1000];
            uniform float2 _generators[1000];
            uniform float4 _spheres0;
            uniform float4 _spheres1;
            uniform float4 _spheres2;
            uniform float4 _spheres3;
            uniform float4 _spheres4;
            uniform float4 _spheres5;
            uniform float4 _spheres6;

            float isInPolygon (float2 startEnd, float2 test)
            {
                int c = 0;
                int j = startEnd.y-1;
                for (int i = startEnd.x; i < startEnd.y; j = i++) {
                if (((_regions[i].y>test.y) != (_regions[j].y>test.y)) &&
                    (test.x < (_regions[j].x-_regions[i].x) * (test.y-_regions[i].y) / (_regions[j].y-_regions[i].y) + _regions[i].x)){
                        c = !c;
                    }
                }
                return c;
            }

            float getDist(float2 isIn, float4 pos2){
                return sqrt((isIn.x - pos2.x)*(isIn.x - pos2.x) + (isIn.y - pos2.y)*(isIn.y - pos2.y)) < pos2.z;
            }

            int GetGenerator(float2 screenPosTest)
            {
                for(int index = 0; index < _startsEndsCount; index++){
                    float2 startEnd = _startsEnds[index];
                    float4 boundingBox = _boundingBoxes[index];
                    if(screenPosTest.x >= boundingBox.x && screenPosTest.x <= boundingBox.y && screenPosTest.y >= boundingBox.z && screenPosTest.y <= boundingBox.w){
                        if(index == 0 && getDist(screenPosTest, _spheres0)>0){
                            return _generators[0].x;
                        }else if(index == 1 && getDist(screenPosTest, _spheres1)>0){
                            return _generators[1].x;
                        }else if(index == 2 && getDist(screenPosTest, _spheres2)>0){
                            return _generators[2].x;
                        }else if(index == 3 && getDist(screenPosTest, _spheres3)>0){
                            return _generators[3].x;
                        }else if(index == 4 && getDist(screenPosTest, _spheres4)>0){
                            return _generators[4].x;
                        }else if(index == 5 && getDist(screenPosTest, _spheres5)>0){
                            return _generators[5].x;
                        }else if(index == 6 && getDist(screenPosTest, _spheres6)>0){
                            return _generators[6].x;
                        }else{
                            float isIn = isInPolygon(startEnd, screenPosTest);
                            if(isIn > 0){
                                return _generators[index].x;
                            }
                        }
                    }
                }
                return _defaultGenerator;
            }

            float4 GetPixelInRegion(int generator, v2f i)
            {
                if(generator == 0){
                        float2 uv2 = UnityStereoScreenSpaceUVAdjust(i.uv, _CameraTex0_ST);
                        uv2.y = 1-uv2.y;
                        return tex2D(_CameraTex0, uv2);
                }else if(generator == 1){
                        float2 uv2 = UnityStereoScreenSpaceUVAdjust(i.uv, _CameraTex1_ST);
                        uv2.y = 1-uv2.y;
                        return tex2D(_CameraTex1, uv2);
                }else if(generator == 2){
                        float2 uv2 = UnityStereoScreenSpaceUVAdjust(i.uv, _CameraTex2_ST);
                        uv2.y = 1-uv2.y;
                        return tex2D(_CameraTex2, uv2);
                }else if(generator == 3){
                        float2 uv2 = UnityStereoScreenSpaceUVAdjust(i.uv, _CameraTex3_ST);
                        uv2.y = 1-uv2.y;
                        return tex2D(_CameraTex3, uv2);
                }else if(generator == 4){
                        float2 uv2 = UnityStereoScreenSpaceUVAdjust(i.uv, _CameraTex4_ST);
                        uv2.y = 1-uv2.y;
                        return tex2D(_CameraTex4, uv2);
                }else if(generator == 5){
                        float2 uv2 = UnityStereoScreenSpaceUVAdjust(i.uv, _CameraTex5_ST);
                        uv2.y = 1-uv2.y;
                        return tex2D(_CameraTex5, uv2);
                }else if(generator == 6){
                        float2 uv2 = UnityStereoScreenSpaceUVAdjust(i.uv, _CameraTex6_ST);
                        uv2.y = 1-uv2.y;
                        return tex2D(_CameraTex6, uv2);
                }else{
                        return float4(0.5,0,0.5,1);
                }
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = v.uv;
                return o;
            }

			half4 frag(v2f i) : SV_Target {
                float2 screenPos = i.screenPos;
                #if !defined(UNITY_SINGLE_PASS_STEREO)
                // #if UNITY_UV_STARTS_AT_TOP
                        i.uv.y = 1-i.uv.y;
                //         i.screenPos.y = 1-i.screenPos.y;
                #endif
                #if defined(UNITY_SINGLE_PASS_STEREO)
                    screenPos.x -= unity_StereoEyeIndex * 0.5;
                    screenPos.x *= 2;
                #endif

                int generator = GetGenerator(screenPos);
                // if(generator == -1)
                // {
                //     return tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));
                // }
				
				return GetPixelInRegion(generator, i);
			}
			ENDCG
		}
	}
}