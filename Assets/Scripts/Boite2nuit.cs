using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Boite2nuit : MonoBehaviour {

    public GameObject end;
    public GameObject uiCards;
    public GameObject listOfScore;

    public Button replay;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Enemy")) {
            GameManager.GM.defeat();
            Time.timeScale = 0;
            GameManager.GM.controller.score = (float) Time.time - GameManager.GM.first_time; 
            end.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = "Your score : " + GameManager.GM.controller.score + " sc" ;
            GameManager.GM.SendScore(GameManager.GM.controller.score, listOfScore);
            uiCards.SetActive(false);
            end.SetActive(true);
           
        }
    }
}
