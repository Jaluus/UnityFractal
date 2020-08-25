﻿Shader "Test/RaymarchShader"
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

            //Precision Params
            uniform float _gradientPrecision;
            uniform float _precision;
            uniform int _Iterations;
            uniform float _maxDistance;
            uniform float _maxIteration;

            //Lighting Params
            uniform fixed4 _mainColor;
            uniform float3 _LightDir, _LightCol;
            uniform float _LightIntensity, _ShadowIntensity, _ShadowPenumbra;
            uniform float3 _ShadowDistance;

            //Ambient Occlusion
            uniform float _AmbientOcclusionStepsize;
            uniform float _AmbientOcclusionIntensity;
            uniform int _AmbientOcclusionIterations;

            //Transform params
            uniform float4 _sphere1;
            uniform float4 _sphere2;
            uniform float4 _box1;

            // ETC Params
            uniform int _enableMod;
            uniform float3 _modInterval;

            //Smoothing Params ( Unused )
            uniform float _roundBox1, _boxSphereSmooth, _SphereIntersectSmooth;

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
                float combine1 = opSS(Sphere1, Box1, _boxSphereSmooth);
                float Sphere2 = sdSphere(p - _sphere2.xyz, _sphere2.w);
                float combine2 = opIS(Sphere2, combine1, _SphereIntersectSmooth);
                return combine2;
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

            float hardShadow(float3 ro, float3 rd, float mint, float maxt)
            {
                for (float t = mint; t < maxt;)
                {
                    float h = distanceField(ro + rd * t);
                    if (h < 0.001) //Maybe change it to make it more crisp
                    {
                        return 0.0;
                    }
                    t += h;
                }
                return 1.0;
            }

            float softShadow(float3 ro, float3 rd, float mint, float maxt, float k)
            {
                float result = 1.0;
                for (float t = mint; t < maxt;)
                {
                    float h = distanceField(ro + rd * t);
                    if (h < 0.001) //Maybe change it to make it more crisp
                    {
                        return 0.0;
                    }
                    result = min(result, k * h / t);
                    t += h;
                }
                return result;
            }

            float3 AmbientOcclusion(float3 p, float3 n)
            {
                float step = _AmbientOcclusionStepsize;
                float ao = 0.0;
                float dist;
                for (int i = 1; i <= _AmbientOcclusionIterations; i++)
                {
                    dist = step * i;
                    ao += max(0.0, (dist - distanceField(p + n * dist)) / dist);
                }
                return (1.0 - ao * _AmbientOcclusionIntensity);
            }

            float3 Shading(float3 p, float3 n)
            {
                float3 result;
                //Diffuse Color
                float3 color = _mainColor.rgb;
                //Directional Light
                float3 light = (_LightCol * dot(-_LightDir, n) * 0.5 + 0.5) * _LightIntensity;
                //Shadows
                float shadow = softShadow(p, -_LightDir, _ShadowDistance.x, _ShadowDistance.y, _ShadowPenumbra) * 0.5 + 0.5; // to change put in soft shadows , _ShadowPenumbra
                shadow = max(0.0, pow(shadow, _ShadowIntensity));
                //Ambient Occlusion
                float ao = AmbientOcclusion(p, n);

                result = shadow * light * color * ao;
                return result;
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
                        float3 s = Shading(p, n);
                        //float light = dot(-_LightDir, n); Old way to make shadows

                        result = fixed4(_mainColor.rgb * s, 1);
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
