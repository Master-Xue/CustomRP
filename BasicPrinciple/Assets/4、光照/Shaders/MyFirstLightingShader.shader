Shader "Custom/MyFirstLightingShader"
{
    Properties
    {
        _Tint ("Tint" , Color)=(1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}
        [Gamma] _Metallic ("Metallic",Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
    }
    SubShader
    {

        Pass
        {
            Tags {
                "LightMode" = "ForwardBase"
            }

            CGPROGRAM
            #pragma target 3.0

            #pragma vertex MyVertexProgram
            #pragma fragment MyFragmentProgram

            //#include "UnityCG.cginc"
            //#include "UnityStandardBRDF.cginc"
            //#include "UnityStandardUtils.cginc"
            #include "UnityPBSLighting.cginc"

            struct VertexData
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 position : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 worldPos :TEXCOORD2;
            };

            float4 _Tint;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Metallic;
            float _Smoothness;

            Interpolators MyVertexProgram (VertexData v)
            {
                Interpolators i;
                i.position = UnityObjectToClipPos(v.position);
                i.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                i.normal = UnityObjectToWorldNormal(v.normal);
                i.worldPos = mul(unity_ObjectToWorld,v.position);
                return i;
            }

            fixed4 MyFragmentProgram (Interpolators i) : SV_Target
            {
                i.normal = normalize(i.normal);
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 lightColor = _LightColor0.rgb;
                float3 albedo = tex2D(_MainTex,i.uv).rgb * _Tint.rgb;
                float3 specularTint;
                float oneMinusReflectivity;
                albedo = DiffuseAndSpecularFromMetallic(albedo,_Metallic,specularTint,oneMinusReflectivity);

                UnityLight light;
                light.color = lightColor;
                light.dir = lightDir;
                light.ndotl = DotClamped(i.normal,lightDir);

                UnityIndirect indirectLight;
                indirectLight.diffuse = 0;
                indirectLight.specular = 0;

                return UNITY_BRDF_PBS(albedo,specularTint,oneMinusReflectivity,_Smoothness,i.normal,viewDir,light,indirectLight);
            }
            ENDCG
        }
    }
}
