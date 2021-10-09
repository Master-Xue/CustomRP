Shader "Custom/TextureSplatting"
{
    Properties
    {
        _MainTex ("Splat Map", 2D) = "white" {}
        [NoScaleOffset] _Texture1 ("Texture 1", 2D)= "white" {}
        [NoScaleOffset] _Texture2 ("Texture 2", 2D)= "white" {}
        [NoScaleOffset] _Texture3 ("Texture 3", 2D)= "white" {}
        [NoScaleOffset] _Texture4 ("Texture 4", 2D)= "white" {}
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
                float2 uvSplat : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Texture1,_Texture2,_Texture3,_Texture4;

            Interpolators MyVertexProgram (VertexData v)
            {
                Interpolators i;
                i.position = UnityObjectToClipPos(v.position);
                i.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                i.uvSplat = v.uv;
                return i;
            }

            fixed4 MyFragmentProgram (Interpolators i) : SV_Target
            {
                float4 splat = tex2D(_MainTex,i.uvSplat);
                return tex2D(_Texture1,i.uv) * splat.r + 
                tex2D(_Texture2,i.uv) * splat.g +
                tex2D(_Texture3,i.uv)*splat.b + 
                tex2D(_Texture4,i.uv)*(1-splat.r-splat.g-splat.b);
            }
            ENDCG
        }
    }
}
