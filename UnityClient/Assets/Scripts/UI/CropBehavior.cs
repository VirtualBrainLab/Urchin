using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CropBehavior : MonoBehaviour, IPointerDownHandler
{

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("clicked");
        Debug.Log(eventData.position);
        //Debug.Log(eventData.)
    }

}
