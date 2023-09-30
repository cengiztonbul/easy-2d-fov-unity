Shader "Unlit/FOVPostRender"
{
    Properties
    {
        [HideInInspector] _MainTex ("_MainTex", 2D) = "white" {}
        [HideInInspector] _FOVTex ("Texture", 2D) = "white" {}
        [HideInInspector] _HideableTex ("Texture", 2D) = "white" {}
        [Header(Blur Settings)]
        _Directions ("Direction", float) = 16
        _Quality ("Quality", float) = 32
        _Size ("Size", float) = 0.01
        [Header(Invisible Appearance)]
        _Saturation ("Saturation", Range(0, 1)) = 0.5
        _OutOfFieldOverlay ("Overlay", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            sampler2D _FOVTex;
            float4 _FOVTex_ST;

            sampler2D _HideableTex;
            float4 _HideableTex_ST;
            
            float _Quality = 4.0;
            float _Directions = 32.0;
            float _Size = 0.01;
            float _Saturation;
            float4 _OutOfFieldOverlay;

            fixed4 blur(sampler2D tex, v2f i)
            {
                float Pi = 6.28318530718; // Pi*2
                
                float4 Color = tex2D(tex, i.uv);
                
                for( float d=0.0; d<Pi; d+=Pi/_Directions)
                {
                    for(float it=1.0/_Quality; it<=1.0; it+=1.0/_Quality)
                    {
                        Color += tex2D(tex, i.uv+float2(cos(d),sin(d))*_Size*it);		
                    }
                }

                Color /= _Quality * _Directions - 15.0;
                return Color;
            }

            fixed4 saturation(float4 color, float saturation)
            {
                float4 grayscale = color.r + color.g + color.b;
                grayscale /= 3;
                return lerp(grayscale, color, saturation);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 viewColor = tex2D(_HideableTex, i.uv);
                float4 blurredFov = blur(_FOVTex, i);
                float4 blurredMain = blur(_MainTex, i);
                float4 mainColor = tex2D(_MainTex, i.uv);
                blurredFov = clamp(blurredFov, 0, 1);
                viewColor *= blurredFov;
                mainColor = (viewColor.a * viewColor) + (1 - viewColor.a) * mainColor;
                return (blurredFov.r) * mainColor +
                    (1 - blurredFov.r) * saturation(blurredMain, _Saturation) * _OutOfFieldOverlay;
            }

            ENDCG
        }
    }
}
