using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[CreateAssetMenu(fileName ="new Card",menuName ="Card")]
public class Card : ScriptableObject {

    public string cardName;
    public Sprite artwork;
    public string description;
    public float cooldown = 5f;
    public float persitence = 5f;

    // ce sera ce gameobject qui fera les effet de la carte
    public GameObject NodePrefab;
    public int availabeClicks = 1;
    public bool isGlobal = false;
}
