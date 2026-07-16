Shader "Custom/AlwaysFront"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _MainColor ("Base Color", Color) = (0, 0.5, 1, 0.3)
        _OutlineColor ("Outline Color", Color) = (0, 0.8, 1, 0.8)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
    }
    SubShader
    {
        // 描画順を最後にし、不透明として処理する
        Tags 
        { 
            "Queue" = "Overlay" 
            "RenderType" = "Opaque" 
        }

        Pass
        {
            // 深度テストを常に通過させる（常に手前に表示）
            ZTest Always
            // ZWriteは基本的にOnのままでOK
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}