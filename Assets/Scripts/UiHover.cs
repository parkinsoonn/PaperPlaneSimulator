using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Button scale: appear (open animations) x hover/press x idle pulse. Settles smoothly on unscaled time.
public class UiHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public float hoverScale = 1.05f;
    public float pressedScale = 0.95f;
    [NonSerialized] public float appear = 1f;
    [NonSerialized] public float pulse = 1f;

    Selectable selectable;
    bool over, down;
    float current = 1f;

    void Awake() => selectable = GetComponent<Selectable>();

    public void OnPointerEnter(PointerEventData e) => over = true;
    public void OnPointerExit(PointerEventData e) { over = false; down = false; }
    public void OnPointerDown(PointerEventData e) => down = true;
    public void OnPointerUp(PointerEventData e) => down = false;

    void OnDisable()
    {
        over = down = false;
        current = 1f;
    }

    void Update()
    {
        bool active = selectable == null || selectable.IsInteractable();
        float target = !active ? 1f : down ? pressedScale : over ? hoverScale : 1f;
        current = Mathf.MoveTowards(current, target, 1.5f * Time.unscaledDeltaTime);
        float s = appear * current * (active ? pulse : 1f);
        transform.localScale = new Vector3(s, s, 1f);
    }
}
