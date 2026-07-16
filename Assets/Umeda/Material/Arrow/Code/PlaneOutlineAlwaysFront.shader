Shader "Custom/PlaneOutlineAlwaysFront"
{
    Properties
    {
        _MainColor ("Base Color", Color) = (0, 0.5, 1, 0.3)
        _OutlineColor ("Outline Color", Color) = (0, 0.8, 1, 0.8)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
    }
    SubShader
    {
        // 透明なものよりさらに後に描画されるように +100 などのオフセットを持たせる
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" }
        LOD 100
        
        // 常に手前に描画するが、ZWriteをOffにすることで、
        // 深度バッファを汚さず、後ろのオブジェクトが正しく透けるようにする
        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        // --- パス1: アウトライン ---
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct v2f { float4 pos : SV_POSITION; };
            float4 _OutlineColor;
            float _OutlineWidth;

            v2f vert(appdata_full v) {
                v2f o;
                float3 normal = normalize(v.normal);
                v.vertex.xyz += normal * _OutlineWidth;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target { return _OutlineColor; }
            ENDCG
        }

        // --- パス2: 中身 ---
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct v2f { 
                float4 pos : SV_POSITION;
                float3 localPos : TEXCOORD0; 
            };
            fixed4 _MainColor;
            float _FadeDistance = 0.5; // 追加

            v2f vert(appdata_full v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.localPos = v.vertex.xyz; // 距離計算用
                return o;
            }

            fixed4 frag(v2f i) : SV_Target { 
                float dist = length(i.localPos);
                float alphaMask = 1.0 - smoothstep(0.0, _FadeDistance, dist);
                fixed4 col = _MainColor;
                col.rgb = lerp(float3(0,0,0), col.rgb, alphaMask); // 外側を黒く
                col.a *= alphaMask; // 外側を透過
                return col;
            }
            ENDCG
        }
    }
}