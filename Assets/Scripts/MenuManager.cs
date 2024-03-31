using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour{
    public GameObject menuPrefab;
    public Transform cameraPosition;
    private void Awake(){
        Vector3 newSpawnPoint = new Vector3(cameraPosition.position.x, cameraPosition.position.y, cameraPosition.position.z + 1);
        menuPrefab.transform.position = newSpawnPoint;
        menuPrefab.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        menuPrefab.SetActive(true);
    }
}
