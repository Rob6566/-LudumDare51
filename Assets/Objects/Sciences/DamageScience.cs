using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DamageScience : Science {
    public override void onActivate() {
        foreach (BattleMapTile tile in gameManager.battleMapTiles) {
            if (tile.hasEnemyObject()) {
                tile.takeDamage(50);
            }
        }
    }
}
