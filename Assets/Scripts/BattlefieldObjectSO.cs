using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "New Object", fileName = "New Object")]
public class BattlefieldObjectSO : ScriptableObject {
    public ObjectOwner objectOwner;
    public string name;
    public Sprite sprite;
    public int hp;
    public int damage;
    public int level;
    public float range;
}
