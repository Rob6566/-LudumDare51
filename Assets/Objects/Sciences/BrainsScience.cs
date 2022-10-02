using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BrainsScience : Science {
    public override void onActivate() {
        gameManager.scienceIncome=gameManager.scienceIncome+2;
    }
}
