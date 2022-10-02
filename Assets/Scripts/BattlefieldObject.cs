using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattlefieldObject
{
    private GameManager gameManager;
    public ObjectOwner objectOwner;
    public string name;
    public Sprite sprite;
    public int hp;
    public int maxHP;
    public int damage;
    public int level;
    public float range;
    public bool AOE;
    public int towerTypeID;

    public GameObject rootGameObject;
    public GameObject hpBar1;
    public GameObject hpBar2;
    public GameObject hpBar3;
    public GameObject hpBar4;
    public GameObject hpBar5;

    public Image image;
    TextMeshProUGUI txtHP;
    TextMeshProUGUI txtPower;
    public GameObject imgPower;
    
    public int tileID;

    //Tracks what the tile did in the latest action
    public bool justMoved=false;
    public bool justAttacked=false;
    public int assignedDamage=0;  //Used to track how much damage other towers have assigned to this object, a (fairly rudimentary) way to reduce overkill
    public int targetTile=-1;

    public List<int> tilesInRange = new List<int>();

    public void init(GameManager newGameManager, GameObject gameObject, GameObject container, BattlefieldObjectSO newBattlefieldObjectSO, TowerModifier towerModifier) {
        gameManager=newGameManager;
        hp=newBattlefieldObjectSO.hp;
        maxHP=newBattlefieldObjectSO.hp;
        name=newBattlefieldObjectSO.name;
        sprite=newBattlefieldObjectSO.sprite;
        damage=newBattlefieldObjectSO.damage;
        level=newBattlefieldObjectSO.level;
        level=newBattlefieldObjectSO.level;
        range=newBattlefieldObjectSO.range;
        AOE=newBattlefieldObjectSO.AOE;
        towerTypeID=newBattlefieldObjectSO.towerTypeID;
        objectOwner=newBattlefieldObjectSO.objectOwner;
        rootGameObject=gameObject;

        if (towerModifier!=null) {
            maxHP+=towerModifier.hp;
            hp+=towerModifier.hp;
            damage+=towerModifier.damage;
            range+=towerModifier.range;
        }

        

        image=rootGameObject.transform.GetComponent<Image>();
        imgPower =rootGameObject.transform.GetChild(1).gameObject;
        
        txtPower=rootGameObject.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        txtHP=rootGameObject.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

        image.sprite=newBattlefieldObjectSO.sprite;


        ObjectHandler eventHandler = rootGameObject.GetComponentInChildren<ObjectHandler>();
        eventHandler.init(this, gameManager);

        updateUI();
        //assignUIControls()
    }


    public void updateUI() {
        txtPower.text=(damage>0 ? damage.ToString() : "");
        txtHP.text=hp.ToString();
        imgPower.SetActive(damage>0);
    }

    //Calculate all the tiles in range of this tower. Fairly heavy and inefficient currently
    public void calculateTilesInRange() {

        if (objectOwner==ObjectOwner.enemy) {
            this.range=1.5f; //A range of 1.5 hits surrounding tiles
        }

        tilesInRange=new List<int>();

        Vector2 towerTileCoords=gameManager.battleMapTiles[this.tileID].getTileCoords();

        foreach(BattleMapTile thisTile in gameManager.battleMapTiles) {
            Vector2 thisTileCoords = thisTile.getTileCoords();
            float distance=Vector2.Distance(towerTileCoords, thisTileCoords);
            if(distance<=this.range) {
                tilesInRange.Add(thisTile.tilePosition);
                if (this.objectOwner==ObjectOwner.player) {
                    thisTile.towersInRange++;
                }
                else {
                    thisTile.enemiesInRange++;
                }
            }
        }
    }

    //Find a target. Since we automatically sort tiles left to right, bottom to top, we just attack the first one we find.
    public void acquireTarget() {
        targetTile=-1;
        if (AOE) {
            targetTile=999;
            return;
        }
        foreach(int tileID in tilesInRange) {
            BattleMapTile thisTile=gameManager.battleMapTiles[tileID];
            if (!thisTile.hasEnemyObject()) {
                continue;
            }
            if (thisTile.battlefieldObject.getHPAfterAssignedDamage()<=0) {
                continue;
            }
            targetTile=thisTile.tilePosition;

            //TODO - draw a line to indicate our attack

            Debug.Log("Target acquired - tower in "+tileID+" will attack "+targetTile);

            thisTile.battlefieldObject.assignedDamage+=this.damage;
            break;
        }
    }

    //Tower attack enemies
    public void damageTarget() {
        if (AOE) {
            Debug.Log(this.name+" tiles in range = "+tilesInRange.Count);
            foreach(int thisTileID in tilesInRange) {
                BattleMapTile thisTile=gameManager.battleMapTiles[thisTileID];
                if (!thisTile.hasEnemyObject()) {
                    continue;
                }

                this.justAttacked=true;
                thisTile.takeDamage(this.damage);
            }
        }
        else {
            if (targetTile<0) {
                return;
            }
            gameManager.battleMapTiles[targetTile].takeDamage(this.damage);
            this.justAttacked=true;    
        }
    }

    public void takeDamage(int damageToTake) {
        hp-=damageToTake;
        updateUI();
    }

    //Amount of damage that can still be assigned to this object before it's dead
    public int getHPAfterAssignedDamage() {
        return hp-assignedDamage;
    }

    public void Destroy() {
        UnityEngine.Object.Destroy(rootGameObject);
    }

    //Enemy attack towers
    public void attackTowers() {
        if (!(this.objectOwner==ObjectOwner.enemy)) {
            return;
        }

        foreach(int thisTileID in tilesInRange) {
            BattleMapTile thisTile=gameManager.battleMapTiles[thisTileID];
            if (!thisTile.hasPlayerObject()) {
                continue;
            }

            this.justAttacked=true;
            thisTile.takeDamage(this.damage);
        }
    }
}
