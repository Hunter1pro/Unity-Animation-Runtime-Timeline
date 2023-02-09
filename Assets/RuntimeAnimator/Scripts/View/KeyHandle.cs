using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyHandle : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private RectTransform _rectTransform;
    public RectTransform RectTransform => _rectTransform;

    private event Action OnPointerDown;

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        OnPointerDown?.Invoke();
    }

    public void SubscribePointerDown(Action action)
    {
        OnPointerDown += action;
    }

    public void UnsubscribePointerDown(Action action)
    {
        OnPointerDown -= action;
    }
}
