using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using String = System.String;

public class Menu : MonoBehaviour
{
    private enum MenuState { Menu }
    private MenuState state;

    private const int optionNumber = 2;
    private GameObject[] optionObjects;
    private Option[] options;
    private int selectedOption;

    // options: resume, key, main menu

    void Awake()
    {
        optionObjects = new GameObject[optionNumber];
        options = new Option[optionNumber * 2];
        for (int i = 0; i < optionNumber; i++)
        {
            optionObjects[i] = General.AddChild(gameObject, "Option" + i);
            options[i] = optionObjects[i].AddComponent<Option>();
        }
    }
    public void Initialize()
    {
        for (int i = 0; i < optionNumber; i++)
        {
            options[i].Initialize("UI", 0, Graphics.optionBox[0], 1.25f, 1.2f, 1, null, Graphics.Font.Mops, 4.5f, Graphics.Green, new Vector2(.6f, .12f), false, useCollider: true);
            optionObjects[i].transform.localPosition = Vector3.up * (1.5f - i * 1.2f);
        }
        options[0].ChangeText("Resume");
        options[1].ChangeText("Return to main menu");

        state = MenuState.Menu;
        selectedOption = 0;
        options[selectedOption].SetSelected(true);
        for (int i = 1; i < optionNumber; i++) options[selectedOption].SetSelected(false);
    }

    // return true if there was an input
    // the output is used for handling an edge case when the menu is closed
    // to prevent closing it doesn't trigger any action in the gameplay
    public bool HandleInput()
    {
        switch (state)
        {
            case MenuState.Menu:
                // detect mouse hover
                for (int i = 0; i < optionNumber; i++)
                {
                    if (options[i].Mouse.GetMouseEnter())
                    {
                        options[selectedOption].SetSelected(false);
                        selectedOption = i;
                        options[selectedOption].SetSelected(true);
                        return true;
                    }
                }

                if (Keyboard.GetDown() && selectedOption < optionNumber - 1)
                {
                    options[selectedOption].SetSelected(false);
                    selectedOption++;
                    options[selectedOption].SetSelected(true);
                    return true;
                }
                else if (Keyboard.GetUp() && selectedOption > 0)
                {
                    options[selectedOption].SetSelected(false);
                    selectedOption--;
                    options[selectedOption].SetSelected(true);
                    return true;
                }
                else if (Keyboard.GetSelect() || options[selectedOption].Mouse.GetMouseClick())
                {
                    switch (selectedOption)
                    {
                        case 0:
                            selectedOption = 0;
                            options[selectedOption].SetSelected(true);
                            for (int i = 1; i < optionNumber; i++) options[selectedOption].SetSelected(false);
                            gameObject.SetActive(false);
                            return true;
                        case 1:
                            SceneLoader.sceneEvent.Invoke("TitleScene");
                            return true;
                    }
                }
                else if (Keyboard.GetCancel())
                {
                    selectedOption = 0;
                    options[selectedOption].SetSelected(true);
                    for (int i = 1; i < optionNumber; i++) options[selectedOption].SetSelected(false);
                    gameObject.SetActive(false);
                    return true;
                }
                return false;
            default:
                Debug.LogWarning($"Menu.HandleInput: not implemented for type {Enum.GetName(typeof(MenuState), state)}");
                state = MenuState.Menu;
                return false;
        }
    }
}
