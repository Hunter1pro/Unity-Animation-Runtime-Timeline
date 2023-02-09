using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SourceHandle : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private Slider _slider;
    public Slider Slider => _slider;

    [SerializeField]
    private RectTransform _rectTransform;

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

    public void ResetHandle()
    {
        _slider.handleRect = _rectTransform;
    }
}
