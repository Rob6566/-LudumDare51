using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    private BattlefieldObject attachedObject;
    private GameManager gameManager;
    private Vector3 positionBeforeDrag;
    //private GameObject hoverObject;
    
    private Transform dragPreviousTransform;
    float offsetX;
    float offsetY;

    public void init(BattlefieldObject newObject, GameManager newGameManager) {
        attachedObject=newObject;
        gameManager=newGameManager;
    }

    public bool allowedToDrag() {
        if (attachedObject.rootGameObject.transform.parent.parent.gameObject.tag=="UnplacedTowerContainer") {
            return true;
        }
        return false;
    }


    public void OnPointerClick(PointerEventData eventData) {  
        if (!allowedToDrag()) {
            return;
        }

        gameManager.selectedTower=attachedObject;
    }


    /************************* 1. DRAGGING *************************/
    public void OnBeginDrag(PointerEventData eventData) {
        if (!allowedToDrag()) {
            return;
        }

        //gameManager.hoverManager.hideCardHover();

        //var clickedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //TODO - drag point should be click point (I can't figure this out)
        offsetX = 0;//65;//eventData.position.x - clickedPos.x;
        offsetY = 0;//85;//eventData.position.y - clickedPos.y;

        //Canvas activeCanvas=gameManager.canvasManager.getActiveCanvas();
        //GameObject cardUI=attachedCard.cardUI;

        attachedObject.rootGameObject.GetComponent<CanvasGroup>().blocksRaycasts=false;
        
        dragPreviousTransform=attachedObject.rootGameObject.transform.parent;
        gameManager.selectedTower=attachedObject;

        positionBeforeDrag=attachedObject.rootGameObject.transform.position;
    }

    //Dragging
    public void OnDrag(PointerEventData eventData) {
        if (!allowedToDrag()) {
            return;
        }
        Vector3 position=eventData.position;
        position.z=gameManager.canvas.planeDistance;
        position.x+=offsetX;
        position.y+=offsetY;
        this.transform.position = Camera.main.ScreenToWorldPoint(position);
    }

    //Dragging
    public void OnEndDrag(PointerEventData eventData) {
        if (!allowedToDrag()) {
            return;
        }

        this.transform.position=positionBeforeDrag;

       // GameObject cardUI=attachedCard.cardUI;
        attachedObject.rootGameObject.GetComponent<CanvasGroup>().blocksRaycasts=true;
        //gameManager.deckManager[1].refreshHandGroupUI();
    }




    

     /************************* 3. HOVERING *************************/
    public void OnPointerEnter(PointerEventData eventData) {
     //   gameManager.hoverManager.showCardHover(attachedCard);
    }

    public void OnPointerExit(PointerEventData eventData) {
     //   gameManager.hoverManager.hideCardHover();
    }
}
