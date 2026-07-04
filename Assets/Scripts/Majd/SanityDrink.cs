using UnityEngine;
using InteractionSystem;

public class SanityDrink : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject promptIcon;
    [SerializeField] private AudioSource drinkSound;
    [SerializeField] private float sanityAmount = 100f;
    [SerializeField] private float respawnTime = 5f;

    private Collider[] colliders;
    private MeshRenderer[] renderers;

    private void Awake()
    {
        colliders = GetComponentsInChildren<Collider>();
        renderers = GetComponentsInChildren<MeshRenderer>();
    }

    public void ShowPrompt()
    {
        if (promptIcon != null)
            promptIcon.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptIcon != null)
            promptIcon.SetActive(false);
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        return true;
    }

    public void Interact(PlayerInteractor interactor)
    {
        SanitySystem sanity = FindFirstObjectByType<SanitySystem>();

        if (sanity != null)
        {
            sanity.AddSanity(sanityAmount);
        }

        if (promptIcon != null)
            promptIcon.SetActive(false);

        if (drinkSound != null)
            drinkSound.Play();

        // إخفاء المشروب
        foreach (Collider c in colliders)
            c.enabled = false;

        foreach (MeshRenderer r in renderers)
            r.enabled = false;

        Invoke(nameof(RespawnDrink), respawnTime);
    }

    private void RespawnDrink()
    {
        foreach (Collider c in colliders)
            c.enabled = true;

        foreach (MeshRenderer r in renderers)
            r.enabled = true;
    }
}