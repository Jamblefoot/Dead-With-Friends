Shader "Stufco/UnderwaterFog"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _DeepColor ("Deep Color", Color) = (0,0,0.1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _WaterHeight ("Water Height", Float) = 0
        _WorldCenter ("World Center", Vector) = (0,0,0,5)
        _Density ("Density", Float) = 1
        _DepthMax ("DepthMax", Float) = 50
    }
    SubShader
    {
        Tags { "Queue"="Transparent+1" }//"RenderType"="Opaque" }
        LOD 200

        //Stencil
        //{
        //    Ref 1
        //    Comp NotEqual
        //    Pass Replace
        //}

        //GrabPass { "_BackgroundTexture" }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        sampler2D _CameraDepthTexture;
        //sampler2D _BackgroundTexture;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float4 screenPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _DeepColor;
        float _WaterHeight;
        float4 _WorldCenter;
        float _Density;
        float _DepthMax;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float distFromCam = distance(IN.worldPos, _WorldSpaceCameraPos) / 2; //THE 2 is the local z offset of fog quad
            float existingDepth01 = tex2Dproj(_CameraDepthTexture, IN.screenPos).r;
            float existingDepthLinear = LinearEyeDepth(existingDepth01);
            float depthDifference = (existingDepthLinear - IN.screenPos.w) * distFromCam;// + distFromCam;
            float waterDepthDifference01 = min(1, pow(depthDifference / _DepthMax, _Density));

        //VVV  THIS IS WRONG, SHOULD BE CALCULATING VECTOR BETWEEN IN.worldPos AND THE WORLD POSITION OF IT'S PIXEL, NOT THE CENTER OF THE CAMERA ITSELF
            float3 worldDepthPos = IN.worldPos + normalize(IN.worldPos - _WorldSpaceCameraPos) * existingDepthLinear;
            worldDepthPos.y = min(worldDepthPos.y, _WaterHeight);

            fixed4 c = lerp(lerp(_Color, _DeepColor, saturate(abs(_WaterHeight - _WorldSpaceCameraPos.y) / (_DepthMax * 30))), _DeepColor, saturate((_WaterHeight - worldDepthPos.y) / (_DepthMax * 30)));

            c.a = waterDepthDifference01;

            o.Albedo = c.rgb;
            o.Alpha = IN.worldPos.y > _WaterHeight ? 0 : c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
