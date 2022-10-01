using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleMapTileHandler : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    /************************* 1. DRAGGING *************************/
    public void OnPointerEnter(PointerEventData eventData) {
        GameManager gameManager = GameObject.FindObjectOfType(typeof(GameManager)) as GameManager;
        //Debug.Log("Dragging over tile "+gameObject.transform.GetSiblingIndex());
    }

    public void OnPointerExit(PointerEventData eventData) {

    }

    public void OnPointerClick(PointerEventData eventData) {    
        GameManager gameManager = GameObject.FindObjectOfType(typeof(GameManager)) as GameManager;
        gameManager.playObjectInSlot(gameObject.transform.GetSiblingIndex());
    }
    
    
    public void OnDrop(PointerEventData eventData) {
        GameManager gameManager = GameObject.FindObjectOfType(typeof(GameManager)) as GameManager;
        gameManager.playObjectInSlot(gameObject.transform.GetSiblingIndex());
        Debug.Log("Dropped in slot "+gameObject.transform.GetSiblingIndex());
    }
}
