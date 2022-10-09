using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RepairScience : Science {
    public override void onActivate() {
        foreach (BattleMapTile tile in gameManager.battleMapTiles) {
            if (tile.hasPlayerObject()) {
                tile.battlefieldObject.hp=Mathf.Min(tile.battlefieldObject.maxHP, tile.battlefieldObject.hp+(int)(tile.battlefieldObject.maxHP*3));
                tile.battlefieldObject.updateUI();
            }
        }
    }
}
