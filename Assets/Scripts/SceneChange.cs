using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string sceneToLoad;

    // Referenz auf das SpawnPrefab-Skript
    public SpawnPrefab spawnPrefabScript;

    public void ChangeScene()
    {
        // Szene wechseln
        if (SceneManager.GetActiveScene().name != sceneToLoad)
        {
            // Sicherstellen, dass das Menü-Prefab gelöscht wird, bevor die Szene gewechselt wird
            if (spawnPrefabScript != null)
            {
                Debug.Log("Scenechanger menü destroy aufgerufen1");
                spawnPrefabScript.DestroyMenuPrefab();
                Debug.Log("Scenechanger menü destroy aufgerufen2");
            }

            Debug.Log("Scenechanger menü destroy aufgerufen3");
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        }
    }
}
