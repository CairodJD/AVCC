using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rembobinage : MonoBehaviour {


    public Node node; // ce node the portals is placed on


    private void Awake() {
        node = GameManager.GM.generator.nodeFrom(transform.position);
    }



    private void OnTriggerEnter(Collider other) {
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        if (other.CompareTag("Enemy") && enemy.canTP == true) {
            other.gameObject.transform.position = TwoNodeBehind(node).worldPosition + Vector3.back + Vector3.up/2;
            GameManager.GM.enemyTP();
            enemy.canTP = false;
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

    public Node TwoNodeBehind(Node node) {
        int yBehind = node.gridY+ 2;
        if (yBehind > GameManager.GM.generator.gridSizeY-1) {
            yBehind = GameManager.GM.generator.gridSizeY - 1;
        }
        Node side = GameManager.GM.generator.get(node.gridX, yBehind);
        return side;
    }
}
