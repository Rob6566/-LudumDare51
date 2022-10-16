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
    public int towersInRange=0;
    public int enemiesInRange=0;

    public BattleMapTile animationMovedFrom;
    public float animationTimeRemaining;
    public bool lockColour=false;

    public void init(GameManager newGameManager, GameObject newGameObject, int newTilePosition) {
        gameManager=newGameManager;
        gameObject=newGameObject;
        tilePosition=newTilePosition;
        updateUI();
    }

    //TODO - needs to check that nothing's in the tile already
    public BattlefieldObject createObjectInTile(BattlefieldObjectSO so) {
        if (battlefieldObject!=null) {
            battlefieldObject.Destroy();
            battlefieldObject=null;
        }

        battlefieldObject=gameManager.spawnObjectFromSO(so);
        positionObject();
        return battlefieldObject;
    }

    //TODO - needs to check that nothing's in the tile already
    public bool moveObjectToTile(BattlefieldObject newObject) {
        battlefieldObject=newObject;
        positionObject();
        return true;
    }

    public void moveExistingObjectToAnotherTile(BattleMapTile newTile) {
        battlefieldObject.justMoved=true;
        newTile.moveObjectToTile(battlefieldObject);
        battlefieldObject=null;

        newTile.animationTimeRemaining=GameManager.ENEMY_ANIMATION_DURATION;
        newTile.animationMovedFrom=this;
        newTile.battlefieldObject.rootGameObject.transform.SetParent(gameManager.battlefieldIndicatorContainer.transform);
        newTile.Update(0f);

        updateUI();
    }


    //Runs every frame. Used to animate between tiles
    public void Update(float timeDelta) {
        if(animationTimeRemaining<0) {
            animationTimeRemaining=0;
            animationMovedFrom.lockColour=false;
            Transform objectTransform=battlefieldObject.rootGameObject.transform;
            objectTransform.SetParent(gameObject.transform);
            positionObject();
            animationMovedFrom.positionObject();
        }
        else if(animationTimeRemaining>0) {
            animationTimeRemaining-=timeDelta;
            Vector3 origin=animationMovedFrom.gameObject.transform.position;
            Vector3 destination=gameObject.transform.position;
            battlefieldObject.rootGameObject.transform.position=origin+((destination-origin)*((GameManager.ENEMY_ANIMATION_DURATION-animationTimeRemaining)/GameManager.ENEMY_ANIMATION_DURATION));
        }
    }

    public bool hasEnemyObject() {
        if (battlefieldObject==null) {
            return false;
        }

        return battlefieldObject.objectOwner==ObjectOwner.enemy;
    }
    

    public bool hasPlayerObject() {
        if (battlefieldObject==null) {
            return false;
        }

        return battlefieldObject.objectOwner==ObjectOwner.player;
    }

    public bool hasObject() {
        return !(battlefieldObject==null);
    }

    public void positionObject() {
        if (battlefieldObject==null) {
            return;
        }

        Transform objectTransform=battlefieldObject.rootGameObject.transform;
        objectTransform.SetParent(gameObject.transform);

        objectTransform.localPosition=new Vector3(0, 0, 0);
        objectTransform.localScale=new Vector3(1,1,1);

        battlefieldObject.tileID=tilePosition;

        updateUI();
    }

    public void updateUI() {
        Color32 tileColour= new Color32(180,180,180, 255);
        Color32 indicatorColour= new Color32(180,180,180, 255);
        if (battlefieldObject!=null) {
            if (lockColour) {
                return;
            }
            else if (battlefieldObject.objectOwner==ObjectOwner.enemy && animationTimeRemaining>0) {
                animationMovedFrom.gameObject.transform.GetChild(0).GetComponent<Image>().color=new Color32(255,105,105, 255);
                animationMovedFrom.lockColour=true;
            }
            else if (battlefieldObject.objectOwner==ObjectOwner.enemy) {
                tileColour=new Color32(255,105,105, 255);
            }
            else {
                tileColour=new Color32(103,244,103, 255);
            }
            indicatorColour=tileColour;
        }
        else {
            if (towersInRange>2) {
                indicatorColour=new Color32(103,244,103, 192);
            }
            else if(towersInRange>1) {
                indicatorColour=new Color32(103,244,103, 129);
            }
            else if(towersInRange>0) {
                indicatorColour=new Color32(103,244,103, 66);
            }
            else if(enemiesInRange>0) {
                indicatorColour=new Color32(255,105,105, 127);
            }
        }
        gameObject.GetComponent<Image>().color=tileColour;
        gameObject.transform.GetChild(0).GetComponent<Image>().color=indicatorColour;
    }

    //Get the x, y coords of the tile
    public Vector2 getTileCoords() {
        int x = tilePosition % 10; //CLEANUP
        int y = (int)Mathf.Floor((tilePosition)/10);
        return new Vector2(x, y);
    }

    public void takeDamage(int damage) {
        if (battlefieldObject==null) {
            return;
        }

        if (hasPlayerObject()) {
            gameManager.damageTaken+=damage;
        }
        else {
            gameManager.damageDealt+=damage;
        }

        battlefieldObject.takeDamage(damage);
        if (battlefieldObject.hp<=0) {
            if (hasPlayerObject()) {
                gameManager.towersLost++;
            }
            else {
                gameManager.kills++;
                gameManager.score+=gameManager.SCORE_PER_LEVEL[battlefieldObject.level];
            }
            battlefieldObject.Destroy();
            battlefieldObject=null;
            updateUI();
        }
    }

}