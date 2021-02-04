using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blackhole : MonoBehaviour {

    public Node node; // ce node the portals is placed on


    private void Awake() {
        //node = GameManager.GM.generator.nodeFrom(transform.position);
    }



    private void OnTriggerEnter(Collider other) {
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        if (other.CompareTag("Enemy") && enemy.canTP == true) {
            Destroy(other.gameObject);
            //Debug.Log("téléporte l'enemie sur la node " + linked.node.worldPosition);
            //exitNode.tile.GetComponent<MeshRenderer>().material.color = Color.green;
        }
    }

    

}
