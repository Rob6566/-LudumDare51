using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

//A science class. Abstract superclass - we'll be implementing abilities in sub-classes
public class Science
{
    protected GameManager gameManager;
    public ScienceSO scienceSO;
    public Sprite sprite;
    public string name;
    public string desc;

    public void init(GameManager newGameManager, ScienceSO newScienceSO) {
        gameManager=newGameManager;
        scienceSO=newScienceSO;
        sprite=scienceSO.sprite;
        name=scienceSO.name;
        desc=scienceSO.desc;
        this.onInit();
    }

    //Overridden in subclasses
    public virtual void onInit() {
    
    } 

    //Overridden in subclasses
    public virtual void onActivate() {
    
    } 
}
