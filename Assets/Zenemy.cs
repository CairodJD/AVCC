using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zenemy : Enemy {

    // un enemie zigzagent sur le terrain et esquivant les carte devant elle grace a sont "capteur" lui 
    // indiquant le type de carte 1 case devant elle 

    // spanw comme un enemy normal mais tourne soit a gauche/droite si il voit une carte devant lui
    // ou les bords du niveau bien sur..



    // Start is called before the first frame update
    private void Start() {
        
    }

    private void Update() {
        
    }

    private void turnLeft() {

    }

    private void turnRight() {

    }

    private Node ahead(int nAhead = 1) {// une ou deux cellule 
        Node pos = GameManager.GM.generator.nodeFrom(transform.position);
        Node test = GameManager.GM.generator.nodeFrom(pos.worldPosition + (Vector3.forward * nAhead));
        if (GameManager.GM.generator.isOnMap(test) && pos != test) {
            return test;
        }
        return null;
    }
}
