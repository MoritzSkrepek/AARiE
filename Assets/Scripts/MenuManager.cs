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
        // Sicherstellen, dass das Men�-Prefab zugewiesen ist
        if (menuPrefab != null)
        {
            // Men�-Prefab spawnen
            spawnedMenu = Instantiate(menuPrefab, transform.position, Quaternion.identity);

            // Gr��e des Men�s �ndern
            spawnedMenu.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            Debug.Log("Menu spawned.");
            Debug.Log(spawnedMenu);
        }
        else
        {
            Debug.LogError("Menu Prefab not assigned!");
        }
    }
}
