Shader "Custom/ReversibleDissolve"
{
    Properties
    {
        // === Reversible のプロパティ ===
        _Color ("Color (Front)", Color) = (1,1,1,1)     // ベース色 (表面)
        _MainTex ("Texture (Front)", 2D) = "white" {}   // 表面テクスチャ
        _MainTex2 ("Texture (Back)", 2D) = "white" {}   // 裏面テクスチャ

        // === CardShine のプロパティ ===
        _ShineColor ("Shine Color", Color) = (1,1,1,1)
        _ShineSpeed ("Shine Speed", Range(0, 10)) = 1
        _ShineWidth ("Shine Width", Range(0, 1)) = 0.1
        _ShinePosition ("Shine Position", Range(-1, 2)) = 0
        _ShineIntensity ("Shine Intensity", Range(0, 1)) = 0
        _IsShining ("Is Shining", Range(0, 1)) = 0
        _EffectTime ("Effect Time", Float) = 0
        
        // === Dissolve から追加したプロパティ ===
        _DisolveTex ("Dissolve Map (RGB)", 2D) = "white" {}
        _Threshold("Dissolve Threshold", Range(0,1))= 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "LightMode"="ForwardBase" "Queue"="Transparent" }
        //Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 200
        Cull Off // 両面描画を有効にする

        // ----------------------------------------------------
        // 1. ShadowCaster Pass (影の描画 - ディゾルブクリッピングを適用)
        // ----------------------------------------------------
        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert_shadow
            #pragma fragment frag_shadow
            #pragma target 3.0
            #include "UnityCG.cginc"

            // ディゾルブ用プロパティを定義
            sampler2D _DisolveTex;
            float _Threshold;
            float4 _DisolveTex_ST; // テクスチャタイリング・オフセット用

            struct v2f_shadow
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0; // UVを追加
            };

            v2f_shadow vert_shadow (appdata_base v)
            {
                v2f_shadow o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _DisolveTex); // UVの変換
                return o;
            }

            fixed4 frag_shadow(v2f_shadow i) : SV_Target
            {
                // === ディゾルブ処理 (影を落とさない部分のクリッピング) ===
                // 影は表面から落ちるため、表面のディゾルブテクスチャで判定
                fixed4 m = tex2D(_DisolveTex, i.uv);
                half g = m.r; 
                
                // 閾値より小さい（暗い）ピクセルは破棄
                if (g < _Threshold)
                {
                    discard; 
                }
                
                // 影を落とす場合は深度情報のみ書き込む
                return 0;
            }
            ENDCG
        }
        
        // ----------------------------------------------------
        // 2. Base Pass (色と光沢の描画 - 裏表切り替えとディゾルブを適用)
        // ----------------------------------------------------
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            // === 統合されたプロパティ ===
            sampler2D _MainTex;
            sampler2D _MainTex2;
            sampler2D _DisolveTex; // 追加

            float4 _Color;
            float4 _ShineColor;
            float _ShineSpeed;
            float _ShineWidth;
            float _ShinePosition;
            float _ShineIntensity;
            float _IsShining;
            float _EffectTime;
            float _Threshold; // 追加
            
            float4 _MainTex_ST;
            float4 _MainTex2_ST;
            float4 _DisolveTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; 
                return o;
            }
            
            // facing: VFACEは、カメラから見てメッシュが裏面か（-1）表面か（1）を示す
            float4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                float4 baseColor;
                
                // UVの変換 (テクスチャのタイリングとオフセットを考慮)
                float2 uv_main = TRANSFORM_TEX(i.uv, _MainTex);
                float2 uv_main2 = TRANSFORM_TEX(i.uv, _MainTex2);
                float2 uv_dissolve = TRANSFORM_TEX(i.uv, _DisolveTex);

                 fixed4 m = tex2D(_DisolveTex, uv_dissolve);
                 half g = m.r; // ディゾルブテクスチャの赤成分を使用

                 if (g < _Threshold)
                 {
                     discard; // ピクセルを破棄（描画しない）
                 }
                // === 裏表のテクスチャ切り替えとディゾルブ ===
                if (facing > 0) // 表面 (Front)
                {
                    // 表面の基本色
                    baseColor = tex2D(_MainTex, uv_main) * _Color;
                    baseColor.a = _Color.a;

                    // === キラッと光る演出 (表面のみに適用) ===
                    if (_IsShining > 0) 
                    {
                        // 1本目
                        float diagonalUV1 = i.uv.x + i.uv.y;
                        float shinePos1 = _ShinePosition + (_EffectTime * _ShineSpeed);
                        float shineGradient1 = abs(diagonalUV1 - shinePos1);
                        float shineFactor1 = smoothstep(_ShineWidth, 0, shineGradient1);
                        
                        // 2本目
                        float diagonalUV2 = i.uv.x + i.uv.y;
                        float shinePos2 = (_ShinePosition - 0.7f) + (_EffectTime * _ShineSpeed);
                        float shineGradient2 = abs(diagonalUV2 - shinePos2);
                        float shineFactor2 = smoothstep(_ShineWidth, 0, shineGradient2);

                        float shineFactor = saturate(shineFactor1 + shineFactor2);

                        float4 shine = _ShineColor * _ShineIntensity * shineFactor;
                        baseColor.rgb += shine.rgb;
                    }
                }
                else // 裏面 (Back)
                {
                    // U座標 (X成分) を 1.0 - U で反転させることで鏡像を修正
                    float2 mirrored_uv = float2(1.0 - i.uv.x, i.uv.y); 
                    
                    // 裏面テクスチャのUV (反転UVを使用)
                    uv_main2 = TRANSFORM_TEX(mirrored_uv, _MainTex2);
                    // 裏面はディゾルブも光沢も適用しない
                    baseColor = tex2D(_MainTex2, uv_main2);
                    baseColor.a = 1.0;
                }
                
                return baseColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}