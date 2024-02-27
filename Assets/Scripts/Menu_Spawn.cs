using UnityEngine;

public class SpawnPrefab : MonoBehaviour
{
    public GameObject menu; // Drag and drop your prefab in the Unity Editor
    public Transform mainCam;

    private GameObject instantiatedMenu; // Referenz auf das instanziierte Menü-Prefab

    void Start()
    {
        // Beim Laden der Szene Menü-Prefab instanziieren
        SpawnPrefabMenu();
    }

    void OnDestroy()
    {
        // Beim Verlassen der Szene Menü-Prefab zerstören
        DestroyMenuPrefab();
    }

    void SpawnPrefabMenu()
    {
        if (menu == null || mainCam == null)
        {
            Debug.LogError("Menu prefab or Main Camera is not assigned!");
            return;
        }

        Vector3 cameraPosition = mainCam.position;
        Vector3 spawnPosition = new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z + 1f);
        instantiatedMenu = Instantiate(menu, spawnPosition, Quaternion.identity);
        instantiatedMenu.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
    }

    // Methode zum Zerstören des Menü-Prefabs
    void DestroyMenuPrefab()
    {
        if (instantiatedMenu != null)
        {
            Destroy(instantiatedMenu);
        }
    }
}
