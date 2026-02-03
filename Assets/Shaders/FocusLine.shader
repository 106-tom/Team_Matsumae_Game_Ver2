Shader "Unlit/FocusLine"
{
    Properties
    {
        [HideInInspector] _MainTex("-",2D)="white"{}
        [KeywordEnum(Rough, Sharp)] _Line("Line", Int) = 0
        _Color("Color", Color) = (0,0,0,1)
        _Alpha("Alpha", Range(0, 1)) = 1

        [Space(10)]
        _NoiseScale("NoiseScale", Range(100, 1000)) = 500
        _PatternSeed("PatternSeed", Range(10, 100)) = 1  
        
        [Space(10)]
        _Edge1("Edge1", Range(0, 1)) = 0.5
        _Edge2("Edge2", Range(0, 1)) = 1

        [Space(10)]
        [Toggle] _IsAutoAnim("IsAutoAnim", float) = 0
        _AutoAnimSpeed("AutoAnimSpeed", Range(1, 20)) = 10

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

            struct appdata
            {
                fixed2 uv : TEXCOORD0;
                fixed4 vertex : POSITION;
            };

            struct v2f
            {
                fixed2 uv : TEXCOORD0;
                fixed4 vertex : POSITION;
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

            //radianの最大値、degreeで言うと360度のこと
            static const float PI2 = 3.14159 * 2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
        
            //あらゆる範囲の2次元変数を0~1の範囲の1次元変数に変換する
            fixed random(fixed2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
            }

            //引数に渡された対象座標の角度を0~1の範囲に圧縮して返す。
            //反時計周りに徐々に値が大きくなる。
            fixed2 getUvAngle(fixed2 uv)
            {
                //uv座標を、画面中央を原点として(0,0)>(1,1)から(-1,-1)>(1,1)の範囲に修正する
                fixed2 fixUv = uv * 2 - 1;
                //修正したuv座標の角度をradian値で取得
                fixed angle = atan2(fixUv.y, fixUv.x);
                //0~PI*2の値を0~1の範囲に圧縮
                return angle / PI2;
            }

            //半時計周りにグラデーションする0~1の値を、バラバラの、かつある程度はまとまった集中線に変換する
            fixed getRadialLine(fixed angle)
            {
                //_NoiseScaleを大きくするほど、集中線が細かくなる。
                //これは_NoiseScaleで値を大きくすると、floorで切り捨てられる要素が少なくなり
                //angleの細かい値まで漏れなく後のrandom関数に反映されるため。
                //random関数は、どんな巨大な値も0~1の範囲に収めるため、与える値の大きさは問題ではなく
                //どこからどこまでのangle値を同値として扱うかが重要になる
                fixed2 lineSeed = floor(fixed2(angle, angle) * _NoiseScale);
                fixed lineVal = random(lineSeed); ;

                //sin関数はあらゆる値を規則的な-1~1の範囲の波形に変換する。
                //ここでは集中線の出現パターンを変える効果に使っている。
                //_IsAutoAnimがtrueの時は、Unity再生時に自動でアニメーションを実行し、
                //_IsAutoAnimがfalseの時は、_PatternSeedを操作することで集中線の見た目が変化する
                lineVal = (_IsAutoAnim * sin(lineVal * _Time.w * _AutoAnimSpeed)) + 
                            ((1 - _IsAutoAnim) * sin(lineVal * _PatternSeed));
                return lineVal;
            }

            //引数のuv座標を参照し、中心から外側に向けて0から1に遷移していく値を返す
            fixed getCenterCircle(fixed2 uv)
            {
                //uv座標を、画面中央を原点として(0,0)>(1,1)から(-1,-1)>(1,1)の範囲に修正する
                fixed2 fixUv = uv * 2 - 1;
#ifdef _LINE_ROUGH
                //ランダム関数を使ってちょっと線をザラザラにする
                fixed2 i = random(uv) * 0.5 + 0.7;
                return length(fixUv) * i;
#else
                return length(fixUv);
#endif
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                if(_IsPlayAnim > 0)
                {
                    //以下の処理は全てalpha値を算出するための処理
                    fixed angle = getUvAngle(i.uv);
                    fixed lineVal = getRadialLine(angle);
                    fixed circle = getCenterCircle(i.uv);
                    //中心まで届く集中線と、中心に近いほど0に近づく円を掛け算して、
                    //中心付近は空になる集中線を作る
                    fixed resultLine = saturate(lineVal * circle);
                    //smoothstepで濃い所はより濃く、薄い所はより薄くする
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