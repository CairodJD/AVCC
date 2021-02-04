using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    
   
    public float speed  = 2f;
    public bool canTP = true;




    public void behaveUpdate() {
       float step = speed * Time.deltaTime;
       transform.Translate(Vector3.forward * step );

    }


    private void Update() {
        behaveUpdate();   
    }

    private void OnDestroy() {
        GameManager.GM.enemyDeath();
    }


}

