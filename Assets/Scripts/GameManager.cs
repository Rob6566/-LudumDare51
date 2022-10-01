using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//TODO - fast forward button

public enum ObjectOwner {enemy, player};

public class GameManager : MonoBehaviour {
    public GameObject battlefieldTilePrefab;
    public GameObject battlefieldObjectPrefab;
    public GameObject battlefieldContainer;
    [SerializeField] Image playerActionUI;
    [SerializeField] List<Image> enemyActionUI = new List<Image>();
    [SerializeField] List<Sprite> enemyActionSprites = new List<Sprite>();



    [SerializeField] TextMeshProUGUI goldUI;
    [SerializeField] TextMeshProUGUI scienceUI;
    [SerializeField] TextMeshProUGUI timerUI;

    public Canvas canvas;

    public int gold=0;
    public int science=0;
    public int nextPlayerAction=0;

    public int goldIncome=10;
    public int scienceIncome=10;
    public int basicTowerCost=10;
    public int researchCost=10;
    public int unplacedTowerSlots=2;
    public int basicTowersToChooseFrom=2;
    public float gameSpeed=1f;


    public GameObject pauseButton;
    public GameObject playButton;
    public GameObject fastForwardButton;

    //Values that change a lot
    public float timeRemainingInTic=10f;
    public bool paused=true;
    public List<BattlefieldObjectSO> currentSelectableTowers = new List<BattlefieldObjectSO>(); 
    public BattlefieldObject selectedTower=null;
    public List<int> enemyActions = new List<int>();

    public List<BattlefieldObjectSO> allEnemies = new List<BattlefieldObjectSO>(); 
    public List<BattlefieldObjectSO> allTowers = new List<BattlefieldObjectSO>(); 
    public List<Sprite> playerActionIcons = new List<Sprite>(); 
    public List<TextMeshProUGUI> playerActionLabels = new List<TextMeshProUGUI>(); 
    public List<GameObject> unplacedTowerContainers = new List<GameObject>();
    public List<GameObject> playerActionButtons = new List<GameObject>();

    
    //Selection overlay
    public GameObject overlay;
    public TextMeshProUGUI overlayTitleTXT;
    public List<GameObject> overlayObjects = new List<GameObject>();
    public List<TextMeshProUGUI> overlayTitles = new List<TextMeshProUGUI>();
    public List<Image> overlayImages = new List<Image>();
    public List<TextMeshProUGUI> overlayDescs = new List<TextMeshProUGUI>();


    List<List<int>> ENEMY_MOVEMENT_SEQUENCES=new List<List<int>>(); //Sequencing of enemy movements per direction to minimise collisions
    
    public const int BATTLEFIELD_WIDTH=10;
    public const int BATTLEFIELD_HEIGHT=10;
    public const int X_BETWEEN_TILES=105;
    public const int Y_BETWEEN_TILES=105;
    public const float battlefieldScale=1.40f;
    public const int ACTION_WORK=0;
    public const int ACTION_STUDY=1;
    public const int ACTION_TINKER=2;
    public const int ACTION_BUILD_BASIC=3;

    public const int ENEMY_ACTION_NOTHING=0;
    public const int ENEMY_ACTION_LEFT=1;
    public const int ENEMY_ACTION_DOWN_LEFT=2;
    public const int ENEMY_ACTION_DOWN=3;
    public const int ENEMY_ACTION_DOWN_RIGHT=4;
    public const int ENEMY_ACTION_RIGHT=5;

    private List<BattleMapTile> battleMapTiles = new List<BattleMapTile>();
    
    void Start() {
        createBattlefield();
        overlay.SetActive(false);
        setActionButtonAvailability();
        updateUI();
        ENEMY_MOVEMENT_SEQUENCES=getEnemyMovementSequences();
    }

    void Update() {
        if (!paused) {
            timeRemainingInTic-=Time.deltaTime*gameSpeed;
        }

        //Our timer just hit 0. Do our player/enemy actions
        if(timeRemainingInTic<=0f) {
            resetCombatVars();
            runPlayerTicAction();
            runEnemyTicAction();

            timeRemainingInTic=10f;
            updateUI();
        } 

        timerUI.text=(paused ? "[" : "")+Mathf.RoundToInt(timeRemainingInTic+.5f).ToString()+(paused ? "]" : "");
    }

    public void setPaused(bool newPaused) {
        paused=newPaused;
        updateUI();
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

          /*spawnEnemyInTopRow(allEnemies[0]);
          spawnEnemyInTopRow(allEnemies[0]);
          spawnEnemyInTopRow(allEnemies[0]);*/
    }


    public void updateUI() {
        Color32 activeColour= new Color32(168,168,168, 255); //CLEANUP - constant
        Color32 inactiveColour= new Color32(65,65,65, 255);

        goldUI.text="Gold: "+gold.ToString();
        scienceUI.text="Science: "+science.ToString();

        playerActionUI.sprite=playerActionIcons[nextPlayerAction];

        playerActionLabels[ACTION_WORK].text="Work (+"+goldIncome+" gold)";
        playerActionLabels[ACTION_STUDY].text="Study (+"+scienceIncome+" science)";
        playerActionLabels[ACTION_TINKER].text="Tinker (discover an upgrade)\n (-"+researchCost+" science)";
        playerActionLabels[ACTION_BUILD_BASIC].text="Build basic tower\n (-"+basicTowerCost+" gold)";

        pauseButton.transform.GetComponent<Button>().interactable=!paused;
        pauseButton.GetComponent<Image>().color=(paused ? inactiveColour : activeColour);

        bool playActive = (paused || gameSpeed!=1f);
        playButton.transform.GetComponent<Button>().interactable=playActive;
        playButton.GetComponent<Image>().color=(playActive ? activeColour : inactiveColour);

        bool ffActive = (paused || gameSpeed!=2f);
        fastForwardButton.transform.GetComponent<Button>().interactable=ffActive;
        fastForwardButton.GetComponent<Image>().color=(ffActive ? activeColour : inactiveColour);

        for (int x=0; x<3; x++) {
            int action=enemyActions[x];
            Sprite enemyActionSprite = (action>-1 ? enemyActionSprites[action] : allEnemies[Mathf.Abs(action+1)].sprite);
            enemyActionUI[x].sprite=enemyActionSprite;
        }
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

    //Gets a list of random enemies or towers
    public List<BattlefieldObjectSO> getRandomObjects(int count, ObjectOwner towerOrEnemy, int objectLevel) {
        //Filter out towers that don't match filters
        List<BattlefieldObjectSO> objectsMatchingFilter = new List<BattlefieldObjectSO>();
        foreach(BattlefieldObjectSO thisObject in allTowers) {
            if (objectLevel!=thisObject.level) {
                continue;
            }
            objectsMatchingFilter.Add(thisObject);
        };

        List<BattlefieldObjectSO> returnObjects = new List<BattlefieldObjectSO>();
        for(int i = 1; i<=count && objectsMatchingFilter.Count>0; i++) {
            int randomElement=UnityEngine.Random.Range(0, objectsMatchingFilter.Count);
            returnObjects.Add(objectsMatchingFilter[randomElement]);
            objectsMatchingFilter.Remove(objectsMatchingFilter[randomElement]);
        }

        return returnObjects;
    }

    //Gets a list of random enemies or towers
    public int getRandomEnemy(int enemyLevel) {
        //Filter out towers that don't match filters
        List<int> enemiesMatchingFilter = new List<int>();
        int upto=0;
        foreach(BattlefieldObjectSO thisEnemy in allEnemies) {
            if (enemyLevel!=thisEnemy.level) {
                upto++;
                continue;
            }
            enemiesMatchingFilter.Add(upto);
            upto++;
        };

        int randomElement=UnityEngine.Random.Range(0, enemiesMatchingFilter.Count);
        return enemiesMatchingFilter[randomElement];
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
            populateAndShowOverlay(ACTION_BUILD_BASIC, basicTowersToChooseFrom);
        }
        setActionButtonAvailability();
    }

    public void populateAndShowOverlay(int actionType, int optionCount) {

        foreach(GameObject thisButton in playerActionButtons) {
            thisButton.GetComponent<Button>().interactable=false;
        }

        for(int x=0; x<3; x++) {
             overlayObjects[x].SetActive(x<optionCount);
        }

        if(nextPlayerAction==ACTION_BUILD_BASIC) {
            overlayTitleTXT.text="Build Basic Tower";
            currentSelectableTowers = getRandomObjects(optionCount, ObjectOwner.player, 1); 
            int optionUpto=0;
            foreach (BattlefieldObjectSO thisObject in currentSelectableTowers) {
                overlayTitles[optionUpto].text=thisObject.name;
                overlayImages[optionUpto].sprite=thisObject.sprite;
                overlayDescs[optionUpto].text=thisObject.desc;
                optionUpto++;
            }
        }

        overlay.SetActive(true);
        setPaused(true);
    }

    //Sets option availabilty based on whether we meet the requirements
    public void setActionButtonAvailability() {

        Color32 activeColour= new Color32(168,168,168, 255);
        Color32 inactiveColour= new Color32(65,65,65, 255);


        //TODO - enable/disable buttons
        foreach(GameObject thisButton in playerActionButtons) {
            thisButton.GetComponent<Button>().interactable=true;
        }

        if (science<researchCost) {
            playerActionButtons[ACTION_TINKER].GetComponent<Button>().interactable=false;
        }

        bool availableTowerContainers=false;
        foreach(GameObject towerContainer in unplacedTowerContainers) {
            if (towerContainer.transform.childCount==0) {
                availableTowerContainers=true;
            }
        }

        if (gold<basicTowerCost || !availableTowerContainers) {
            playerActionButtons[ACTION_BUILD_BASIC].GetComponent<Button>().interactable=false;
        }
        

        foreach(GameObject thisButton in playerActionButtons) {
            thisButton.GetComponent<Image>().color=(thisButton.GetComponent<Button>().interactable ? activeColour : inactiveColour);
        }    

    }

    public void playObjectInSlot(int slot) {
        if (selectedTower==null) {
            return;
        }

        if (battleMapTiles[slot].moveObjectToTile(selectedTower)) {
            selectedTower=null;
        }
    }

    
    public void resetCombatVars() {
        foreach (BattleMapTile tile in battleMapTiles) {
            if (!tile.hasObject()) {
                continue;
            }
            tile.battlefieldObject.justAttacked=false;
            tile.battlefieldObject.justMoved=false;
        }
    }



    /****************************************************************
                            ENEMY HANDLING
    ****************************************************************/

    /*
    public const int ENEMY_ACTION_NOTHING=0;
    public const int ENEMY_ACTION_LEFT=1;
    public const int ENEMY_ACTION_DOWN_LEFT=2;
    public const int ENEMY_ACTION_DOWN=3;
    public const int ENEMY_ACTION_DOWN_RIGHT=4;
    public const int ENEMY_ACTION_RIGHT=5;
    */

    public void runEnemyTicAction() {
    
        //Run this action
        int thisAction=enemyActions[0];
        if (thisAction<0) {
            spawnEnemyInTopRow(allEnemies[thisAction+1]);
        }
        else if(thisAction>ENEMY_ACTION_NOTHING) {
            moveEnemiesInDirection(thisAction);
        }
        
        
        //Calculate next action
        float chanceOfSpawn=.5f;
        float spawnRoll=Random.Range(0f,1f);
        int nextAction=0;
        if (spawnRoll<=chanceOfSpawn) {
            nextAction = -getRandomEnemy(1)-1;
        }
        else {
            nextAction=UnityEngine.Random.Range(1, 5);
        }

        //Shuffle Actions
        enemyActions[0]=enemyActions[1];
        enemyActions[1]=enemyActions[2];
        enemyActions[2]=nextAction;
    }

    //Since all enemies move in the same diretion simultaneously, we need to process them in a specific sequence to minimise collissions
    //CLEANUP - make this work for grids that aren't 10x10
    public List<List<int>> getEnemyMovementSequences() {
        List<List<int>> returnList = new List<List<int>>();
        returnList.Add(new List<int>()); //A dummy list to offset our IDs for ENEMY_ACTION_NOTHING
        for (int direction=0; direction<=ENEMY_ACTION_RIGHT; direction++) {
            List<int> thisDirectionList = new List<int>();
            if (direction==ENEMY_ACTION_LEFT || direction==ENEMY_ACTION_DOWN_LEFT) {
                for(int x=9; x>=0; x--) {
                    for(int y=0; x<=9; y++) {
                        thisDirectionList.Add(x+(y*10));
                    }
                }
            }
            else if (direction==ENEMY_ACTION_RIGHT || direction==ENEMY_ACTION_DOWN_RIGHT) {
                for(int x=0; x<=9; x++) {
                    for(int y=0; x<=9; y++) {
                        thisDirectionList.Add(x+(y*10));
                    }
                }
            }
            else if (direction==ENEMY_ACTION_DOWN) {
                for(int y=0; y<=9; y++) {
                    for(int x=0; x<=9; x++) {
                        thisDirectionList.Add(x+(y*10));
                    }
                }
            }
            returnList.Add(thisDirectionList);
        }
        return returnList;
    }

    //Move all enemies in given a direction during the enemies actiom
    public void moveEnemiesInDirection(int direction) {
        if(direction<=ENEMY_ACTION_NOTHING || direction>ENEMY_ACTION_RIGHT) {
            return;
        }

        Debug.Log("Trying to move enemies in direction: "+direction+" Count of direction array: "+ENEMY_MOVEMENT_SEQUENCES.Count);

        List<int> tilesToMove = ENEMY_MOVEMENT_SEQUENCES[direction];

        foreach (int tileID in tilesToMove) {
            BattleMapTile sourceTile=battleMapTiles[tileID];
            if (!sourceTile.hasEnemyObject()) {
                continue; //No enemy in tile
            }

            int tileInDirection=getTileInDirection(tileID, direction);
            if (tileInDirection<0) {
                continue;  //Blocked by edge of map
            }

            BattleMapTile destinationTile=battleMapTiles[tileInDirection];            
            if (destinationTile.hasObject()) {
                continue; //Blocked by object
            }

            sourceTile.moveExistingObjectToAnotherTile(destinationTile);
        }
    }


    //Gets the index of a tile adjacent to a given one, in a given direction (or null, if there's no tile in that direction)
    public int getTileInDirection(int sourceTile, int direction) {
        bool onLeftEdge=(sourceTile % 10 == 0);
        bool onRightEdge=((sourceTile+1) % 10 == 0);
        bool onBottomEdge=(sourceTile<10);

        if (direction==ENEMY_ACTION_LEFT) {
            return (onLeftEdge ? -1 : (sourceTile-1));
        } 
        else if(direction==ENEMY_ACTION_RIGHT) {
            return (onRightEdge ? -1 : sourceTile+1);
        }
        else if(onBottomEdge) {
            return -1;
        }
        else if(direction==ENEMY_ACTION_DOWN_LEFT) {
            return (onLeftEdge ? -1 : sourceTile-11);
        }
        else if(direction==ENEMY_ACTION_DOWN_RIGHT) {
            return (onRightEdge ? -1 : sourceTile-9);
        }
        else if(direction==ENEMY_ACTION_DOWN) {
            return sourceTile-10;
        }

        return -1;
    }









    /****************************************************************
                                EVENTS
    ****************************************************************/

    public void clickPlayerActionButton(int buttonClicked) {
        nextPlayerAction=buttonClicked;
        updateUI();
    }


    public void clickOptionButton(int buttonClicked) {
        overlay.SetActive(false);
        setPaused(false);
        setActionButtonAvailability();

        if (nextPlayerAction==ACTION_BUILD_BASIC) {
           createTowerAndMakeAvailable(currentSelectableTowers[buttonClicked]);
        }

        clickPlayerActionButton(ACTION_WORK);
    }

    public void createTowerAndMakeAvailable(BattlefieldObjectSO newTowerSO) {
        BattlefieldObject newTower=spawnObjectFromSO(newTowerSO);   
        foreach(GameObject towerContainer in unplacedTowerContainers) {
            Transform containerTransform=towerContainer.transform;
            if (containerTransform.childCount==0) {
                Transform objectTransform=newTower.rootGameObject.transform;
                objectTransform.SetParent(containerTransform.transform);
                objectTransform.localPosition=new Vector3(0, 0, 0);
                objectTransform.localScale=new Vector3(1,1,1);
                break;
            }
        }

        setActionButtonAvailability();
    }

    public void clickPauseButton() {
        setPaused(true);
        updateUI();
    }

    public void clickPlayButton() {
        setPaused(false);
        gameSpeed=1f;
        updateUI();
    }

    public void clickFastForwardButton() {
        setPaused(false);
        gameSpeed=2f;
        updateUI();
    }
}
