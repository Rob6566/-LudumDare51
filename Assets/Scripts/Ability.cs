using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Ability {
    public virtual void onGainIncome() {}
    public virtual void onDefending() {}
    public virtual void onAttacking() {}
    public virtual void onTick() {}
    public virtual void onPlay() {}
}
