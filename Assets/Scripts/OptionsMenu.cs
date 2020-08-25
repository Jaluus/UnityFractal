using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public GameObject optionSliders;
    public GameObject controlsMenu;
    public static bool isActive = false;
    public static bool controlsActive = true;


    private void Start()
    {
        optionSliders.SetActive(false);
        controlsMenu.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
        {
            if (isActive)
            {
                CloseOptions();
            }
            else
            {
                OpenOptions();
            }
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            ToogleControls();
        }
    }

    private void ToogleControls()
    {
        controlsActive = !controlsActive;
        controlsMenu.SetActive(controlsActive);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void OpenOptions()
    {
        controlsMenu.SetActive(false);
        optionSliders.SetActive(true);
        isActive = true;
        Cursor.lockState = CursorLockMode.None;
    }
    void CloseOptions()
    {
        controlsMenu.SetActive(true);
        optionSliders.SetActive(false);
        isActive = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

}
