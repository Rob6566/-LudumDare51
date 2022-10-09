using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "New Object", fileName = "New Object")]
public class BattlefieldObjectSO : ScriptableObject {
    public int towerTypeID;
    public ObjectOwner objectOwner;
    public string name;
    public string desc;
    public Sprite sprite;
    public int hp;
    public int damage;
    public int level;
    public float range;
    public bool AOE=false;
    public float projectileSpeed=1f;
}
