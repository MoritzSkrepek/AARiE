using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefab : MonoBehaviour
{
    public GameObject menu; // Drag and drop your prefab in the Unity Editor

    void Start()
    {
        SpawnPrefabMenu();
    }

    void SpawnPrefabMenu()
    {
        // Holen Sie die aktuelle Kameraposition
        Vector3 cameraPosition = Camera.main.transform.position;

        // Versetzen Sie das Prefab-Menü um 1 auf der Z-Achse
        Vector3 spawnPosition = new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z + 1f);

        // Spawnt das Prefab-Menü an der neuen Position
        Instantiate(menu, spawnPosition, Quaternion.identity);
        menu.SetActive(true);
    }
}
