using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class ClickLogger : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData e) { Debug.Log("CLICKED: " + name); }
}
