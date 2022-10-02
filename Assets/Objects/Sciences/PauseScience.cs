using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PauseScience : Science {
    public override void onActivate() {
        gameManager.enemyActions[0]=gameManager.enemyActions[1]=GameManager.ENEMY_ACTION_NOTHING;
    }
}
