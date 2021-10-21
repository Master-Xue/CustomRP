Shader "Custom/MyFirstShader"
{
    Properties
    {
        _Tint ("Tint" , Color)=(1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {

        Pass
        {
            CGPROGRAM
            #pragma vertex MyVertexProgram
            #pragma fragment MyFragmentProgram

            #include "UnityCG.cginc"

            struct VertexData
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 position : SV_POSITION;
            };

            float4 _Tint;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            Interpolators MyVertexProgram (VertexData v)
            {
                Interpolators i;
                i.position = UnityObjectToClipPos(v.position);
                i.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return i;
            }

            fixed4 MyFragmentProgram (Interpolators i) : SV_Target
            {
                return tex2D(_MainTex,i.uv) * _Tint;
            }
            ENDCG
        }
    }
}
