using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rollback : MonoBehaviour {

    public Node node; // ce node the portals is placed on


    private void Awake() {
        node = GameManager.GM.generator.nodeFrom(transform.position);
    }

    public void effect() {
        int y = GameManager.GM.generator.gridSizeY - 1; 
        int x = GameManager.GM.generator.gridSizeX - 1;
        foreach (Enemy item in GameManager.GM.GetEnemies()) {
            item.gameObject.transform.position = GameManager.GM.generator.get(x,y).worldPosition + Vector3.up/2;
            x--;
            if (x < 0) {
                y--;
                x = GameManager.GM.generator.gridSizeX - 1;
            }
        }
    }


}

