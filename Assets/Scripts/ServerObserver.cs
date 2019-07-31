using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerObserver : MonoBehaviour {
	public GameObject playingFieldPrefab, cardPrefab, playerContainer, winScreen;
	public TMPro.TextMeshProUGUI winText;

	private List<AcePlayer> players;
	private List<GameObject> playingFields;

	public void StartObserving(List<AcePlayer> players) {
		this.players = players;
		playingFields = new List<GameObject>();

		for (int i = 0; i < players.Count; i++) {
			var field = Instantiate(playingFieldPrefab, playerContainer.transform);
			playingFields.Add(field);
			UpdateFields(field, players[i]);
		}
	}

	public void UpdatePlayers() {
		if (playingFields == null) return;

		for (int i = 0; i < playingFields.Count; i++) {
			UpdateFields(playingFields[i], players[i]);
		}
	}

	private void UpdateFields(GameObject field, AcePlayer player) {
		NukeField(field);
		StartCoroutine(RefreshFields(field, player));
	}

	private IEnumerator RefreshFields(GameObject field, AcePlayer player) {
		var state = true;
		while (state) {
			yield return new WaitForSeconds(0.2f);

			if (!player.hasAce) {
				if (field.transform.GetChild(4).transform.childCount > 0) {
					Destroy(field.transform.GetChild(4).transform.GetChild(0).gameObject);
					NukeField(field);
				}
				state = false;
				yield return null;
			}

			var name = field.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
			name.text = player.playerName;

			for (int i = 0; i < player.CardsAttackCount(); i++) {
				var attack = field.transform.GetChild(1).transform;
				var card = Instantiate(cardPrefab, attack).GetComponent<CardDisplay>();
				card.ForceFlipCard();
			}
			
			for (int j = 0; j < player.CardsDefenseCount(); j++) {
				var defense = field.transform.GetChild(2).transform;
				var card = Instantiate(cardPrefab, defense).GetComponent<CardDisplay>();
				card.transform.rotation = Quaternion.Euler(0, 0, 90);
				card.ForceFlipCard();
			}

			for (int k = 0; k < player.CardsBonusCount(); k++) {
				var attack = field.transform.GetChild(3).transform;
				var card = Instantiate(cardPrefab, attack).GetComponent<CardDisplay>();
				if (player.bonusField[k, 1] == 0)
					card.SetCard(player.bonusField[k, 0]);
				else
					card.ForceFlipCard();
			}

			//Debug.Log("aaaaaaaa2 " + player.hand.Count);
			for (int c = 0; c < player.hand.Count; c++) {
				var card = Instantiate(cardPrefab, field.transform.GetChild(5).transform).GetComponent<CardDisplay>();
				card.ForceFlipCard();
			}

			state = false;
			yield return null;
		}

		yield return null;
	}

	private void NukeField(GameObject field) {
		var childCount = 0;
		//ATTACK
		childCount = field.transform.GetChild(1).transform.childCount;
		for (int i = 0; i < childCount; i++) {
			var child = field.transform.GetChild(1).transform.GetChild(0);
			child.SetParent(transform);
			Destroy(child.gameObject);
		}
		//DEFENSE
		childCount = field.transform.GetChild(2).transform.childCount;
		for (int i = 0; i < childCount; i++) {
			var child = field.transform.GetChild(2).transform.GetChild(0);
			child.SetParent(transform);
			Destroy(child.gameObject);
		}
		//BONUS
		childCount = field.transform.GetChild(3).transform.childCount;
		for (int i = 0; i < childCount; i++) {
			var child = field.transform.GetChild(3).transform.GetChild(0);
			child.SetParent(transform);
			Destroy(child.gameObject);
		}
		//HAND
		childCount = field.transform.GetChild(5).transform.childCount;
		for (int i = 0; i < childCount; i++) {
			var child = field.transform.GetChild(5).transform.GetChild(0);
			child.SetParent(transform);
			Destroy(child.gameObject);
		}
	}

	public void ShowEndScreen(string winnerName) {
		winScreen.SetActive(true);
		winText.text = "Congratulations " + winnerName + "! You've officially saved your ace!";
	}
}
