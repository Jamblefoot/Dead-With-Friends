Shader "Stufco/WaterSurface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _DeepColor ("Deep Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        //_Amplitude("Wave Size", Range(0,1)) = 0.4
        //_Frequency("Wave Frequency", Range(1,8)) = 2
        _AnimationSpeed("Animation Speed", Range(0,5)) = 1

        _Tess ("Tessellation", Range(1, 32)) = 4
        _TessDistMin("Tessellation Min Distance", Float) = 10
        _TessDistMax("Tessellation Max Distance", Float) = 100
        _DispTex ("Displacement Texture", 2D) = "gray" {}
        _Displacement ("Displacement", Range(0, 1.0)) = 0.3

        _NormalMap ("Normal Map", 2D) = "bump" {}

        _Cube ("Cubemap", CUBE) = "" {}

        _Bump1Scale ("Bump 1 Scale", Range(0, 1)) = 0.1
        _Bump2Scale ("Bump 2 Scale", Range(0, 1)) = 1
        _RippleSpeed ("Ripple Speed", Vector) = (0.01, 0.01, -0.02, -0.02)
        _RippleStrength ("Ripple Strength", Range(0, 1)) = 1
        _MaxRippleDistance ("Max Ripple Distance", Float) = 250
        _DepthMaxDistance ("Depth Max Distance", Float) = 50
        _FoamHeight ("Foam Height", Float) = 0.5
        _FoamDot ("Foam Min Dot", Range(0, 1)) = 0.1

        //_NormalShift("Normal Shift", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }//"RenderType"="Transparent" }
        LOD 200
        Cull Off

        Stencil
        {
            Ref 1
            //Comp notequal
            //Pass keep
            Comp NotEqual
            Pass Replace
        }

        GrabPass { "_BackgroundTexture" }

        //Pass{
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard alpha vertex:vert tessellate:tessDistance //fullforwardshadows addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        //#pragma target 3.0
        //USE MODEL 4.6 for tessellation
        #pragma target 4.6
        #include "UnityCG.cginc"
        #include "Tessellation.cginc"

        struct appdata {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float2 texcoord : TEXCOORD0;

            fixed4 color : COLOR;
            //fixed4 color2 : COLOR2;
            //fixed4 texcoord1 : TEXCOORD1;
            float4 texcoord2 : TEXCOORD2;
            //float4 texcoord3 : TEXCOORD3;
        };

        float _Tess;
        float _TessDistMin;
        float _TessDistMax;

        float4 tessDistance (appdata v0, appdata v1, appdata v2){
            float minDist = _TessDistMin;//10.0;
            float maxDist = _TessDistMax;//100.0;
            return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
        }

        sampler2D _DispTex;
        float _Displacement;

        float _AnimationSpeed;
        float _DepthMaxDistance;

        /*void disp (inout appdata v)
        {
            float d = tex2Dlod(_DispTex, float4(v.texcoord.xy + _Time.y * _AnimationSpeed, 0, 0)).r * _Displacement;
            v.vertex.xyz += v.normal * d;
        }*/

        sampler2D _MainTex;
        samplerCUBE _Cube;

        //float _Amplitude;
        //float _Frequency;
        float _Bump1Scale;
        float _Bump2Scale;

        sampler2D _BackgroundTexture;

        sampler2D _CameraDepthTexture;

        struct Input
        {
            float2 uv_MainTex;
            //float3 newNormal; //Can't pass data with tessellation
            float3 worldRefl; INTERNAL_DATA
            float3 worldPos;
            float4 color : COLOR;
            //float4 color2 :COLOR2;
            //float4 texcoord1 : TEXCOORD1;//Grab Position
            float4 texcoord2 : TEXCOORD2;
            //float4 texcoord3 : TEXCOORD3;
            float4 screenPos;
            float3 viewDir;

            float facing : VFACE;
            
        };

        void vert(inout appdata data)//, out Input o)
        {
            data.texcoord2 = float4(normalize(mul(data.normal, unity_ObjectToWorld)).xyz, 1);

            float4 worldPos = mul(unity_ObjectToWorld, data.vertex);
            //float d = tex2Dlod(_DispTex, float4(data.texcoord.xy + _Time.y * _AnimationSpeed, 0, 0)).r * _Displacement;
            float d = tex2Dlod(_DispTex, float4(worldPos.xz * _Bump1Scale + _Time.y * _AnimationSpeed, 0, 0)).r * _Displacement;

            float d2 = tex2Dlod(_DispTex, float4(worldPos.xz * _Bump2Scale - _Time.y * _AnimationSpeed, 0, 0)).r * _Displacement;
            //data.vertex.xyz += data.normal * d;

            float4 modifiedPos = data.vertex;
            //modifiedPos.xyz -= data.normal * (_Displacement * 0.25);
            //modifiedPos.xyz += data.normal * (d + (d2 * 0.3));
            modifiedPos.xyz += data.normal * lerp(d,d2,0.3);

            //COMPUTE TANGENT AND BITANGENT SHIFT FOR NORMALS
            float3 posPlusTangent = data.vertex + data.tangent * 0.01;
            float4 worldPosPlusTangent = mul(unity_ObjectToWorld, posPlusTangent);
            float td = tex2Dlod(_DispTex, float4(worldPosPlusTangent.xz * _Bump1Scale + _Time.y * _AnimationSpeed, 0, 0)).r * _Displacement;
            float td2 = tex2Dlod(_DispTex, float4(worldPosPlusTangent.xz * _Bump2Scale - _Time.y * _AnimationSpeed, 0, 0)).r * _Displacement;
            posPlusTangent.xyz += data.normal * lerp(td, td2, 0.3);

            float3 bitangent = cross(data.normal, data.tangent);
            float3 posPlusBitangent = data.vertex + bitangent * 0.01;
            float4 worldPosPlusBitangent = mul(unity_ObjectToWorld, posPlusBitangent);
            float btd = tex2Dlod(_DispTex, float4(worldPosPlusBitangent.xz * _Bump1Scale + _Time.y * _AnimationSpeed, 0, 0)).r * _Displacement;
            float btd2 = tex2Dlod(_DispTex, float4(worldPosPlusBitangent.xz * _Bump2Scale - _Time.y * _AnimationSpeed, 0, 0)).r * _Displacement;
            posPlusBitangent += data.normal * lerp(btd, btd2, 0.3);

            /*modifiedPos.y += sin(data.vertex.x * _Frequency + _Time.y * _AnimationSpeed) * _Amplitude;

            float3 posPlusTangent = data.vertex + data.tangent * 0.01;
            posPlusTangent.y += sin(posPlusTangent.x * _Frequency + _Time.y * _AnimationSpeed) * _Amplitude;

            float3 bitangent = cross(data.normal, data.tangent);
            float3 posPlusBitangent = data.vertex + bitangent * 0.01;
            posPlusBitangent.y += sin(posPlusBitangent.x * _Frequency + _Time.y * _AnimationSpeed) * _Amplitude;*/

            float3 modifiedTangent = posPlusTangent - modifiedPos;
            float3 modifiedBitangent = posPlusBitangent - modifiedPos;
            float3 modifiedNormal = cross(modifiedTangent, modifiedBitangent);

            float3 move = modifiedPos - data.vertex;
            float delta = length(move) * sign(dot(move, data.normal));

            data.normal = normalize(modifiedNormal);
            data.vertex = modifiedPos;

            data.color = float4(normalize(mul(data.normal, unity_WorldToObject)).xyz, delta);
            //data.color = float4(data.normal.xyz, delta);


            //float4 pos = UnityObjectToClipPos(modifiedPos);
            //data.texcoord3 = ComputeGrabScreenPos(pos);//THIS IS THE GRABPASS TEXTURE POSITION

            //UNITY_INITIALIZE_OUTPUT(Input,o);//Can't pass data when tessellating
            //o.newNormal = data.normal;
        }

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _DeepColor;
        sampler2D _NormalMap;
        float4 _RippleSpeed;
        float _RippleStrength;
        float _MaxRippleDistance;
        float _FoamHeight;
        float _FoamDot;

        //float _NormalShift;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        //UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        //UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {

            // Albedo comes from a texture tinted by color
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            fixed4 c = _Color;// + tex2Dproj(_BackgroundTexture, IN.screenPos);// + tex2Dproj(_BackgroundTexture, UNITY_PROJ_COORD(IN.texcoord3));//
            
            //c = tex2Dproj(_BackgroundTexture, IN.grabPos);

            /*float2 grabTexcoord = IN.screenPos.xy / IN.screenPos.w;
            grabTexcoord.y = 1.0 - grabTexcoord.y;
            half4 background = tex2D(_GrabTexture, grabTexcoord);*/

            //float surfDot = dot(normalize(IN.viewDir), o.Normal);

            float3 initNormal = o.Normal;

            //APPLY NORMAL MAP RIPPLES AND CALCULATE FINAL NORMAL
            float3 norm1 = UnpackNormal(tex2D(_NormalMap, IN.worldPos.xz * _Bump1Scale + _Time.y * _RippleSpeed.xy));
            float3 norm2 = UnpackNormal(tex2D(_NormalMap, IN.worldPos.xz * _Bump2Scale + _Time.y * _RippleSpeed.zw));
            //float rippleStrength = max(0, _RippleStrength - min(_RippleStrength, IN.screenPos.w / _MaxRippleDistance));
            float3 resultNormal = normalize(lerp(IN.color.xzy, lerp(norm1, norm2, 0.5) * _RippleStrength, 0.5));
            //o.Normal = normalize(lerp(IN.color.xzy, lerp(norm1, norm2, 0.5) * _RippleStrength, 0.5));
            //o.Normal = surfDot > 0 ? resultNormal : normalize(lerp(resultNormal, -IN.viewDir, 1 + surfDot));

            float surfDot = dot(normalize(IN.viewDir), resultNormal);//initNormal);//IN.texcoord2.xyz);//

            //resultNormal = surfDot > 0 ? resultNormal : normalize(lerp(resultNormal, -IN.viewDir, _NormalShift));//saturate(IN.screenPos.w / _DepthMaxDistance)));

            //GET GRAB PASS COLOR
            float4 grabPassUV = IN.screenPos;
            float2 distortion = resultNormal.xy;//o.Normal.xy;
            grabPassUV.xy += distortion * grabPassUV.z;
            fixed4 grabCol = tex2Dproj(_BackgroundTexture, grabPassUV);//IN.screenPos);

            //GET DISTANCE OF PIXEL FROM CAMERA AND COMPARE TO DEPTH TEXTURE
            //float dist = length(IN.worldPos.xyz);//get distance to position on surface in world space
            float distFromCam = distance(IN.worldPos, _WorldSpaceCameraPos);
            float existingDepth01 = tex2Dproj(_CameraDepthTexture, grabPassUV).r;//UNITY_PROJ_COORD(IN.screenPos)).r;
            float existingDepthLinear = LinearEyeDepth(existingDepth01);
            existingDepthLinear += existingDepthLinear < _ProjectionParams.z ? 0 : _DepthMaxDistance;
            float depthDifference = existingDepthLinear - IN.screenPos.w;//distFromCam;//
            float waterDepthDifference01 = saturate(depthDifference / _DepthMaxDistance);

            

            //c += grabCol * abs(dot(normalize(IN.viewDir), o.Normal));
            
            c = c * (1 - abs(surfDot)) + grabCol * abs(surfDot);

            float4 surfaceColor = lerp(c, lerp(_DeepColor, _Color, abs(IN.color.w) / max(1, (IN.screenPos.w / (_DepthMaxDistance * 0.5)))), waterDepthDifference01);
            //float4 surfaceColor = c;
            float4 underwaterColor = lerp(c, lerp(_Color, _DeepColor, abs(IN.worldPos.y - _WorldSpaceCameraPos.y) / (_DepthMaxDistance * 30)), min(1, distFromCam / _DepthMaxDistance));

            c = IN.facing < 0 ? underwaterColor : surfaceColor;

            //c = surfDot < 0 ? lerp(c, _DeepColor, min(1, IN.screenPos.w / _DepthMaxDistance) * (1 + surfDot)/*1 + surfDot*/)
            //         : lerp(c, lerp(_DeepColor, _Color, IN.color.w), waterDepthDifference01);

            //c = surfDot > 0 ? c : IN.screenPos.w > _DepthMaxDistance ? _DeepColor : c;
            //c = surfDot < 0 ? lerp(c, _DeepColor, min(1, IN.screenPos.w / (_DepthMaxDistance * 2))) : c;
            //FOAM ATTEMPT
            //c = IN.color.w < _FoamHeight ? c : abs(dot(o.Normal, initNormal)) < _FoamDot ? c : float4(1,1,1,1);

            o.Albedo = c.rgb;

            o.Normal = IN.facing > 0 ? resultNormal : lerp(resultNormal, IN.viewDir, min(1, distFromCam / _DepthMaxDistance));//surfDot < 0 ? float3(0,1,0) : resultNormal;
            
            o.Metallic = _Metallic;
            o.Smoothness = IN.facing < 0 ? min(1, distFromCam / _DepthMaxDistance) : _Glossiness;//surfDot < 0 ? 0 : _Glossiness;// surfDot > 0 ? _Glossiness : IN.screenPos.w > _DepthMaxDistance ? 1 : _Glossiness;
            o.Alpha = c.a;

            //o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex - _Time.y * 0.005)); 
            //o.Normal = IN.newNormal + UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex - _Time.y * 0.005)); 
                        //+ UnpackNormal(tex2D(_NormalMap, worldPos.xz + _Time.y * 0.01));

            
            //o.Normal = UnpackNormal(tex2D(_NormalMap, IN.worldPos.xz * _Bump1Scale + _Time.y * _RippleSpeed));

            /*float d = UnpackNormal(tex2D(_NormalMap, IN.worldPos.xz * _Bump1Scale + _Time.y * _RippleSpeed));

            float d2 = UnpackNormal(tex2D(_NormalMap, IN.worldPos.xz * _Bump2Scale - _Time.y * _RippleSpeed * 0.5));
            //o.Normal = normalize(IN.color.xzy * (d + d2) * 0.5);
            o.Normal = IN.color.xzy * (d + d2) * 0.5;*/
            //o.Normal = d;

            half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflect(IN.viewDir.xzy, o.Normal));
            half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
            half reflectionFactor = dot(IN.viewDir, o.Normal);//bump);
            o.Emission.rgb = IN.facing < 0 ? float3(0,0,0) : skyColor;// * reflectionFactor;//lerp(o.Emission.rgb, skyColor, reflectionFactor);

            //o.Emission = texCUBE (unity_SpecCube0, WorldReflectionVector(IN, o.Normal)).rgb;
        }
        ENDCG
    }
    //}
    //FallBack "Diffuse"
}


