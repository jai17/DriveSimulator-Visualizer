using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public Camera mainCamera;
    public Camera settingsCamera;
    private int currentCameraIndex;

    private bool settingsOpen = false;

    private void Awake()
    {
        //Turn off all cameras except the first one
        settingsCamera.gameObject.SetActive(false);

        //Enable the first camera
        mainCamera.gameObject.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void toggleSettings()
    {
        mainCamera.gameObject.SetActive(settingsOpen);
        settingsOpen = !settingsOpen;
        settingsCamera.gameObject.SetActive(settingsOpen);
    }
}
