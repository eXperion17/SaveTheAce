using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TurnResultMessage : MessageBase {
	public string attackerName;
	public string defenderName;
	public int attackerCard;
	public int defenderCard;

	public bool attackerWon;
	//If its a tie, it'll supercede all checks and destroy both cards
	public bool tie;

	public int attackCardPosition;
	public int defenseCardPosition;
	public int bonusCardPosition;

	public bool attackingAce;
	public bool gameEnder;
}