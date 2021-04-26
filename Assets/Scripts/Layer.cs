using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layer : MonoBehaviour
{
    public int Depth { get; private set; }
    private Vector2Int size;
    private GameObject tileParent;
    public GameObject[,] TileObjects { get; private set; }
    public SpriteBox[,] TileSprites { get; private set; }

    void Awake()
    {
        tileParent = General.AddChild(gameObject, "Tile Parent");
    }

    public void Initialize(int depth, Vector2Int size)
    {
        Depth = depth;
        this.size = size;
        TileObjects = new GameObject[size.x, size.y];
        TileSprites = new SpriteBox[size.x, size.y];
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                TileObjects[i, j] = General.AddChild(tileParent, $"Tile({i}, {j})");
                TileSprites[i, j] = TileObjects[i, j].AddComponent<SpriteBox>();
                TileSprites[i, j].Initialize(Graphics.tile[(i + j) % 2], "Tile", Map.SortingOrderPerLayer * depth, Map.Get3DPoint(new Vector2Int(i, j)));
                TileObjects[i, j].SetActive(depth == 0);
            }
        }
    }

    public void ChangeSprite(int x, int y, bool isSide)
    {
        TileSprites[x, y].spriteRenderer.sprite = isSide ? Graphics.side : Graphics.tile[(x + y) % 2];
        TileSprites[x, y].spriteRenderer.sortingLayerName = isSide ? "Side" : "Tile";
        TileSprites[x, y].spriteRenderer.sortingOrder = isSide ? Depth : Map.SortingOrderPerLayer * Depth;
    }
    public void SetActive(int x, int y, bool isActive)
    {
        TileObjects[x, y].SetActive(isActive);
    }
}
