Shader "UI/ScrollingDotsUnscaled"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Texture_Speed ("Texture Speed", Vector) = (0.15, 0.1, 0, 0)
        _Texture_Tiling ("Texture Tiling", Vector) = (2, 2, 0, 0)
        _UnscaledTime ("Unscaled Time", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float4 _Texture_Speed;
            float4 _Texture_Tiling;
            float _UnscaledTime;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv * _Texture_Tiling.xy + _Texture_Speed.xy * _UnscaledTime;
                fixed4 col = tex2D(_MainTex, uv) * _Color * i.color;
                return col;
            }
            ENDCG
        }
    }

    Fallback "UI/Default"
}
