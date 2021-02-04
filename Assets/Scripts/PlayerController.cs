using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public Camera cam;
    
    public GameObject CardsHand;
    public int cardsHandNumber = 3;

    public bool isSelectingNode = false;
    private Node currentOvering;
    private List<CardDisplay> hands;
    private int selectedCardIndex = -1;
    private int selectedCardClickCount = 0;


    public GameObject pausePanel;

    private List<Node> selectedNodes = new List<Node>();

    public float score = 0;


    private void Awake() {
        hands = new List<CardDisplay>(cardsHandNumber);
        getHand();
    }


    private void Update() {


        if (Input.GetKeyDown(KeyCode.Space) ) {
            if (Time.timeScale != 0) {
                pausePanel.SetActive(true);
                Time.timeScale = 0;
            } else {
                Time.timeScale = 1;
                pausePanel.SetActive(false);
            }
            
        }

        


        // on first click -> passer en mode selection de case pour cette carte
        // ca selectionne le prefab de la carte ( place ca dans le scriptable object )
        // then quand on passe la souris sur une node on voit une previsualisation de l'endroit
        // then 2nd click -> on place le prefab sur les case previsualisé


        //over 
        if (isSelectingNode && NodefromMouse != null) {
            CardDisplay currCard = hands[selectedCardIndex];

            // old
            if (currentOvering != null) {
                currentOvering.tile.GetComponent<MeshRenderer>().material.color = Color.white;
            }
            
            //getnew   previsualisation
            currentOvering = NodefromMouse;
            currentOvering.tile.GetComponent<MeshRenderer>().material.color = Color.blue;


            // click faire l'effect de la carte en fonction du click count
            if (Input.GetMouseButtonDown(0) && currCard.isReady()) {
                GameManager.GM.placedCard();
                selectedNodes.Add(currentOvering);
                selectedCardClickCount++;
                if (selectedCardClickCount == currCard.card.availabeClicks) {
                    Transform holder = transform.Find("Generated");
                    for (int i = 0; i < selectedNodes.Count; i++) {
                        //remplacer la/les node par le prefab de la carte jouée
                        Node toplace = selectedNodes[i];
                        
                        StartCoroutine(tkt(currCard,toplace,holder));
                    }
                    // on a fini d'utiliser la carte , la retirer et go back to selection de carte
                    currCard.CoolDown();
                    isSelectingNode = false;
                    selectedCardClickCount = 0;
                }

                
    
                //currentOvering.tile = hands[selectedCardIndex].NodePrefab;
                
            }


        }

       
    }


    public IEnumerator tkt (CardDisplay currCard, Node toplace,Transform holder) {
        GameObject newTile = Instantiate(currCard.card.NodePrefab, toplace.worldPosition, Quaternion.Euler(Vector3.right * 90), holder);

        GameManager.GM.generator.setNode(toplace, newTile);
        yield return new WaitForSeconds(currCard.card.persitence);
        // reset la cell
        Destroy(newTile);
        GameManager.GM.generator.reset(toplace);
    }



    public Node NodefromMouse {
        get {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                return GameManager.GM.generator.nodeFrom(hit.point);
            }
            return null;
        }
    }




    public void getHand() {
        foreach (Transform carte in CardsHand.transform) {
            hands.Add(carte.GetComponent<CardDisplay>());
        }
    }


    public void selectCard(int index) {
        selectedNodes.Clear();
        selectedCardIndex = index;
        if (hands[selectedCardIndex].card.isGlobal) {
            //jouer l'effet de la carte 
            hands[selectedCardIndex].card.NodePrefab.GetComponent<Rollback>().effect();
            GameManager.GM.RollBackSE();
            hands[selectedCardIndex].CoolDown();
        } else {
            isSelectingNode = true;
        }
       
    }


}
