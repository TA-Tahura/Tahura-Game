using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        SetAlpha(0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetAlpha(1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetAlpha(0);
    }

    private void SetAlpha(float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}