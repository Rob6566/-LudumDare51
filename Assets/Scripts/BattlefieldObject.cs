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
    public int damage;
    public int level;
    public float range;

    public GameObject rootGameObject;
    public GameObject hpBar1;
    public GameObject hpBar2;
    public GameObject hpBar3;
    public GameObject hpBar4;
    public GameObject hpBar5;

    public Image image;
    TextMeshProUGUI txtHP;
    TextMeshProUGUI txtPower;

    public void init(GameManager newGameManager, GameObject gameObject, GameObject container, BattlefieldObjectSO newBattlefieldObjectSO) {
        gameManager=newGameManager;
        hp=newBattlefieldObjectSO.hp;
        name=newBattlefieldObjectSO.name;
        sprite=newBattlefieldObjectSO.sprite;
        damage=newBattlefieldObjectSO.damage;
        level=newBattlefieldObjectSO.level;
        range=newBattlefieldObjectSO.range;
        objectOwner=newBattlefieldObjectSO.objectOwner;
        rootGameObject=gameObject;

        image=rootGameObject.transform.GetComponent<Image>();
        txtPower=rootGameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        txtHP=rootGameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        image.sprite=newBattlefieldObjectSO.sprite;

        updateUI();
        //assignUIControls()
    }


    public void updateUI() {
        txtPower.text=damage.ToString();
        txtHP.text=hp.ToString();
    }
}
