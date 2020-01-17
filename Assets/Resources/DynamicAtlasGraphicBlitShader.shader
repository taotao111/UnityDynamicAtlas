// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "DynamicAtlas/GraphicBlit" {
	Properties {
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}
	}
	SubShader {
		ZTest Always
		Cull Off
		ZWrite Off
		Fog { Mode off }
		
		Pass  {
			Blend Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			half4 _DrawRect;

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			v2f o;

			v2f vert(appdata_t v) {
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				return o;
			}

			half4 frag(v2f i) : COLOR {
				if (i.texcoord.x < _DrawRect.x || i.texcoord.x > _DrawRect.z ||
					i.texcoord.y < _DrawRect.y || i.texcoord.y > _DrawRect.w) {
					discard;
				}
				float2 size = _DrawRect.zw - _DrawRect.xy;
				float2 uv = (i.texcoord.xy - _DrawRect.xy) / size;
				return tex2D(_MainTex, uv);
			}

			ENDCG
		}
	} 
	FallBack off
}
