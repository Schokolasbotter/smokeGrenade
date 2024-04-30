Shader "Unlit/Combiner"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SmokeTex("Smoke Texture", 2D) = "white" {}
        _VoronoiTex("Voronoi Texture", 2D) = "white" {}
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _SmokeTex;
            sampler2D _VoronoiTex;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 colSrc = tex2D(_MainTex, i.uv);
                fixed4 colSmoke = tex2D(_SmokeTex, i.uv);
                float colNoise = tex2D(_VoronoiTex, i.uv).r;

                colNoise = saturate(colNoise + .1);


            //    float n1 = noise(i.uv); // Corrected to use a vector if 'i.uv' is a float2 type
             //   fixed4 noiseContribution = fixed4(n1, n1, n1, 1);

                fixed4 finalColor = colSmoke.a * (colSmoke*(colNoise*0.4)) + (1 - colSmoke.a) * colSrc;

                return finalColor;
            }
            ENDCG
        }
    }
}
