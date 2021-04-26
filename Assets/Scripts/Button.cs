using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class Button : MonoBehaviour
{
    private bool selected;
    public bool Selected { get { return selected; } }

    private SpriteRenderer buttonRenderer;
    private BoxCollider2D buttonCollider;
    private MouseCollider mouse;
    private float defaultScale, expansionScale;
    private UnityEvent clickEvent;

    void Awake()
    {
        buttonRenderer = gameObject.AddComponent<SpriteRenderer>();
        buttonCollider = gameObject.AddComponent<BoxCollider2D>();
        buttonCollider.enabled = false;
        mouse = gameObject.AddComponent<MouseCollider>();
        mouse.Initialize(buttonCollider);
    }

    public void Initialize(string sortingLayerName, int sortingOrder, Sprite sprite, Vector2 size, float defaultScale, float scale, UnityEvent clickEvent)
    {
        selected = false;
        buttonRenderer.sortingLayerName = sortingLayerName;
        buttonRenderer.sortingOrder = sortingOrder;
        buttonRenderer.sprite = sprite;
        buttonCollider.size = size;
        this.defaultScale = defaultScale;
        if (scale < 1)
        {
            Debug.LogWarning(String.Format("Initialize: scale {0} must not be smaller than 1", scale));
            scale = 1;
        }
        this.expansionScale = scale;
        this.clickEvent = clickEvent;

        transform.localScale = new Vector3(defaultScale, defaultScale, 1);
    }
    public void ChangeSortingOrder(int sortingOrder) { buttonRenderer.sortingOrder = sortingOrder; }
    public void ChangeAlpha(byte alpha)
    {
        buttonRenderer.color = new Color32((byte)(buttonRenderer.color.r * 255), (byte)(buttonRenderer.color.g * 255), (byte)(buttonRenderer.color.b * 255), alpha);
    }
    public void ChangeColor(Color32 color) { buttonRenderer.color = color; }
    public void ChangeSprite(Sprite sprite) { buttonRenderer.sprite = sprite; }

    public void Activate(bool b)
    {
        buttonCollider.enabled = b;
        selected = false;
    }
    public void Select() { selected = true; }

    void Update()
    {
        if (mouse.GetMouseEnter()) { gameObject.transform.localScale = new Vector3(defaultScale * expansionScale, defaultScale * expansionScale, 1); }
        if (mouse.GetMouseExit()) { gameObject.transform.localScale = new Vector3(defaultScale, defaultScale, 1); }
        if (mouse.GetMouseRelease())
        {
            selected = true;
            gameObject.transform.localScale = new Vector3(defaultScale, defaultScale, 1);
            buttonCollider.enabled = false;
            clickEvent.Invoke();
        }
    }
}
