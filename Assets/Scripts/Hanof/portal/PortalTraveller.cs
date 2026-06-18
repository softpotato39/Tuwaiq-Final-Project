using System.Collections.Generic;
using UnityEngine;

public class PortalTraveller : MonoBehaviour {

    public GameObject graphicsObject;
    public GameObject graphicsClone { get; set; }
    public Vector3 previousOffsetFromPortal { get; set; }

    public Material[] originalMaterials { get; set; }
    public Material[] cloneMaterials { get; set; }

    //old teleport method, does not support unity's physics components >:(

    //public virtual void Teleport (Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot) 
    //{
    //    transform.position = pos;
    //    transform.rotation = rot;
    //}
    //-----------------------------------------------------------------------

    // this is the updated teleport method, supports unity's physics components now :))
    public virtual void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        // 1. Get references to components
        CharacterController cc = GetComponent<CharacterController>();
        PlayerMovement movement = GetComponent<PlayerMovement>();

        // 2. Temporarily disable CharacterController so Unity physics allows manual positioning
        if (cc != null) cc.enabled = false;

        // 3. Teleport the physical transform positions
        transform.position = pos;
        transform.rotation = rot;

        // 4. Sync our internal PlayerMovement momentum
        if (movement != null)
        {
            movement.SyncPortalTeleport(fromPortal, toPortal, rot);
        }

        // 5. Safely turn the CharacterController back on
        if (cc != null) cc.enabled = true;
    }

    // Called when first touches portal
    public virtual void EnterPortalThreshold () 
    {
        if (graphicsClone == null) {
            graphicsClone = Instantiate (graphicsObject);
            graphicsClone.transform.parent = graphicsObject.transform.parent;
            graphicsClone.transform.localScale = graphicsObject.transform.localScale;
            originalMaterials = GetMaterials (graphicsObject);
            cloneMaterials = GetMaterials (graphicsClone);
        } else {
            graphicsClone.SetActive (true);
        }
    }

    // Called once no longer touching portal (excluding when teleporting)
    public virtual void ExitPortalThreshold () 
    {
        graphicsClone.SetActive (false);
        // Disable slicing
        for (int i = 0; i < originalMaterials.Length; i++) 
        {
            originalMaterials[i].SetVector ("sliceNormal", Vector3.zero);
        }
    }

    public void SetSliceOffsetDst (float dst, bool clone) 
    {
        for (int i = 0; i < originalMaterials.Length; i++) 
        {
            if (clone) 
            {
                cloneMaterials[i].SetFloat ("sliceOffsetDst", dst);
            } 
            else 
            {
                originalMaterials[i].SetFloat ("sliceOffsetDst", dst);
            }

        }
    }

    Material[] GetMaterials (GameObject g) 
    {
        var renderers = g.GetComponentsInChildren<MeshRenderer> ();
        var matList = new List<Material> ();
        foreach (var renderer in renderers) 
        {
            foreach (var mat in renderer.materials) 
            {
                matList.Add (mat);
            }
        }
        return matList.ToArray ();
    }
}