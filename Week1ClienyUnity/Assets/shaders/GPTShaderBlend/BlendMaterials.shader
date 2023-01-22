Shader "Custom/BlendMaterials" {
    Properties{
        _MainTex1("Material 1 (RGB)", 2D) = "white" {}
        _MainTex2("Material 2 (RGB)", 2D) = "white" {}
        _Blend("Blend", Range(0, 1)) = 0.5
    }

        SubShader{
            Tags {"Queue" = "Opaque"}
            LOD 200

            CGPROGRAM
            #pragma surface surf Lambert

            sampler2D _MainTex1;
            sampler2D _MainTex2;
            float _Blend;

            struct Input {
                float2 uv_MainTex1;
                float2 uv_MainTex2;
            };

            fixed4 _MainTex1_ST2;
            fixed4 _MainTex2_ST2;

            void surf(Input IN, inout SurfaceOutput o) {
                fixed4 texel1 = tex2D(_MainTex1, IN.uv_MainTex1);
                fixed4 texel2 = tex2D(_MainTex2, IN.uv_MainTex2);
                o.Albedo = lerp(texel1, texel2, _Blend);
                o.Alpha = 1;
            }
            ENDCG
        }
            FallBack "Diffuse"
}