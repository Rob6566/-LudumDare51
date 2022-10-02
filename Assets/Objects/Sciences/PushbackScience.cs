using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PushbackScience : Science {
    public override void onActivate() {
        gameManager.moveEnemiesInDirection(GameManager.ENEMY_ACTION_UP);
    }
}
