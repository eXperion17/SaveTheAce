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
		var enemy = GetEnemyByName(msg.playerName);

		if (enemy != null) {
			enemy.UpdateInfo(msg);
		} else {
			enemies.Add(new OtherPlayer(msg));
		}

		//If we just updated the enemy that we're currently displaying on the screen, refresh it
		if (enemies[currentEnemyID].playerName == msg.playerName) {
			DisplayEnemy(currentEnemyID);
		}
	}

	public void DisplayEnemy(int id) {
		var player = enemies[id];
		DisplayEnemy(player);
	}

	public void DisplayEnemy(string name) {
		var player = enemies.Find(x => x.playerName == name);
		DisplayEnemy(player);
	}

	public void DisplayEnemy(OtherPlayer player) {
		var playerIndex = enemies.FindIndex(x => x == player);
		/*if (playerIndex == currentEnemyID)
			return;*/

		currentEnemyID = playerIndex;
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
				Destroy(cardsContainer.transform.GetChild(0).gameObject);
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
		int count = enemyBonus.transform.childCount;
		for (int i = 0; i < count; i++) {
			var obj = enemyBonus.transform.GetChild(0);
			obj.transform.SetParent(transform.parent.parent);
			Destroy(obj.gameObject);
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

	public CardDisplay GetAttackCard(int cardID) {
		return enemyAttack.transform.GetChild(cardID).GetComponent<CardDisplay>();
	}

	public CardDisplay GetDefenseCard(int cardID) {
		return enemyDefense.transform.GetChild(cardID).GetComponent<CardDisplay>();
	}

	public CardDisplay GetBonusCard(int cardID) {
		return enemyBonus.transform.GetChild(cardID).GetComponent<CardDisplay>();
	}

	private OtherPlayer GetEnemyByName(string name) {
		for (int i = 0; i < enemies.Count; i++) {
			if (enemies[i].playerName == name)
				return enemies[i];
		}
		return null;
	}

}
