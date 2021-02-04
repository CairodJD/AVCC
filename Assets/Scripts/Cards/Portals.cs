using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Portals : MonoBehaviour {

    /**
     * 2 portal lié bi direction
     */

    public Node node; // ce node the portals is placed on
    public Portals linked;

    private void Awake() {
        node = GameManager.GM.generator.nodeFrom(transform.position);
        GameManager.GM.generator.newNode += OnNewNode;
    }

    private void OnNewNode(GameObject node) {
        Portals tp = node.GetComponent<Portals>();
        
        if (tp != null && tp.gameObject.GetInstanceID() != gameObject.GetInstanceID() && linked == null) {
            linked = tp;
            tp.linked = this;
        }
    }


    private void OnDestroy() {
        GameManager.GM.generator.newNode -= OnNewNode;
    }
    /* Quanbd un ennemy passr sur cette cellule , le téléporté sur la node linked
     * **/
    private void OnTriggerEnter(Collider other) {
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        if (other.CompareTag("Enemy") && enemy.canTP == true ) {
            other.gameObject.transform.position = linked.node.worldPosition + Vector3.back + Vector3.up/2;
            GameManager.GM.enemyTP();
            enemy.canTP = false; 
            //Debug.Log("téléporte l'enemie sur la node " + linked.node.worldPosition);
            linked.node.tile.GetComponent<MeshRenderer>().material.color = Color.green;
        }
    }


    private void OnTriggerExit(Collider other) {
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        if (other.CompareTag("Enemy")) {
            enemy.canTP = !enemy.canTP;
        }
    }
}
