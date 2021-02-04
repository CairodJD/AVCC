using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Shunpo : MonoBehaviour {

    // random sideStep left or right

    public Node node; // ce node the portals is placed on


    private void Awake() {
        node = GameManager.GM.generator.nodeFrom(transform.position);
    }


    /* Quanbd un ennemy passr sur cette cellule , le téléporté sur la node linked
    * **/
    private void OnTriggerEnter(Collider other) {
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        if (other.CompareTag("Enemy") && enemy.canTP == true) {
            Debug.Log(other.gameObject);
            Node rand = randomSide(node);
            if (rand != null) {
                other.gameObject.transform.position = rand.worldPosition + Vector3.up/2;
                GameManager.GM.enemyTP();
                enemy.canTP = false;
            }
            
            //Debug.Log("téléporte l'enemie sur la node " + linked.node.worldPosition);
            //exitNode.tile.GetComponent<MeshRenderer>().material.color = Color.green;
        }
    }

    private void OnTriggerExit(Collider other) {
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        if (other.CompareTag("Enemy")) {
            enemy.canTP = !enemy.canTP;
        }
    }

    public Node randomSide(Node node) {
        int randX = Random.value > 0.5f ? node.gridX - 1  : node.gridX + 1;
        if (randX < 0 ) {
            randX = node.gridX + 1;
        } else if (randX > GameManager.GM.generator.gridSizeX-1) {
            randX = node.gridX - 1;
        }
        Node side = GameManager.GM.generator.get(randX, node.gridY);


        return side;
    }

}
