Shader "Custom/Unlit Blend" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Unlitness ("Unlitness", Range(0, 1)) = 0
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert

        struct Input {
            float2 uv_MainTex;
        };
        sampler2D _MainTex;
        float _Unlitness;

        void surf(Input IN, inout SurfaceOutput o) {
            o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb * (1 - _Unlitness);
            o.Emission = tex2D (_MainTex, IN.uv_MainTex).rgb * _Unlitness;
            o.Alpha = tex2D (_MainTex, IN.uv_MainTex).a;
        }

        ENDCG
    }
    Fallback "Diffuse"
}