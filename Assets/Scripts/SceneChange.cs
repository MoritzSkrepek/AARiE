using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour{
    public string sceneToLoad;
    public void ChangeScene(){
        if (SceneManager.GetActiveScene().name != sceneToLoad){
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        }
    }
}
