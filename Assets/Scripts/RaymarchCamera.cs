using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class RaymarchCamera : SceneViewFilter
{ 
    [SerializeField]
    private Shader _shader;

    public Material _raymarchMaterial
    {
        get
        {
            if (!_raymarchMat && _shader)
            {
                _raymarchMat = new Material(_shader);
                _raymarchMat.hideFlags = HideFlags.HideAndDontSave;
            }
            return _raymarchMat;
        }
    }
    private Material _raymarchMat;

    public Camera _camera
    {
        get
        {
            if (!_cam)
            {
                _cam = GetComponent<Camera>();
            }
            return _cam;
        }
    }
    private Camera _cam;

    [Header("Fractal Option")]
    public int _renderMode; //Atm there are 5 modes
    public int _mandelExp; //Atm there are 5 modes

    [Header("Precision")]
    public float _maxDistance = 200;
    public float _maxIteration = 200;
    public float _gradientPrecision = 1e-05f;
    public float _precision = 1e-05f;
    public int _Iterations = 6;

    [Header("Lighting")]
    public Transform _directionalLight;
    public Color _backgroundColor;
    public Color _mainColor;
    public Color _LightCol;
    public float _LightIntensity;

    [Header("Shadow")]
    public bool _enableShadows;
    public float _ShadowIntensity;
    public float _ShadowPenumbra;
    public Vector2 _ShadowDistance;

    [Header("Ambient Occlusion")]
    public bool _enableAmbientOcclusion;
    public float _AmbientOcclusionStepsize;
    public float _AmbientOcclusionIntensity;
    public int _AmbientOcclusionIterations;

    [Header("Glow")]
    public bool _enableGlow;
    public float _glowIntensity;
    public float _glowExp;
    public Color _glowColor;
    public float _realGlowPower;
    public bool _enableRealGlow;
    public float _realGlowIntensity;
    public Color _realGlowColor;

    [Header("ETC Params")]
    public bool _enableMod = false;
    public Vector3 _modInterval = new Vector3(2, 2, 2);
    public Vector3 _modOffsetPos = Vector3.one;
    public Vector3 _iterationOffsetRot = Vector3.zero;
    public float _smoothingRadius;
    public float _width;
    public float _scalingPerIteration;
    public FlexibleColorPicker pickedFractalColor;
    public FlexibleColorPicker pickedGlowColor;
    public FlexibleColorPicker pickedLightingColor;

    [Header("Transform Params ( Unused )")]
    public Vector4 _sphere1;
    public Vector4 _box1;
    public Vector4 _sphere2;

    [Header("Unused Params")]
    public float _roundBox1;
    public float _boxSphereSmooth;
    public float _SphereIntersectSmooth;


    [HideInInspector]
    public Matrix4x4 _iterationTransform;
    public float _iterationOffsetPos;

    private void Update()
    {
        ChangeIterations();
        ToggleModulo();
        ToggleShadows();
        ChangeSmoothingRadius();
        ChangeScalingPerIteration();
        CycleFractals();
        SetColor();
    }

    void SetColor()
    {
        _mainColor = pickedFractalColor.color;
        _LightCol = pickedLightingColor.color;
        _glowColor = pickedGlowColor.color;
        _realGlowColor = pickedGlowColor.color;
     }
    public void SetMandelPower(Slider slider)
    {
        _mandelExp = 2* (int)slider.value;
    }
    public void SetFractal(Slider slider)
    {
        _renderMode = (int)slider.value;
    }
    public void SetModIntervall(Slider slider)
    {
        _modInterval = Vector3.one *slider.value;
    }
    public void SetScalingPerIteration(Slider slider)
    {
        _scalingPerIteration = slider.value;
    }
    public void SetSmoothingRadius(Slider slider)
    {
        _smoothingRadius = slider.value;
    }
    public void ToogleMod(Toggle toggle)
    {
        _enableMod = toggle.isOn;
    }
    public void SetRealGlowExp(Slider slider)
    {
        _realGlowPower = slider.value;
    }
    public void SetRealGlowIntensity(Slider slider)
    {
        _realGlowIntensity = slider.value;
    }
    public void ToogleRealGlow(Toggle toggle)
    {
        _enableRealGlow = toggle.isOn;
    }
    public void SetgGlowExp(Slider slider)
    {
        _glowExp = slider.value;
    }
    public void SetGlowIntensity(Slider slider)
    {
        _glowIntensity = slider.value;
    }
    public void ToogleGlow(Toggle toggle)
    {
        _enableGlow = toggle.isOn;
    }
    public void SetAmbientOcclusionIterations(Slider slider)
    {
        _AmbientOcclusionIterations = (int)slider.value;
    }
    public void SetAmbientOcclusionStepsize(Slider slider)
    {
        _AmbientOcclusionStepsize = slider.value;
    }
    public void SetAmbientOcclusionIntensity(Slider slider)
    {
        _AmbientOcclusionIntensity = slider.value;
    }
    public void ToogleAO(Toggle toggle)
    {
        _enableAmbientOcclusion = toggle.isOn;
    }
    public void ToggleShadows(Toggle toggle)
    {
        _enableShadows = toggle.isOn;
    }
    public void SetShadowIntensity(Slider slider)
    {
        _ShadowIntensity = slider.value;
    }
    public void SetLightIntensity(Slider slider)
    {
        _LightIntensity = slider.value;
    }
    public void SetMaxDistance(Slider slider)
    {
        _maxDistance = slider.value;
    }
    public void SetIterations(Slider slider)
    {
        _Iterations = (int)slider.value;
    }
    public void SetMaxIteration(Slider slider)
    {
        _maxIteration = (int)slider.value;
    }
    public void CycleFractals()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            _renderMode += 1;
            if (_renderMode > 4)
            {
                _renderMode = 0;
            }
        }
    }
    public void ChangeScalingPerIteration()
    {
        if (Input.GetKey(KeyCode.Z))
        {
            _scalingPerIteration += Time.deltaTime * 0.5f;
        }
        if (Input.GetKey(KeyCode.H))
        {
            _scalingPerIteration -= Time.deltaTime * 0.5f;
        }
        if (Input.GetKey(KeyCode.J))
        {
            _scalingPerIteration = 1;
        }
    }
    public void ChangeSmoothingRadius()
    {
        if (Input.GetKey(KeyCode.I))
        {
            _smoothingRadius += Time.deltaTime * 0.5f;
        }
        if (Input.GetKey(KeyCode.K))
        {
            _smoothingRadius -= Time.deltaTime * 0.5f;
        }
        if (Input.GetKey(KeyCode.U))
        {
            _smoothingRadius = 0;
        }
    }
    public void ChangeIterations()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            _Iterations += 1;
        }
        if (Input.GetKeyDown(KeyCode.L) && _Iterations > 0)
        {
            _Iterations -= 1;
        }
    }
    public void ToggleModulo()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            _enableMod = !_enableMod;
        }
    }
    public void ToggleShadows()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            _enableShadows = !_enableShadows;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!_raymarchMaterial)
        {
            Graphics.Blit(source, destination);
            return;
        }

        //testing
        //_modInterval.x = (Mathf.Sin(Time.time) + 1) + 2;
        //_modInteral.y = (Mathf.Sin(Time.time) + 1) + 2;
        //_modInteral.z = (Mathf.Sin(Time.time) + 1) + 2;
        //testing

        _iterationTransform = Matrix4x4.TRS(
        Vector3.zero,
        Quaternion.Euler(_iterationOffsetRot),
        Vector3.one);

        //Fractal Option_mandelExp
        _raymarchMaterial.SetInt("_renderMode", _renderMode);
        _raymarchMaterial.SetInt("_mandelExp", _mandelExp);


        //Precision Params
        _raymarchMaterial.SetFloat("_maxDistance", _maxDistance);
        _raymarchMaterial.SetFloat("_maxIteration", _maxIteration);
        _raymarchMaterial.SetInt("_Iterations", _Iterations);
        _raymarchMaterial.SetFloat("_gradientPrecision", _gradientPrecision);
        _raymarchMaterial.SetFloat("_precision", _precision);

        //Lighting Params
        _raymarchMaterial.SetColor("_backgroundColor", _backgroundColor);
        _raymarchMaterial.SetColor("_mainColor", _mainColor);
        _raymarchMaterial.SetFloat("_LightIntensity", _LightIntensity);
        _raymarchMaterial.SetColor("_LightCol", _LightCol);
        _raymarchMaterial.SetVector("_LightDir", _directionalLight ? _directionalLight.forward : Vector3.down);

        //Shadow Params
        _raymarchMaterial.SetInt("_enableShadows", _enableShadows ? 1 : 0);
        _raymarchMaterial.SetVector("_ShadowDistance", _ShadowDistance);
        _raymarchMaterial.SetFloat("_ShadowPenumbra", _ShadowPenumbra);
        _raymarchMaterial.SetFloat("_ShadowIntensity", _ShadowIntensity);

        //Ambient Occlusion Params
        _raymarchMaterial.SetInt("_enableAmbientOcclusion", _enableAmbientOcclusion ? 1 : 0);
        _raymarchMaterial.SetFloat("_AmbientOcclusionStepsize", _AmbientOcclusionStepsize);
        _raymarchMaterial.SetFloat("_AmbientOcclusionIntensity", _AmbientOcclusionIntensity);
        _raymarchMaterial.SetInt("_AmbientOcclusionIterations", _AmbientOcclusionIterations);

        //Glow Params
        _raymarchMaterial.SetInt("_enableGlow", _enableGlow ? 1 : 0);
        _raymarchMaterial.SetColor("_glowColor", _glowColor);
        _raymarchMaterial.SetFloat("_glowExp", _glowExp);
        _raymarchMaterial.SetFloat("_glowIntensity", _glowIntensity);
        _raymarchMaterial.SetInt("_enableRealGlow", _enableRealGlow ? 1 : 0);
        _raymarchMaterial.SetFloat("_realGlowPower", _realGlowPower);
        _raymarchMaterial.SetColor("_realGlowColor", _realGlowColor);
        _raymarchMaterial.SetFloat("_realGlowIntensity", _realGlowIntensity);

        //Transform Params
        _raymarchMaterial.SetVector("_sphere1", _sphere1); // Sphere variables
        _raymarchMaterial.SetVector("_sphere2", _sphere2); // Sphere variables
        _raymarchMaterial.SetVector("_box1", _box1); // Box variables

        //ETC Params
        _raymarchMaterial.SetMatrix("_iterationTransform", _iterationTransform);
        _raymarchMaterial.SetInt("_enableMod", _enableMod ? 1 : 0);
        _raymarchMaterial.SetVector("_modInterval", _modInterval);
        _raymarchMaterial.SetVector("_modOffsetPos", _modOffsetPos);
        _raymarchMaterial.SetFloat("_scalingPerIteration", _scalingPerIteration);
        _raymarchMaterial.SetFloat("_width", _width);
        _raymarchMaterial.SetFloat("_smoothingRadius", _smoothingRadius);


        //Unused Params
        _raymarchMaterial.SetFloat("_SphereIntersectSmooth", _SphereIntersectSmooth);
        _raymarchMaterial.SetFloat("_boxSphereSmooth", _boxSphereSmooth);
        _raymarchMaterial.SetFloat("_roundBox1", _roundBox1);

        //DONT TOUCH THESE
        _raymarchMaterial.SetMatrix("_CamFrustum", CamFrustum(_camera));
        _raymarchMaterial.SetMatrix("_CamToWorld", _camera.cameraToWorldMatrix);

        RenderTexture.active = destination;
        _raymarchMaterial.SetTexture("_MainTex", source);
        GL.PushMatrix();
        GL.LoadOrtho();
        _raymarchMaterial.SetPass(0);
        GL.Begin(GL.QUADS);

        //BL
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f);
        //BR
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);
        //TR
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);
        //TL
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.End();
        GL.PopMatrix();
    }

    private Matrix4x4 CamFrustum(Camera cam)
    {
        Matrix4x4 frustum = Matrix4x4.identity;
        float fov = Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad);

        Vector3 goUp = Vector3.up * fov;
        Vector3 goRight = Vector3.right * fov * cam.aspect;

        Vector3 TL = (-Vector3.forward - goRight + goUp);
        Vector3 TR = (-Vector3.forward + goRight + goUp);
        Vector3 BR = (-Vector3.forward + goRight - goUp);
        Vector3 BL = (-Vector3.forward - goRight - goUp);

        frustum.SetRow(0, TL);
        frustum.SetRow(1, TR);
        frustum.SetRow(2, BR);
        frustum.SetRow(3, BL);

        return frustum; 
    }

}
