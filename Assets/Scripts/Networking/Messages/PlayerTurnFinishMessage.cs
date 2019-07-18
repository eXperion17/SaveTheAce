using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerTurnFinishMessage : MessageBase {
	public string enemyName;
	public string attackerName;

	public int attackCardPosition;
	public int defenseCardPosition;
	public int bonusCardPosition;
}
