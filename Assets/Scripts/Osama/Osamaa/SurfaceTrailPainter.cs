using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SurfaceTrailPainter : MonoBehaviour
{
    [Header("≈⁄œ«œ«  «··ÊÕ…")]
    public int textureSize = 512;
    public Color trailColor = new Color(0.55f, 0.2f, 0.85f, 1f);
    [Range(0.001f, 0.3f)] public float stampRadius = 0.04f;
    [Range(0.9f, 0.999f)] public float decayRate = 0.992f;

    [Header("«·≈”‰«œ (Ì⁄»¯Ï  ·ﬁ«∆Ì)")]
    public Renderer overlayRenderer; // «·”ÿÕ «·≈÷«›Ì «·‘›«› «··Ì Ì⁄—÷ «·√À—

    private RenderTexture _bufferA;
    private RenderTexture _bufferB;
    private RenderTexture _current;
    private Material _stampMat;

    private Vector2 _pendingUV;
    private bool _hasPendingStamp;

    private void Awake()
    {
        _bufferA = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        _bufferB = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        _bufferA.Create();
        _bufferB.Create();

        Shader stampShader = Shader.Find("Hidden/TrailStamp");
        _stampMat = new Material(stampShader);

        _current = _bufferA;
        ClearRT(_bufferA);
        ClearRT(_bufferB);

        if (overlayRenderer != null)
        {
            overlayRenderer.material.SetTexture("_TrailTex", _current);
        }
    }

    private void ClearRT(RenderTexture rt)
    {
        RenderTexture prevActive = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = prevActive;
    }

    private void Update()
    {
        RenderTexture target = (_current == _bufferA) ? _bufferB : _bufferA;

        _stampMat.SetTexture("_MainTex", _current);
        _stampMat.SetFloat("_DecayRate", decayRate);
        _stampMat.SetColor("_StampColor", trailColor);
        _stampMat.SetFloat("_StampRadius", stampRadius);

        if (_hasPendingStamp)
        {
            _stampMat.SetVector("_StampUV", new Vector4(_pendingUV.x, _pendingUV.y, 0, 0));
            _stampMat.SetFloat("_StampActive", 1f);
            _hasPendingStamp = false;
        }
        else
        {
            _stampMat.SetFloat("_StampActive", 0f);
        }

        Graphics.Blit(_current, target, _stampMat);
        _current = target;

        if (overlayRenderer != null)
        {
            overlayRenderer.material.SetTexture("_TrailTex", _current);
        }
    }

    public void PaintAt(Vector2 uv)
    {
        _pendingUV = uv;
        _hasPendingStamp = true;
    }

    private void OnDestroy()
    {
        if (_bufferA != null) _bufferA.Release();
        if (_bufferB != null) _bufferB.Release();
    }
}