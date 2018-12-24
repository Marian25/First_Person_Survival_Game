using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class CameraBloodEffect : MonoBehaviour {

    [SerializeField] private Texture2D bloodTexture = null;
    [SerializeField] private Texture2D bloodNormalMap = null;

    [SerializeField] private float _bloodAmount = 0;
    [SerializeField] private float _minBloodAmount = 0;
    [SerializeField] private float distortion = 1f;
    [SerializeField] private bool _autoFade = false;
    [SerializeField] private float _fadeSpeed = 0.05f;

    [SerializeField] private Shader shader = null;
    private Material material = null;

    public float bloodAmount { get { return _bloodAmount; } set { _bloodAmount = value; } }
    public float minBloodAmount { get { return _minBloodAmount; } set { _minBloodAmount = value; } }
    public float fadeSpeed { get { return _fadeSpeed; } set { _fadeSpeed = value; } }
    public bool autoFade { get { return _autoFade; } set { _autoFade = value; } }

    private void Update()
    {
        if (autoFade)
        {
            bloodAmount -= fadeSpeed * Time.deltaTime;
            bloodAmount = Mathf.Max(bloodAmount, minBloodAmount);
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (shader == null) return;

        if (material == null)
        {
            material = new Material(shader);
        }

        if (material == null) return;

        if (bloodTexture != null)
            material.SetTexture("_BloodTex", bloodTexture);

        if (bloodNormalMap != null)
            material.SetTexture("_BloodBump", bloodNormalMap);

        material.SetFloat("_Distortion", distortion);
        material.SetFloat("_BloodAmount", _bloodAmount);

        Graphics.Blit(source, destination, material);
    }


}
