using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardDisplay : BaseCard, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    public Card card;

    public Text nameText;
    public Text description;
    public Image artwork;

    public Image cooldown;




    public void CoolDown() {
        cooldown.fillAmount = 1;
        StartCoolDown(card.cooldown);
    }



    private void Update() {
        if (onCD) {
            cooldown.fillAmount -= 1 / card.cooldown * Time.deltaTime;
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (!onCD) {

            int index = eventData.pointerCurrentRaycast.gameObject.transform.GetSiblingIndex();
            GameManager.GM.controller.selectCard(index);
        } else {
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        //gameObject.GetComponent<Image>().color = Color.black;
        nameText.text = card.cardName;
        description.text = card.description;
        artwork.sprite = card.artwork;
    }

    public void OnPointerExit(PointerEventData eventData) {
        //gameObject.GetComponent<Image>().color = Color.white;
        nameText.text = "";
        description.text = "";
        //artwork.sprite = card.artwork;
    }
}
