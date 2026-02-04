Shader "Hidden/ScreamDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Scream Intensity", Range(0, 1)) = 0
        _NoiseTex ("Noise Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            Name "ScreamPass"
            
            HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            ENDHLSL

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float _Intensity;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                float2 noise = tex2D(_NoiseTex, input.uv + _Time.y).rg;
                float2 offset = (noise - 0.5) * _Intensity * 0.05;
                
                float4 col = tex2D(_MainTex, input.uv + offset);
                
                // Shift colors slightly towards blood red at high intensity
                col.r += _Intensity * 0.2;
                col.g -= _Intensity * 0.1;
                col.b -= _Intensity * 0.1;
                
                return col;
            }
            ENDHLSL
        }
    }
}
