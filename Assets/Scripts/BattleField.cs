using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BattleField : MonoBehaviour {

	public GameObject enemyAttack, enemyDefense, enemyBonus, enemyAce;
	public GameObject cardPrefab;
	public GameObject prevButton, nextButton;
	public TMPro.TextMeshProUGUI enemyName;
	public PlayingField playingField;

	private List<OtherPlayer> enemies;
	private int currentEnemyID;

	public void Init(int enemyCount) {
		enemies = new List<OtherPlayer>(enemyCount);
		currentEnemyID = 0;

		if (enemyCount == 1) {
			prevButton.SetActive(false);
			nextButton.SetActive(false);
		}
	}

	public void ProcessPlayer(ObscuredPlayerInfoMessage msg) {
		//int id = msg.connID - 1;
		enemies.Add(new OtherPlayer(msg));
	}


	public void DisplayEnemy(int id) {
		var player = enemies[id];

		enemyName.text = player.playerName;

		//Attack
		int difference = player.attackCardCount - enemyAttack.transform.childCount;
		BalanceCards(enemyAttack, player, difference, false);
		//Defense
		difference = player.defenseCardCount - enemyDefense.transform.childCount;
		BalanceCards(enemyDefense, player, difference, true);
		//Bonus (oh god)
		BalanceBonusCards(player);
	}


	private void BalanceCards(GameObject cardsContainer, OtherPlayer player, int diff, bool rotate) {
		//If there are more cards visually then the player has, remove the difference in card count
		if (diff < 0) {
			for (int i = 0; i < Math.Abs(diff); i++) {
				Destroy(cardsContainer.transform.GetChild(0));
			}
			//Otherwise just add extra cards for each
		}
		else if (diff > 0) {
			for (int i = 0; i < diff; i++) {
				var card = Instantiate(cardPrefab).GetComponent<CardDisplay>();
				card.OnSelect.AddListener(playingField.OnCardSelect);
				card.ForceFlipCard();
				card.name = "Card " + Extensions.UniqueID;
				card.transform.SetParent(cardsContainer.transform, false);
				if (rotate)
					card.transform.rotation = Quaternion.Euler(0, 0, 90);
				card.LockCard(false);
			}
		}
	}

	/// <summary>
	/// I'm a big dum dum for just hard-coding this while keeping the one above legit, pls
	/// </summary>
	private void BalanceBonusCards(OtherPlayer player) {
		//fuck it just clear everything
		for (int i = 0; i < enemyBonus.transform.childCount; i++) {
			Destroy(enemyBonus.transform.GetChild(0));
		}

		for (int i = 0; i < player.bonusCards.Length; i++) {
			if (player.bonusCards[i] == 0)
				continue;

			var card = Instantiate(cardPrefab).GetComponent<CardDisplay>();
			card.OnSelect.AddListener(playingField.OnCardSelect);
			card.name = "Card " + Extensions.UniqueID;

			//1 == hidden card
			if (player.bonusCards[i] == 1)
				card.ForceFlipCard();
			else
				card.SetCard(player.bonusCards[i]);

			card.transform.SetParent(enemyBonus.transform, false);
		}
	}

	/// <summary>
	/// Cycles through the next enemy (called from the scene)
	/// </summary>
	public void NextEnemy() {
		//If we're at the max, loop back to the first
		if (currentEnemyID == (enemies.Count - 1))
			DisplayEnemy(0);
		else
			DisplayEnemy(currentEnemyID + 1);
	}

	/// <summary>
	/// Cycles through the previous enemy (called from the scene)
	/// </summary>
	public void PreviousEnemy() {
		//If we're at the min, loop back to the last
		if (currentEnemyID == 0)
			DisplayEnemy(enemies.Count-1);
		else
			DisplayEnemy(currentEnemyID - 1);
	}

}
