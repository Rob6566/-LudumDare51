using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "New Science", fileName = "New Science")]
public class ScienceSO : ScriptableObject {
    public string name;
    public string desc;
    public Sprite sprite;
    public string scienceClass;
}
