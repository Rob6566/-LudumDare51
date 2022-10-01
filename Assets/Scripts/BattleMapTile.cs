using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Internal class to store a map tile. We store adjacentToRooms, as we don't want to destoy these tiles when 
public class BattleMapTile {
    public int tilePosition;
    public BattlefieldObject battlefieldObject;
    private GameManager gameManager;
    public GameObject gameObject;

    public void init(GameManager newGameManager, GameObject newGameObject, int newTilePosition) {
        gameManager=newGameManager;
        gameObject=newGameObject;
        tilePosition=newTilePosition;
        updateUI();
    }

    public BattlefieldObject createObjectInTile(BattlefieldObjectSO so) {
        battlefieldObject=gameManager.spawnObjectFromSO(so);
        positionObject();
        return battlefieldObject;
    }

    public void positionObject() {
        Transform objectTransform=battlefieldObject.rootGameObject.transform;
        objectTransform.SetParent(gameObject.transform);

        objectTransform.localPosition=new Vector3(0, 0, 0);
        objectTransform.localScale=new Vector3(1,1,1);

        updateUI();
    }

    public void updateUI() {
        Debug.Log("UpdateUI for tile "+tilePosition);
        Color32 colour= new Color32(180,180,180, 255);
        if (battlefieldObject!=null) {
            if (battlefieldObject.objectOwner==ObjectOwner.enemy) {
                colour=new Color32(255,105,105, 255);
            }
            else {
                colour=new Color32(103,244,103, 255);
            }
        }
        gameObject.GetComponent<Image>().color=colour;
    }

}