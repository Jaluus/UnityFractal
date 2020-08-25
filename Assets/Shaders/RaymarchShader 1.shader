Shader "Test/RaymarchShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "DistanceFunctions.cginc"

            sampler2D _MainTex;
            uniform sampler2D _CameraDepthTexture;
            uniform float4x4 _CamFrustum, _CamToWorld;
            uniform int _enableMod;
            uniform float _gradientPrecision;
            uniform float _precision;
            uniform float _maxDistance;
            uniform float _maxIteration;
            uniform float _roundBox1;
            uniform float4 _sphere1;
            uniform float4 _box1;
            uniform float3 _modInterval;
            uniform float3 _LightDir;
            uniform fixed4 _mainColor;
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ray : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                half index = v.vertex.z;
                v.vertex.z = 0;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                o.ray = _CamFrustum[(int)index].xyz;

                o.ray /= abs(o.ray.z);

                o.ray = mul(_CamToWorld, o.ray);

                return o;
            }

            float BoxSphere(float3 p)
            {
                float Sphere1 = sdSphere(p - _sphere1.xyz, _sphere1.w);
                float Box1 = sdRoundBox(p - _box1.xyz, _box1.www, _roundBox1);
                return opS(Sphere1, Box1);
            }

            float distanceField(float3 p)
            {
                if (_enableMod == 1) {
                    float modX = pMod1(p.x, _modInterval.x);
                    float modY = pMod1(p.y, _modInterval.y);
                    float modZ = pMod1(p.z, _modInterval.z);
                }

                float ground = sdPlane2(p, float4(0, 1, 0, 0));
                
                float BoxSphere1 = BoxSphere(p - float3(0,1,0));

                return opU(ground,BoxSphere1);
            }

            float3 getNormal(float3 p)
            {
                const float2 offset = float2(_gradientPrecision, 0.0); //change to change gradient
                float3 n = float3(
                    distanceField(p + offset.xyy) - distanceField(p - offset.xyy),
                    distanceField(p + offset.yxy) - distanceField(p - offset.yxy),
                    distanceField(p + offset.yyx) - distanceField(p - offset.yyx));
                return normalize(n);
            }

            fixed4 raymarching(float3 ro,float3 rd, float depth)
            {
                fixed4 result = fixed4(_mainColor.rgb, 0); //per defualt return the main color 0 for opaque
                float t = 0; // Distance travllt
                
                for (int i = 0; i < _maxIteration; i++)
                {
                    if (t > _maxDistance || t >= depth)
                    {
                        // Environemt
                        result = fixed4(rd, 0); //Changed from result = fixed4(rd, 0);
                        break;
                    }

                    float3 p = ro + rd * t;
                    //check for hit
                    float d = distanceField(p);

                    if (d < _precision) //We have hit something!
                    {
                        //shading!
                        float3 n = getNormal(p);
                        float light = dot(-_LightDir, n);

                        result = fixed4(_mainColor.rgb * light  , 1);
                        break;
                    }
                    t += d;
                }

                return result;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
                depth *= length(i.ray);
                fixed3 col = tex2D(_MainTex, i.uv);
                float3 rayDirection = normalize(i.ray.xyz);
                float3 rayOrigin = _WorldSpaceCameraPos;
                fixed4 result = raymarching(rayOrigin, rayDirection, depth);
                return fixed4(col * (1.0 - result.w) + result.xyz * result.w ,1.0);
            }
            ENDCG
        }
    }
}
