using UnityEngine;

public class SpawnPrefab : MonoBehaviour
{
    public GameObject menu; // Drag and drop your prefab in the Unity Editor
    public Transform mainCam;

    void Start()
    {
        SpawnPrefabMenu();
    }

    void SpawnPrefabMenu()
    {
        Vector3 cameraPosition = mainCam.position;
        Vector3 spawnPosition = new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z + 1f);
        GameObject instantiatedObject = Instantiate(menu, spawnPosition, Quaternion.identity);
        instantiatedObject.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
    }
}