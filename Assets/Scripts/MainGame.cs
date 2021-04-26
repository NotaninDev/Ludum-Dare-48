using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using String = System.String;

public class MainGame : MonoBehaviour
{
    private UnityEvent mainEvent;
    private static GameState gameState;
    private enum GameState
    {
        Prepare,
        Ready,
        Move,
        Finish
    }

    private static int level;
    private static string levelTag;
    private static GameObject mapObject;
    public static Map map;
    public static MapData mapData, initialState;
    public const int LevelCount = 7;
    public const float MoveDuration = .3f;

    private static int moveCount, totalMoveCount;

    private static KeyCode previousInput;
    private static IEnumerator waiting;

    private static GameObject mapParent;
    private const int optionCount = 5;
    private static GameObject[] optionObjects;
    private static Option[] options;
    private static GameObject menuObject;
    private static Menu menu;

    static MainGame()
    {
        mapData = new MapData();
    }

    void Awake()
    {
        mapObject = General.AddChild(gameObject, "Map");
        mapParent = General.AddChild(gameObject, "Map Parent");
        map = mapObject.AddComponent<Map>();
        mapParent.transform.localPosition = Vector3.zero;
        mapParent.transform.localScale = new Vector3(1, 1, 1);
        optionObjects = new GameObject[optionCount];
        options = new Option[optionCount];
        for (int i = 0; i < optionCount; i++)
        {
            optionObjects[i] = General.AddChild(gameObject, "Message Box" + i.ToString());
            options[i] = optionObjects[i].AddComponent<Option>();
        }
        menuObject = General.AddChild(gameObject, "Menu");
        menu = menuObject.AddComponent<Menu>();

        level = GameManager.level;
        levelTag = MapData.GetLevelTag(level);
        mapData.Initialize(levelTag);
        initialState = mapData.Clone();

        mainEvent = new UnityEvent();
        mainEvent.AddListener(ChangeState);
    }
    void Start()
    {
        for (int i = 0; i < optionCount; i++) optionObjects[i].SetActive(false);
        options[0].Initialize("UI", 0, Graphics.optionBox[0], 1.2f, 1f, 1, null, Graphics.Font.Mops, 6f, Graphics.Green, new Vector2(.6f, .12f), false, lineSpacing: -6f);
        options[1].Initialize("Message", 0, null, 1f, 1f, 1, null, Graphics.Font.Mops, 4.8f, Graphics.White,
            Vector2.zero, false, lineSpacing: -6f, alignment: TextAlignmentOptions.MidlineLeft);
        menuObject.SetActive(false);
        menu.Initialize();

        gameState = GameState.Ready;
        map.Initialize(mapData);
        moveCount = 0;
        totalMoveCount = 0;
        History.Initialize();
        InitializeHistoryTurn();
        options[0].ChangeText(MapData.GetLevelName(levelTag));

        optionObjects[0].SetActive(true);
        optionObjects[0].transform.localPosition = new Vector3(-8.8f + options[0].Size.x * 1.2f / 2, 5.59f, 0);
        StartCoroutine(Graphics.SlowDownMove(optionObjects[0], optionObjects[0].transform.localPosition, new Vector3(-8.8f + options[0].Size.x * 1.2f / 2, 4.43f, 0), .6f, .25f, delay: .9f));
        switch (levelTag)
        {
            case "1":
            default:
                options[1].ChangeText("Arrows: Move" + Environment.NewLine + "Z: Undo" + Environment.NewLine + "R: Reset");
                optionObjects[1].transform.localPosition = new Vector3(-7.41f, -4.13f, 0);
                optionObjects[1].SetActive(true);
                break;
        }
    }

    void Update()
    {
        if (SceneLoader.Loading) return;

        // go back to the main menu
        bool noMoreInput = false;
        if (!menuObject.activeInHierarchy && Keyboard.GetMenu()) menuObject.SetActive(!menuObject.activeSelf);
        else if (menuObject.activeInHierarchy)
        {
            menu.HandleInput();
            noMoreInput = true;
        }

        switch (gameState)
        {
            case GameState.Ready:
                if (menuObject.activeInHierarchy) break;
                float wait = -1;
                if (!noMoreInput && Keyboard.GetReset() && moveCount > 0)
                {
                    RegisterHistory(mapData, initialState);
                    History.AddHistory(History.Type.MoveCount, moveCount);
                    mapData.Reset(initialState);
                    map.Reset();
                    moveCount = 0;
                    InitializeHistoryTurn();
                    noMoreInput = true;
                }
                if (!noMoreInput && Keyboard.GetUndo() && totalMoveCount > 0)
                {
                    if (moveCount > 0)
                    {
                        moveCount--;
                        totalMoveCount--;
                    }
                    History.RollBack();
                    InitializeHistoryTurn();
                    noMoreInput = true;
                }
                if (!noMoreInput && Keyboard.GetPlayerUp())
                {
                    MapData tempMap = mapData.Clone();
                    if (mapData.Move(MapData.Direction.Up))
                    {
                        moveCount++;
                        totalMoveCount++;
                        RegisterHistory(tempMap, mapData);
                        InitializeHistoryTurn();
                    }
                    wait = map.Move(tempMap, MapData.Direction.Up) + .02f;
                    gameState = GameState.Move;
                    previousInput = KeyCode.UpArrow;
                    noMoreInput = true;
                }
                if (!noMoreInput && Keyboard.GetPlayerRight())
                {
                    MapData tempMap = mapData.Clone();
                    if (mapData.Move(MapData.Direction.Right))
                    {
                        moveCount++;
                        totalMoveCount++;
                        RegisterHistory(tempMap, mapData);
                        InitializeHistoryTurn();
                    }
                    wait = map.Move(tempMap, MapData.Direction.Right) + .02f;
                    gameState = GameState.Move;
                    previousInput = KeyCode.RightArrow;
                    noMoreInput = true;
                }
                if (!noMoreInput && Keyboard.GetPlayerDown())
                {
                    MapData tempMap = mapData.Clone();
                    if (mapData.Move(MapData.Direction.Down))
                    {
                        moveCount++;
                        totalMoveCount++;
                        RegisterHistory(tempMap, mapData);
                        InitializeHistoryTurn();
                    }
                    wait = map.Move(tempMap, MapData.Direction.Down) + .02f;
                    gameState = GameState.Move;
                    previousInput = KeyCode.DownArrow;
                    noMoreInput = true;
                }
                if (!noMoreInput && Keyboard.GetPlayerLeft())
                {
                    MapData tempMap = mapData.Clone();
                    if (mapData.Move(MapData.Direction.Left))
                    {
                        moveCount++;
                        totalMoveCount++;
                        RegisterHistory(tempMap, mapData);
                        InitializeHistoryTurn();
                    }
                    wait = map.Move(tempMap, MapData.Direction.Left) + .02f;
                    gameState = GameState.Move;
                    previousInput = KeyCode.LeftArrow;
                    noMoreInput = true;
                }
                if (wait > 0)
                {
                    waiting = General.WaitEvent(mainEvent, wait);
                    StartCoroutine(waiting);
                }
                break;
            case GameState.Move:
                if (false && Keyboard.GetAnyKeyPressed() && !mapData.Win())
                {
                    if (waiting != null) StopCoroutine(waiting);
                    switch (previousInput)
                    {
                        //////////////////////////////////////////////
                        //                                          //
                        // depending previous input stop animations //
                        //                                          //
                        //////////////////////////////////////////////
                        case KeyCode.UpArrow:
                        case KeyCode.RightArrow:
                        case KeyCode.DownArrow:
                        case KeyCode.LeftArrow:
                        default:
                            Debug.LogWarning(String.Format("Updata-Move: not implemented for KeyCode {0}", previousInput));
                            break;
                    }
                    gameState = GameState.Ready;
                    goto case GameState.Ready;
                }
                break;
            case GameState.Finish:
                break;
            default:
                Debug.LogWarning(String.Format("Update: not implemented for type {0}", Enum.GetNames(typeof(GameState))[(int)gameState]));
                break;
        }
    }

    private IEnumerator EndGame()
    {
        General.AddSolvedLevel(levelTag);
        yield return new WaitForSeconds(.8f);
        level++;
        GameManager.level++;
        if (CutScene.GetEventNumber(level) >= 0)
        {
            GameManager.eventNumber = CutScene.GetEventNumber(level);
            SceneLoader.sceneEvent.Invoke("EndScene");
        }
        else if (level < LevelCount)
        {
            SceneLoader.sceneEvent.Invoke("MainScene");
        }
        else
        {
            GameManager.level = LevelCount - 1;
            SceneLoader.sceneEvent.Invoke("TitleScene");
        }
    }
    private void ChangeState()
    {
        switch (gameState)
        {
            case GameState.Prepare:
                gameState = GameState.Ready;
                break;

            case GameState.Move:
                if (mapData.Win())
                {
                    StartCoroutine(EndGame());
                    gameState = GameState.Finish;
                }
                else
                {
                    map.StopAnimation();
                    gameState = GameState.Ready;
                }
                break;

            default:
                Debug.LogWarning(String.Format("ChangeState: not implemented yet for state {0}", Enum.GetNames(typeof(GameState))[(int)gameState]));
                break;
        }
    }

    private static void InitializeHistoryTurn()
    {
        History.StartTurn();
    }
    private static void RegisterHistory(MapData oldMap, MapData newMap)
    {
        History.AddHistory(History.Type.Player, oldMap.Player, oldMap.PlayerDepth);
        for (int i = 0; i < oldMap.Edges.Count; i++)
        {
            if (oldMap.Edges[i].Position != newMap.Edges[i].Position || oldMap.Edges[i].Depth != newMap.Edges[i].Depth || oldMap.Edges[i].Active != newMap.Edges[i].Active)
            {
                History.AddHistory(History.Type.Edge, i, oldMap.Edges[i].Position, oldMap.Edges[i].Depth, oldMap.Edges[i].Active);
            }
        }
        for (int i = 0; i < oldMap.Size.x; i++)
        {
            for (int j = 0; j < oldMap.Size.y; j++)
            {
                if (oldMap.Depth[i, j] != newMap.Depth[i, j]) History.AddHistory(History.Type.Tile, new Vector2Int(i, j), oldMap.Depth[i, j]);
            }
        }
        for (int i = 0; i < oldMap.Targets.Count; i++)
        {
            if (oldMap.Depth[oldMap.Targets[i].Position.x, oldMap.Targets[i].Position.y] != newMap.Depth[oldMap.Targets[i].Position.x, oldMap.Targets[i].Position.y])
            {
                History.AddHistory(History.Type.Target, i, oldMap.Depth[oldMap.Targets[i].Position.x, oldMap.Targets[i].Position.y]);
            }
        }
    }
    public static void RollBack(History.HistoryUnit unit)
    {
        switch (unit.type)
        {
            case History.Type.Player:
                mapData.Player = unit.position;
                mapData.PlayerDepth = unit.depth;
                map.MovePlayer();
                break;
            case History.Type.Edge:
                mapData.Edges[unit.target].ChangePosition(unit.position);
                mapData.Edges[unit.target].ChangeDepth(unit.depth);
                mapData.Edges[unit.target].SetActive(unit.flag);
                map.MoveEdge(unit.target);
                break;
            case History.Type.Tile:
                mapData.Depth[unit.position.x, unit.position.y] = unit.depth;
                map.MoveTile(unit.position.x, unit.position.y);
                break;
            case History.Type.Target:
                map.MoveTarget(unit.target, unit.depth);
                break;
            case History.Type.MoveCount:
                moveCount = unit.target;
                break;
            default:
                Debug.LogWarning(String.Format("RollBack: not implemented for type {0}", Enum.GetNames(typeof(History.Type))[(int)unit.type]));
                break;
        }
    }
}
