using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string sceneToLoad;

    public void ChangeScene()
    {
        // Prüfen, ob die zu ladende Szene level_1 oder Level_2 ist
        if (sceneToLoad == "Level_1" || sceneToLoad == "Level_2")
        {
            GameObject menuManager = GameObject.FindGameObjectWithTag("MenuManager");
            MenuManager menuScript = menuManager.GetComponent<MenuManager>();
            Destroy(menuScript.spawnedMenu);
            Debug.Log("menugone");
        }
        // Wenn die zu ladende Szene "main_menu" ist, dann Menü spawnen
        else if (sceneToLoad == "main_menu")
        {
            // Menü-Prefab spawnen, falls Menu_Spawn vorhanden
            GameObject menuSpawn = GameObject.FindGameObjectWithTag("menu(Clone)");
            if (menuSpawn != null)
            {
                MenuManager menuSpawnScript = menuSpawn.GetComponent<MenuManager>();
                if (menuSpawnScript != null)
                {
                    menuSpawnScript.SpawnMenu();
                }
            }
            Debug.Log("scene to load is main_menu");
        }

        // Szene laden
        if (SceneManager.GetActiveScene().name != sceneToLoad)
        {
            Debug.Log(sceneToLoad);
            Debug.Log("szene laden nach menu");
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        }
    }
}
