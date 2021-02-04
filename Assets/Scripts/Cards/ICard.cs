using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCard : MonoBehaviour {

    protected bool onCD = false;

    public bool isReady() {
        if (!onCD) {
            return true;
        }
        return false;
    }


    public void StartCoolDown(float timer) {
        StartCoroutine(startCoolDown(timer));
    }

    private IEnumerator startCoolDown(float cooldown) {
        onCD = true;
        yield return new WaitForSeconds(cooldown);
        onCD = false;
    }

}