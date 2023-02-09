using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChouseAvatarBtn : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private TMP_Text text;
        
    public void SetText(string value)
    {
        this.text.text = value;
    }

    public void SubscribeAction(UnityAction action)
    {
        this.button.onClick.AddListener(action);
    }
}
