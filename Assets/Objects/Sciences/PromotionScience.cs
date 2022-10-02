using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PromotionScience : Science {
    public override void onActivate() {
        gameManager.goldIncome=gameManager.goldIncome+2;
    }
}
