using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public const float CellWidth = 1.2f, CellHeight = 1.2f, ExpansionRate = 1.01f, DepthRate = .95f;
    public const int SortingOrderPerLayer = -15;
    public static Vector3 vectorX, vectorY, mapOffset, targetOffset;
    private static Vector2Int[] directionDictionary;
    public MapData mapData;
    private string levelTag;

    private GameObject playerObject;
    private SpriteBox playerSprite;
    private List<GameObject> tileObjects;
    private List<SpriteBox> tileSprites;
    private GameObject[] targetObjects, rockObjects, edgeObjects;
    private Target[] targets;
    private SpriteBox[] rockSprits, edgeSprites;
    private List<GameObject> layerObjects;
    private List<Layer> layers;

    private const float MoveTime = .1f, FallTime = .1f, DigTime = .3f, stuckTime = .1f;
    private IEnumerator fallAnimationManager;
    private IEnumerator[] moveAnimations, playerFallAnimations, edgeFallAnimations, targetFallAnimations, rockFallAnimations;
    private IEnumerator[,,] tileFallAnimations;

    // Scale is the scale of the map
    public Vector3 DefaultPosition { get; private set; }
    public float Scale { get; private set; }

    public enum Direction
    {
        Up, Right, Down, Left
    }

    static Map()
    {
        vectorX = new Vector3(CellWidth, 0, 0);
        vectorY = new Vector3(0, CellHeight, 0);
        targetOffset = new Vector3(0, -.3f, 0);
        directionDictionary = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
    }

    void Awake()
    {
        mapData = new MapData();

        layerObjects = new List<GameObject>();
        layerObjects.Add(General.AddChild(gameObject, "Layer0"));
        layers = new List<Layer>();
        layers.Add(layerObjects[0].AddComponent<Layer>());

        playerObject = General.AddChild(layerObjects[0], "Player");
        playerSprite = playerObject.AddComponent<SpriteBox>();

        tileObjects = new List<GameObject>();
        tileSprites = new List<SpriteBox>();
    }

    private void AddLayers(int bottomLayer)
    {
        if (bottomLayer <= 0)
        {
            //Debug.LogWarning($"Map.AddLayers: bottomLayer ({bottomLayer}) must be positive");
            return;
        }
        for (int i = layerObjects.Count; layerObjects.Count <= bottomLayer; i++)
        {
            layerObjects.Add(General.AddChild(layerObjects[i - 1], "Layer" + i));
            layers.Add(layerObjects[i].AddComponent<Layer>());
            layers[i].Initialize(i, mapData.Size);
            layerObjects[i].transform.localScale = Vector3.one * DepthRate;
        }
    }

    public void Initialize(MapData mapData)
    {
        // load raw data
        this.mapData = mapData;
        levelTag = mapData.LevelTag;

        // set the map size and position
        float tempWidth = CellWidth * (mapData.Size.x + 2), tempHeight = CellHeight * (mapData.Size.y + 2);
        if (tempHeight / tempWidth > Graphics.ScreenRatio)
        {
            Scale = Graphics.Height / tempHeight;
        }
        else
        {
            Scale = Graphics.Width / tempWidth;
        }
        DefaultPosition = new Vector3(CellWidth * (mapData.Size.x - 1) * -.5f, CellHeight * ((mapData.Size.y - 1) * -.5f), 0);
        mapOffset = DefaultPosition;
        transform.localScale = new Vector3(Scale, Scale, 1);
        transform.localPosition = Vector3.zero;

        // initialize the player
        playerSprite.Initialize(Graphics.player[0], "Tile", 4, Get3DPoint(mapData.Player));

        // initialize the floor tiles
        layers[0].Initialize(0, mapData.Size);
        for (int j = 0; j < mapData.Size.y; j++)
        {
            for (int i = 0; i < mapData.Size.x; i++)
            {
                if (mapData.Depth[i, j] < 0) layers[0].SetActive(i, j, false);
            }
        }

        targetObjects = new GameObject[mapData.Targets.Count];
        targets = new Target[targetObjects.Length];
        for (int i = 0; i < targetObjects.Length; i++)
        {
            targetObjects[i] = General.AddChild(layerObjects[0], "Target" + i);
            targets[i] = targetObjects[i].AddComponent<Target>();
            targets[i].Initialize(mapData.Targets[i]);
            targetObjects[i].transform.localPosition = Get3DPoint(mapData.Targets[i].Position) + targetOffset;
        }

        rockObjects = new GameObject[mapData.Rocks.Count];
        rockSprits = new SpriteBox[rockObjects.Length];
        for (int i = 0; i < rockObjects.Length; i++)
        {
            rockObjects[i] = General.AddChild(layerObjects[0], "Rock" + mapData.Rocks[i]);
            rockSprits[i] = rockObjects[i].AddComponent<SpriteBox>();
            rockSprits[i].Initialize(Graphics.tile[2], "Tile", 2, Get3DPoint(mapData.Rocks[i]));
        }

        edgeObjects = new GameObject[mapData.Edges.Count];
        edgeSprites = new SpriteBox[edgeObjects.Length];
        for (int i = 0; i < edgeObjects.Length; i++)
        {
            edgeObjects[i] = General.AddChild(layerObjects[0], $"Edge {mapData.Edges[i].Position} {(mapData.Edges[i].Horizontal ? "H" : "V")}");
            edgeSprites[i] = edgeObjects[i].AddComponent<SpriteBox>();
            edgeSprites[i].Initialize(Graphics.tile[3], "Tile", 3, Get3DPoint(mapData.Edges[i]));
            edgeObjects[i].transform.localEulerAngles = mapData.Edges[i].Horizontal ? Vector3.zero : new Vector3(0, 0, 90);
        }

        moveAnimations = new IEnumerator[mapData.Edges.Count + 1];
        playerFallAnimations = new IEnumerator[3];
        edgeFallAnimations = new IEnumerator[mapData.Edges.Count * 3];
        targetFallAnimations = new IEnumerator[mapData.Targets.Count * 2];
        rockFallAnimations = new IEnumerator[mapData.Rocks.Count * 3];
        tileFallAnimations = new IEnumerator[mapData.Size.x, mapData.Size.y, 3];
    }
    // reset the level
    public void Reset()
    {
        MovePlayer();
        for (int i = 0; i < mapData.Edges.Count; i++) MoveEdge(i);
        for (int i = 0; i < mapData.Size.x; i++)
        {
            for (int j = 0; j < mapData.Size.y; j++)
            {
                MoveTile(i, j);
            }
        }
        for (int i = 0; i < mapData.Targets.Count; i++) MoveTarget(i, 0);
    }

    public float Move(MapData previousMap, MapData.Direction direction)
    {
        bool stuck = mapData.Player == previousMap.Player;
        moveAnimations[0] = stuck ? AnimateStuck() : Graphics.Move(playerObject, Get3DPoint(previousMap.Player), Get3DPoint(mapData.Player), MoveTime);
        for (int i = 0; i < mapData.Edges.Count; i++)
        {
            if (mapData.Edges[i].Position == previousMap.Edges[i].Position) moveAnimations[i + 1] = null;
            else moveAnimations[i + 1] = Graphics.Move(edgeObjects[i], Get3DPoint(previousMap.Edges[i]), Get3DPoint(mapData.Edges[i]), MoveTime);
        }
        foreach (IEnumerator animation in moveAnimations)
        {
            if (animation != null) StartCoroutine(animation);
        }
        if (mapData.Histories.Count > 0)
        {
            fallAnimationManager = AnimateFall(previousMap, direction);
            StartCoroutine(fallAnimationManager);
        }
        return stuck ? stuckTime : (MoveTime + FallTime * mapData.FallDistanceSum + DigTime * Math.Max(0, mapData.Histories.Count - 1));
    }

    private IEnumerator AnimateStuck()
    {
        playerSprite.spriteRenderer.sprite = Graphics.player[1];
        yield return new WaitForSeconds(stuckTime);
        playerSprite.spriteRenderer.sprite = Graphics.player[0];
    }
    private IEnumerator AnimateFall(MapData previousMap, MapData.Direction direction)
    {
        int tempId = General.rand.Next(1000);
        yield return new WaitForSeconds(MoveTime);
        for (int i = 0; i < previousMap.Edges.Count; i++)
        {
            if (mapData.MovedEdges[i])
            {
                previousMap.Edges[i].ChangePosition(previousMap.Edges[i].Position + directionDictionary[(int)direction]);
            }
        }
        while (mapData.Histories.Count > 0)
        {
            // fall animation
            MapData.MicroHistory microHistory = mapData.Histories.Dequeue();
            if (microHistory.PlayerFallDistance > 0)
            {
                AddLayers(previousMap.PlayerDepth + microHistory.PlayerFallDistance);
                playerObject.transform.parent = layerObjects[previousMap.PlayerDepth + microHistory.PlayerFallDistance].transform;
                playerSprite.spriteRenderer.sortingOrder = 5 + (previousMap.PlayerDepth + microHistory.PlayerFallDistance) * SortingOrderPerLayer;
                playerFallAnimations[0] = Graphics.Move(playerObject, Get3DPoint(mapData.Player) * (float)Math.Pow(DepthRate, -microHistory.PlayerFallDistance), Get3DPoint(mapData.Player),
                    FallTime * microHistory.PlayerFallDistance);
                playerFallAnimations[1] = Graphics.Resize(playerObject, Vector3.one * (float)Math.Pow(DepthRate, -microHistory.PlayerFallDistance), Vector3.one,
                    FallTime * microHistory.PlayerFallDistance);
                playerFallAnimations[2] = Graphics.ChangeColor(playerSprite.spriteRenderer, GetLayerShade(previousMap.PlayerDepth),
                    GetLayerShade(previousMap.PlayerDepth + microHistory.PlayerFallDistance), FallTime * microHistory.PlayerFallDistance);
                StartCoroutine(playerFallAnimations[0]);
                StartCoroutine(playerFallAnimations[1]);
                StartCoroutine(playerFallAnimations[2]);
            }
            for (int i = 0; i < previousMap.Edges.Count; i++)
            {
                if (microHistory.FallDistance[i] == 0) continue;
                AddLayers(previousMap.Edges[i].Depth + microHistory.FallDistance[i]);
                edgeObjects[i].transform.parent = layerObjects[previousMap.Edges[i].Depth + microHistory.FallDistance[i]].transform;
                edgeSprites[i].spriteRenderer.sortingOrder = 3 + (previousMap.Edges[i].Depth + microHistory.FallDistance[i]) * SortingOrderPerLayer;
                edgeFallAnimations[i * 3] = Graphics.Move(edgeObjects[i], Get3DPoint(previousMap.Edges[i]) * (float)Math.Pow(DepthRate, -microHistory.FallDistance[i]),
                    Get3DPoint(previousMap.Edges[i]), FallTime * microHistory.FallDistance[i]);
                edgeFallAnimations[i * 3 + 1] = Graphics.Resize(edgeObjects[i], Vector3.one * (float)Math.Pow(DepthRate, -microHistory.FallDistance[i]), Vector3.one,
                    FallTime * microHistory.FallDistance[i]);
                edgeFallAnimations[i * 3 + 2] = Graphics.ChangeColor(edgeSprites[i].spriteRenderer, GetLayerShade(previousMap.Edges[i].Depth),
                    GetLayerShade(previousMap.Edges[i].Depth + microHistory.FallDistance[i]), FallTime * microHistory.FallDistance[i]);
                StartCoroutine(edgeFallAnimations[i * 3]);
                StartCoroutine(edgeFallAnimations[i * 3 + 1]);
                StartCoroutine(edgeFallAnimations[i * 3 + 1]);
            }

            // update previousMap
            previousMap.PlayerDepth += microHistory.PlayerFallDistance;
            for (int i = 0; i < previousMap.Edges.Count; i++) previousMap.Edges[i].ChangeDepth(previousMap.Edges[i].Depth + microHistory.FallDistance[i]);

            yield return new WaitForSeconds(FallTime * microHistory.MaxFallDistance);

            // update edge sprites
            for (int i = 0; i < previousMap.Edges.Count; i++)
            {
                if (microHistory.Overlapped[i])
                {
                    edgeObjects[i].SetActive(false);
                }
                if (microHistory.UsedInLoop[i])
                {
                    edgeSprites[i].spriteRenderer.sprite = Graphics.tile[4];
                    edgeSprites[i].spriteRenderer.sortingOrder = 4 + SortingOrderPerLayer * previousMap.Edges[i].Depth;
                    edgeObjects[i].SetActive(false);
                }
            }

            // digging animation

            // animate player fall
            if (microHistory.Excavated[mapData.Player.x, mapData.Player.y])
            {
                AddLayers(previousMap.PlayerDepth + 1);
                playerSprite.spriteRenderer.sortingOrder = 5 + (previousMap.PlayerDepth + 1) * SortingOrderPerLayer;
                playerObject.transform.parent = layerObjects[previousMap.PlayerDepth + 1].transform;
                playerFallAnimations[0] = Graphics.Move(playerObject, Get3DPoint(mapData.Player) / DepthRate, Get3DPoint(mapData.Player), DigTime);
                playerFallAnimations[1] = Graphics.Resize(playerObject, Vector3.one / DepthRate, Vector3.one, DigTime);
                playerFallAnimations[2] = Graphics.ChangeColor(playerSprite.spriteRenderer, GetLayerShade(previousMap.PlayerDepth),
                    GetLayerShade(previousMap.PlayerDepth + 1), DigTime);
                StartCoroutine(playerFallAnimations[0]);
                StartCoroutine(playerFallAnimations[1]);
                StartCoroutine(playerFallAnimations[2]);
            }
            // animate tile fall
            for (int i = 0; i < previousMap.Size.x; i++)
            {
                for (int j = 0; j < previousMap.Size.y; j++)
                {
                    if (microHistory.Excavated[i, j])
                    {
                        AddLayers(previousMap.Depth[i, j] + 1);
                        layers[previousMap.Depth[i, j]].ChangeSprite(i, j, true);
                        layers[previousMap.Depth[i, j] + 1].ChangeSprite(i, j, false);
                        layers[previousMap.Depth[i, j] + 1].SetActive(i, j, true);
                        tileFallAnimations[i, j, 0] = Graphics.Move(layers[previousMap.Depth[i, j] + 1].TileObjects[i, j], Get3DPoint(new Vector2Int(i, j)) / DepthRate,
                            Get3DPoint(new Vector2Int(i, j)), DigTime);
                        tileFallAnimations[i, j, 1] = Graphics.Resize(layers[previousMap.Depth[i, j] + 1].TileObjects[i, j], Vector3.one / DepthRate, Vector3.one, DigTime);
                        tileFallAnimations[i, j, 2] = Graphics.ChangeColor(layers[previousMap.Depth[i, j] + 1].TileSprites[i, j].spriteRenderer, GetLayerShade(previousMap.Depth[i, j]),
                            GetLayerShade(previousMap.Depth[i, j] + 1), DigTime);
                        StartCoroutine(tileFallAnimations[i, j, 0]);
                        StartCoroutine(tileFallAnimations[i, j, 1]);
                        StartCoroutine(tileFallAnimations[i, j, 2]);
                    }
                    else
                    {
                        tileFallAnimations[i, j, 0] = null;
                        tileFallAnimations[i, j, 1] = null;
                        tileFallAnimations[i, j, 2] = null;
                    }
                }
            }
            // animate target fall
            for (int i = 0; i < previousMap.Targets.Count; i++)
            {
                int x = previousMap.Targets[i].Position.x, y = previousMap.Targets[i].Position.y;
                if (microHistory.Excavated[x, y])
                {
                    AddLayers(previousMap.Depth[x, y] + 1);
                    targetObjects[i].transform.parent = layerObjects[previousMap.Depth[x, y] + 1].transform;
                    targets[i].ChangeDepth(previousMap.Depth[x, y] + 1);
                    targetFallAnimations[i * 2] = Graphics.Move(targetObjects[i], (Get3DPoint(new Vector2Int(x, y)) + targetOffset) / DepthRate,
                        (Get3DPoint(new Vector2Int(x, y)) + targetOffset), DigTime);
                    targetFallAnimations[i * 2 + 1] = Graphics.Resize(targetObjects[i], Vector3.one / DepthRate, Vector3.one, DigTime);
                    StartCoroutine(targetFallAnimations[i * 2]);
                    StartCoroutine(targetFallAnimations[i * 2 + 1]);
                }
            }
            // animate rock fall
            for (int i = 0; i < previousMap.Rocks.Count; i++)
            {
                int x = previousMap.Rocks[i].x, y = previousMap.Rocks[i].y;
                if (microHistory.Excavated[x, y])
                {
                    AddLayers(previousMap.Depth[x, y] + 1);
                    rockObjects[i].transform.parent = layerObjects[previousMap.Depth[x, y] + 1].transform;
                    rockSprits[i].spriteRenderer.sortingOrder = 2 + (previousMap.Depth[x, y] + 1) * SortingOrderPerLayer;
                    rockFallAnimations[i * 3] = Graphics.Move(rockObjects[i], Get3DPoint(new Vector2Int(x, y)) / DepthRate, Get3DPoint(new Vector2Int(x, y)), DigTime);
                    rockFallAnimations[i * 3 + 1] = Graphics.Resize(rockObjects[i], Vector3.one / DepthRate, Vector3.one, DigTime);
                    rockFallAnimations[i * 3 + 2] = Graphics.ChangeColor(rockSprits[i].spriteRenderer, GetLayerShade(previousMap.Depth[x, y]),
                        GetLayerShade(previousMap.Depth[x, y] + 1), DigTime);
                    StartCoroutine(rockFallAnimations[i * 3]);
                    StartCoroutine(rockFallAnimations[i * 3 + 1]);
                    StartCoroutine(rockFallAnimations[i * 3 + 2]);
                }
            }
            // animate edge fall
            for (int i = 0; i < previousMap.Edges.Count; i++)
            {
                if (microHistory.FallInExcavation[i])
                {
                    AddLayers(previousMap.Edges[i].Depth + 1);
                    edgeObjects[i].transform.parent = layerObjects[previousMap.Edges[i].Depth + 1].transform;
                    edgeSprites[i].spriteRenderer.sortingOrder = 3 + (previousMap.Edges[i].Depth + 1) * SortingOrderPerLayer;
                    edgeFallAnimations[i * 3] = Graphics.Move(edgeObjects[i], Get3DPoint(previousMap.Edges[i]) / DepthRate, Get3DPoint(previousMap.Edges[i]), DigTime);
                    edgeFallAnimations[i * 3 + 1] = Graphics.Resize(edgeObjects[i], Vector3.one / DepthRate, Vector3.one, DigTime);
                    edgeFallAnimations[i * 3 + 2] = Graphics.ChangeColor(edgeSprites[i].spriteRenderer, GetLayerShade(previousMap.Edges[i].Depth),
                        GetLayerShade(previousMap.Edges[i].Depth + 1), DigTime);
                    StartCoroutine(edgeFallAnimations[i * 3]);
                    StartCoroutine(edgeFallAnimations[i * 3 + 1]);
                    StartCoroutine(edgeFallAnimations[i * 3 + 2]);
                }
            }

            yield return new WaitForSeconds(DigTime);

            // update previousMap
            if (microHistory.Excavated[mapData.Player.x, mapData.Player.y]) previousMap.PlayerDepth++;
            for (int i = 0; i < previousMap.Size.x; i++)
            {
                for (int j = 0; j < previousMap.Size.y; j++)
                {
                    if (microHistory.Excavated[i, j]) previousMap.Depth[i, j]++;
                }
            }
            for (int i = 0; i < previousMap.Targets.Count; i++)
            {
                int x = previousMap.Targets[i].Position.x, y = previousMap.Targets[i].Position.y;
                if (microHistory.Excavated[x, y])
                {
                    targets[i].ChangeRelativeDepth(mapData.Targets[i].Depth - previousMap.Depth[x, y]);
                }
            }
            for (int i = 0; i < previousMap.Edges.Count; i++)
            {
                if (microHistory.UsedInLoop[i])
                {
                    previousMap.Edges[i].SetActive(false);
                    edgeObjects[i].SetActive(false);
                }
                if (microHistory.FallInExcavation[i])
                {
                    previousMap.Edges[i].ChangeDepth(previousMap.Edges[i].Depth + 1);
                }
            }
        }
    }

    // transform a 2D map point to a 3D world point
    public static Vector3 Get3DPoint(Vector2Int _2dPoint) { return vectorX * _2dPoint.x + vectorY * _2dPoint.y + mapOffset; }
    public static Vector3 Get3DPoint(Edge edge) { return vectorX * (edge.Position.x - (edge.Horizontal ? 0 : .5f)) + vectorY * (edge.Position.y - (edge.Horizontal ? .5f : 0)) + mapOffset; }
    private static Color32 GetLayerShade(int layer)
    {
        int r = 255, g = 141, b = 0, maxLayer = 6;
        double shade = .60;
        return new Color32((byte)(Math.Max(255 - (double)layer * (255 - r) / maxLayer, r) * Math.Max(1 - (double)layer * (1 - shade) / maxLayer, 0)),
            (byte)(Math.Max(255 - (double)layer * (255 - g) / maxLayer, g) * Math.Max(1 - (double)layer * (1 - shade) / maxLayer, 0)),
            (byte)(Math.Max(255 - (double)layer * (255 - b) / maxLayer, b) * Math.Max(1 - (double)layer * (1 - shade) / maxLayer, 0)), 255);
    }

    public void MovePlayer()
    {
        AddLayers(mapData.PlayerDepth);
        playerObject.transform.parent = layerObjects[mapData.PlayerDepth].transform;
        playerObject.transform.localPosition = Get3DPoint(mapData.Player);
        playerObject.transform.localScale = Vector3.one;
        playerSprite.spriteRenderer.color = GetLayerShade(mapData.PlayerDepth);
        playerSprite.spriteRenderer.sortingOrder = 5 + mapData.PlayerDepth * SortingOrderPerLayer;
    }
    public void MoveEdge(int index)
    {
        AddLayers(mapData.Edges[index].Depth);
        edgeObjects[index].transform.parent = layerObjects[mapData.Edges[index].Depth].transform;
        edgeObjects[index].transform.localPosition = Get3DPoint(mapData.Edges[index]);
        edgeObjects[index].transform.localScale = Vector3.one;
        edgeSprites[index].spriteRenderer.sprite = Graphics.tile[3];
        edgeSprites[index].spriteRenderer.color = GetLayerShade(mapData.Edges[index].Depth);
        edgeSprites[index].spriteRenderer.sortingOrder = 3 + mapData.Edges[index].Depth * SortingOrderPerLayer;
        edgeObjects[index].SetActive(mapData.Edges[index].Active);
    }
    public void MoveTile(int x, int y)
    {
        AddLayers(mapData.Depth[x, y]);
        for (int i = 0; i < mapData.Depth[x, y]; i++)
        {
            layers[i].ChangeSprite(x, y, true);
            layers[i].SetActive(x, y, true);
            layers[i].TileSprites[x, y].spriteRenderer.color = GetLayerShade(i);
        }
        layers[mapData.Depth[x, y]].ChangeSprite(x, y, false);
        layers[mapData.Depth[x, y]].SetActive(x, y, true);
        layers[mapData.Depth[x, y]].TileSprites[x, y].spriteRenderer.color = GetLayerShade(mapData.Depth[x, y]);

        for (int i = 0; i < mapData.Rocks.Count; i++)
        {
            if (mapData.Rocks[i].x == x && mapData.Rocks[i].y == y)
            {
                rockObjects[i].transform.parent = layerObjects[mapData.Depth[x, y]].transform;
                rockObjects[i].transform.localPosition = Get3DPoint(new Vector2Int(x, y));
                rockObjects[i].transform.localScale = Vector3.one;
                rockSprits[i].spriteRenderer.color = GetLayerShade(mapData.Depth[x, y]);
                rockSprits[i].spriteRenderer.sortingOrder = 2 + mapData.Depth[x, y] * SortingOrderPerLayer;
            }
        }
    }
    public void MoveTarget(int index, int depth)
    {
        AddLayers(depth);
        targetObjects[index].transform.parent = layerObjects[depth].transform;
        targetObjects[index].transform.localPosition = Get3DPoint(mapData.Targets[index].Position) + targetOffset;
        targetObjects[index].transform.localScale = Vector3.one;
        edgeSprites[index].spriteRenderer.color = GetLayerShade(mapData.Edges[index].Depth);
        targets[index].ChangeRelativeDepth(mapData.Targets[index].Depth - depth);
        targets[index].ChangeDepth(depth);
    }

    public void StopAnimation()
    {
        if (fallAnimationManager != null) StopCoroutine(fallAnimationManager);
        foreach (IEnumerator animation in moveAnimations)
        {
            if (animation != null) StopCoroutine(animation);
        }
        foreach (IEnumerator animation in playerFallAnimations)
        {
            if (animation != null) StopCoroutine(animation);
        }
        foreach (IEnumerator animation in edgeFallAnimations)
        {
            if (animation != null) StopCoroutine(animation);
        }
        foreach (IEnumerator animation in targetFallAnimations)
        {
            if (animation != null) StopCoroutine(animation);
        }
        foreach (IEnumerator animation in rockFallAnimations)
        {
            if (animation != null) StopCoroutine(animation);
        }
        for (int i = 0; i < mapData.Size.x; i++)
        {
            for (int j = 0; j < mapData.Size.y; j++)
            {
                if (tileFallAnimations[i, j, 0] != null) StopCoroutine(tileFallAnimations[i, j, 0]);
                if (tileFallAnimations[i, j, 1] != null) StopCoroutine(tileFallAnimations[i, j, 1]);
                if (tileFallAnimations[i, j, 2] != null) StopCoroutine(tileFallAnimations[i, j, 2]);
            }
        }

        // update objects' position
        MovePlayer();
        for (int i = 0; i < mapData.Edges.Count; i++) MoveEdge(i);
        for (int i = 0; i < mapData.Size.x; i++)
        {
            for (int j = 0; j < mapData.Size.y; j++)
            {
                MoveTile(i, j);
            }
        }
        for (int i = 0; i < mapData.Targets.Count; i++) MoveTarget(i, mapData.Depth[mapData.Targets[i].Position.x, mapData.Targets[i].Position.y]);
    }
}
