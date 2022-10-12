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
    public float projectileSpeed;

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
    public float targetTileDistance=0f;
    public int shootAnimationsInitiated;

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
        projectileSpeed=newBattlefieldObjectSO.projectileSpeed;
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


        if (towerTypeID==GameManager.TOWER_BALLISTA) {
            int intRange=(int)Mathf.Floor(this.range);
            for(int i=1; i<=intRange; i++) {
                int thisTileID=tileID+(i*10);
                if (thisTileID>99) {
                    break;
                }
                BattleMapTile thisTile=gameManager.battleMapTiles[thisTileID];
                thisTile.towersInRange++;
                tilesInRange.Add(thisTile.tilePosition);
            }

            return;
        }


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
        shootAnimationsInitiated=0;
        if (towerTypeID==GameManager.TOWER_BALLISTA) {
            foreach(int thisTileID in tilesInRange) {
                BattleMapTile thisTile=gameManager.battleMapTiles[thisTileID];
                if (!thisTile.hasEnemyObject()) {
                    continue;
                }
                thisTile.battlefieldObject.assignedDamage+=this.damage;
                targetTile=thisTileID;
            }
            targetTileDistance=tilesInRange.Count;
            return;
        }

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
            Vector2 towerTileCoords=gameManager.battleMapTiles[this.tileID].getTileCoords();
            targetTileDistance=Vector2.Distance(thisTile.getTileCoords(), towerTileCoords);

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
        else if (towerTypeID==GameManager.TOWER_INCOME) {
                GameObject textPopup = UnityEngine.Object.Instantiate(gameManager.textAlertPrefab);   
                textPopup.transform.SetParent(gameManager.battlefieldIndicatorContainer.transform);
                BattleMapTile originTile=gameManager.battleMapTiles[this.tileID];
                
                TextHandler textHandler = textPopup.GetComponent<TextHandler>();
                Vector2 tilePosition=originTile.getTileCoords();

                float resourcesGained=(tilePosition.y+1)*0.1f+gameManager.incomeTowerBonus;
                gameManager.gold+=resourcesGained;
                gameManager.science+=resourcesGained;
                gameManager.updateUI();
                textHandler.init(4f, originTile.gameObject.transform.position, gameManager, "+"+resourcesGained.ToString()+" gold/science"); 
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
        float scale=165f;
        float animationDuration=.4f;
        GameObject destroyAnimation = rootGameObject;

        if (objectOwner==ObjectOwner.enemy) {
            destroyAnimation=UnityEngine.Object.Instantiate(gameManager.enemyDestroyedPrefab);
        }
        else if (/*towerTypeID==1*/objectOwner==ObjectOwner.player) {
            destroyAnimation=UnityEngine.Object.Instantiate(gameManager.arrowDestroyedPrefab);
        }

        if (destroyAnimation!=rootGameObject) {
            destroyAnimation.transform.SetParent(gameManager.battleMapTiles[tileID].gameObject.transform);
            destroyAnimation.transform.localPosition=new Vector3(0, 0, 0);
            destroyAnimation.transform.localScale=new Vector3(scale,scale,scale);
            UnityEngine.Object.Destroy(destroyAnimation, animationDuration);    
        }

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


    //Since this isn't a Monobehaviour, so we're calling it from GameManager.
    public void Update(float timeRemainingInTic) {
        //Shoot missiles
        BattleMapTile originTile=gameManager.battleMapTiles[this.tileID];
        if ((this.towerTypeID==GameManager.TOWER_ARROW || this.towerTypeID==GameManager.TOWER_SNIPER || this.towerTypeID==GameManager.TOWER_BALLISTA) && targetTile>=0 && shootAnimationsInitiated==0) {    
            if (timeRemainingInTic<=(projectileSpeed*targetTileDistance)) {
                shootAnimationsInitiated=1;
                BattleMapTile destinationTile=gameManager.battleMapTiles[targetTile];
                
                GameObject prefab=null;
                if (this.towerTypeID==GameManager.TOWER_ARROW || this.towerTypeID==GameManager.TOWER_BALLISTA) {
                    prefab=gameManager.arrowProjectilePrefab;
                }
                else if (this.towerTypeID==GameManager.TOWER_SNIPER) {
                    prefab=gameManager.sniperProjectilePrefab;
                }

                GameObject projectile = UnityEngine.Object.Instantiate(prefab);   
                projectile.transform.SetParent(gameManager.battlefieldIndicatorContainer.transform/*originTile.gameObject.transform*/);
                
                ProjectileHandler projectileHandler = projectile.GetComponent<ProjectileHandler>();
                projectileHandler.init(projectileSpeed*targetTileDistance, originTile.gameObject.transform.position, destinationTile.gameObject.transform.position, gameManager);
            }
        }
        else if (this.towerTypeID==GameManager.TOWER_FIRE && (timeRemainingInTic<=(projectileSpeed)) && shootAnimationsInitiated<6) {
            shootAnimationsInitiated++;
            GameObject projectile = UnityEngine.Object.Instantiate(gameManager.fireCirclePrefab);   
            projectile.transform.SetParent(gameManager.battlefieldIndicatorContainer.transform);
            projectile.transform.position=originTile.gameObject.transform.position;
            
            FireCircleHandler fireCircleHandler = projectile.GetComponent<FireCircleHandler>();
            fireCircleHandler.init(projectileSpeed, this.range, gameManager);
        }
                
    }
}
