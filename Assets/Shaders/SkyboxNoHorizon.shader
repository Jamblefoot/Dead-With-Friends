Shader "Skybox/Procedural Skybox No Horizon" {

	Properties {

		_SunSize ("Sun Size", Range(0,1)) = 0.04
		_SunSizeConvergence("Sun Size Convergence", Range(1,10)) = 5
		_SkyTint ("Sky Tint", Color) = (.5, .5, .5, 1)
		_MidTint ("Mid Tint", Color) = (.5, 0, 0, 1)
		_LowTint ("Low Tint", Color) = (1, .5, 0, 1)
		_Exposure("Exposure", Range(0, 8)) = 1.3

	}

	SubShader {
    
		Tags {
		
			"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"
			
		}
    
		Cull Off ZWrite Off

		Pass {

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"

			uniform half _Exposure;        
			uniform half _SunSize;
			uniform half _SunSizeConvergence;
			uniform half3 _SkyTint;
			uniform half3 _MidTint;
			uniform half3 _LowTint;

			#if defined(UNITY_COLORSPACE_GAMMA)
				#define GAMMA 2
				#define COLOR_2_GAMMA(color) color
				#define COLOR_2_LINEAR(color) color*color
				#define LINEAR_2_OUTPUT(color) sqrt(color)
			#else
				#define GAMMA 2.2
				// HACK: to get gfx-tests in Gamma mode to agree until UNITY_ACTIVE_COLORSPACE_IS_GAMMA is working properly
				#define COLOR_2_GAMMA(color) ((unity_ColorSpaceDouble.r>2.0) ? pow(color,1.0/GAMMA) : color)
				#define COLOR_2_LINEAR(color) color
				#define LINEAR_2_LINEAR(color) color
			#endif
			
			#define MIE_G (-0.990)
			#define MIE_G2 0.9801
        
			half getRayleighPhase(half eyeCos2){
				return 0.75 + 0.75*eyeCos2;
			}

			half getRayleighPhase(half3 light, half3 ray){
				half eyeCos = dot(light, ray);
				return getRayleighPhase(eyeCos * eyeCos);
			}

			half getMiePhase(half eyeCos, half eyeCos2){
				half temp = 1.0 + MIE_G2 - 2.0 * MIE_G * eyeCos;
				temp = pow(temp, pow(_SunSize,0.65) * 10);
				temp = max(temp,1.0e-4); // prevent division by zero, esp. in half precision
				temp = 1.5 * ((1.0 - MIE_G2) / (2.0 + MIE_G2)) * (1.0 + eyeCos2) / temp;
				#if defined(UNITY_COLORSPACE_GAMMA) && SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
					temp = pow(temp, .454545);
				#endif
				return temp;
			}

			// Calculates the sun shape

			half calcSunAttenuation(half3 lightPos, half3 ray){
				half focusedEyeCos = pow(saturate(dot(lightPos, ray)), _SunSizeConvergence);
				return getMiePhase(-focusedEyeCos, focusedEyeCos * focusedEyeCos);
			}

			struct appdata_t {
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {

				float4  pos : SV_POSITION;
				half3 vertex : TEXCOORD0;
				half3 skyColor : TEXCOORD2;
				half3 sunColor : TEXCOORD3;
				UNITY_VERTEX_OUTPUT_STEREO

			};
					
			v2f vert(appdata_t v){
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.pos = UnityObjectToClipPos(v.vertex);

				float3 eyeRay = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));

				OUT.vertex = -v.vertex;
				//OUT.skyColor = _Exposure * (_SkyTint * getRayleighPhase(_WorldSpaceLightPos0.xyz, -eyeRay));//ORIGINAL LINE
				half rayleigh = getRayleighPhase(_WorldSpaceLightPos0.xyz, -eyeRay);

				half sd = dot(_WorldSpaceLightPos0.xyz, float3(0, -1, 0));

				half sundot = max(1-eyeRay.y, max(0, sd)) * max(0, dot(_WorldSpaceLightPos0.xyz, eyeRay));
				half sunset = (1 - abs(eyeRay.y)) * (1 - (sd > -0.1 ? abs(min(0, sd)) / 0.1 : 1));// * max(0, (dot(_WorldSpaceLightPos0.xyz, eyeRay) + 1) * 0.5);
				half4 mid = half4(lerp(_SkyTint.r, _MidTint.r, sunset), lerp(_SkyTint.g, _MidTint.g, sunset), lerp(_SkyTint.b, _MidTint.b, sunset), 1);
				//half4 eve = half4(lerp(_SkyTint.r, _LowTint.r, sundot), lerp(_SkyTint.g, _LowTint.g, sundot), lerp(_SkyTint.b, _LowTint.b, sundot), 1);
				half4 eve = half4(lerp(mid.r, _LowTint.r, sundot), lerp(mid.g, _LowTint.g, sundot), lerp(mid.b, _LowTint.b, sundot), 1);
				/*OUT.skyColor = _Exposure * 
								(half4(lerp(_SkyTint.r, eve.r, -_WorldSpaceLightPos0.y + 0.75),
								 	lerp(_SkyTint.g, eve.g, -_WorldSpaceLightPos0.y + 0.75),
								  	lerp(_SkyTint.b, eve.b, -_WorldSpaceLightPos0.y + 0.75),
								   	1) 
								* rayleigh);*/
				OUT.skyColor = _Exposure * 
								(half4(lerp(mid.r, eve.r, -_WorldSpaceLightPos0.y + 0.75),
								 	lerp(mid.g, eve.g, -_WorldSpaceLightPos0.y + 0.75),
								  	lerp(mid.b, eve.b, -_WorldSpaceLightPos0.y + 0.75),
								   	1) 
								* rayleigh);
       
				half lightColorIntensity = clamp(length(_LightColor0.xyz), 0.25, 1);

				OUT.sunColor = _LightColor0.xyz / lightColorIntensity;

				return OUT;
			}
			
			half4 frag(v2f IN) : SV_Target {
				half3 col = IN.skyColor;
				half3 ray = normalize(mul((float3x3)unity_ObjectToWorld, IN.vertex));
                   
				col += IN.sunColor * calcSunAttenuation(_WorldSpaceLightPos0.xyz, -ray);

				//Add colors for additional sun here...
				//col += 
        
				return half4(col,1.0);

			}

			ENDCG
		}
	}

}