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
        Debug.Log("MURUK 1");
        // Holen Sie die aktuelle Kameraposition
        Vector3 cameraPosition = Camera.main.transform.position;
        Debug.Log("MURUK 2");
        // Versetzen Sie das Prefab-Menü um 1 auf der Z-Achse
        Vector3 spawnPosition = new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z + 1f);
        Debug.Log("MURUK 3");
        // Spawnt das Prefab-Menü an der neuen Position
        Instantiate(menu, spawnPosition, Quaternion.identity);
        Debug.Log("MURUK 4");
        menu.SetActive(true);
        Debug.Log("MURUK 5");
    }
}
