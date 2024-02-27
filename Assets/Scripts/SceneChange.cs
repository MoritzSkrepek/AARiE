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
            // Sicherstellen, dass das Men�-Prefab gel�scht wird, bevor die Szene gewechselt wird
            if (spawnPrefabScript != null)
            {
                Debug.Log("Scenechanger men� destroy aufgerufen1");
                spawnPrefabScript.DestroyMenuPrefab();
                Debug.Log("Scenechanger men� destroy aufgerufen2");
            }

            Debug.Log("Scenechanger men� destroy aufgerufen3");
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        }
    }
}
