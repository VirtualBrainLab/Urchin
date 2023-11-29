Shader "Unlit/VolumeRayMarchShader"
{
    Properties
    {
        _MainTex ("Texture", 3D) = "white" {}
        _Alpha ("Alpha", Range(0.0,1.0)) = 0.02
        _StepSize ("Step Size", float) = 0.01
        _MLClip("MLClip", Vector) = (-1, 1, 0, 0)
        _APClip("APClip", Vector) = (-1, 1, 0, 0)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend One OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Maximum amount of raymarching samples
            #define MAX_STEP_COUNT 128

            // Allowed floating point inaccuracy
            #define EPSILON 0.00001f

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 objectVertex : TEXCOORD0;
                float3 vectorToSurface : TEXCOORD1;
            };

            sampler3D _MainTex;
            //float4 _MainTex_ST;
            float _Alpha;
            float _StepSize;
            float4 _MLClip;
            float4 _APClip;

            v2f vert (appdata v)
            {
                v2f o;

                // Vertex in object space this will be the starting point of raymarching
                o.objectVertex = v.vertex;

                // Calculate vector from camera to vertex in world space
                float3 worldVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vectorToSurface = worldVertex - _WorldSpaceCameraPos;

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 BlendUnder(float4 color, float4 newColor)
            {
                color.rgb += (1.0 - color.a) * newColor.a * newColor.rgb;
                color.a += (1.0 - color.a) * newColor.a;
                return color;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Start raymarching at the front surface of the object
                //float3 rayOrigin = i.objectVertex;

                // Check if the ray origin or any subsequent sample position is outside the clipping boundaries
                if ((i.objectVertex.y < _MLClip.x || i.objectVertex.y > _MLClip.y) &&
                    (i.objectVertex.x < _APClip.x || i.objectVertex.x > _APClip.y))
                {
                    // Set color to transparent and exit the loop
                    return float4(0, 0, 0, 0);
                }

                // Use vector from camera to object surface to get ray direction
                float3 rayDirection = mul(unity_WorldToObject, float4(normalize(i.vectorToSurface), 1));

                float4 color = float4(0, 0, 0, 0);
                float3 samplePosition = i.objectVertex;

                int sampleCount = 0;
                // Raymarch through object space
                for (int j = 0; j < MAX_STEP_COUNT; j++)
                {
                    // Accumulate color only within unit cube bounds
                    if(max(abs(samplePosition.x), max(abs(samplePosition.y), abs(samplePosition.z))) < 0.5f + EPSILON)
                    {
                        // Calculate gradients for texture sampling
                        float3 dx = ddx(samplePosition + float3(0.5f, 0.5f, 0.5f));
                        float3 dy = ddy(samplePosition + float3(0.5f, 0.5f, 0.5f));

                        float4 sampledColor = tex3Dgrad(_MainTex, samplePosition + float3(0.5f, 0.5f, 0.5f), dx, dy);
                        sampledColor.a *= _Alpha;
                        color = BlendUnder(color, sampledColor);
                        samplePosition += rayDirection * _StepSize;
                    }
                }

                return color;
            }
            ENDCG
        }
    }
}