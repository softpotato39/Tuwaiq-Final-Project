using UnityEngine;
using UnityEngine.Rendering.Universal;


public class FootprintSpawner : MonoBehaviour
{

    
    public GameObject footprintDecalPrefab;   

    
    public Transform leftFootBone;            
    public Transform rightFootBone;           

    
    public float footprintLifetime = 2f;      
    public float decalOffsetY = 0.02f;        


    public void SpawnFootprint(string foot)
    {
        if (footprintDecalPrefab == null) return;

        
        Transform targetBone = foot == "left" ? leftFootBone : rightFootBone;
        if (targetBone == null)
        {
            Debug.LogWarning($"FootprintSpawner: عظمة القدم '{foot}' غير محددة في Inspector.");
            return;
        }

       
        Vector3 spawnPos = targetBone.position;
        spawnPos.y += decalOffsetY;

        
        Quaternion spawnRot = Quaternion.LookRotation(Vector3.down, transform.forward);

        
        GameObject decal = Instantiate(footprintDecalPrefab, spawnPos, spawnRot);

        
        Destroy(decal, footprintLifetime);
    }
}
