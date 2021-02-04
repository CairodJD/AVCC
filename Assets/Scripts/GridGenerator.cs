using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.AI;

public class GridGenerator : MonoBehaviour{

    #region Vavs

    public Transform holder;
    public Transform casePrefab;
    public Transform blackholeprefab;
    public Vector2 mapSize = new Vector2(4, 7);

    [Range(0, 1)]
    public float pacing; // % of pace between tile
    public Transform cubePrefab; // External wall prefab
    public LayerMask holeMask;
    public float holeChance = 0.1f;

    public float nodeRadius = 0.5f; // what each individual node represent;
    [HideInInspector]
    public int gridSizeX, gridSizeY;
    float nodeDiameter;
    List<Node> AstarPath;

     
    [HideInInspector]
    public Vector3 PlayerSpawnPos;
    [HideInInspector]
    public Vector3 ExitPos;

    
    Collider endCollider;
    Node[,] grid;
    Queue<Vector3> intersections;
    Vector3 worldBottomLeft;
    Camera main;
    float externalWallLengh = 2f;

    #endregion

    static Vector3[] CROSS_NEIGHBOR_SET = new Vector3[] {
        new Vector3(-1,0,0),
        new Vector3(0,0,-1),
        new Vector3(0,0,1),
        new Vector3(1,0,0),
    };
   
    //Triggered when player has completed the level
    public event Action playerCompletedLevel;

    public event Action<GameObject> newNode;

    public List<Vector3> enemiesSpawnPos = new List<Vector3>();

    private void OnDestroy() {
        if (holder) {
            Destroy(holder.gameObject);
        }
    }

    private void Awake() {
        nodeDiameter = nodeRadius * 2;
        //endCollider = GetComponent<BoxCollider>();
        main = Camera.main;
        if (holder) {
            Destroy(holder.gameObject);
        }
        holder = new GameObject("Generated").transform;
        holder.parent = transform;

    }


    public void SetGame(Vector2 nMapSize) {
        if (nMapSize != Vector2.zero) {
            mapSize = nMapSize;
            gridSizeX = Mathf.RoundToInt(mapSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(mapSize.y / nodeDiameter);
            worldBottomLeft = transform.position - Vector3.right * mapSize.x / 2 - Vector3.forward * mapSize.y / 2;
            GenerateMap();

        } else {
            throw new NullReferenceException();
        }
      
    }


    public void GenerateMap() {  
        grid = new Node[gridSizeX, gridSizeY];
   
        /* Grid */
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                Transform newTile;
                Vector3 pos = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);

                if (Random.value < holeChance && y != (gridSizeY -1) ) {
                    // this node = hole
                    newTile = Instantiate(blackholeprefab, pos, Quaternion.Euler(Vector3.right * 90), transform) as Transform;
                    newTile.localScale = Vector3.one * (1 - pacing);
                    newTile.parent = holder;
                    newTile.gameObject.name = x + " : " + y;
                    newTile.GetComponent<MeshRenderer>().material.color = Color.black;
                    //NavMeshObstacle ob = newTile.gameObject.AddComponent<NavMeshObstacle>();
                    //ob.carving = true;
                } else {
                    newTile = Instantiate(casePrefab, pos, Quaternion.Euler(Vector3.right * 90), transform) as Transform;
                    newTile.localScale = Vector3.one * (1 - pacing);
                    newTile.parent = holder;
                    newTile.gameObject.name = x + " : " + y;
                    //Populate grid and list                 
                }

                grid[x, y] = new Node(pos, x, y, newTile.gameObject);

                //Player spawn pos
                if (y == (gridSizeY - 1)) {
                    enemiesSpawnPos.Add(new Vector3(pos.x,pos.y,pos.z + 1f) );
                    //newTile.GetComponent<MeshRenderer>().material.color = Color.red;
                }
            }
        }
    }
    //remet la celle a son etat init avec le caseprefab
    public void reset(Node toplace) {
        GameObject newtile =  Instantiate(casePrefab, toplace.worldPosition, Quaternion.Euler(Vector3.right * 90), transform).gameObject;
        setNode(toplace, newtile);
    }



    //returns the node based on Worlpos
    public Node nodeFrom(Vector3 WorldPos) {
        //on cherche a quel poucentage de la grid la "worldPos" se trouve 
        // lets say WorldPOS = (0,0) le centre de la grille 
        // percentX sera egal a 0.5 vu qu'on se trouve au centre 
        float percentX = (WorldPos.x / mapSize.x) + 0.5f;
        float percentY = (WorldPos.z / mapSize.y) + 0.5f;
        // valeur entre 0 et 1 pour pouvoir la multiplier a notre gridsize
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        int x = Mathf.RoundToInt((gridSizeX-1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
 
        if (( x >= 0 && x < mapSize.x)  && (y >= 0 && y < mapSize.y)) {
            return grid[x, y];
        }
        return null;
    }

    //doit retourner les voisins accésibles
    public List<Node> getNeighbour(Node curr) {
        List<Node> neighbors = new List<Node>();

        
        foreach (Vector3 item in CROSS_NEIGHBOR_SET) {
            Vector3 test = curr.worldPosition + item;
            Node n = nodeFrom(test);
          
            if (isOnMap(n) && n != curr && !areAccessible(curr, n)) {
                neighbors.Add(n);
            }

        }


        return neighbors;
    }

    //retourne true si il y un mur entre ces 2 nodes
    private bool areAccessible(Node a,Node b) {
        Debug.DrawRay(a.worldPosition + Vector3.up / 2, (b.worldPosition + Vector3.up / 2) - (a.worldPosition + Vector3.up / 2), Color.red);
        if (Physics.Raycast(a.worldPosition + Vector3.up / 2, (b.worldPosition + Vector3.up / 2) - (a.worldPosition + Vector3.up / 2), 1f, holeMask)) {
            return true;
        }
        return false;
    }

    //return true if test Node is part of the grid
    public bool isOnMap(Node test) {
        if ( test != null  && (test.gridX >=0 && test.gridX < mapSize.x) && (test.gridY >= 0 && test.gridY < mapSize.y)) {
            return true;
        }
        return false;
    }




    // return Manathan distance between 2 nodes
    int getDistance(Node nodeA, Node nodeB) {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            playerCompletedLevel(); // trigger event
        }
    }


  
    // find shortest path form start to target
    List<Node> findPathAstar(Vector3 startPos, Vector3 targetPos) {
        Node startNode = nodeFrom(startPos);
        Node targetNode = nodeFrom(targetPos);

        //path we're looking for
        List<Node> path = null;

        // key = Value.parent to retrace path
        IDictionary<Node, Node> parentNodes = new Dictionary<Node, Node>();

        List<Node> openNodes = new List<Node>(); // Nodes to check
        HashSet<Node> closedNodes = new HashSet<Node>(); // already checked nodes
        openNodes.Add(startNode);

        while (openNodes.Count > 0) {
            Node node = openNodes[0];
 
           // lowest fcost || lowest hcost
            for (int i = 1; i < openNodes.Count; i++) {
                if (openNodes[i].fCost() < node.fCost() || openNodes[i].fCost() == node.fCost()) {
                    if (openNodes[i].hCost < node.hCost)
                        node = openNodes[i];
                }
            }

            openNodes.Remove(node);
            closedNodes.Add(node);

            //path found
            if (node == targetNode) {
                path = new List<Node>();

                while (parentNodes.ContainsKey(node)) {
                    path.Add(node);
                    node = parentNodes[node];
                }

                break;
            }

            foreach (Node neighbour in getNeighbour(node)) {
              
                if (closedNodes.Contains(neighbour)) {
                    continue;
                }
              
                int newCostToNeighbour = node.gCost + getDistance(node, neighbour);
                if (newCostToNeighbour < neighbour.gCost || !openNodes.Contains(neighbour)) {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = getDistance(neighbour, targetNode);

                    parentNodes[neighbour] = node;


                    if (!openNodes.Contains(neighbour)) {
                        openNodes.Add(neighbour);
                    }
                      
                }
            }
        }
        return path;
    }

    // try to return a path to ExitPos from a giwen pos
    public List<Node> getPath(Vector3 pos) {
        return findPathAstar(pos, ExitPos);
    }

    public Node get(int x , int y) {
        return grid[x, y];
    }

    public void setNode(int x , int y,GameObject prefab) {
        grid[x, y].tile = prefab;
    }
    public void setNode(Node with, GameObject prefab) {
        prefab.transform.localScale = Vector3.one * (1 - pacing);
        prefab.transform.parent = holder;
        Destroy(grid[with.gridX, with.gridY].tile);
        grid[with.gridX, with.gridY].tile = prefab;

        if (newNode != null) {
            newNode(grid[with.gridX, with.gridY].tile);
        }
    }


    private int toInt(float a) {
        return Mathf.RoundToInt(a);
    }
}
