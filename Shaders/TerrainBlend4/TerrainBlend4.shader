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

        _MetallicSmoothnessMap ("Metallic (R) / Smoothness (A)", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.0
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

        sampler2D _MetallicSmoothnessMap;
        half _Metallic;
        half _Smoothness;
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
            float2 uv_MetallicSmoothnessMap;
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

            fixed4 ms = tex2D(_MetallicSmoothnessMap, IN.uv_MetallicSmoothnessMap);

            o.Albedo = albedo.rgb;
            o.Normal = blended;
            o.Metallic = ms.r * _Metallic;
            o.Smoothness = ms.a * _Smoothness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
