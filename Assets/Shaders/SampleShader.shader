Shader "Custom/SampleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PosTex ("PositionTexture", 2D) = "black" {}
        _NormalText ("NormalTexture", 2D) = "white" {}
        _Length("AnimatorLength", float) = 1
        _DeltaTime("DeltaTime", float) = 0
    }
    
    SubShader {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define TS _PosTex_TexelSize

            struct appdata {
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float3 normal: TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex, _PosTex, _NormalText;
            float4 _PosTex_TexelSize;
            float _Length, _DeltaTime;

            v2f vert(appdata v, uint vid : SV_VertexID) {
                float t = (_Time.y - _DeltaTime ) / _Length;
                t = fmod(t, 1.0);
                float x = (vid + 0.5) + TS.x;
                float y = t;
                float3 pos = tex2Dlod(_PosTex, float4 (x, y, 0, 0));
                float normal = tex2Dlod(_NormalText, float4 (x, y, 0, 0));
                v2f o;
                o.vertex = UnityObjectToClipPos(pos);
                o.normal = UnityObjectToWorldNormal(normal);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target{
                half diff = dot(i.normal, float3(0,1,0) * 0.5 + 0.5);
                half4 col = tex2D(_MainTex, i.uv);
                return diff * col;
            }
            ENDCG
        }
    }
}
