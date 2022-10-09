using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHandler : MonoBehaviour
{
    Vector3 origin;
    Vector3 destination;
    float travelTime;
    float timeSpent;
    GameManager gameManager;


    public void init(float newTravelTime, Vector3 newOrigin,  Vector3 newDestination, GameManager newGameManager) {
        origin=newOrigin;
        destination=newDestination;
        travelTime=newTravelTime;
        timeSpent=0f;
        gameManager=newGameManager;
        gameObject.transform.localScale=new Vector3(.5f, .5f, .5f);
        gameObject.transform.rotation= Quaternion.LookRotation(newOrigin, newDestination);
    }

    void Update() {
        if (destination==null || origin==null) {
            return;
        }
        
        timeSpent+=Time.deltaTime*gameManager.gameSpeed;

        gameObject.transform.position=origin+((destination-origin)*(timeSpent/travelTime));

        if (timeSpent>=travelTime) {
            Destroy(gameObject);
        }
    }
}
