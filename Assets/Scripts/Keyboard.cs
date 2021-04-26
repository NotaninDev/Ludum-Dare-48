﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using TMPro;
using String = System.String;

public class Keyboard : MonoBehaviour
{
    private static KeyCode[] defaultKeys;
    private static KeyCode[,] assinedKeys;

    private GameObject controlObject;
    private SpriteBox controlSprite;
    private const int KeyNumber = 15;
    private GameObject[] keyObjects;
    private Option[] keys;
    private int selectedOption;
    private bool waitingInput;

    private static readonly string keyMappingPath = Application.persistentDataPath + "/controls.txt";
    [Serializable]
    public class Preference
    {
        public KeyCode KeySelect, KeyMenu, KeyUp, KeyLeft, KeyDown, KeyRight, KeyClockwise, KeyCounterclockwise,
            KeyUndo, KeyReset, KeySwitch, KeyFaceUp, KeyFaceLeft, KeyFaceDown, KeyFaceRight;
        public Preference()
        {
            if (defaultKeys != null && defaultKeys.Length >= KeyNumber)
            {
                KeySelect = defaultKeys[0];
                KeyMenu = defaultKeys[1];
                KeyUp = defaultKeys[2];
                KeyLeft = defaultKeys[3];
                KeyDown = defaultKeys[4];
                KeyRight = defaultKeys[5];
                KeyClockwise = defaultKeys[6];
                KeyCounterclockwise = defaultKeys[7];
                KeyUndo = defaultKeys[8];
                KeyReset = defaultKeys[9];
                KeySwitch = defaultKeys[10];
                KeyFaceUp = defaultKeys[11];
                KeyFaceLeft = defaultKeys[12];
                KeyFaceDown = defaultKeys[13];
                KeyFaceRight = defaultKeys[14];
            }
            else
            {
                if (defaultKeys == null) Debug.LogWarning(String.Format("Preference: defaultKeys is not initialized"));
                else Debug.LogWarning(String.Format("Preference: the length of defaultKeys ({0}) is shorter than {1}", defaultKeys.Length, KeyNumber));
            }
        }
    }

    void Awake()
    {
        controlObject = General.AddChild(gameObject, "Controls");
        controlSprite = controlObject.AddComponent<SpriteBox>();
        keyObjects = new GameObject[KeyNumber + 4];
        keys = new Option[KeyNumber + 4];
        for (int i = 0; i < KeyNumber + 4; i++)
        {
            keyObjects[i] = General.AddChild(controlObject, i < KeyNumber ? GetControlName(i) : null);
            keys[i] = keyObjects[i].AddComponent<Option>();
        }

        keyObjects[15].name = "Text Menu";
        keyObjects[16].name = "Text Gameplay";
        keyObjects[17].name = "Text Back";
        keyObjects[18].name = "Text Default";
    }

    void Start()
    {
        //controlSprite.Initialize(Graphics.optionBox[1], "UI", 2, new Vector3(0, 0, 0));
        //controlSprite.spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        //controlSprite.spriteRenderer.size = new Vector2(11.8f, 7.84f);
        for (int i = 0; i < 6; i++)
        {
            keyObjects[i].transform.localPosition = new Vector3(-3.2f, 3.36f - i * .84f, 0);
            keys[i].Initialize("UI", 3, Graphics.optionBox[1], 1f, 1.2f, 4, null, Graphics.Font.Mops, 5.4f, Graphics.Green, new Vector2(5.8f, .66f), true,
                alignment: TextAlignmentOptions.MidlineLeft, textAreaSize: new Vector2(5.4f, 1f), useCollider: true);
        }
        for (int i = 6; i < KeyNumber; i++)
        {
            keyObjects[i].transform.localPosition = new Vector3(3.2f, 3.36f - (i - 6) * .84f, 0);
            keys[i].Initialize("UI", 3, Graphics.optionBox[1], 1f, 1.2f, 4, null, Graphics.Font.Mops, 5.4f, Graphics.Green, new Vector2(5.8f, .66f), true,
                alignment: TextAlignmentOptions.MidlineLeft, textAreaSize: new Vector2(5.4f, 1f), useCollider: true);
        }

        keyObjects[15].transform.localPosition = new Vector3(-3.2f, 4.28f, 0);
        keys[15].Initialize("UI", 3, Graphics.optionBox[1], 1f, 1.2f, 4, "Menu", Graphics.Font.Mops, 6f, Graphics.BlackBrown, new Vector2(.6f, .12f), false);
        keyObjects[16].transform.localPosition = new Vector3(3.2f, 4.28f, 0);
        keys[16].Initialize("UI", 3, Graphics.optionBox[1], 1f, 1.2f, 4, "Gameplay", Graphics.Font.Mops, 6f, Graphics.BlackBrown, new Vector2(.6f, .12f), false);
        keyObjects[17].transform.localPosition = new Vector3(-3.2f, -4.28f, 0);
        keys[17].Initialize("UI", 3, Graphics.optionBox[1], 1f, 1.2f, 4, "Back", Graphics.Font.Mops, 6f, Graphics.Brown, new Vector2(.6f, .12f), false, useCollider: true);
        keyObjects[18].transform.localPosition = new Vector3(3.2f, -4.28f, 0);
        keys[18].Initialize("UI", 3, Graphics.optionBox[1], 1f, 1.2f, 4, "Default", Graphics.Font.Mops, 6f, Graphics.Brown, new Vector2(.6f, .12f), false, useCollider: true);

        for (int i = 0; i < KeyNumber; i++) keys[i].ChangeText(GetControlName(i) + ": " + GetKeyName(i));
    }

    public static void Initialize()
    {
        defaultKeys = new KeyCode[KeyNumber];
        assinedKeys = new KeyCode[KeyNumber, 2];
        defaultKeys[0] = KeyCode.Space;
        defaultKeys[1] = KeyCode.Escape;
        defaultKeys[2] = KeyCode.UpArrow;
        defaultKeys[3] = KeyCode.LeftArrow;
        defaultKeys[4] = KeyCode.DownArrow;
        defaultKeys[5] = KeyCode.RightArrow;
        defaultKeys[6] = KeyCode.C;
        defaultKeys[7] = KeyCode.X;
        defaultKeys[8] = KeyCode.Z;
        defaultKeys[9] = KeyCode.R;
        defaultKeys[10] = KeyCode.Space;
        defaultKeys[11] = KeyCode.UpArrow;
        defaultKeys[12] = KeyCode.LeftArrow;
        defaultKeys[13] = KeyCode.DownArrow;
        defaultKeys[14] = KeyCode.RightArrow;
        for (int i = 0; i < KeyNumber; i++) assinedKeys[i, 0] = defaultKeys[i];

        // load key mapping
        if (File.Exists(keyMappingPath))
        {
            try
            {
                string encodedPreference = File.ReadAllText(keyMappingPath);
                Preference preference = new Preference();
                JsonUtility.FromJsonOverwrite(encodedPreference, preference);
                assinedKeys[0, 0] = preference.KeySelect;
                assinedKeys[1, 0] = preference.KeyMenu;
                assinedKeys[2, 0] = preference.KeyUp;
                assinedKeys[3, 0] = preference.KeyLeft;
                assinedKeys[4, 0] = preference.KeyDown;
                assinedKeys[5, 0] = preference.KeyRight;
                assinedKeys[6, 0] = preference.KeyClockwise;
                assinedKeys[7, 0] = preference.KeyCounterclockwise;
                assinedKeys[8, 0] = preference.KeyUndo;
                assinedKeys[9, 0] = preference.KeyReset;
                assinedKeys[10, 0] = preference.KeySwitch;
                assinedKeys[11, 0] = preference.KeyFaceUp;
                assinedKeys[12, 0] = preference.KeyFaceLeft;
                assinedKeys[13, 0] = preference.KeyFaceDown;
                assinedKeys[14, 0] = preference.KeyFaceRight;
            }
            catch (Exception e)
            {
                Debug.LogError(String.Format("Initialize: error occured while loading key mapping: {0}", e));
            }
        }

        for (int i = 0; i < KeyNumber; i++) assinedKeys[i, 1] = GetPairKey(assinedKeys[i, 0]);
    }
    private static void SaveKeyMapping()
    {
        Preference preference = new Preference();
        string encodedPreference;
        preference.KeySelect = assinedKeys[0, 0];
        preference.KeyMenu = assinedKeys[1, 0];
        preference.KeyUp = assinedKeys[2, 0];
        preference.KeyLeft = assinedKeys[3, 0];
        preference.KeyDown = assinedKeys[4, 0];
        preference.KeyRight = assinedKeys[5, 0];
        preference.KeyClockwise = assinedKeys[6, 0];
        preference.KeyCounterclockwise = assinedKeys[7, 0];
        preference.KeyUndo = assinedKeys[8, 0];
        preference.KeyReset = assinedKeys[9, 0];
        preference.KeySwitch = assinedKeys[10, 0];
        preference.KeyFaceUp = assinedKeys[11, 0];
        preference.KeyFaceLeft = assinedKeys[12, 0];
        preference.KeyFaceDown = assinedKeys[13, 0];
        preference.KeyFaceRight = assinedKeys[14, 0];
        encodedPreference = JsonUtility.ToJson(preference);
        try
        {
            File.WriteAllText(keyMappingPath, encodedPreference);
        }
        catch (Exception e)
        {
            Debug.LogError(String.Format("SaveKeyMapping: error occured while saving: {0}", e));
        }
    }

    private static string GetControlName(int n)
    {
        switch (n)
        {
            case 0:
                return "Select";
            case 1:
                return "Menu/Cancel";
            case 2:
                return "Up";
            case 3:
                return "Left";
            case 4:
                return "Down";
            case 5:
                return "Right";
            case 6:
                return "Clockwise";
            case 7:
                return "Anticlockwise";
            case 8:
                return "Undo";
            case 9:
                return "Reset";
            case 10:
                return "Switch";
            case 11:
                return "Player Up";
            case 12:
                return "Player Left";
            case 13:
                return "Player Down";
            case 14:
                return "Player Right";
            default:
                Debug.LogWarning(String.Format("GetControlName: not implemented for number {0}", n));
                return "Error";
        }
    }
    private static int GetControlID(string name)
    {
        switch (name)
        {
            case "Select":
                return 0;
            case "Menu/Cancel":
                return 1;
            case "Up":
                return 2;
            case "Right":
                return 3;
            case "Down":
                return 4;
            case "Left":
                return 5;
            case "Clockwise":
                return 6;
            case "Anticlockwise":
            case "Counterclockwise":
                return 7;
            case "Undo":
                return 8;
            case "Reset":
                return 9;
            case "Switch":
                return 10;
            case "Player Up":
                return 11;
            case "Player Right":
                return 12;
            case "Player Down":
                return 13;
            case "Player Left":
                return 14;
            default:
                return -1;
        }
    }
    // GetKeyName(KeyCode key) returns a key name of the specified keycode
    // GetKeyName(int n) returns a key name of the n-th assigned key
    private static string GetKeyName(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.A:
            case KeyCode.B:
            case KeyCode.C:
            case KeyCode.D:
            case KeyCode.E:
            case KeyCode.F:
            case KeyCode.G:
            case KeyCode.H:
            case KeyCode.I:
            case KeyCode.J:
            case KeyCode.K:
            case KeyCode.L:
            case KeyCode.M:
            case KeyCode.N:
            case KeyCode.O:
            case KeyCode.P:
            case KeyCode.Q:
            case KeyCode.R:
            case KeyCode.S:
            case KeyCode.T:
            case KeyCode.U:
            case KeyCode.V:
            case KeyCode.W:
            case KeyCode.X:
            case KeyCode.Y:
            case KeyCode.Z:
            case KeyCode.Backspace:
            case KeyCode.Delete:
            case KeyCode.Tab:
            case KeyCode.Clear:
            case KeyCode.Return:
            case KeyCode.Pause:
            case KeyCode.Escape:
            case KeyCode.Space:
            case KeyCode.Insert:
            case KeyCode.Home:
            case KeyCode.End:
            case KeyCode.PageUp:
            case KeyCode.PageDown:
            case KeyCode.F1:
            case KeyCode.F2:
            case KeyCode.F3:
            case KeyCode.F4:
            case KeyCode.F5:
            case KeyCode.F6:
            case KeyCode.F7:
            case KeyCode.F8:
            case KeyCode.F9:
            case KeyCode.F10:
            case KeyCode.F11:
            case KeyCode.F12:
            case KeyCode.F13:
            case KeyCode.F14:
            case KeyCode.F15:
            case KeyCode.Numlock:
            case KeyCode.CapsLock:
            case KeyCode.ScrollLock:
            case KeyCode.AltGr:
            case KeyCode.Help:
            case KeyCode.Print:
            case KeyCode.SysReq:
            case KeyCode.Break:
            case KeyCode.Menu:
                return Enum.GetName(typeof(KeyCode), key);
            case KeyCode.Keypad0:
            case KeyCode.Alpha0:
                return "0";
            case KeyCode.Keypad1:
            case KeyCode.Alpha1:
                return "1";
            case KeyCode.Keypad2:
            case KeyCode.Alpha2:
                return "2";
            case KeyCode.Keypad3:
            case KeyCode.Alpha3:
                return "3";
            case KeyCode.Keypad4:
            case KeyCode.Alpha4:
                return "4";
            case KeyCode.Keypad5:
            case KeyCode.Alpha5:
                return "5";
            case KeyCode.Keypad6:
            case KeyCode.Alpha6:
                return "6";
            case KeyCode.Keypad7:
            case KeyCode.Alpha7:
                return "7";
            case KeyCode.Keypad8:
            case KeyCode.Alpha8:
                return "8";
            case KeyCode.Keypad9:
            case KeyCode.Alpha9:
                return "9";
            case KeyCode.KeypadPeriod:
            case KeyCode.Period:
                return ".";
            case KeyCode.KeypadDivide:
            case KeyCode.Slash:
                return "/";
            case KeyCode.KeypadMultiply:
            case KeyCode.Asterisk:
                return "*";
            case KeyCode.KeypadMinus:
            case KeyCode.Minus:
                return "-";
            case KeyCode.KeypadPlus:
            case KeyCode.Plus:
                return "+";
            case KeyCode.KeypadEnter:
                return "Enter";
            case KeyCode.KeypadEquals:
            case KeyCode.Equals:
                return "=";
            case KeyCode.UpArrow:
                return "↑";
            case KeyCode.RightArrow:
                return "→";
            case KeyCode.DownArrow:
                return "↓";
            case KeyCode.LeftArrow:
                return "←";
            case KeyCode.Exclaim:
                return "!";
            case KeyCode.DoubleQuote:
                return "\"";
            case KeyCode.Hash:
                return "#";
            case KeyCode.Dollar:
                return "$";
            case KeyCode.Percent:
                return "%";
            case KeyCode.Ampersand:
                return "&";
            case KeyCode.Quote:
                return "'";
            case KeyCode.LeftParen:
                return "(";
            case KeyCode.RightParen:
                return ")";
            case KeyCode.Comma:
                return ",";
            case KeyCode.Colon:
                return ":";
            case KeyCode.Semicolon:
                return ";";
            case KeyCode.Less:
                return "<";
            case KeyCode.Greater:
                return ">";
            case KeyCode.Question:
                return "?";
            case KeyCode.At:
                return "@";
            case KeyCode.LeftBracket:
                return "[";
            case KeyCode.Backslash:
                return "\\";
            case KeyCode.RightBracket:
                return "]";
            case KeyCode.Caret:
                return "^";
            case KeyCode.Underscore:
                return "_";
            case KeyCode.BackQuote:
                return "`";
            case KeyCode.LeftCurlyBracket:
                return "{";
            case KeyCode.Pipe:
                return "|";
            case KeyCode.RightCurlyBracket:
                return "}";
            case KeyCode.Tilde:
                return "~";
            case KeyCode.RightShift:
                return "Shift R";
            case KeyCode.LeftShift:
                return "Shift L";
            case KeyCode.RightControl:
                return "Control R";
            case KeyCode.LeftControl:
                return "Control L";
            case KeyCode.RightAlt:
                return "Alt R";
            case KeyCode.LeftAlt:
                return "Alt L";
            case KeyCode.LeftCommand:
                return "Command L";
            case KeyCode.LeftWindows:
                return "Windows L";
            case KeyCode.RightCommand:
                return "Command R";
            case KeyCode.RightWindows:
                return "Windows R";
            default:
                Debug.LogWarning(String.Format("GetControlName: not implemented for key {0}", Enum.GetName(typeof(KeyCode), key)));
                return "ERROR";
        }
    }
    public static string GetKeyName(int n)
    {
        if (n < 0 || n >= KeyNumber)
        {
            Debug.LogWarning(String.Format("GetKeyName: invalid number {0}", n));
            return "ERROR";
        }
        return GetKeyName(assinedKeys[n, 0]);
    }
    private static KeyCode GetPairKey(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.A:
            case KeyCode.B:
            case KeyCode.C:
            case KeyCode.D:
            case KeyCode.E:
            case KeyCode.F:
            case KeyCode.G:
            case KeyCode.H:
            case KeyCode.I:
            case KeyCode.J:
            case KeyCode.K:
            case KeyCode.L:
            case KeyCode.M:
            case KeyCode.N:
            case KeyCode.O:
            case KeyCode.P:
            case KeyCode.Q:
            case KeyCode.R:
            case KeyCode.S:
            case KeyCode.T:
            case KeyCode.U:
            case KeyCode.V:
            case KeyCode.W:
            case KeyCode.X:
            case KeyCode.Y:
            case KeyCode.Z:
            case KeyCode.Backspace:
            case KeyCode.Delete:
            case KeyCode.Tab:
            case KeyCode.Clear:
            case KeyCode.Return:
            case KeyCode.Pause:
            case KeyCode.Escape:
            case KeyCode.Space:
            case KeyCode.Insert:
            case KeyCode.Home:
            case KeyCode.End:
            case KeyCode.PageUp:
            case KeyCode.PageDown:
            case KeyCode.F1:
            case KeyCode.F2:
            case KeyCode.F3:
            case KeyCode.F4:
            case KeyCode.F5:
            case KeyCode.F6:
            case KeyCode.F7:
            case KeyCode.F8:
            case KeyCode.F9:
            case KeyCode.F10:
            case KeyCode.F11:
            case KeyCode.F12:
            case KeyCode.F13:
            case KeyCode.F14:
            case KeyCode.F15:
            case KeyCode.Numlock:
            case KeyCode.CapsLock:
            case KeyCode.ScrollLock:
            case KeyCode.AltGr:
            case KeyCode.Help:
            case KeyCode.Print:
            case KeyCode.SysReq:
            case KeyCode.Break:
            case KeyCode.Menu:
            case KeyCode.KeypadEnter:
            case KeyCode.UpArrow:
            case KeyCode.RightArrow:
            case KeyCode.DownArrow:
            case KeyCode.LeftArrow:
            case KeyCode.Exclaim:
            case KeyCode.DoubleQuote:
            case KeyCode.Hash:
            case KeyCode.Dollar:
            case KeyCode.Percent:
            case KeyCode.Ampersand:
            case KeyCode.Quote:
            case KeyCode.LeftParen:
            case KeyCode.RightParen:
            case KeyCode.Comma:
            case KeyCode.Colon:
            case KeyCode.Semicolon:
            case KeyCode.Less:
            case KeyCode.Greater:
            case KeyCode.Question:
            case KeyCode.At:
            case KeyCode.LeftBracket:
            case KeyCode.Backslash:
            case KeyCode.RightBracket:
            case KeyCode.Caret:
            case KeyCode.Underscore:
            case KeyCode.BackQuote:
            case KeyCode.LeftCurlyBracket:
            case KeyCode.Pipe:
            case KeyCode.RightCurlyBracket:
            case KeyCode.Tilde:
            case KeyCode.RightShift:
            case KeyCode.LeftShift:
            case KeyCode.RightControl:
            case KeyCode.LeftControl:
            case KeyCode.RightAlt:
            case KeyCode.LeftAlt:
            case KeyCode.LeftCommand:
            case KeyCode.LeftWindows:
            case KeyCode.RightCommand:
            case KeyCode.RightWindows:
                return KeyCode.None;

            case KeyCode.Keypad0:
                return KeyCode.Alpha0;
            case KeyCode.Alpha0:
                return KeyCode.Keypad0;

            case KeyCode.Keypad1:
                return KeyCode.Alpha1;
            case KeyCode.Alpha1:
                return KeyCode.Keypad1;

            case KeyCode.Keypad2:
                return KeyCode.Alpha2;
            case KeyCode.Alpha2:
                return KeyCode.Keypad2;

            case KeyCode.Keypad3:
                return KeyCode.Alpha3;
            case KeyCode.Alpha3:
                return KeyCode.Keypad3;

            case KeyCode.Keypad4:
                return KeyCode.Alpha4;
            case KeyCode.Alpha4:
                return KeyCode.Keypad4;

            case KeyCode.Keypad5:
                return KeyCode.Alpha5;
            case KeyCode.Alpha5:
                return KeyCode.Keypad5;

            case KeyCode.Keypad6:
                return KeyCode.Alpha6;
            case KeyCode.Alpha6:
                return KeyCode.Keypad6;

            case KeyCode.Keypad7:
                return KeyCode.Alpha7;
            case KeyCode.Alpha7:
                return KeyCode.Keypad7;

            case KeyCode.Keypad8:
                return KeyCode.Alpha8;
            case KeyCode.Alpha8:
                return KeyCode.Keypad8;

            case KeyCode.Keypad9:
                return KeyCode.Alpha9;
            case KeyCode.Alpha9:
                return KeyCode.Keypad9;

            case KeyCode.KeypadPeriod:
                return KeyCode.Period;
            case KeyCode.Period:
                return KeyCode.KeypadPeriod;

            case KeyCode.KeypadDivide:
                return KeyCode.Slash;
            case KeyCode.Slash:
                return KeyCode.KeypadDivide;

            case KeyCode.KeypadMultiply:
                return KeyCode.Asterisk;
            case KeyCode.Asterisk:
                return KeyCode.KeypadMultiply;

            case KeyCode.KeypadMinus:
                return KeyCode.Minus;
            case KeyCode.Minus:
                return KeyCode.KeypadMinus;

            case KeyCode.KeypadPlus:
                return KeyCode.Plus;
            case KeyCode.Plus:
                return KeyCode.KeypadPlus;

            case KeyCode.KeypadEquals:
                return KeyCode.Equals;
            case KeyCode.Equals:
                return KeyCode.KeypadEquals;

            default:
                Debug.LogWarning(String.Format("GetPairKey: not implemented for key {0}", Enum.GetName(typeof(KeyCode), key)));
                return KeyCode.None;
        }
    }

    // CheckKeyInput returns one valid key input that is pressed down
    private static KeyCode CheckKeyInput()
    {
        KeyCode[] validKeys = { KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N,
            KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z, KeyCode.Backspace, KeyCode.Delete, KeyCode.Tab,
            KeyCode.Clear, KeyCode.Return, KeyCode.Pause, KeyCode.Escape, KeyCode.Space, KeyCode.Insert, KeyCode.Home, KeyCode.End, KeyCode.PageUp, KeyCode.PageDown, KeyCode.F1, KeyCode.F2,
            KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.F6, KeyCode.F7, KeyCode.F8, KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12, KeyCode.F13, KeyCode.F14, KeyCode.F15, KeyCode.Numlock,
            KeyCode.CapsLock, KeyCode.ScrollLock, KeyCode.AltGr, KeyCode.Help, KeyCode.Print, KeyCode.SysReq, KeyCode.Break, KeyCode.Menu, KeyCode.Keypad0, KeyCode.Alpha0, KeyCode.Keypad1,
            KeyCode.Alpha1, KeyCode.Keypad2, KeyCode.Alpha2, KeyCode.Keypad3, KeyCode.Alpha3, KeyCode.Keypad4, KeyCode.Alpha4, KeyCode.Keypad5, KeyCode.Alpha5, KeyCode.Keypad6,
            KeyCode.Alpha6, KeyCode.Keypad7, KeyCode.Alpha7, KeyCode.Keypad8, KeyCode.Alpha8, KeyCode.Keypad9, KeyCode.Alpha9, KeyCode.KeypadPeriod, KeyCode.Period, KeyCode.KeypadDivide,
            KeyCode.Slash, KeyCode.KeypadMultiply, KeyCode.Asterisk, KeyCode.KeypadMinus, KeyCode.Minus, KeyCode.KeypadPlus, KeyCode.Plus, KeyCode.KeypadEnter, KeyCode.KeypadEquals,
            KeyCode.Equals, KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.Exclaim, KeyCode.DoubleQuote, KeyCode.Hash, KeyCode.Dollar, KeyCode.Percent,
            KeyCode.Ampersand, KeyCode.Quote, KeyCode.LeftParen, KeyCode.RightParen, KeyCode.Comma, KeyCode.Colon, KeyCode.Semicolon, KeyCode.Less, KeyCode.Greater, KeyCode.Question,
            KeyCode.At, KeyCode.LeftBracket, KeyCode.Backslash, KeyCode.RightBracket, KeyCode.Caret, KeyCode.Underscore, KeyCode.BackQuote, KeyCode.LeftCurlyBracket, KeyCode.Pipe,
            KeyCode.RightCurlyBracket, KeyCode.Tilde, KeyCode.RightShift, KeyCode.LeftShift, KeyCode.RightControl, KeyCode.LeftControl, KeyCode.RightAlt, KeyCode.LeftAlt,
            KeyCode.LeftCommand, KeyCode.LeftWindows, KeyCode.RightCommand, KeyCode.RightWindows };
        foreach (KeyCode validKey in validKeys) if (Input.GetKeyDown(validKey)) return validKey;
        return KeyCode.None;
    }
    private void AssignKey(int n, KeyCode key)
    {
        if (n < 0 || n >= KeyNumber)
        {
            Debug.LogWarning(String.Format("AssignKey: invalid number {0}", n));
            return;
        }
        assinedKeys[n, 0] = key;
        assinedKeys[n, 1] = GetPairKey(key);
        keys[n].ChangeText(GetControlName(n) + ": " + GetKeyName(key));
    }

    // call these function only in Update()
    public static bool GetNumberKeyPressed()
    {
        return Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Keypad3) ||
            Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Keypad7) ||
            Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Alpha5) ||
            Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Alpha9);
    }

    public static bool GetSelect(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[0, 0]) || Input.GetKeyDown(assinedKeys[0, 1]) : Input.GetKey(assinedKeys[0, 0]) || Input.GetKey(assinedKeys[0, 1]); }
    public static bool GetMenu(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[1, 0]) || Input.GetKeyDown(assinedKeys[1, 1]) : Input.GetKey(assinedKeys[1, 0]) || Input.GetKey(assinedKeys[1, 1]); }
    public static bool GetCancel(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[1, 0]) || Input.GetKeyDown(assinedKeys[1, 1]) : Input.GetKey(assinedKeys[1, 0]) || Input.GetKey(assinedKeys[1, 1]); }
    public static bool GetUp(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[2, 0]) || Input.GetKeyDown(assinedKeys[2, 1]) : Input.GetKey(assinedKeys[2, 0]) || Input.GetKey(assinedKeys[2, 1]); }
    public static bool GetLeft(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[3, 0]) || Input.GetKeyDown(assinedKeys[3, 1]) : Input.GetKey(assinedKeys[3, 0]) || Input.GetKey(assinedKeys[3, 1]); }
    public static bool GetDown(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[4, 0]) || Input.GetKeyDown(assinedKeys[4, 1]) : Input.GetKey(assinedKeys[4, 0]) || Input.GetKey(assinedKeys[4, 1]); }
    public static bool GetRight(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[5, 0]) || Input.GetKeyDown(assinedKeys[5, 1]) : Input.GetKey(assinedKeys[5, 0]) || Input.GetKey(assinedKeys[5, 1]); }
    public static bool GetClockwise(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[6, 0]) || Input.GetKeyDown(assinedKeys[6, 1]) : Input.GetKey(assinedKeys[6, 0]) || Input.GetKey(assinedKeys[6, 1]); }
    public static bool GetCounterclockwise(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[7, 0]) || Input.GetKeyDown(assinedKeys[7, 1]) : Input.GetKey(assinedKeys[7, 0]) || Input.GetKey(assinedKeys[7, 1]); }
    public static bool GetUndo(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[8, 0]) || Input.GetKeyDown(assinedKeys[8, 1]) : Input.GetKey(assinedKeys[8, 0]) || Input.GetKey(assinedKeys[8, 1]); }
    public static bool GetReset(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[9, 0]) || Input.GetKeyDown(assinedKeys[9, 1]) : Input.GetKey(assinedKeys[9, 0]) || Input.GetKey(assinedKeys[9, 1]); }
    public static bool GetSwitch(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[10, 0]) || Input.GetKeyDown(assinedKeys[10, 1]) : Input.GetKey(assinedKeys[10, 0]) || Input.GetKey(assinedKeys[10, 1]); }
    public static bool GetPlayerUp(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[11, 0]) || Input.GetKeyDown(assinedKeys[11, 1]) : Input.GetKey(assinedKeys[11, 0]) || Input.GetKey(assinedKeys[11, 1]); }
    public static bool GetPlayerLeft(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[12, 0]) || Input.GetKeyDown(assinedKeys[12, 1]) : Input.GetKey(assinedKeys[12, 0]) || Input.GetKey(assinedKeys[12, 1]); }
    public static bool GetPlayerDown(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[13, 0]) || Input.GetKeyDown(assinedKeys[13, 1]) : Input.GetKey(assinedKeys[13, 0]) || Input.GetKey(assinedKeys[13, 1]); }
    public static bool GetPlayerRight(bool pressedDown = true)
    { return pressedDown ? Input.GetKeyDown(assinedKeys[14, 0]) || Input.GetKeyDown(assinedKeys[14, 1]) : Input.GetKey(assinedKeys[14, 0]) || Input.GetKey(assinedKeys[14, 1]); }

    public static bool GetActionKeyPressed()
    {
        return GetSwitch() || GetClockwise() || GetCounterclockwise() || GetNumberKeyPressed() || GetPlayerUp() || GetPlayerRight() || GetPlayerDown() || GetPlayerLeft();
    }

    public static bool GetCommandKeyPressed()
    {
        return GetUndo() || GetReset();
    }

    public static bool GetAnyKeyPressed()
    {
        return GetActionKeyPressed() || GetCommandKeyPressed();
    }
}
