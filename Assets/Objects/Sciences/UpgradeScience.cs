using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UpgradeScience : Science {

    private BattlefieldObjectSO upgradeTowerSO;
    private string statToUpgrade;  //hp, range, damage //CLEANUP - name enum

    private int hpBoost=20;
    private float rangeBoost=0.5f;
    private int damageBoost=5;

    public override void onInit() {
        //Figure out which tower we're upgrading
        int randomElement=UnityEngine.Random.Range(0, gameManager.allTowers.Count);
        upgradeTowerSO=gameManager.allTowers[randomElement];

        //Figure out which stat we're upgrading
        int stat=UnityEngine.Random.Range(0, 3);
        if (stat==0) {
            statToUpgrade="HP";
            this.desc="Increase the HP of "+upgradeTowerSO.name+"s by "+hpBoost;
        }
        else if(stat==1) {
            statToUpgrade="Range";
            this.desc="Increase the Range of "+upgradeTowerSO.name+"s by "+rangeBoost;
        }
        else if(stat==2) {
            statToUpgrade="Damage";
            this.desc="Increase the Damage of "+upgradeTowerSO.name+"s by "+damageBoost;
        }


        //Modify the display of this science
        this.sprite=upgradeTowerSO.sprite;
        this.name="Upgrade "+upgradeTowerSO.name+" "+statToUpgrade;
    } 
    
    public override void onActivate() {
        TowerModifier towerModifier=gameManager.getModifierByTowerTypeID(upgradeTowerSO.towerTypeID);

        if (statToUpgrade=="HP") {
            towerModifier.hp+=hpBoost;
        }
        else if (statToUpgrade=="Range") {
            towerModifier.range+=rangeBoost;
        }
        else if (statToUpgrade=="Damage") {
            towerModifier.damage+=damageBoost;
        }
    }
}
