using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using String = System.String;

public class CutScene : MonoBehaviour
{
    private UnityEvent sceneEvent;
    private static int eventNumber;
    public static bool InEvent { get; set; }
    private const int optionCount = 6;
    private static GameObject[] optionObjects;
    private static Option[] options;

    void Awake()
    {
        eventNumber = 0;
        InEvent = false;
        optionObjects = new GameObject[optionCount];
        options = new Option[optionCount];
        for (int i = 0; i < optionCount; i++)
        {
            optionObjects[i] = General.AddChild(gameObject, "Message Box" + i.ToString());
            options[i] = optionObjects[i].AddComponent<Option>();
        }
        sceneEvent = new UnityEvent();
        sceneEvent.AddListener(UnlockEvent);
    }

    void Start()
    {
        switch (GameManager.eventNumber)
        {
            case 0:
            case 1:
                options[2].Initialize("Message", 0, Graphics.optionBox[0], 1f, 1f, 1, null, Graphics.Font.Recurso, 8f,
                    Graphics.Blue, new Vector2(.85f, .28f), false, lineSpacing: -6f, alignment: TextAlignmentOptions.MidlineLeft);
                break;

            case 100:
                // initialize text
                options[0].Initialize("Message", 1, null, 1f, 1f, 2, "Everyday Housing Problem", Graphics.Font.Mops, 14f,
                    Graphics.LightBrown, Vector2.zero, false, lineSpacing: -6f);
                optionObjects[0].transform.localPosition = new Vector3(0, 2.97f, 0);
                options[1].Initialize("Message", 1, null, 1f, 1f, 2, "A game made for <color=#f79122>Ludum Dare 47</color>", Graphics.Font.Mops, 6f,
                    Graphics.White, Vector2.zero, false, lineSpacing: -6f);
                optionObjects[1].transform.localPosition = new Vector3(0, .84f, 0);
                options[2].Initialize("Message", 1, null, 1f, 1f, 2, "which took much more than 72 hours to finish", Graphics.Font.Mops, 6f,
                    Graphics.White, Vector2.zero, false, lineSpacing: -6f);
                optionObjects[2].transform.localPosition = new Vector3(0, .14f, 0);
                options[3].Initialize("Message", 1, null, 1f, 1f, 2, "by <color=#ebd561>Notan</color>", Graphics.Font.Mops, 6f,
                    Graphics.White, Vector2.zero, false, lineSpacing: -6f);
                optionObjects[3].transform.localPosition = new Vector3(0, -.79f, 0);
                options[4].Initialize("Message", 1, null, 1f, 1f, 2, "The End", Graphics.Font.Mops, 7.2f,
                    Graphics.White, Vector2.zero, false, lineSpacing: -6f);
                optionObjects[4].transform.localPosition = new Vector3(0, -2.35f, 0);
                options[5].Initialize("Message", 1, null, 1f, 1f, 2, "Thank you for playing!", Graphics.Font.Mops, 7.2f,
                    Graphics.White, Vector2.zero, false, lineSpacing: -6f);
                optionObjects[5].transform.localPosition = new Vector3(0, -3.72f, 0);
                break;
            default:
                Debug.LogError(String.Format("Start: GameManager.eventNumber {0} is invalid", GameManager.eventNumber));
                InEvent = true;
                break;
        }
        for (int i = 0; i < optionCount; i++) optionObjects[i].SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneLoader.Loading) return;

        if (!InEvent)
        {
            switch (GameManager.eventNumber)
            {
                case 0:
                    switch (eventNumber)
                    {
                        case 0:
                            UpdateText(Speaker.Narrator, 2, "Thank you for coming to" + Environment.NewLine + "our presentation.");
                            optionObjects[2].SetActive(true);
                            eventNumber++;
                            break;
                        case 1:
                            if (Keyboard.GetSelect())
                            {
                                UpdateText(Speaker.Narrator, 2, "Today, we will demonstrate our" + Environment.NewLine + "cutting-edge invention.");
                                eventNumber++;
                            }
                            break;
                        case 2:
                            if (Keyboard.GetSelect())
                            {
                                UpdateText(Speaker.Narrator, 2, "It's name is...");
                                eventNumber++;
                            }
                            break;
                        case 3:
                            if (Keyboard.GetSelect())
                            {
                                UpdateText(Speaker.Narrator, 2, "... \"The Cutting Edge\"!");
                                eventNumber++;
                            }
                            break;
                        case 4:
                            if (Keyboard.GetSelect())
                            {
                                optionObjects[2].SetActive(false);
                                SceneLoader.sceneEvent.Invoke("MainScene");
                            }
                            break;
                        default:
                            Debug.LogWarning($"Updata-event0: not implemented for event {eventNumber}");
                            InEvent = true;
                            break;
                    }
                    break;

                case 1:
                    switch (eventNumber)
                    {
                        case 0:
                            UpdateText(Speaker.Narrator, 2, "The Cutting Edge provides you" + Environment.NewLine + "a new style of excavation.");
                            optionObjects[2].SetActive(true);
                            eventNumber++;
                            break;
                        case 1:
                            if (Keyboard.GetSelect())
                            {
                                optionObjects[2].SetActive(false);
                                SceneLoader.sceneEvent.Invoke("MainScene");
                            }
                            break;
                        default:
                            Debug.LogWarning($"Updata-event0: not implemented for event {eventNumber}");
                            InEvent = true;
                            break;
                    }
                    break;

                case 10:
                    switch (eventNumber)
                    {
                        case 100:
                            //StartCoroutine(Graphics.Move(UFObject, UFObject.transform.localPosition, new Vector3(11.79f, 2.97f, 0), duration: .6f, delay: 6f));
                            //InEvent = true;
                            //StartCoroutine(General.WaitEvent(sceneEvent, 6.62f));
                            eventNumber++;
                            break;
                        case 111:
                            if (Keyboard.GetSelect())
                            {
                                GameManager.level = 0;
                                GameManager.eventNumber = 0;
                                SceneLoader.sceneEvent.Invoke("TitleScene");
                            }
                            break;
                        default:
                            Debug.LogWarning($"Updata-event1: not implemented for event {eventNumber}");
                            InEvent = true;
                            break;
                    }
                    break;

                default:
                    Debug.LogError(String.Format("Update: GameManager.eventNumber {0} is invalid", GameManager.eventNumber));
                    InEvent = true;
                    break;
            }
        }
    }
    private void UnlockEvent() { InEvent = false; }
    private enum Speaker
    {
        Narrator,
        Speaker0,
        Speaker1,
    }
    private void UpdateText(Speaker speaker, int optionNumber, string text)
    {
        options[optionNumber].ChangeText(text);
        switch (speaker)
        {
            case Speaker.Narrator:
                optionObjects[optionNumber].transform.localPosition = new Vector3(0, -.12f + options[optionNumber].Size.y * .15f, 0);
                break;
            case Speaker.Speaker0:
                optionObjects[optionNumber].transform.localPosition = new Vector3(-2.23f - options[optionNumber].Size.x * .25f, -.12f + options[optionNumber].Size.y * .15f, 0);
                break;
            case Speaker.Speaker1:
                optionObjects[optionNumber].transform.localPosition = new Vector3(2.17f + options[optionNumber].Size.x * .12f, .27f + options[optionNumber].Size.y * .35f, 0);
                break;
            default:
                Debug.LogError(String.Format("GetTextPosition: not implemented for speaker {0}", Enum.GetName(typeof(Speaker), speaker)));
                break;
        }
    }

    public static int GetEventNumber(int level)
    {
        switch (level)
        {
            case 0:
                return 0;
            case 1:
                return 1;
            default:
                return -1;
        }
    }
}
