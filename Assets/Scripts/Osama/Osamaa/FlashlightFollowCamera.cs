using UnityEngine;


public class FlashlightFollowCamera : MonoBehaviour
{
    [Header("ÇáßÇãíÑÇ ÇáãÑÌÚíÉ")]
    [Tooltip("ÇÓÍÈ ÚáíåÇ Main Camera (Çááí ÊÊÍßã ÈíåÇ Cinemachine)")]
    public Transform cameraToFollow;

    [Header("ÇáãæÖÚ")]
    [Tooltip("åá íÊÈÚ ãæŞÚ ÇáßÇãíÑÇ ÈÇáßÇãá¿ áæ false íÍÇİÙ Úáì ãæŞÚå ÇáÃÕáí æíÊÈÚ ÇáÏæÑÇä İŞØ")]
    public bool followPosition = true;

    [Tooltip("ÅÒÇÍÉ ãÍáíÉ Úä ãæŞÚ ÇáßÇãíÑÇ (ÈÇáäÓÈÉ áãÍÇæÑ ÇáßÇãíÑÇ äİÓåÇ)")]
    public Vector3 positionOffset = Vector3.zero;

    private void LateUpdate()
    {
        if (cameraToFollow == null)
        {
            return;
        }

        // äÇÎĞ äİÓ ÏæÑÇä ÇáßÇãíÑÇ ÊãÇãÇğ
        transform.rotation = cameraToFollow.rotation;

        if (followPosition)
        {
            transform.position = cameraToFollow.position
                + cameraToFollow.right * positionOffset.x
                + cameraToFollow.up * positionOffset.y
                + cameraToFollow.forward * positionOffset.z;
        }
    }
}