using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Only used by the server to quickly assign the correct GameObjects
public class PlayingFieldReferences : MonoBehaviour {
	public GameObject playerHand, attackField, defenseField, bonusField, AceField;
	public TMPro.TextMeshProUGUI playerName;
}
