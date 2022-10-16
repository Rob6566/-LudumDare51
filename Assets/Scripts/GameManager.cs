using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEditor.VersionControl;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

//TODO - fast forward button

public enum ObjectOwner {enemy, player};

public static class FirstLoad {
    public static bool firstLoad=true;
}

public class GameManager : MonoBehaviour {
    public GameObject battlefieldTilePrefab;
    public GameObject battlefieldObjectPrefab;
    public GameObject battlefieldLinePrefab;
    public GameObject arrowProjectilePrefab;
    public GameObject sniperProjectilePrefab;
    public GameObject fireCirclePrefab;
    public GameObject battlefieldContainer;
    public GameObject battlefieldIndicatorContainer;
    public GameObject textAlertPrefab;

    //Scoring Prefabs
    public GameObject scoreHolder;
    public GameObject scorePrefab;

    [SerializeField] Image playerActionUI;
    [SerializeField] List<Image> enemyActionUI = new List<Image>();
    [SerializeField] List<Sprite> enemyActionSprites = new List<Sprite>();

    [SerializeField] List<GameObject> canvases = new List<GameObject>();

    [SerializeField] TextMeshProUGUI goldUI;
    [SerializeField] TextMeshProUGUI scienceUI;
    [SerializeField] TextMeshProUGUI scoreUI;
    [SerializeField] TextMeshProUGUI timerUI;


    //Animations
    public GameObject arrowDestroyedPrefab;
    public GameObject enemyDestroyedPrefab;
    //public AnimatorController arrowTowerAnimatorController;

    //Player details
    public string playerName="";
    public TMP_InputField playerNameInput;
    public GameObject invalidName;


    //Audio
    [SerializeField] List<AudioClip> sounds;
    [SerializeField] AudioSource audioSource;

    public Canvas canvas;

    public float gold=0;
    public float science=0;
    public int nextPlayerAction=0;
    public int lastOverlayAction=0;
    public int score=0;

    public int goldIncome=10;
    public int scienceIncome=10;
    public int researchIncrementalCost=1;
    public int basicTowerCost=10;
    public int basicTowerIncrementalCost=1;
    public int researchCost=10;
    public int unplacedTowerSlots=2;
    public int basicTowersToChooseFrom=2;
    public int researchOptionsToChooseFrom=3;
    public float gameSpeed=1.2f;
    
    private bool gameOver=false;


    public GameObject pauseButton;
    public GameObject playButton;
    public GameObject fastForwardButton;

    //Values that change a lot
    public float timeRemainingInTic=10f;
    public bool paused=true;
    public List<BattlefieldObjectSO> currentSelectableTowers = new List<BattlefieldObjectSO>(); 
    public List<Science> currentSelectableSciences = new List<Science>(); 
    public BattlefieldObject selectedTower=null;
    public List<int> enemyActions = new List<int>{-1,0,0};
    public List<int> enemyQtys = new List<int>{1,0,0};
    int enemiesOnBattlefield=0;
    int enemiesInTopRow=0;
    bool showingOverlay=false;
    bool selectOverlayMinimised=false;
    float previousOverlayWidth=0f;
    float previousOverlayHeight=0f;
    List<bool> overlayOptionVisibility=new List<bool>{false,false,false};

    //Enemy quantities
    public List<GameObject> enemyQuantityCircles = new List<GameObject>();
    public List<TextMeshProUGUI> enemyQuantityTexts = new List<TextMeshProUGUI>();

    public List<BattlefieldObjectSO> allEnemies = new List<BattlefieldObjectSO>(); 
    public List<BattlefieldObjectSO> allTowers = new List<BattlefieldObjectSO>(); 
    public List<ScienceSO> allSciences = new List<ScienceSO>(); 
    public ScienceSO upgradeScience;
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
    //public GameObject overlayHolder;
    public TextMeshProUGUI toggleOverlayTXT;


    //Tower Modifiers
    public List<TowerModifier> towerModifiers = new List<TowerModifier>(); //Cleanup - shouldn't be matching array values. This should be "TowerType" Class and contain the values and ScriptableObject

    //Game Stats to show player
    public int ticsSurvived=0;
    public int kills=0;
    public int towersLost=0;
    public int damageDealt=0;
    public int damageTaken=0;
    public int goldEarned=0;
    public int scienceEarned=0;
    public GameObject statsOverlay;
    public GameObject loseOverlay;
    public TextMeshProUGUI ticsSurvivedUI;
    public TextMeshProUGUI killsUI;
    public TextMeshProUGUI towersLostUI;
    public TextMeshProUGUI damageDealtUI;
    public TextMeshProUGUI damageTakenUI;
    public TextMeshProUGUI goldEarnedUI;
    public TextMeshProUGUI scienceEarnedUI;
    public TextMeshProUGUI statsButtonUI;
    public TextMeshProUGUI scoreOverlayUI;


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
    public const int ENEMY_ACTION_UP=6;

    const int INCREASE_NUMBER_OF_ENEMIES_EVERY_X_TICS=25;
    const int ADD_DIFFICULT_ENEMIES_AFTER_X_TICS=60;
    const int ADD_ULTRA_ENEMIES_AFTER_X_TICS=120;

    public const int SOUND_CLICK=8;
        public const float SOUND_CLICK_VOLUME=.15f;
    public const int SOUND_SCIENCE=6;
        public const float SOUND_SCIENCE_VOLUME=.2f;
    public const int SOUND_PLACE_TOWER=7;
        public const float SOUND_PLACE_TOWER_VOLUME=.5f;
    public const int SOUND_TIC=9;
        public const float SOUND_TIC_VOLUME=.2f;

    public const float NORMAL_GAME_SPEED=1.2f;
    public const float FAST_GAME_SPEED=5f;

    public const float ENEMY_ANIMATION_DURATION=.8f;
    
    //Towers
    public const int TOWER_ARROW=1;
    public const int TOWER_BRICK=2;
    public const int TOWER_FIRE=3;
    public const int TOWER_SNIPER=4;
    public const int TOWER_INCOME=5;
        public float incomeTowerBonus=0f;
    public const int TOWER_BALLISTA=6;


    //Scores
    public const int SCORE_PER_TIC=10;
    public List<int> SCORE_PER_LEVEL=new List<int>{0,5,20,200};
    public const string SCORES_URL="http://18.234.230.231/scores.php";
    public const string ADD_SCORE_URL="http://18.234.230.231/add_score.php";

    public const string COLOUR_GOLD="#000000";
    public const string COLOUR_SCIENCE="#000000";

    //Version
    public List<TextMeshProUGUI> versionTxts = new List<TextMeshProUGUI>();


    public List<BattleMapTile> battleMapTiles = new List<BattleMapTile>();

    
    async void Start() {
        createBattlefield();
        overlay.SetActive(false);
        statsOverlay.SetActive(false);
        loseOverlay.SetActive(false);
        setActionButtonAvailability();
        initTowerModifiers();
        updateUI();
        ENEMY_MOVEMENT_SEQUENCES=getEnemyMovementSequences();
        gameOver=false;
        statsButtonUI.text="Close";
        setActiveCanvas("TitleCanvas");
        paused=true;

        foreach(TextMeshProUGUI txtVersion in versionTxts) {
            txtVersion.text="Version: "+Application.version;
        }
        if (!FirstLoad.firstLoad) {
            //startGame();
        }

        FirstLoad.firstLoad=false;

        await UnityServices.InitializeAsync();
        Debug.Log("State= "+UnityServices.State);

        //SetupEvents();

        StartCoroutine(LoadScores());
    }

    IEnumerator SaveScore() {
        
        WWWForm form = new WWWForm();
        form.AddField("user_name", playerName);
        form.AddField("score", score);
        form.AddField("user_id", AuthenticationService.Instance.PlayerId);

        Debug.Log("Save Score");

        using (UnityWebRequest webRequest = UnityWebRequest.Post(ADD_SCORE_URL, form)) {
            // Request and wait for the desired page.
            //webRequest.SetRequestHeader("secretkey", "12345");
            yield return webRequest.SendWebRequest();

            Debug.Log("Save Score - sent");

            switch (webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("Post Score Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("Save Score - success");
                    break;
            }
        }
    }

    IEnumerator LoadScores() {
        
         //webRequest= new UnityWebRequest();
        string uri=SCORES_URL;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            //webRequest.SetRequestHeader("secretkey", "12345");
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                    dynamic allScoreData = JArray.Parse(webRequest.downloadHandler.text);
                    int scoreUpto=0;
                    foreach(dynamic thisScore in allScoreData) {
                        if (scoreUpto>10) {
                            break;
                        }

                        GameObject gameObject = Instantiate(scorePrefab);

                        gameObject.transform.SetParent(scoreHolder.transform);      
                        gameObject.transform.localPosition=new Vector3(0, 10-(50*scoreUpto), 0);
                        gameObject.transform.localScale=new Vector3(1f, 1f, 1f);
                        TextMeshProUGUI txtName =gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                            txtName.text=thisScore.name;
                        TextMeshProUGUI txtScore =gameObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
                            txtScore.text=thisScore.score;

                        scoreUpto++;
                    }
                    break;
            }
        }
    }



    public void startGame() {
        playSound(SOUND_CLICK, SOUND_CLICK_VOLUME);

        //Check if Player name is entered. If not, ask them to enter it

        if (playerName=="") {
            playerName = PlayerPrefs.GetString("playername");
        }

        if (playerName=="") {
            setActiveCanvas("EnterNameCanvas");
            invalidName.SetActive(false);
        }
        else {
            setActiveCanvas("GameCanvas");
            paused=false;
        }
    }

    public void enterName() {
        playSound(SOUND_CLICK, SOUND_CLICK_VOLUME);

        string tempPlayerName=playerNameInput.text;
        tempPlayerName=tempPlayerName.Trim();
        if (tempPlayerName.Length>30 || tempPlayerName.Length<2) {
            invalidName.SetActive(true);
        }
        else if (!Regex.IsMatch(tempPlayerName, "^[a-zA-Z0-9 ]*$")) {
            invalidName.SetActive(true);
        }
        else {
            playerName=tempPlayerName;
            PlayerPrefs.SetString("playername", playerName);
            startGame();
        }
    }

    public void startTutorial() {
        playSound(SOUND_CLICK, SOUND_CLICK_VOLUME);
        setActiveCanvas("TutorialCanvas");
    }

    //Runs every frame. Does our countdown, and triggers countdonwn events if we've hit 0.
    void Update() {
        if (!paused) {
            timeRemainingInTic-=Time.deltaTime*gameSpeed;
        }
        
        Debug.developerConsoleVisible = false;


        handleKeyPress();

        //Call update on all towers. This'll let them start the fire animation
        foreach (BattleMapTile tile in battleMapTiles) {
            if (tile.hasPlayerObject()) {
                tile.battlefieldObject.Update(timeRemainingInTic);
            }
            else if (tile.hasEnemyObject()) {
                tile.Update(Time.deltaTime*gameSpeed);
            }
            
        } 

        //Our timer just hit 0. Do our player/enemy actions
        if(timeRemainingInTic<=0f) {
            playSound(SOUND_TIC, SOUND_TIC_VOLUME);
            ticsSurvived++;
            resetCombatVars();
            
            runPlayerAttacks();
            runEnemyAttacks();

            runPlayerTicAction();
            runEnemyTicAction();

            acquirePlayerTargets();

            calculateTilesInRange(); //TODO - this is probably a bit heavy to do every Tic

            timeRemainingInTic=10f;
            score+=SCORE_PER_TIC;
            updateUI();
        } 

        timerUI.text=(paused ? "[" : "")+Mathf.RoundToInt(timeRemainingInTic+.5f).ToString()+(paused ? "]" : "");
    }



    //User authentication (from https://docs.unity.com/authentication/InitializeSDK.html)
    void SetupEvents() {
        AuthenticationService.Instance.SignedIn += () => {
            // Shows how to get a playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

        };

        AuthenticationService.Instance.SignInFailed += (err) => {
            Debug.LogError(err);
        };

        AuthenticationService.Instance.SignedOut += () => {
            Debug.Log("Player signed out.");
        };
        
        AuthenticationService.Instance.Expired += () => {
            Debug.Log("Player session could not be refreshed and expired.");
        };
    }

    //User sign in (from https://docs.unity.com/authentication/UsingAnonSignIn.html)
    /*async Task SignInAnonymouslyAsync() {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");
            
            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}"); 

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }*/





    //Create an array of modifiers for our tower. CLEANUP - move to TowerType class which links to the scriptableobject
    void initTowerModifiers() {
        foreach(BattlefieldObjectSO tower in allTowers) {
            TowerModifier towerModifier = new TowerModifier();
            towerModifier.towerTypeID=tower.towerTypeID;
            towerModifiers.Add(towerModifier);
        }
    }

    public TowerModifier getModifierByTowerTypeID(int towerTypeID) {
        foreach(TowerModifier thisTowerMod in towerModifiers) {
            if (thisTowerMod.towerTypeID==towerTypeID) {
                return thisTowerMod;
            }
        }
        return null;
    }

    public void setPaused(bool newPaused) {
        paused=newPaused;
        updateUI();
    }

    void createBattlefield() {
          int tileUpto=0;
          for(int y=0; y<BATTLEFIELD_HEIGHT; y++) {
            for(int x=0; x<BATTLEFIELD_WIDTH; x++) {
                battleMapTiles.Add(createBattleMapTile(tileUpto, battlefieldContainer, x*X_BETWEEN_TILES, y*Y_BETWEEN_TILES));
                tileUpto++;
            }
          }
    }


    public void updateUI() {

        if (gameOver) {
            return;
        }
        Color32 activeColour= new Color32(168,168,168, 255); //CLEANUP - constant
        Color32 inactiveColour= new Color32(65,65,65, 255);

        goldUI.text="Gold: <color="+COLOUR_GOLD+">"+Mathf.Floor(gold).ToString()+"</color>";
        scienceUI.text="Science: <color="+COLOUR_SCIENCE+">"+Mathf.Floor(science).ToString()+"</color>";;

        playerActionUI.sprite=playerActionIcons[nextPlayerAction];

        playerActionLabels[ACTION_WORK].text="<color=#660000>W</color>ork (+<color="+COLOUR_GOLD+">"+goldIncome+" gold</color>)";
        playerActionLabels[ACTION_STUDY].text="<color=#660000>S</color>tudy (+"+scienceIncome+" science)";
        playerActionLabels[ACTION_TINKER].text="Tinker (<color=#660000>a</color>)\n (-"+researchCost+" science)";
        playerActionLabels[ACTION_BUILD_BASIC].text="Buil<color=#660000>d</color> tower\n (-"+basicTowerCost+" gold)";

        pauseButton.transform.GetComponent<Button>().interactable=!paused;
        pauseButton.GetComponent<Image>().color=(paused ? inactiveColour : activeColour);

        bool playActive = (paused || gameSpeed!=NORMAL_GAME_SPEED);
        playButton.transform.GetComponent<Button>().interactable=playActive;
        playButton.GetComponent<Image>().color=(playActive ? activeColour : inactiveColour);

        bool ffActive = (paused || gameSpeed!=FAST_GAME_SPEED);
        fastForwardButton.transform.GetComponent<Button>().interactable=ffActive;
        fastForwardButton.GetComponent<Image>().color=(ffActive ? activeColour : inactiveColour);

        for (int x=0; x<3; x++) {
            int action=enemyActions[x];
            int qty = enemyQtys[x];
            Sprite enemyActionSprite = (action>-1 ? enemyActionSprites[action] : allEnemies[Mathf.Abs(action+1)].sprite);
            enemyActionUI[x].sprite=enemyActionSprite;

            enemyQuantityCircles[x].SetActive(qty>0);
            enemyQuantityTexts[x].text=(qty>0 ? "x"+qty.ToString() : "");
        }

        ticsSurvivedUI.text="Actions Survived: "+ticsSurvived.ToString();
        killsUI.text="Kills: "+kills.ToString();
        towersLostUI.text="Towers Lost: "+towersLost.ToString();
        damageDealtUI.text="Damage Dealt: "+damageDealt.ToString();
        damageTakenUI.text="Damage Taken: "+damageTaken.ToString();
        goldEarnedUI.text="Gold Earned: "+goldEarned.ToString();
        scienceEarnedUI.text="Science Earned: "+scienceEarned.ToString();
        scoreOverlayUI.text="Score: "+score.ToString();
        scoreUI.text="Score: "+score.ToString();
    }

    public float getScience() {
        return science;
    }
    
    public void setScience(float newScience) {
        science=newScience;
        updateUI();
    }

    public float getGold() {
        return gold;
    }
    
    public void setGold(float newGold) {
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

        TowerModifier towerModifier = getModifierByTowerTypeID(so.towerTypeID);

        newObject.init(this, gameObject, battlefieldContainer, so, towerModifier);

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

    public List<Science> getRandomSciences(int count) {
        List<Science> returnSciences = new List<Science>();

        for(int x=0; x<count; x++) {
            float upgradeRoll=Random.Range(0f,1f); 
            //40% of sciences will be Upgrade
            if (upgradeRoll<=0.4f) {
                Science newScience=createScienceFromSO(upgradeScience); 
                returnSciences.Add(newScience);
            }
            else {
                int randomElement=UnityEngine.Random.Range(0, allSciences.Count);
                Science newScience=createScienceFromSO(allSciences[randomElement]); 
                returnSciences.Add(newScience);
            }
        }
        return returnSciences;
    }

    //Create a science based on the class attached to the scienceSO
    public Science createScienceFromSO(ScienceSO scienceSO) {
        Science newScience = (Science)System.Activator.CreateInstance(System.Type.GetType(scienceSO.scienceClass));
        newScience.init(this, scienceSO);
        return newScience;
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

    //Spawns an enemy in an available slot in the top row. Will override player towers
    public void spawnEnemyInTopRow(BattlefieldObjectSO enemySO, int enemiesToSpawn) {
        
        List<int> availableSlots=new List<int>{90,91,92,93,94,95,96,97,98,99}; //CLEANUP
        List<int> availableSlotsSorted=ShuffleList(availableSlots);
        int enemiesSpawned=0;
        foreach (int slot in availableSlotsSorted) {
            if (!battleMapTiles[slot].hasEnemyObject()) {
                battleMapTiles[slot].createObjectInTile(enemySO);
                enemiesSpawned++;
                if (enemiesSpawned>=enemiesToSpawn) {
                    break;
                }
            }
        }
        //int slot=UnityEngine.Random.Range(90, 100);
    }

    public List<int> ShuffleList(List<int> list) {
        for (int i = 0; i < list.Count; i++) {
            list=_ShuffleListSwapItems(list, i, UnityEngine.Random.Range(0, list.Count-1));
        }
        return list;
    }
    
    //Helper function for ShuffleList
    private List<int> _ShuffleListSwapItems(List<int> list, int i, int j) {
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
        return list;
    }


    
    //Run the player action
    public void runPlayerTicAction() {
        if (nextPlayerAction==ACTION_WORK) {
            gold+=goldIncome;
            goldEarned+=goldIncome;
        }
        else if(nextPlayerAction==ACTION_STUDY) {
            science+=scienceIncome;
            scienceEarned+=scienceIncome;
        }
        else if(nextPlayerAction==ACTION_TINKER) {
            science-=researchCost;
            researchCost+=researchIncrementalCost;
            populateAndShowOverlay(ACTION_TINKER, researchOptionsToChooseFrom);
        }
        else if(nextPlayerAction==ACTION_BUILD_BASIC) {
            gold-=basicTowerCost;
            basicTowerCost+=basicTowerIncrementalCost;
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

        RectTransform overlayTransform=overlay.GetComponent<RectTransform>();
        overlayTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, optionCount==3 ? 1555 : 980); //CLEANUP - this is for 2/3 options
        
        Vector3 overlayPosition=overlayTransform.localPosition;
        overlayPosition.x=(optionCount==3 ? 770 : 460);
        overlayTransform.localPosition=overlayPosition;


        if(actionType==ACTION_BUILD_BASIC) {
            overlayTitleTXT.text="Build Tower";
            currentSelectableTowers = getRandomObjects(optionCount, ObjectOwner.player, 1); 
            int optionUpto=0;
            foreach (BattlefieldObjectSO thisObject in currentSelectableTowers) {
                TowerModifier modifier=getModifierByTowerTypeID(thisObject.towerTypeID);
                overlayTitles[optionUpto].text=thisObject.name;
                overlayImages[optionUpto].sprite=thisObject.sprite;
                overlayDescs[optionUpto].text=thisObject.desc+
                "\nDamage: "+(thisObject.damage+modifier.damage).ToString()+
                "\nRange: "+(thisObject.range+modifier.range).ToString()+
                "\nHP: "+(thisObject.hp+modifier.hp).ToString();
                optionUpto++;
            }
        }
        else if (actionType==ACTION_TINKER) {
            overlayTitleTXT.text="Tinker";
            currentSelectableSciences = getRandomSciences(optionCount); 
            int optionUpto=0;
            foreach (Science thisScience in currentSelectableSciences) {
                overlayTitles[optionUpto].text=thisScience.name;
                overlayImages[optionUpto].sprite=thisScience.sprite;
                overlayDescs[optionUpto].text=thisScience.desc;
                optionUpto++;
            }
        }

        overlay.SetActive(true);
        showingOverlay=true;
        selectOverlayMinimised=false;
        setPaused(true);
    }

    //Sets option availabilty based on whether we meet the requirements
    public void setActionButtonAvailability() {

        Color32 activeColour= new Color32(168,168,168, 255);
        Color32 inactiveColour= new Color32(65,65,65, 255);


        foreach(GameObject thisButton in playerActionButtons) {
            thisButton.GetComponent<Button>().interactable=true;
        }

        if (science<researchCost) {
            playerActionButtons[ACTION_TINKER].GetComponent<Button>().interactable=false;
            if (nextPlayerAction==ACTION_TINKER) {
                nextPlayerAction=ACTION_WORK;
            }
        }

        bool availableTowerContainers=false;
        foreach(GameObject towerContainer in unplacedTowerContainers) {
            if (towerContainer.transform.childCount==0) {
                availableTowerContainers=true;
            }
        }

        if (gold<basicTowerCost || !availableTowerContainers) {
            playerActionButtons[ACTION_BUILD_BASIC].GetComponent<Button>().interactable=false;
            if (nextPlayerAction==ACTION_BUILD_BASIC) {
                nextPlayerAction=ACTION_WORK;
            }
        }
        

        foreach(GameObject thisButton in playerActionButtons) {
            thisButton.GetComponent<Image>().color=(thisButton.GetComponent<Button>().interactable ? activeColour : inactiveColour);
        }    

        updateUI();

    }

    public void playObjectInSlot(int slot) {
        if (selectedTower==null) {
            return;
        }

        if (battleMapTiles[slot].hasObject()) {
            return;
        }

        if (battleMapTiles[slot].moveObjectToTile(selectedTower)) {
            selectedTower=null;
            battleMapTiles[slot].battlefieldObject.acquireTarget();
            playSound(SOUND_PLACE_TOWER, SOUND_PLACE_TOWER_VOLUME);
        }
    }

    
    public void resetCombatVars() {
        enemiesOnBattlefield=0;
        enemiesInTopRow=0;
        foreach (BattleMapTile tile in battleMapTiles) {
            tile.towersInRange=0;
            tile.enemiesInRange=0;
            if (!tile.hasObject()) {
                continue;
            }
            if (tile.hasEnemyObject()) {
                enemiesOnBattlefield++;
                if (tile.tilePosition>89) {
                    enemiesInTopRow++;
                }
            }
            tile.battlefieldObject.justAttacked=false;
            tile.battlefieldObject.justMoved=false;
            tile.battlefieldObject.assignedDamage=0;
        }
        Debug.Log("Enemies: "+enemiesOnBattlefield+" topRow="+enemiesInTopRow);
    }

    //Have player towers attack, then calculate next attacks
    public void runPlayerAttacks() {
        foreach (BattleMapTile tile in battleMapTiles) {
            if (!tile.hasPlayerObject()) {
                continue;
            }
            tile.battlefieldObject.damageTarget();
        }  
    }


    //Have player towers attack, then calculate next attacks
    public void acquirePlayerTargets() {
        
        //Targeting and damage calculation for dumb towers (Ballista and Fire)
        foreach (BattleMapTile tile in battleMapTiles) {
            if (!tile.hasPlayerObject()) {
                continue;
            }
            if (tile.battlefieldObject.towerTypeID!=TOWER_BALLISTA && tile.battlefieldObject.towerTypeID!=TOWER_FIRE) {
                continue;
            }

            tile.battlefieldObject.acquireTarget();
        }  

        //Targeting and damage calculation for smart towers (Arrow and Sniper). Since our dumb tower damage is assigned, we can be smarter here
        foreach (BattleMapTile tile in battleMapTiles) {
            if (!tile.hasPlayerObject()) {
                continue;
            }
            if (tile.battlefieldObject.towerTypeID==TOWER_BALLISTA || tile.battlefieldObject.towerTypeID==TOWER_FIRE) {
                continue;
            }

            tile.battlefieldObject.acquireTarget();
        }  
    }

    //Have player towers attack, then calculate next attacks
    public void calculateTilesInRange() {
        foreach (BattleMapTile tile in battleMapTiles) {
            if (!tile.hasObject()) {
                continue;
            }
            tile.battlefieldObject.calculateTilesInRange();
        }  
        foreach (BattleMapTile tile in battleMapTiles) {
            tile.updateUI();
        }
    }


    
    public void playSound(int selectedSound, float volume) {
        audioSource.PlayOneShot(sounds[selectedSound], volume);
    }

    


    /****************************************************************
                            ENEMY HANDLING
    ****************************************************************/

    public void runEnemyTicAction() {
    
        //Run this action
        int thisAction=enemyActions[0];
        if (thisAction<0) {
            //Spawn more enemies as it gets more difficult
            spawnEnemyInTopRow(allEnemies[Mathf.Abs(thisAction)-1], enemyQtys[0]);
        }
        else if(thisAction>ENEMY_ACTION_NOTHING) {
            moveEnemiesInDirection(thisAction);
        }
        
        
        //Calculate next action
        float chanceOfSpawn=.5f;
        if (enemiesOnBattlefield<5) {
            chanceOfSpawn=.8f;
        }
        else if (enemiesOnBattlefield>12) {
            chanceOfSpawn=.2f;
        }

        //Project enemies in top row when our newly added one pops.
        int calculatedEnemiesInTopRow=enemiesInTopRow;
        for(int x=1; x<3; x++) {
            if (enemyActions[x]<0) {
                calculatedEnemiesInTopRow+=enemyQtys[x];
            }
            if (enemyActions[x]>=ENEMY_ACTION_DOWN_LEFT && enemyActions[x]<=ENEMY_ACTION_DOWN_RIGHT) {
                calculatedEnemiesInTopRow=0;
            }
        }


        if (calculatedEnemiesInTopRow>7) {
            chanceOfSpawn=0f;
        }

        float spawnRoll=Random.Range(0f,1f);
        int nextAction=0;
        int nextActionQty=0;
        if (spawnRoll<=chanceOfSpawn) {
            int enemyLevel=1;
            if (ticsSurvived>=ADD_ULTRA_ENEMIES_AFTER_X_TICS) {
                enemyLevel=UnityEngine.Random.Range(2,4);
            }
            else if (ticsSurvived>=ADD_DIFFICULT_ENEMIES_AFTER_X_TICS) {
                enemyLevel=UnityEngine.Random.Range(1,3);
            }

            nextAction = -getRandomEnemy(enemyLevel)-1;
            nextActionQty=UnityEngine.Random.Range(1, Mathf.Max(2,2+Mathf.FloorToInt(ticsSurvived/INCREASE_NUMBER_OF_ENEMIES_EVERY_X_TICS)));
        }
        else {
            if (calculatedEnemiesInTopRow>7) {
                nextAction=UnityEngine.Random.Range(2, 5);
            }
            else {
                nextAction=UnityEngine.Random.Range(1, 6);
            }
        }

        //Shuffle Actions
        enemyQtys[0]=enemyQtys[1];
        enemyQtys[1]=enemyQtys[2];
        enemyQtys[2]=nextActionQty;
        enemyActions[0]=enemyActions[1];
        enemyActions[1]=enemyActions[2];
        enemyActions[2]=nextAction;
    }

    //Since all enemies move in the same diretion simultaneously, we need to process them in a specific sequence to minimise collissions
    //CLEANUP - make this work for grids that aren't 10x10
    public List<List<int>> getEnemyMovementSequences() {
        List<List<int>> returnList = new List<List<int>>();
        //returnList.Add(new List<int>()); //A dummy list to offset our IDs for ENEMY_ACTION_NOTHING
        for (int direction=0; direction<=ENEMY_ACTION_UP; direction++) {
            List<int> thisDirectionList = new List<int>();
            if (direction==ENEMY_ACTION_LEFT || direction==ENEMY_ACTION_DOWN_LEFT) {
                for(int x=0; x<=9; x++) {
                    for(int y=0; y<=9; y++) {
                        thisDirectionList.Add(x+(y*10));
                    }
                }
            }
            else if (direction==ENEMY_ACTION_RIGHT || direction==ENEMY_ACTION_DOWN_RIGHT) {
                for(int x=9; x>=0; x--) {
                    for(int y=0; y<=9; y++) {
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
            else if (direction==ENEMY_ACTION_UP) {
                for(int y=9; y>=0; y--) {
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
        if(direction<=ENEMY_ACTION_NOTHING || direction>ENEMY_ACTION_UP) {
            return;
        }

        List<int> tilesToMove = ENEMY_MOVEMENT_SEQUENCES[direction];

        foreach (int tileID in tilesToMove) {
            BattleMapTile sourceTile=battleMapTiles[tileID];
            if (!sourceTile.hasEnemyObject()) {
                continue; //No enemy in tile
            }

            if (tileID<10 && (direction==ENEMY_ACTION_DOWN_LEFT || direction==ENEMY_ACTION_DOWN || direction==ENEMY_ACTION_DOWN_RIGHT)) {
                loseGame();
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
        bool onTopEdge=(sourceTile>89);

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
        else if(direction==ENEMY_ACTION_UP && !onTopEdge) {
            return sourceTile+10;
        }

        return -1;
    }

    
    
    public void runEnemyAttacks() {
        foreach (BattleMapTile tile in battleMapTiles) {
            if (!tile.hasEnemyObject()) {
                continue;
            }
            tile.battlefieldObject.attackTowers();
        }  
    }





    /****************************************************************
                                EVENTS
    ****************************************************************/

    public void clickPlayerActionButton(int buttonClicked) {
        if(buttonClicked==ACTION_TINKER && !playerActionButtons[ACTION_TINKER].GetComponent<Button>().interactable)  {
            return;
        }
        if(buttonClicked==ACTION_BUILD_BASIC && !playerActionButtons[ACTION_BUILD_BASIC].GetComponent<Button>().interactable)  {
            return;
        }
        if (showingOverlay) {
            return;    
        }


        nextPlayerAction=lastOverlayAction=buttonClicked;
        playSound(SOUND_CLICK, SOUND_CLICK_VOLUME);
        updateUI();
    }


    public void clickToggleOverlayButton() {
        selectOverlayMinimised=!selectOverlayMinimised;
        RectTransform overlayTransform=overlay.GetComponent<RectTransform>();

        if (selectOverlayMinimised) {
            previousOverlayWidth=overlayTransform.rect.width;
            previousOverlayHeight=overlayTransform.rect.height;

            for(int x=0; x<3; x++) {
               overlayOptionVisibility[x]=overlayObjects[x].activeSelf;
               overlayObjects[x].SetActive(false);
            }
        }
        else {
            for(int x=0; x<3; x++) {
              overlayObjects[x].SetActive(overlayOptionVisibility[x]);
            }
        }
        
        overlayTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, selectOverlayMinimised ? 120 : previousOverlayWidth);
        overlayTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, selectOverlayMinimised ? 120 : previousOverlayHeight);

        //+ / - button to minimise/maximise
        toggleOverlayTXT.text=selectOverlayMinimised ? "+" : "-";
        float yModifier=-.06f;
        Transform toggleTransform = toggleOverlayTXT.gameObject.transform;
        Vector3 togglePosition = toggleTransform.position;
        togglePosition.y+=selectOverlayMinimised ? yModifier : -yModifier;
        toggleTransform.position=togglePosition;

        //overlayHolder.SetActive(!selectOverlayMinimised);
    }


    public void clickOptionButton(int buttonClicked) {
        overlay.SetActive(false);
        showingOverlay=false;
        setPaused(false);

        setActionButtonAvailability();

        Debug.Log("Option button clicked "+buttonClicked+", lastOverlayction="+lastOverlayAction);

        if (lastOverlayAction==ACTION_BUILD_BASIC) {
           createTowerAndMakeAvailable(currentSelectableTowers[buttonClicked]);
           playSound(SOUND_CLICK, SOUND_CLICK_VOLUME);
        }
        else if (lastOverlayAction==ACTION_TINKER) {
            currentSelectableSciences[buttonClicked].onActivate();
            playSound(SOUND_SCIENCE, SOUND_SCIENCE_VOLUME);
            updateUI();
        }
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

    public void togglePause() {
        if (showingOverlay) {
            return;
        }
        setPaused(!paused);
        playSound(SOUND_CLICK, SOUND_CLICK_VOLUME);
    }

    public void clickPauseButton() {
        if (showingOverlay) {
            return;
        }
        setPaused(true);
        updateUI();
        playSound(SOUND_CLICK, SOUND_CLICK_VOLUME);
    }

    public void clickPlayButton() {
        if (showingOverlay) {
            return;
        }
        setPaused(false);
        gameSpeed=NORMAL_GAME_SPEED;
        updateUI();
        playSound(SOUND_CLICK, SOUND_CLICK_VOLUME);
    }

    public void clickFastForwardButton() {
        if (showingOverlay) {
            return;
        }
        setPaused(false);
        gameSpeed=FAST_GAME_SPEED;
        updateUI();
        playSound(SOUND_CLICK, SOUND_CLICK_VOLUME);
    }

    public void clickShowStatsOverlay() {
        if (showingOverlay) {
            return;   
        }
        toggleStatsOverlay(true);
    }

    public void toggleStatsOverlay(bool showOverlay) {
        if (gameOver && !showOverlay) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            startGame();
        }
        else {
            statsOverlay.SetActive(showOverlay);
            setPaused(showOverlay);
        }
        showingOverlay=showOverlay;
    }


    public void handleKeyPress() {
        //Keys are disable when an overlay is showing
        if (showingOverlay) {
            return;
        }
        
        if (Input.GetKeyDown("space")) {
            togglePause();
        }
        else if (Input.GetKeyDown("w")) {
            clickPlayerActionButton(ACTION_WORK);
        }
        else if (Input.GetKeyDown("s")) {
            clickPlayerActionButton(ACTION_STUDY);
        }
        else if (Input.GetKeyDown("a")) {
            clickPlayerActionButton(ACTION_TINKER);
        }
        else if (Input.GetKeyDown("d")) {
            clickPlayerActionButton(ACTION_BUILD_BASIC);
        }
        else if (Input.GetKeyDown("1")) {
            clickPauseButton();
        }
        else if (Input.GetKeyDown("2")) {
            clickPlayButton();
        }
        else if (Input.GetKeyDown("3")) {
            clickFastForwardButton();
        }
    }


    public void loseGame() {
        toggleStatsOverlay(true);
        loseOverlay.SetActive(true);
        showingOverlay=true;
        gameOver=true;
        statsButtonUI.text="Try Again";

        for(int x=0; x<3; x++) {
            enemyQuantityCircles[x].SetActive(false);
            enemyQuantityTexts[x].text="";
        }

        StartCoroutine(SaveScore());
    }

     public void setActiveCanvas(string tag) {
        canvases.ForEach((canvas) => {
                canvas.SetActive(canvas.tag==tag);
        });
     }


    //Gets the rotation direction for a projectile's rotation (based on a north facing sprite)
    /* public static float getRotationDirectionForProjectile (Vector2 origin, Vector2 destination) {
        //TODO - this is pretty hacky atm
     }*/
}
