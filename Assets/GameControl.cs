using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour
{
    public static GameControl instance;

    bool inMenu = false;
    
    public Canvas menuCanvas;
    public Canvas gameCanvas;

    public float waterLevel = 0;
    public Transform waterPlane;
    // Start is called before the first frame update
    void Start()
    {
        if(GameControl.instance == null)
        {
            instance = this;
        }
        else DestroyImmediate(gameObject);

        Cursor.lockState = CursorLockMode.Locked;

        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(false);

        if(waterPlane != null)
            waterPlane.position = new Vector3(waterPlane.position.x, waterLevel, waterPlane.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            inMenu = !inMenu;
            if(inMenu) 
            {
                Cursor.lockState = CursorLockMode.None;
                if(menuCanvas != null)
                    menuCanvas.gameObject.SetActive(true);
            }
            else 
            {
                Cursor.lockState = CursorLockMode.Locked;
                if (menuCanvas != null)
                    menuCanvas.gameObject.SetActive(false);
            }
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ExitMenu()
    {
        inMenu = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(false);
    }
}
