using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ObjectOwner {enemy, player};

public class GameManager : MonoBehaviour {
    public GameObject battlefieldTilePrefab;
    public GameObject battlefieldObjectPrefab;
    public GameObject battlefieldContainer;
    [SerializeField] Image playerActionUI;
    [SerializeField] Image enemyActionUI;

    [SerializeField] TextMeshProUGUI goldUI;
    [SerializeField] TextMeshProUGUI scienceUI;
    [SerializeField] TextMeshProUGUI timerUI;

    public int gold=0;
    public int science=0;
    public int nextPlayerAction=0;

    public int goldIncome=10;
    public int scienceIncome=10;
    public int basicTowerCost=10;
    public int researchCost=10;
    public int unplacedTowerSlots=2;

    public float timeRemainingInTic=10f;
    public bool paused=true;

    public List<BattlefieldObjectSO> allEnemies = new List<BattlefieldObjectSO>(); 
    public List<Sprite> playerActionIcons = new List<Sprite>(); 
    public List<TextMeshProUGUI> playerActionLabels = new List<TextMeshProUGUI>(); 
    public List<GameObject> unplacedTowerContainers = new List<GameObject>(); 
    
    public const int BATTLEFIELD_WIDTH=10;
    public const int BATTLEFIELD_HEIGHT=10;
    public const int X_BETWEEN_TILES=105;
    public const int Y_BETWEEN_TILES=105;
    public const float battlefieldScale=1.40f;
    public const int ACTION_WORK=0;
    public const int ACTION_STUDY=1;
    public const int ACTION_TINKER=2;
    public const int ACTION_BUILD_BASIC=3;

    private List<BattleMapTile> battleMapTiles = new List<BattleMapTile>();
    
    void Start() {
        createBattlefield();
        updateUI();
    }

    void Update() {
        if (!paused) {
            timeRemainingInTic-=Time.deltaTime;
        }

        if(timeRemainingInTic<=0f) {
            //TODO - do end of tic actions
            runPlayerTicAction();

            timeRemainingInTic=10f;
            updateUI();
        } 

        timerUI.text=(paused ? "[" : "")+Mathf.RoundToInt(timeRemainingInTic+.5f).ToString()+(paused ? "]" : "");
    }

    //TODO - array of tower upgrade modifiers


    void createBattlefield() {
          int tileUpto=0;
          for(int y=0; y<BATTLEFIELD_HEIGHT; y++) {
            for(int x=0; x<BATTLEFIELD_WIDTH; x++) {
                battleMapTiles.Add(createBattleMapTile(tileUpto, battlefieldContainer, x*X_BETWEEN_TILES, y*Y_BETWEEN_TILES));
                tileUpto++;
            }
          }

          spawnEnemyInTopRow(allEnemies[0]);
          spawnEnemyInTopRow(allEnemies[0]);
          spawnEnemyInTopRow(allEnemies[0]);
    }


    public void updateUI() {
        goldUI.text="Gold: "+gold.ToString();
        scienceUI.text="Science: "+science.ToString();

        playerActionUI.sprite=playerActionIcons[nextPlayerAction];

        playerActionLabels[ACTION_WORK].text="Work (+"+goldIncome+" gold)";
        playerActionLabels[ACTION_STUDY].text="Study (+"+scienceIncome+" science)";
        playerActionLabels[ACTION_TINKER].text="Tinker (discover an upgrade)\n (-"+researchCost+" science)";
        playerActionLabels[ACTION_BUILD_BASIC].text="Build basic tower\n (-"+basicTowerCost+" gold)";
    }

    public int getScience() {
        return science;
    }
    
    public void setScience(int newScience) {
        science=newScience;
        updateUI();
    }

    public int getGold() {
        return gold;
    }
    
    public void setGold(int newGold) {
        gold=newGold;
        updateUI();
    }

    public BattleMapTile createBattleMapTile(int tilePosition, GameObject parentContainer, int x, int y) {        
        BattleMapTile battleMapTile = new BattleMapTile();

        GameObject gameObject = Instantiate(battlefieldTilePrefab);

        gameObject.transform.SetParent(parentContainer.transform);
        gameObject.transform.localPosition=new Vector3(x, y, 0)+new Vector3(-550, -475, 0);
        gameObject.transform.localScale=new Vector3(battlefieldScale, battlefieldScale, battlefieldScale);

        battleMapTile.init(this, gameObject, tilePosition);
        return battleMapTile;
    }


    public BattlefieldObject spawnObjectFromSO(BattlefieldObjectSO so) {        
        BattlefieldObject newObject = new BattlefieldObject();

        GameObject gameObject=Instantiate(battlefieldObjectPrefab);
        //newArtifact.artifactUI=artifactGameObject;

        newObject.init(this, gameObject, battlefieldContainer, so);

        return newObject;
    }

    //Spawn an enemy
    public BattlefieldObjectSO getRandomEnemy(int enemyLevel) {
        //TODO - handle spawning different enemies
        return allEnemies[0];
    }

    public void spawnEnemyInTopRow(BattlefieldObjectSO enemySO) {
        int slot=UnityEngine.Random.Range(90, 100);
        battleMapTiles[slot].createObjectInTile(enemySO); //TODO - check if the slot is filled first. If it has a player structure, destroy it.
    }

    
    //Run the player action
    public void runPlayerTicAction() {
        if (nextPlayerAction==ACTION_WORK) {
            gold+=goldIncome;
        }
        else if(nextPlayerAction==ACTION_STUDY) {
            science+=scienceIncome;
        }
        else if(nextPlayerAction==ACTION_TINKER) {
            //TODO
        }
        else if(nextPlayerAction==ACTION_BUILD_BASIC) {
            //TODO
        }
    }






    //Events
    public void clickPlayerActionButton(int buttonClicked) {
        Debug.Log(buttonClicked);
        nextPlayerAction=buttonClicked;
        updateUI();
    }
}
