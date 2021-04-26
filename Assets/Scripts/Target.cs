using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Target : MonoBehaviour
{
    public class Data
    {
        public Vector2Int Position { get; private set; }
        public int Depth { get; private set; }

        public Data(Vector2Int position, int depth)
        {
            Position = position;
            Depth = depth;
        }
        public Data Clone()
        {
            return (Data)this.MemberwiseClone();
        }
    }

    private Option option;

    void Awake()
    {
        option = gameObject.AddComponent<Option>();
    }

    public void Initialize(Data data, int depth = 0)
    {
        option.Initialize("Tile", 1 + depth * Map.SortingOrderPerLayer, null, 1f, 1f, 1 + depth * Map.SortingOrderPerLayer, (data.Depth - depth).ToString(), Graphics.Font.Recurso, 9f, Graphics.Pink, Vector2.zero, false, alignment: TextAlignmentOptions.BaselineGeoAligned);
    }
    public void ChangeDepth(int depth)
    {
        option.ChangeSortingOrder(1 + depth * Map.SortingOrderPerLayer, 1 + depth * Map.SortingOrderPerLayer);
    }
    public void ChangeRelativeDepth(int depth)
    {
        option.ChangeText(depth.ToString());
    }
}
