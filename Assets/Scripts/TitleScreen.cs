using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using TMPro;
using String = System.String;
using System;

public class TitleScreen : MonoBehaviour
{
    private TitleState titleState;
    private enum TitleState
    {
        MainMenu,
        LevelSelect,
        Credits
    }

    private static GameObject creditObject;
    private static SpriteRenderer creditRenderer;
    private static GameObject textObject;
    private static TextMeshPro text;
    private static MeshRenderer textRenderer;
    private static GameObject[] optionObjects;
    private static Option[] options;
    private static Option levelOption;
    private static GameObject versionObject;
    private static Option version;
    private static int level;
    private static string[] levelNames;
    private static GameObject[] arrowObjects;
    private static SpriteBox[] arrowSprites;
    private const int buttonCount = 4, textCount = buttonCount + 1;
    private static int selectedOption;
    private const float OptionX = 4.29f, OptionYTop = 2.31f, OptionInterval = 1.19f;

    private static GameObject[] checkBoxObjects;
    private static SpriteBox[] checkBoxSprites;
    private static GameObject logoObject;
    private static SpriteBox logo;

    private static UnityEvent buttonEvent;

    void Awake()
    {
        titleState = TitleState.MainMenu;
        creditObject = General.AddChild(gameObject, "Credit Box");
        creditRenderer = creditObject.AddComponent<SpriteRenderer>();
        creditRenderer.sortingLayerName = "UI";
        creditRenderer.sortingOrder = 0;
        creditRenderer.drawMode = SpriteDrawMode.Sliced;
        textObject = General.AddChild(creditObject, "Credits Text");
        text = textObject.AddComponent<TextMeshPro>();
        text.alignment = TextAlignmentOptions.Midline;
        textRenderer = textObject.GetComponent<MeshRenderer>();
        if (textRenderer == null)
        {
            Debug.LogWarning(String.Format("Awake: MeshRenderer not found"));
        }
        else
        {
            textRenderer.sortingLayerName = "UI";
            textRenderer.sortingOrder = 1;
        }
        optionObjects = new GameObject[textCount];
        options = new Option[buttonCount];
        for (int i = 0; i < buttonCount; i++)
        {
            optionObjects[i] = General.AddChild(gameObject, "Option" + i.ToString());
            options[i] = optionObjects[i].AddComponent<Option>();
        }
        optionObjects[buttonCount] = General.AddChild(gameObject, "Arrow Option");
        levelOption = optionObjects[buttonCount].AddComponent<Option>();
        versionObject = General.AddChild(gameObject, "Version");
        version = versionObject.AddComponent<Option>();
        levelNames = new String[MainGame.LevelCount];
        for (int i = 0; i < levelNames.Length; i++)
        {
            levelNames[i] = MapData.GetLevelName(MapData.GetLevelTag(i));
        }
        arrowObjects = new GameObject[2];
        arrowSprites = new SpriteBox[2];
        for (int i = 0; i < 2; i++)
        {
            arrowObjects[i] = General.AddChild(optionObjects[buttonCount], "Arrow" + i);
            arrowSprites[i] = arrowObjects[i].AddComponent<SpriteBox>();
        }
        logoObject = General.AddChild(gameObject, "Logo");
        logo = logoObject.AddComponent<SpriteBox>();

        checkBoxObjects = new GameObject[2];
        checkBoxSprites = new SpriteBox[2];
        checkBoxObjects[0] = General.AddChild(levelOption.SpriteObjects[0], "Level Checkbox");
        checkBoxObjects[1] = General.AddChild(checkBoxObjects[0], "Check Mark");
        checkBoxSprites[0] = checkBoxObjects[0].AddComponent<SpriteBox>();
        checkBoxSprites[1] = checkBoxObjects[1].AddComponent<SpriteBox>();
    }
    void Start()
    {
        creditObject.transform.localPosition = new Vector3(OptionX, 1.14f, 0);
        creditRenderer.sprite = Graphics.optionBox[0];
        creditRenderer.size = new Vector2(5.56f, 1.28f);
        creditObject.transform.localScale = new Vector3(1f, 1f, 0);
        text.text = "MADE BY   Notan";
        textObject.name = "Text Credit";
        text.font = Graphics.fonts[(int)Graphics.Font.Mops];
        text.fontSize = 7.2f;
        text.color = Graphics.Green;
        creditObject.SetActive(false);
        for (int i = 0; i < buttonCount; i++)
        {
            options[i].Initialize("UI", 0, Graphics.optionBox[0], 1.25f, 1.2f, 1, null, Graphics.Font.Mops, 4.5f,
                Graphics.Green, new Vector2(2.3f, .6f), true, alignment: TextAlignmentOptions.Center, useCollider: true);
        }
        levelOption.Initialize("UI", 0, Graphics.optionBox[0], 1.25f, 1.2f, 1, null, Graphics.Font.Mops, 4.5f,
            Graphics.Green, new Vector2(.6f, .12f), false, alignment: TextAlignmentOptions.Center, useCollider: true);
        version.Initialize("UI", 0, null, 1f, 1f, 1, "Made for LD48", Graphics.Font.Mops, 4.8f, Graphics.White, Vector2.zero, false);
        //version.Initialize("UI", 0, null, 1f, 1f, 1, "v" + Application.version, Graphics.Font.Mops, 4.5f, Graphics.Green, Vector2.zero, false);

        optionObjects[buttonCount].transform.localPosition = new Vector3(OptionX, 1.15f - OptionInterval, 0);
        optionObjects[buttonCount].SetActive(false);
        versionObject.transform.localPosition = new Vector3(6.25f, -3.29f, 0);
        for (int i = 0; i < 2; i++)
        {
            arrowObjects[i].transform.parent = levelOption.SpriteObjects[0].transform;
            arrowObjects[i].transform.eulerAngles = new Vector3(0, 0, i == 0 ? 90 : -90);
            arrowObjects[i].transform.localScale = new Vector3(.75f, .75f, 1);
            arrowObjects[i].transform.localPosition = new Vector3(.1f * (i * 2 - 1), -.51f, 0);
            arrowSprites[i].Initialize(Graphics.arrow[0], "UI", 1, Vector3.zero, useCollider: true);
        }

        for (int i = 0; i < buttonCount; i++) optionObjects[i].transform.localPosition = new Vector3(OptionX, OptionYTop - OptionInterval * i, 0);

        options[0].ChangeText("Start");
        options[1].ChangeText("Select level");
        options[2].ChangeText("Credits");
        options[3].ChangeText("Quit");
        selectedOption = 0;
        options[0].SetSelected(true);
        for (int i = 1; i < buttonCount; i++) options[0].SetSelected(false);

        checkBoxSprites[0].Initialize(Graphics.checkbox[0], "UI", 1, Vector3.zero);
        checkBoxSprites[1].Initialize(Graphics.checkbox[1], "UI", 2, Vector3.zero);
        checkBoxObjects[0].transform.localScale = new Vector3(.71f, .71f, 1);
        checkBoxObjects[1].transform.localScale = new Vector3(.71f, .71f, 1);
        logo.Initialize(Graphics.logo, "Background", 1, new Vector3(-3.04f, 1.96f, 0));
        logoObject.transform.localScale = new Vector3(1.44f, 1.44f, 1);
    }
    void Update()
    {
        if (SceneLoader.Loading) return;

        bool mouseDetected = false;
        switch (titleState)
        {
            case TitleState.MainMenu:
                for (int i = 0; i < buttonCount; i++)
                {
                    if (options[i].Mouse.GetMouseEnter())
                    {
                        mouseDetected = true;
                        options[selectedOption].SetSelected(false);
                        selectedOption = i;
                        options[selectedOption].SetSelected(true);
                        break;
                    }
                }
                if (mouseDetected) break;
                if (Keyboard.GetDown())
                {
                    options[selectedOption].SetSelected(false);
                    selectedOption++;
                    if (selectedOption >= buttonCount) selectedOption = 0;
                    options[selectedOption].SetSelected(true);
                }
                else if (Keyboard.GetUp())
                {
                    options[selectedOption].SetSelected(false);
                    selectedOption--;
                    if (selectedOption < 0) selectedOption = buttonCount - 1;
                    options[selectedOption].SetSelected(true);
                }
                else if (Keyboard.GetSelect() || options[selectedOption].Mouse.GetMouseClick())
                {
                    switch (selectedOption)
                    {
                        case 0:
                            if (GameManager.previousScene == "")
                            {
                                level = 0;
                                if (General.Progress.Contains(MapData.GetLevelTag(MainGame.LevelCount - 1))) level = MainGame.LevelCount - 1;
                                else
                                {
                                    for (int i = MainGame.LevelCount - 2; i >= 0; i--)
                                    {
                                        if (General.Progress.Contains(MapData.GetLevelTag(i)))
                                        {
                                            level = i + 1;
                                            break;
                                        }
                                    }
                                }
                                GameManager.level = level;
                            }
                            int eventNumber = CutScene.GetEventNumber(level);
                            if (eventNumber >= 0)
                            {
                                GameManager.eventNumber = eventNumber;
                                SceneLoader.sceneEvent.Invoke("EndScene");
                            }
                            else SceneLoader.sceneEvent.Invoke("MainScene");
                            break;
                        case 1:
                            options[1].ChangeText("Back");
                            optionObjects[1].transform.localPosition = new Vector3(OptionX, -.57f, 0);
                            optionObjects[buttonCount].transform.localPosition = new Vector3(OptionX, 1.15f, 0);
                            options[1].SetSelected(false);
                            for (int i = 0; i < buttonCount; i++)
                            {
                                if (i == 1) continue;
                                optionObjects[i].SetActive(false);
                            }
                            selectedOption = 0;
                            if (GameManager.previousScene == "")
                            {
                                level = 0;
                                if (General.Progress.Contains(MapData.GetLevelTag(MainGame.LevelCount - 1))) level = MainGame.LevelCount - 1;
                                else
                                {
                                    for (int i = MainGame.LevelCount - 2; i >= 0; i--)
                                    {
                                        if (General.Progress.Contains(MapData.GetLevelTag(i)))
                                        {
                                            level = i + 1;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                level = GameManager.level;
                                if (level < 0 || level >= MainGame.LevelCount) level = 0;
                            }
                            levelOption.SetSelected(true);
                            SetLevelName(level);
                            optionObjects[buttonCount].SetActive(true);
                            checkBoxObjects[0].SetActive(true);
                            titleState = TitleState.LevelSelect;
                            break;
                        case 2:
                            options[2].ChangeText("Back");
                            optionObjects[2].transform.localPosition = new Vector3(OptionX, -.41f, 0);
                            for (int i = 0; i < buttonCount; i++)
                            {
                                if (i == 2) continue;
                                optionObjects[i].SetActive(false);
                            }
                            creditObject.SetActive(true);
                            titleState = TitleState.Credits;
                            break;
                        case 3:
#if UNITY_EDITOR
                                EditorApplication.isPlaying = false;
#else
                            Application.Quit();
#endif
                            break;
                    }
                }
                break;
            case TitleState.LevelSelect:
                if ((Keyboard.GetRight() && selectedOption == 0 || arrowSprites[1].Mouse.GetMouseClick()) && level < MainGame.LevelCount - 1)
                {
                    levelOption.SetSelected(true);
                    options[1].SetSelected(false);
                    selectedOption = 0;
                    level++;
                    SetLevelName(level);
                }
                else if ((Keyboard.GetLeft() && selectedOption == 0 || arrowSprites[0].Mouse.GetMouseClick()) && level > 0)
                {
                    levelOption.SetSelected(true);
                    options[1].SetSelected(false);
                    selectedOption = 0;
                    level--;
                    SetLevelName(level);
                }
                else if (selectedOption == 0 && (Keyboard.GetDown() || Keyboard.GetUp()) || options[1].Mouse.GetMouseEnter())
                {
                    selectedOption = 1;
                    levelOption.SetSelected(false);
                    options[1].SetSelected(true);
                }
                else if (selectedOption == 1 && (Keyboard.GetDown() || Keyboard.GetUp()) || levelOption.Mouse.GetMouseEnter())
                {
                    selectedOption = 0;
                    levelOption.SetSelected(true);
                    options[1].SetSelected(false);
                }
                else if ((Keyboard.GetSelect() || options[1].Mouse.GetMouseClick()) && selectedOption == 1 || Keyboard.GetCancel())
                {
                    options[1].ChangeText("Select level");
                    options[1].SetSelected(true);
                    optionObjects[1].transform.localPosition = new Vector3(OptionX, OptionYTop - OptionInterval, 0);
                    for (int i = 0; i < buttonCount; i++) optionObjects[i].SetActive(true);
                    selectedOption = 1;
                    levelOption.SetSelected(false);
                    optionObjects[buttonCount].SetActive(false);
                    titleState = TitleState.MainMenu;
                }
                else if ((Keyboard.GetSelect() || levelOption.Mouse.GetMouseClick()) && selectedOption == 0)
                {
                    GameManager.level = level;
                    int eventNumber = CutScene.GetEventNumber(level);
                    if (eventNumber >= 0)
                    {
                        GameManager.eventNumber = eventNumber;
                        SceneLoader.sceneEvent.Invoke("EndScene");
                    }
                    else SceneLoader.sceneEvent.Invoke("MainScene");
                }
                break;
            case TitleState.Credits:
                if (Keyboard.GetSelect() || options[2].Mouse.GetMouseClick() || Keyboard.GetCancel())
                {
                    options[2].ChangeText("Credits");
                    optionObjects[2].transform.localPosition = new Vector3(OptionX, OptionYTop - OptionInterval * 2, 0);
                    for (int i = 0; i < buttonCount; i++) optionObjects[i].SetActive(true);
                    creditObject.SetActive(false);
                    titleState = TitleState.MainMenu;
                }
                break;
            default:
                Debug.LogWarning(String.Format("Update: not implemented for State {0}", titleState.ToString()));
                break;
        }
    }

    private static void SetLevelName(int level)
    {
        levelOption.ChangeText(String.Format("{0}", levelNames[level]));
        for (int i = 0; i < 2; i++)
        {
            arrowObjects[i].transform.localPosition = new Vector3(.1f * (i * 2 - 1), -.51f, 0);
        }
        checkBoxObjects[0].transform.localPosition = new Vector3(-levelOption.Size.x / 2 - .32f, 0, 0);
        checkBoxObjects[1].SetActive(General.Progress.Contains(MapData.GetLevelTag(level)));
        arrowSprites[0].spriteRenderer.color = (level == 0 ? Graphics.Gray : Graphics.White);
        arrowSprites[1].spriteRenderer.color = (level == MainGame.LevelCount - 1 ? Graphics.Gray : Graphics.White);
    }
}
