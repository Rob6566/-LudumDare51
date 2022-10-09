using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireCircleHandler : MonoBehaviour
{
    float travelTime;
    float timeSpent;
    float range;
    GameManager gameManager;


    public void init(float newTravelTime, float newRange, GameManager newGameManager) {
        travelTime=newTravelTime;
        range=newRange;
        timeSpent=0f;
        gameManager=newGameManager;
    }

    void Update() {
        if (gameManager==null) {
            return;
        }
        
        timeSpent+=Time.deltaTime*gameManager.gameSpeed;

        float size=1f+((timeSpent/travelTime)*(range));

        gameObject.transform.localScale=new Vector3(size, size, size);

        Color32 colour=new Color32(255,255,255,(byte)Mathf.Floor(180-((timeSpent/travelTime)*180)));
        gameObject.GetComponent<Image>().color=colour;

        if (timeSpent>=travelTime) {
            Destroy(gameObject);
        }
    }
}
