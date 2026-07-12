using UnityEngine;
using System.Collections;

public class ScanWave : MonoBehaviour
{
    [SerializeField] private float expandSpeed = 10f;
    [SerializeField] private float maxRadius = 15f;
    [SerializeField] private Renderer waveRenderer;

    private Material _mat;

    private void Awake()
    {
        // instance the material so we don't affect any shared assets :)
        _mat = waveRenderer.material;
        transform.localScale = Vector3.zero;
    }

    private void Start() => StartCoroutine(ExpandRoutine());

    private IEnumerator ExpandRoutine()
    {
        float currentRadius = 0f;

        while (currentRadius < maxRadius)
        {
            // instead of doing animation, this code does the scaling ! :)

            currentRadius += expandSpeed * Time.deltaTime;
            float t = Mathf.Clamp01(currentRadius / maxRadius);

            // scale the sphere outward (localScale is diameter so x2)
            transform.localScale = Vector3.one * (currentRadius * 2f);

            // fade alpha from full to zero as it expands
            Color c = _mat.color;
            c.a = Mathf.Lerp(0.6f, 0f, t);
            _mat.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }
}
