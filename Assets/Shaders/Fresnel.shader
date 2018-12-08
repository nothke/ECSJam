// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Fresnel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
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

			#pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
				float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float3 worldPos : TEXCOORD2;
				float3 normal : TEXCOORD3;
				UNITY_VERTEX_OUTPUT_STEREO
            };

			//float4 _Color;

			UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color) // Make _Color an instanced property (i.e. an array)
			#define _Color_arr Props
			UNITY_INSTANCING_BUFFER_END(Props)

            sampler2D _MainTex;
            float4 _MainTex_ST;

			float _RimPower;
			float4 _RimColor;

            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = 1;
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.normal = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				col = 1;

				// View direction of camera
				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
				half rim = 1.0 - saturate(dot(normalize(viewDir), i.normal));
				half4 newCol = half4(col * _RimColor);
				newCol.a = col.a;
				newCol *= pow(rim, _RimPower);
				col = newCol;
				col = rim;
	
				return col;
            }
            ENDCG
        }
    }
}
