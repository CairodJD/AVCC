using UnityEngine;



public class Node {
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public int gCost; // disntace from start
    public int hCost; // distance from end
    public GameObject tile;

    public Node(Vector3 worldP, int gridx, int gridy, GameObject ntile) {
        worldPosition = worldP;
        gridX = gridx;
        gridY = gridy;
        tile = ntile;
    }


    public int fCost() {
        return gCost + hCost;
    }


   

}

