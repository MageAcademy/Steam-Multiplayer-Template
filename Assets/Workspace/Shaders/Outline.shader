Shader "Outline/PostprocessOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
    }

    SubShader
    {
        CGINCLUDE
        #include "UnityCG.cginc"

        struct v2f
        {
            float2 uv[9]: TEXCOORD0;
            float4 vertex: SV_POSITION;
        };

        sampler2D _MainTex;
        sampler2D _Outline;
        sampler2D _Mask;
        float4 _OutlineColor;
        float _OutlineWidth;
        float _OutlineHardness;
        float4 _MainTex_TexelSize;

        v2f vert(appdata_img v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            float2 uv = v.texcoord;
            float width = _OutlineWidth;
            o.uv[0] = float2(uv.x - _MainTex_TexelSize.x * width, uv.y + _MainTex_TexelSize.y * width);
            o.uv[1] = float2(uv.x, uv.y + _MainTex_TexelSize.y * width);
            o.uv[2] = float2(uv.x + _MainTex_TexelSize.x * width, uv.y + _MainTex_TexelSize.y * width);
            o.uv[3] = float2(uv.x - _MainTex_TexelSize.x * width, uv.y);
            o.uv[4] = uv;
            o.uv[5] = float2(uv.x + _MainTex_TexelSize.x * width, uv.y);
            o.uv[6] = float2(uv.x - _MainTex_TexelSize.x * width, uv.y - _MainTex_TexelSize.y * width);
            o.uv[7] = float2(uv.x, uv.y - _MainTex_TexelSize.y * width);
            o.uv[8] = float2(uv.x + _MainTex_TexelSize.x * width, uv.y - _MainTex_TexelSize.y * width);
            return o;
        }

        fixed4 frag1(v2f i): SV_Target
        {
            float4 finalRender;
            float sum = 0;
            for (int it = 0; it < 9; it ++)
            {
                float4 mask = tex2D(_Mask, i.uv[it]);
                sum += step(0.001, mask.r);
            }
            finalRender = saturate(sum / 9);
            finalRender.a = 1;
            return finalRender;
        }

        fixed4 frag2(v2f i): SV_Target
        {
            float sum = 0;
            float sum2 = 0;
            for (int it = 0; it < 9; it ++)
            {
                float4 mask = tex2D(_Mask, i.uv[it]);
                sum += step(0.001, mask.r);
                float4 outl = tex2D(_Outline, i.uv[it]);
                sum2 += outl.r;
            }
            float4 col = tex2D(_MainTex, i.uv[4]);
            float value = saturate(sum / _OutlineHardness) * (1 - saturate(pow(abs(sum2) / 9, 10)));
            float4 finalRender = lerp(col, _OutlineColor, value);
            finalRender.a = 1;
            return finalRender;
        }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag1
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag2
            ENDCG
        }
    }
}