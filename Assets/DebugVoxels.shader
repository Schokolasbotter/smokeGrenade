Shader "Unlit/DebugVoxels"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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


            float3 _BoundsExtent;
            uint3 _VoxelResolution;
            float _VoxelSize;
            int _MaxFillSteps, _DebugVoxels;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 hashCol : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float hash(uint n) {
                // integer hash copied from https://www.shadertoy.com/view/flX3R2
                n = (n << 13U) ^ n;
                n = n * (n * n * 15731U + 0x789221U) + 0x1376312589U;
                return float(n & uint(0x7fffffffU)) / float(0x7fffffff);
            }

            v2f vert (appdata v, uint instanceID : SV_INSTANCEID)
            {
                v2f i;

                //calculates the xyz transforms based on the instance ID
                uint x = instanceID % (_VoxelResolution.x);
                uint y = (instanceID / _VoxelResolution.x) % _VoxelResolution.y;
                uint z = instanceID / (_VoxelResolution.x * _VoxelResolution.y);
                //calculates the new positions
                i.vertex = UnityObjectToClipPos((v.vertex + float3(x, y, z)) * _VoxelSize + (_VoxelSize * 0.5f) - _BoundsExtent);
                //calculates normal
                i.normal = UnityObjectToWorldNormal(v.normal);
                i.hashCol = float3(hash(instanceID), hash(instanceID * 2), hash(instanceID * 3));
                return i;
            }

            

            fixed4 frag(v2f i) : SV_Target
            {
                // Generate a random color based on instance ID
                fixed3 randomColor = fixed3(hash(i.vertex.x), hash(i.vertex.y), hash(i.vertex.z));

                // Combine the random color with the normal as alpha
                fixed4 col = fixed4(i.hashCol, 1.0);
                return col;
            }
            ENDCG
        }
    }
}
