using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float interactDistance = 5f;
    public LayerMask interactLayer;

    [Header("Hands Holder Reference")]
    public Transform handsHolder;

    [Header("Item Placement Offset (Ê“‰Ì… „ﬂ«‰ «·ﬂ‘«›)")]
    public Vector3 itemLocalPosition = new Vector3(0.3f, 1.6f, 0.5f); // X=Ì„Ì‰° Y= Õ ° Z=ﬁœ«„ «·ﬂ«„Ì—«
    public Vector3 itemLocalRotation = new Vector3(0f, 0f, 0f); // “«ÊÌ… œÊ—«‰ «·ﬂ‘«›

    private GameObject currentHeldItem;
    private Transform mainCameraTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (mainCameraTransform == null) return;

        // «·Ìœ  ·Õﬁ «·ﬂ«„Ì—« «·√”«”Ì… »«·„·¯Ì
        if (handsHolder != null)
        {
            handsHolder.position = mainCameraTransform.position;
            handsHolder.rotation = mainCameraTransform.rotation;
        }

        // ≈ÿ·«ﬁ «·‹ Raycast
        Debug.DrawRay(mainCameraTransform.position, mainCameraTransform.forward * interactDistance, Color.cyan);

        RaycastHit hit;
        if (Physics.Raycast(mainCameraTransform.position, mainCameraTransform.forward, out hit, interactDistance, interactLayer))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                PickupItem(hit.collider.gameObject);
            }
        }

        if (Input.GetKeyDown(KeyCode.G) && currentHeldItem != null)
        {
            DropItem();
        }
    }

    private void PickupItem(GameObject itemToPickup)
    {
        if (currentHeldItem != null) return;

        currentHeldItem = itemToPickup;

        if (currentHeldItem.GetComponent<Rigidbody>())
        {
            currentHeldItem.GetComponent<Rigidbody>().isKinematic = true;
        }

        Collider[] allColliders = currentHeldItem.GetComponentsInChildren<Collider>();
        foreach (Collider col in allColliders)
        {
            col.enabled = false;
        }

        currentHeldItem.transform.SetParent(handsHolder);

        // «·ÕÌ‰ «·ﬂÊœ €’» ⁄‰Â »Ì”„⁄ ﬂ·«„ «·√—ﬁ«„ «··Ì »‰⁄ÿÌÂ ≈Ì«Â« ›Ì «·‹ Inspector!
        currentHeldItem.transform.localPosition = itemLocalPosition;
        currentHeldItem.transform.localRotation = Quaternion.Euler(itemLocalRotation);
    }

    private void DropItem()
    {
        currentHeldItem.transform.SetParent(null);

        if (currentHeldItem.GetComponent<Rigidbody>())
        {
            Rigidbody rb = currentHeldItem.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.AddForce(mainCameraTransform.forward * 8f, ForceMode.Impulse);
        }

        Collider[] allColliders = currentHeldItem.GetComponentsInChildren<Collider>();
        foreach (Collider col in allColliders)
        {
            col.enabled = true;
        }

        currentHeldItem = null;
    }
}