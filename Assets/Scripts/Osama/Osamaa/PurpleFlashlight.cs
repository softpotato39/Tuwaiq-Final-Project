using UnityEngine;

/// <summary>
/// ÓßÑÈÊ ÇáİáÇÔ áÇíÊ ÇáÈäİÓÌí.
/// íÖÇİ Úáì GameObject İíå Light Component ãä äæÚ Spot Light.
///
/// ØÑíŞÉ ÇáÅÚÏÇÏ:
/// 1. ÃäÔÆ GameObject İÇÖí ÊÍÊ ÇáßÇãíÑÇ Ãæ ÇááÇÚÈ (Empty Child).
/// 2. Öíİ Úáíå Component ãä äæÚ "Light" æÇÎÊÇÑ Type = Spot.
/// 3. Öíİ Úáíå åĞÇ ÇáÓßÑÈÊ (PurpleFlashlight.cs).
/// 4. æÌøå ÇáÜ Light ÈÍíË íØáÚ ááÃãÇã (äİÓ ÇÊÌÇå äÙÑ ÇááÇÚÈ).
/// </summary>
[RequireComponent(typeof(Light))]
public class PurpleFlashlight : MonoBehaviour
{
    [Header("ÇáÅÚÏÇÏÇÊ ÇáÃÓÇÓíÉ")]
    [Tooltip("áæä ÇáİáÇÔ áÇíÊ")]
    public Color flashlightColor = new Color(0.55f, 0.2f, 0.85f); // ÈäİÓÌí

    [Tooltip("ÔÏÉ ÇáÖæÁ")]
    public float intensity = 3.5f;

    [Tooltip("äØÇŞ æÕæá ÇáÖæÁ")]
    public float range = 12f;

    [Tooltip("ÒÇæíÉ ÇäÊÔÇÑ ÇáÖæÁ (Spot Angle)")]
    public float spotAngle = 45f;

    [Header("ÇáÊÍßã")]
    [Tooltip("ÇáÒÑ Çááí íæáøÚ/íØİøí ÇáİáÇÔ áÇíÊ")]
    public KeyCode toggleKey = KeyCode.F;

    [Tooltip("åá ÇáİáÇÔ áÇíÊ ãæáøÚ ãä ÇáÈÏÇíÉ¿")]
    public bool startsOn = false;

    private Light _light;
    public bool IsOn { get; private set; }

    private void Awake()
    {
        _light = GetComponent<Light>();
        _light.type = LightType.Spot;
        ApplySettings();
    }

    private void Start()
    {
        SetFlashlight(startsOn);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }
    }

    private void ApplySettings()
    {
        _light.color = flashlightColor;
        _light.intensity = intensity;
        _light.range = range;
        _light.spotAngle = spotAngle;
    }

    public void ToggleFlashlight()
    {
        SetFlashlight(!IsOn);
    }

    public void SetFlashlight(bool turnOn)
    {
        IsOn = turnOn;
        _light.enabled = turnOn;
    }

    // íÓãÍ áÓßÑÈÊÇÊ ËÇäíÉ (ãËá RevealOnLight) ÊÌíÈ ãÚáæãÇÊ ÇáãÎÑæØ
    public Light GetLight() => _light;
}
