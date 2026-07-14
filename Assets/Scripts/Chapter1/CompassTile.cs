using UnityEngine;
using UnityEngine.EventSystems;

/// Tile arah mata angin yang bisa di-drag. target null = pengecoh (selalu salah).
public class CompassTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform target;      // slot yang benar (null = decoy)
    public float snapDistance = 130f; // dalam satuan kanvas referensi (1920x1080)

    [HideInInspector] public CompassMiniGame game;

    RectTransform rt;
    Vector2 homePos;
    bool locked;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        homePos = rt.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (locked || game == null || !game.InputEnabled) return;
        rt.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData e)
    {
        if (locked || game == null || !game.InputEnabled) return;
        // Screen-space overlay: posisi world = piksel layar, jadi cukup geser sebesar delta pointer.
        rt.position += (Vector3)e.delta;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (locked || game == null || !game.InputEnabled) return;

        float scale = GetCanvasScale();
        if (target != null &&
            Vector2.Distance(rt.position, target.position) <= snapDistance * scale)
        {
            // Snap ke slot: samakan posisi layar dengan slot di lengan kompas.
            var p = rt.position;
            p.x = target.position.x;
            p.y = target.position.y;
            rt.position = p;
            locked = true;
            game.NotifyPlaced();
        }
        else
        {
            // Salah tempat: balik ke posisi awal.
            bool moved = Vector2.Distance(rt.anchoredPosition, homePos) > 30f;
            rt.anchoredPosition = homePos;
            if (moved) game.NotifyWrong();
        }
    }

    float GetCanvasScale()
    {
        var canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.scaleFactor : 1f;
    }
}
