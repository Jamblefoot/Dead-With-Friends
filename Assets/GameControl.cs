using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    public static GameControl instance;

    public float gameTime = 0f;
    public int livingCount;
    List<AICharacter> people = new List<AICharacter>();
    [SerializeField] Text timer;
    [SerializeField] Text counter;

    public bool inMenu = false;
    
    public Canvas menuCanvas;
    public Canvas settingsCanvas;
    public Canvas gameCanvas;

    public float waterLevel = 0;
    public Transform waterPlane;

    public FollowCam followCam;

    [Header("SETTINGS")]
    [SerializeField] Dropdown qualityDropdown;
    [SerializeField] Dropdown aaDropdown;
    //[SerializeField] Dropdown screenmodeDropdown;
    [SerializeField] Text screenmodeText;
    [SerializeField] Dropdown resolutionDropdown;
    [SerializeField] Dropdown vsyncDropdown;

    FullScreenMode lastScreenMode = FullScreenMode.ExclusiveFullScreen;
    //bool blockScreenmodeChange = false;

    [Header("AUDIO")]
    [SerializeField] AudioClip[] footsteps;
    [SerializeField] AudioClip[] deathSounds;

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
        if (settingsCanvas != null)
            settingsCanvas.gameObject.SetActive(false);

        if(waterPlane != null)
            waterPlane.position = new Vector3(waterPlane.position.x, waterLevel, waterPlane.position.z);

        followCam = FindObjectOfType<FollowCam>();

        List<string> resOptions = new List<string>();
        foreach(Resolution r in Screen.resolutions)
        {
            resOptions.Add(r.ToString());
        }
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resOptions);
        SetSettingsMenu();

        foreach(AICharacter person in FindObjectsOfType<AICharacter>())
        {
            people.Add(person);
        }
        livingCount = people.Count;
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
                if(settingsCanvas != null)
                    settingsCanvas.gameObject.SetActive(false);
            }
        }

        if(Screen.fullScreenMode != lastScreenMode)
        {
            //SetScreenmodeDropdown();
            lastScreenMode = Screen.fullScreenMode;
            SetScreenmodeText();
            //blockScreenmodeChange = true;
        }

        if(!inMenu)
        {
            livingCount = people.Count;
            if(livingCount > 0)
                gameTime += Time.deltaTime;
            else
            {
                //do endgame stuff
            }

            timer.text = gameTime.ToString("F1");
            counter.text = livingCount.ToString();
        }
    }

    public void RemoveFromLiving(AICharacter person)
    {
        if(people.Contains(person))
            people.Remove(person);
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

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenSettings(bool value)
    {
        if(settingsCanvas == null) return;

        settingsCanvas.gameObject.SetActive(value);
        menuCanvas.gameObject.SetActive(!value);
    }

    ///////////////\/\\\\\\\\\\\\\\\
    /////       SETTINGS       \\\\\
    ///////////////\/\\\\\\\\\\\\\\\

    void SetSettingsMenu()
    {
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        //SetScreenmodeDropdown();
        SetScreenmodeText();
        SetResolutionDropdown();
        SetAntialiasingDropdown();
        SetVsyncDropdown();
    }

    public void SetQualityLevel()
    {
        QualitySettings.SetQualityLevel(qualityDropdown.value, true);
        SetSettingsMenu();
    }

    void SetScreenmodeText()
    {
        if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
            screenmodeText.text = "FOR WINDOWED, PRESS ALT + F4";
        else screenmodeText.text = "FOR FULLSCREEN, PRESS ALT + F4";
    }
    /*void SetScreenmodeDropdown()
    {
        switch(Screen.fullScreenMode)
        {
            case FullScreenMode.ExclusiveFullScreen:
                screenmodeDropdown.value = 0;
                break;
            case FullScreenMode.FullScreenWindow:
                screenmodeDropdown.value = 1;
                break;
            case FullScreenMode.MaximizedWindow:
                screenmodeDropdown.value = 2;
                break;
            case FullScreenMode.Windowed:
                screenmodeDropdown.value = 3;
                break;
        }
    }*/
    /*public void SetScreenMode()
    {
        if(blockScreenmodeChange)
        {
            blockScreenmodeChange = false;
            return;
        }

        switch(screenmodeDropdown.value)
        {
            case 0: //Exclusive Fullscreen
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1: //Fullscreen Window
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 2: //Maximized Window
                Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                break;
            case 3: //Windowed
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }

        SetResolution();
    }*/

    void SetResolutionDropdown()
    {
        int index = 0;
        for(int i = 0; i < Screen.resolutions.Length; i++)
        {
            if(Screen.resolutions[i].width == Screen.width 
                && Screen.resolutions[i].height == Screen.height
                && Screen.resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
            {
                index = i;
                break;
            }
        }

        resolutionDropdown.value = index;
    }
    public void SetResolution()
    {
        Resolution res = Screen.resolutions[resolutionDropdown.value];
        if(Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
        {
            res = Screen.currentResolution;
            //Screen.fullScreen = true;
            Screen.SetResolution(res.width, res.height, true, res.refreshRate);
        }
        else
        {
            //Screen.fullScreen = false;
            Screen.SetResolution(res.width, res.height, false);
        }

        lastScreenMode = Screen.fullScreenMode;
    }

    void SetAntialiasingDropdown()
    {
        switch(QualitySettings.antiAliasing)
        {
            case 0:
                aaDropdown.value = 0;
                break;
            case 2:
                aaDropdown.value = 1;
                break;
            case 4:
                aaDropdown.value = 2;
                break;
            case 8:
                aaDropdown.value = 3;
                break;
        }
    }
    public void SetAntialiasing()
    {
        switch(aaDropdown.value)
        {
            case 0:
                QualitySettings.antiAliasing = 0;
                break;
            case 1:
                QualitySettings.antiAliasing = 2;
                break;
            case 2:
                QualitySettings.antiAliasing = 4;
                break;
            case 3:
                QualitySettings.antiAliasing = 8;
                break;
        }
    }

    void SetVsyncDropdown()
    {
        vsyncDropdown.value = QualitySettings.vSyncCount;
    }
    public void SetVsync()
    {
        QualitySettings.vSyncCount = vsyncDropdown.value;
    }




    //////////////\/\\\\\\\\\\\\\
    /////       AUDIO       \\\\\
    //////////////\/\\\\\\\\\\\\\

    public AudioClip GetFootstep()
    {
        return footsteps[Random.Range(0, footsteps.Length)];
    }
    public AudioClip GetDeathSound()
    {
        return deathSounds[Random.Range(0, deathSounds.Length)];
    }
}
