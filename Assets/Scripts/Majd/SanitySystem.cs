using UnityEngine;
using UnityEngine.UI;

public class SanitySystem : MonoBehaviour
{
    public Image brainImage;

    public float sanity = 100f;
    public float drainSpeed = 100f / 180f;

    void Update()
    {
        if (RoomManager.Instance != null && RoomManager.Instance.IsInRoom())
        {
            sanity -= drainSpeed * Time.deltaTime;
            sanity = Mathf.Clamp(sanity, 0, 100);
        }

        brainImage.fillAmount = sanity / 100f;
    }
    public void AddSanity(float amount)
{
    sanity += amount;
    sanity = Mathf.Clamp(sanity, 0f, 100f);
}
}