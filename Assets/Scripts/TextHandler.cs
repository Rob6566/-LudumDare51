using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextHandler : MonoBehaviour
{
    Vector3 origin;
    Vector3 destination;
    float travelTime;
    float timeSpent;
    GameManager gameManager;


    public void init(float newTravelTime, Vector3 newOrigin, GameManager newGameManager, string text) {
        origin=newOrigin+new Vector3(0f, .35f, 0);
        travelTime=newTravelTime;
        timeSpent=0f;
        gameManager=newGameManager;
        gameObject.transform.localScale=new Vector3(1f, 1f, 1f);
        destination=origin+new Vector3(0,1,0);
        gameObject.GetComponent<TextMeshProUGUI>().text=text;
        gameObject.transform.position=origin;
    }

    void Update() {
        if (destination==null || origin==null) {
            return;
        }
        
        timeSpent+=Time.deltaTime*gameManager.gameSpeed;

        //Hover in starting position for half our time
        if (timeSpent<(travelTime/2)) {
            return;
        }

        float reducedTravelTime=travelTime-(travelTime/2);
        float reducedTimeSpent=timeSpent-(travelTime/2);

        gameObject.transform.position=origin+((destination-origin)*(reducedTimeSpent/reducedTravelTime));
        gameObject.GetComponent<TextMeshProUGUI>().color=new Color32(0,0,0,(byte)Mathf.Floor(255-(255*(reducedTimeSpent/reducedTravelTime))));

        if (timeSpent>=travelTime) {
            Destroy(gameObject);
        }
    }
}
