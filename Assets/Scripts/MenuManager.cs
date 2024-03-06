using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPrefab;
    public GameObject spawnedMenu;

    private void Start()
    {
        SpawnMenu();
    }

    public void SpawnMenu()
    {
        // Sicherstellen, dass das Menü-Prefab zugewiesen ist
        if (menuPrefab != null)
        {
            // Menü-Prefab spawnen
            spawnedMenu.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            spawnedMenu = Instantiate(menuPrefab, transform.position, Quaternion.identity);

            Debug.Log("Menu spawned.");
            Debug.Log(spawnedMenu);
        }
        else
        {
            Debug.LogError("Menu Prefab not assigned!");
        }
    }
}
