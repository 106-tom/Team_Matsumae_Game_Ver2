Shader "Unlit/HorizontalFocusLine"
{
   Properties
    {
        [HideInInspector] _MainTex("-",2D)="white"{}
        [KeywordEnum(Rough, Sharp)] _Line("Line", Int) = 0
        _Color("Color", Color) = (0,0,0,1)
        _Alpha("Alpha", Range(0, 1)) = 1

        [Space(10)]
        _NoiseScale("NoiseScale", Range(10, 1000)) = 200 // 水平線の密度
        _PatternSeed("PatternSeed", Range(10, 100)) = 1 // 線のランダムパターンシード
        
        [Space(10)]
        _Edge1("Edge1", Range(0, 1)) = 0.5 // 線の濃淡の開始エッジ
        _Edge2("Edge2", Range(0, 1)) = 1   // 線の濃淡の終了エッジ

        [Space(10)]
        [Toggle] _IsAutoAnim("IsAutoAnim", float) = 0
        _AutoAnimSpeed("AutoAnimSpeed", Range(1, 20)) = 10 // 線の濃淡変化速度/流速

        [Space(10)]
        [Toggle] _IsPlayAnim("IsPlayAnim", float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _LINE_ROUGH _LINE_SHARP 

            #include "UnityCG.cginc" // _Timeを使用するためにインクルード

            struct appdata
            {
                fixed2 uv : TEXCOORD0;
                fixed4 vertex : POSITION;
            };

            struct v2f
            {
                fixed2 uv : TEXCOORD0;
                fixed4 pos : SV_POSITION; // SV_POSITIONを使用
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed _Alpha;

            fixed _NoiseScale;
            fixed _PatternSeed;

            fixed _Edge1;
            fixed _Edge2;

            fixed _IsAutoAnim;
            fixed _AutoAnimSpeed;

            fixed _IsPlayAnim;

            // radianの最大値、degreeで言うと360度のこと (今回は使用しないが定数として残す)
            // static const float PI2 = 3.14159 * 2; 

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            // あらゆる範囲の2次元変数を0~1の範囲の1次元変数に変換する
            fixed random(fixed2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
            }

            // 【変更箇所】Y座標に基づいたラインの濃淡値 (0～1) を返す
            // 水平線のパターン生成に使用
            fixed getHorizontalLine(fixed2 uv)
            {
                // Y座標を使用し、_NoiseScaleで細分化する
                // _PatternSeedはランダムのシード値として使用
                fixed lineSeed = floor(uv.y * _NoiseScale);
                
                // ランダム関数に (ラインのインデックス, シード値) を与えて、ラインごとに一意な値を生成
                fixed lineVal = random(fixed2(lineSeed, _PatternSeed)); 

                // アニメーション/シードのロジック: sin関数で値を変化させる
                lineVal = (_IsAutoAnim * sin(lineVal * _Time.w * _AutoAnimSpeed)) + 
                          ((1 - _IsAutoAnim) * sin(lineVal * _PatternSeed));
                
                // lineValを0～1の範囲に変換し、ラインの濃淡として使用する
                return abs(lineVal); 
            }

            // 【削除】getUvAngle、getRadialLine、getCenterCircle関数は水平線に不要なため削除

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                if(_IsPlayAnim > 0)
                {
                    // 以下の処理は全てalpha値を算出するための処理
                    fixed lineVal = getHorizontalLine(i.uv);
                    
                    // 水平線は中心からの距離でマスクしないため、lineValをそのまま結果とする
                    fixed resultLine = lineVal; 

                    // smoothstepで濃い所はより濃く、薄い所はより薄くする（コントラスト調整）
                    // _Edge1と_Edge2の間で0から1に変化させることで、線の太さを調整
                    fixed smoothAlpha = smoothstep(_Edge1, _Edge2, resultLine);

                    col = _Color;
                    col.a = smoothAlpha * _Alpha;
                }
                else
                {
                    col.a = 0.0f;
                }
                return col;
            }
            ENDCG
        }
    }
}