Shader "Puru/TerrainBlend4"
{
    Properties
    {
        _MaskTex ("Splat Mask (RGBA = вес слоя 0..3)", 2D) = "white" {}

        _Texture0 ("Layer 0 Albedo", 2D) = "white" {}
        _Normal0 ("Layer 0 Normal", 2D) = "bump" {}

        _Texture1 ("Layer 1 Albedo", 2D) = "white" {}
        _Normal1 ("Layer 1 Normal", 2D) = "bump" {}

        _Texture2 ("Layer 2 Albedo", 2D) = "white" {}
        _Normal2 ("Layer 2 Normal", 2D) = "bump" {}

        _Texture3 ("Layer 3 Albedo", 2D) = "white" {}
        _Normal3 ("Layer 3 Normal", 2D) = "bump" {}

        _MetallicSmoothness0 ("Layer 0 Metallic (R) / Smoothness (A)", 2D) = "white" {}
        _Metallic0 ("Layer 0 Metallic", Range(0,1)) = 0.0
        _Smoothness0 ("Layer 0 Smoothness", Range(0,1)) = 0.0

        _MetallicSmoothness1 ("Layer 1 Metallic (R) / Smoothness (A)", 2D) = "white" {}
        _Metallic1 ("Layer 1 Metallic", Range(0,1)) = 0.0
        _Smoothness1 ("Layer 1 Smoothness", Range(0,1)) = 0.0

        _MetallicSmoothness2 ("Layer 2 Metallic (R) / Smoothness (A)", 2D) = "white" {}
        _Metallic2 ("Layer 2 Metallic", Range(0,1)) = 0.0
        _Smoothness2 ("Layer 2 Smoothness", Range(0,1)) = 0.0

        _MetallicSmoothness3 ("Layer 3 Metallic (R) / Smoothness (A)", 2D) = "white" {}
        _Metallic3 ("Layer 3 Metallic", Range(0,1)) = 0.0
        _Smoothness3 ("Layer 3 Smoothness", Range(0,1)) = 0.0

        _NormalStrength ("Normal Strength", Range(0,2)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.5

        sampler2D _MaskTex;

        sampler2D _Texture0; sampler2D _Normal0;
        sampler2D _Texture1; sampler2D _Normal1;
        sampler2D _Texture2; sampler2D _Normal2;
        sampler2D _Texture3; sampler2D _Normal3;

        sampler2D _MetallicSmoothness0; half _Metallic0; half _Smoothness0;
        sampler2D _MetallicSmoothness1; half _Metallic1; half _Smoothness1;
        sampler2D _MetallicSmoothness2; half _Metallic2; half _Smoothness2;
        sampler2D _MetallicSmoothness3; half _Metallic3; half _Smoothness3;

        half _NormalStrength;

        // uv_TextureN -> TEXCOORD0 (channel "uv" mesh, мировые координаты в метрах),
        // каждая своей _TextureN_ST-трансформацией (тайлинг из инспектора материала).
        // Тот же канал используется Mesh.RecalculateTangents(), так что normal mapping
        // тангенты совпадают по направлению с реальной UV нормалей.
        // maskUV читается вручную из TEXCOORD2 (channel "uv3" mesh) в vert() —
        // именование uv3_/uv4_ не гарантированно поддерживается генератором surface-шейдеров,
        // поэтому не полагаемся на авто-трансформ для маски (она и не должна тайлиться).
        struct Input
        {
            float2 uv_Texture0;
            float2 uv_Texture1;
            float2 uv_Texture2;
            float2 uv_Texture3;
            float2 maskUV;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.maskUV = v.texcoord2.xy;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 mask = tex2D(_MaskTex, IN.maskUV);
            fixed sum = max(mask.r + mask.g + mask.b + mask.a, 0.0001);
            fixed4 w = mask / sum;

            fixed4 albedo0 = tex2D(_Texture0, IN.uv_Texture0);
            fixed4 albedo1 = tex2D(_Texture1, IN.uv_Texture1);
            fixed4 albedo2 = tex2D(_Texture2, IN.uv_Texture2);
            fixed4 albedo3 = tex2D(_Texture3, IN.uv_Texture3);

            fixed4 albedo = albedo0 * w.r + albedo1 * w.g + albedo2 * w.b + albedo3 * w.a;

            fixed3 n0 = UnpackNormal(tex2D(_Normal0, IN.uv_Texture0));
            fixed3 n1 = UnpackNormal(tex2D(_Normal1, IN.uv_Texture1));
            fixed3 n2 = UnpackNormal(tex2D(_Normal2, IN.uv_Texture2));
            fixed3 n3 = UnpackNormal(tex2D(_Normal3, IN.uv_Texture3));

            fixed3 blended = n0 * w.r + n1 * w.g + n2 * w.b + n3 * w.a;
            blended.xy *= _NormalStrength;
            blended = normalize(blended);

            fixed4 ms0 = tex2D(_MetallicSmoothness0, IN.uv_Texture0);
            fixed4 ms1 = tex2D(_MetallicSmoothness1, IN.uv_Texture1);
            fixed4 ms2 = tex2D(_MetallicSmoothness2, IN.uv_Texture2);
            fixed4 ms3 = tex2D(_MetallicSmoothness3, IN.uv_Texture3);

            fixed metallic =
                ms0.r * _Metallic0 * w.r +
                ms1.r * _Metallic1 * w.g +
                ms2.r * _Metallic2 * w.b +
                ms3.r * _Metallic3 * w.a;

            fixed smoothness =
                ms0.a * _Smoothness0 * w.r +
                ms1.a * _Smoothness1 * w.g +
                ms2.a * _Smoothness2 * w.b +
                ms3.a * _Smoothness3 * w.a;

            o.Albedo = albedo.rgb;
            o.Normal = blended;
            o.Metallic = metallic;
            o.Smoothness = smoothness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
