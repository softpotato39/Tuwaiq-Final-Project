using UnityEngine;

// =========================================================
//  PurpleFlashlight
//  Controls the purple light itself: color / intensity /
//  range / spot angle, and turning it on/off.
//
//  It does NOT decide WHEN to toggle from gameplay.
//  Toggling is requested by PurpleFlashlightTool.Use()
//  (Hanof's tool system, the "Use" button), or by any
//  other script that calls ToggleFlashlight() / SetFlashlight().
//
//  Where it lives:
//  On the object that has the Light (Type = Spot) inside the
//  flashlight TOOL prefab. That prefab is what ToolController
//  spawns into the player's hand when the tool is equipped --
//  you do NOT pick it up by hand.
//
//  Suggested prefab layout:
//    PurpleFlashlight (root)   <- PurpleFlashlightTool.cs
//      |- Mesh    (visual)
//      |- Light   (Spot) + PurpleFlashlight.cs   <- THIS script
//      |- Sound   (AudioSource)
// =========================================================

[RequireComponent(typeof(Light))]
public class PurpleFlashlight : MonoBehaviour
{
    [Header("Light Settings")]
    [Tooltip("Flashlight color")]
    public Color flashlightColor = new Color(0.55f, 0.2f, 0.85f); // purple

    [Tooltip("Light intensity")]
    public float intensity = 3.5f;

    [Tooltip("Light range")]
    public float range = 12f;

    [Tooltip("Spot cone angle")]
    public float spotAngle = 45f;

    [Tooltip("Is the flashlight ON when the game starts?")]
    public bool startsOn = false;

    [Header("Direct Toggle")]
    [Tooltip("Toggle the flashlight directly with a mouse click. Turn OFF if the tool system's Use button already handles the same click, so it doesn't double-toggle.")]
    public bool allowDirectMouseToggle = true;

    [Tooltip("Mouse button used to toggle: 0 = Left, 1 = Right, 2 = Middle.")]
    public int toggleMouseButton = 0;

    private Light _light;
    public bool IsOn { get; private set; }

    private void Awake()
    {
        _light = GetComponent<Light>();
        _light.type = LightType.Spot; // forced to Spot at runtime (edit mode may still show Point)
        ApplySettings();
    }

    private void Start()
    {
        SetFlashlight(startsOn);
    }

    private void Update()
    {
        // Left mouse click toggles the flashlight while it's held in the player's hand.
        // If the tool system's "Use" button is ALSO on left-click, turn allowDirectMouseToggle
        // OFF (or use a different button) so the flashlight doesn't toggle twice = stays off.
        if (allowDirectMouseToggle && Input.GetMouseButtonDown(toggleMouseButton))
            ToggleFlashlight();
    }

    private void ApplySettings()
    {
        _light.color = flashlightColor;
        _light.intensity = intensity;
        _light.range = range;
        _light.spotAngle = spotAngle;
    }

    // Called by PurpleFlashlightTool.Use() (the tool system's "Use" button).
    public void ToggleFlashlight()
    {
        SetFlashlight(!IsOn);
    }

    public void SetFlashlight(bool turnOn)
    {
        IsOn = turnOn;
        _light.enabled = turnOn;
    }

    // Lets other scripts (e.g. RevealOnLight) read the light cone.
    public Light GetLight() => _light;
}
