using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStarter : MonoBehaviour
{
    [SerializeField] Dropdown levelDropdown;

    void Start()
    {
        if(levelDropdown != null)
        {
            List<string> levels = new List<string>();
            for(int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string name = SceneUtility.GetScenePathByBuildIndex(i);
                name = name.Remove(0, name.LastIndexOf('/') + 1);
                name = name.Remove(name.LastIndexOf('.'));
                name = name.ToUpper();
                Debug.Log("Adding " + name + " to scenes dropdown");
                levels.Add(name);
            }

            levelDropdown.ClearOptions();
            levelDropdown.AddOptions(levels);
        }
    }

    public void StartGame()
    {
        if(levelDropdown != null)
            SceneManager.LoadScene(levelDropdown.value + 1);
        else SceneManager.LoadScene(1);
    }
}
