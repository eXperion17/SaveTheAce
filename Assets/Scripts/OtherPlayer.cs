using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data class, only used for clientside.
/// </summary>
public class OtherPlayer {

	public string playerName;
	public int drawPileCount, discardPileCount, handCount;

	public int attackCardCount;
	public int defenseCardCount;
	public int[] bonusCards;

	public bool hasAce;

	public OtherPlayer(ObscuredPlayerInfoMessage msg) {
		drawPileCount = msg.drawPileCount;
		discardPileCount = msg.discardPileCount;
		handCount = msg.handCount;

		attackCardCount = msg.attackCount;
		defenseCardCount = msg.defenseCount;
		bonusCards = Extensions.ParseStringIntoIntArray(msg.bonus, 3);
		playerName = msg.playerName;
		hasAce = msg.hasAce;
	}
}
