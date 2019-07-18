using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayingField : MonoBehaviour {
	public GameObject playerHand, attackField, defenseField, bonusField, AceField;
	public GameObject cardPrefab;

	public GameObject inputBlocker, readyButton, waitingTurn, confirmation;
	public BattleField battleField;

	[Header("TextFields")]
	public TMPro.TextMeshProUGUI attackText;
	public TMPro.TextMeshProUGUI defenseText, bonusText;

	[Header("Events")]
	public PlayerCardsEvent OnPlanningPhaseEndEvent;
	public PlayerAttackEvent OnConfirmationAttack;

	[Header("Battle Select Cards")]
	public CardDisplay attackCard;
	public CardDisplay targetCard;

	private void Start() {
		//AddCardsToHand("4_6_8_13");
		readyButton.SetActive(true);
		var cardObj = battleField.enemyAce.transform.GetChild(0);

		cardObj.GetComponent<CardDisplay>().OnSelect.AddListener(OnCardSelect);
	}

	public void AddCardsToHand(string hand) {
		string[] cards = hand.Split('_');
		for (int i = 0; i < cards.Length; i++) {
			AddCardToHand(int.Parse(cards[i]));
		}
	}

	public void AddCardToHand(int power) {
		var card = Instantiate(cardPrefab).GetComponent<CardDisplay>();
		card.SetCard(power);
		card.name = "Card " + Extensions.UniqueID;
		//fuck you unity
		card.elementDragger.enableDragging = true;

		//Subscribe to events
		card.OnDrag.AddListener(OnCardDrag);
		card.OnDragEnd.AddListener(OnCardDragEnd);
		card.OnSelect.AddListener(OnCardSelect);

		AddCardToHand(card.gameObject);
	}

	public void AddCardToHand(GameObject obj) {
		obj.transform.SetParent(playerHand.transform);
	}

	public void OnCardDrag(int power) {
		if (GameState.currentState == GameState.PlanningPhase) {
			//If a regular card
			if (power <= 10) {
				attackText.gameObject.SetActive(true);
				defenseText.gameObject.SetActive(true);
			}
			else if (power > 10 && power < 14) {
				bonusText.gameObject.SetActive(true);
			}
		}
	}

	public void OnCardDragEnd() {
		attackText.gameObject.SetActive(false);
		defenseText.gameObject.SetActive(false);
		bonusText.gameObject.SetActive(false);
	}

	public void OnFinishPlanning() {
		//Create changes on string n shit
		string hand, attack, defense, bonus;
		hand = attack = defense = bonus = "";

		//Hand
		for (int i = 0; i < playerHand.transform.childCount; i++) {
			var card = playerHand.transform.GetChild(i).GetComponent<CardDisplay>();
			if (i > 0)
				hand += "_";

			hand += card.power;
		}

		//Attack
		for (int i = 0; i < attackField.transform.childCount; i++) {
			var card = attackField.transform.GetChild(i).GetComponent<CardDisplay>();
			if (i > 0)
				attack += "_";

			attack += card.power;
		}

		//Defense
		for (int i = 0; i < defenseField.transform.childCount; i++) {
			var card = defenseField.transform.GetChild(i).GetComponent<CardDisplay>();
			if (i > 0)
				defense += "_";

			defense += card.power;
		}

		//Bonus
		for (int i = 0; i < bonusField.transform.childCount; i++) {
			var card = bonusField.transform.GetChild(i).GetComponent<CardDisplay>();
			if (i > 0)
				bonus += "_";

			bonus += card.power + "-" + (card.faceDown ? 1 : 0);
		}

		OnPlanningPhaseEndEvent.Invoke(hand, attack, defense, bonus);
		inputBlocker.SetActive(true);
		LockActiveCards();
	}

	private void LockActiveCards() {
		var fields = new GameObject[3] { attackField, defenseField, bonusField };
		for (int i = 0; i < fields.Length; i++) {
			var currField = fields[i].transform;
			for (int child = 0; child < currField.childCount; child++) {
				var display = currField.GetChild(child).GetComponent<CardDisplay>();
				display.LockCard();
			}
		}
	}

	public void OnBattlePhaseStart() {
		waitingTurn.SetActive(true);
	}

	public void StartTurn() {
		waitingTurn.SetActive(false);
	}

	public void OnCardSelect(CardDisplay card) {
		if (GameState.currentState == GameState.BattlePhase) {
			//If the card is actually in attack mode
			card.ToggleSelection(true);

			if (card.transform.IsChildOf(attackField.transform)) {
				if (attackCard && card == attackCard) {
					attackCard.ToggleSelection(false);
					attackCard = null;
				}
				else {
					if (attackCard)
						attackCard.ToggleSelection(false);

					attackCard = card;
				}
			}
			else if (card.transform.IsChildOf(battleField.enemyDefense.transform)) {
				if (targetCard && card == targetCard) {
					targetCard.ToggleSelection(false);
					targetCard = null;
				}
				else {
					if (targetCard)
						targetCard.ToggleSelection(false);
					targetCard = card;
				}
			}
			//If there's no defense cards on the enemy & selecting a bonus card
			else if (battleField.enemyDefense.transform.childCount == 0 && card.transform.IsChildOf(battleField.enemyBonus.transform)) {
				if (targetCard && card == targetCard) {
					targetCard.ToggleSelection(false);
					targetCard = null;
				}
				else {
					if (targetCard)
						targetCard.ToggleSelection(false);
					targetCard = card;
				}
			} else if (battleField.enemyDefense.transform.childCount == 0 &&
					   battleField.enemyBonus.transform.childCount == 0 &&
					   card.transform.IsChildOf(battleField.enemyAce.transform)) {
				if (targetCard && card == targetCard) {
					targetCard.ToggleSelection(false);
					targetCard = null;
				}
				else {
					if (targetCard)
						targetCard.ToggleSelection(false);
					targetCard = card;
				}
			} else {
				card.ToggleSelection(false);
			}
		}

		if (attackCard && targetCard) {
			confirmation.SetActive(true);
		}
	}

	public void ConfirmTurn() {
		int attackCardPos = Extensions.FindObjectIndexWithinParent(attackField.transform, attackCard.transform);
		int defenseCardPos = Extensions.FindObjectIndexWithinParent(battleField.enemyDefense.transform, targetCard.transform);
		int bonusCardPos = -1;

		//If there's no defense card selected, check the bonus one
		if (defenseCardPos == -1)
			bonusCardPos = Extensions.FindObjectIndexWithinParent(battleField.enemyBonus.transform, targetCard.transform);

		OnConfirmationAttack.Invoke(attackCardPos, defenseCardPos, bonusCardPos, battleField.enemyName.text);
		confirmation.SetActive(false);
	}

	public void CancelTurn() {
		attackCard.ToggleSelection(false);
		attackCard = null;
		targetCard.ToggleSelection(false);
		targetCard = null;
	}




}
