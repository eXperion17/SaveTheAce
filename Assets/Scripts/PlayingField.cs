using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayingField : MonoBehaviour {
	public GameObject playerHand, attackField, defenseField, bonusField, AceField;
	public GameObject cardPrefab;

	public GameObject inputBlocker, readyButton, waitingTurn, confirmation, gameOver, skipTurn;
	public BattleField battleField;

	[Header("TextFields")]
	public TMPro.TextMeshProUGUI attackText;
	public TMPro.TextMeshProUGUI defenseText, bonusText, gameOverText;
	

	[Header("Events")]
	public PlayerCardsEvent OnPlanningPhaseEndEvent;
	public PlayerAttackEvent OnConfirmationAttack;
	public UnityEvent OnGameLoss, OnTurnSkip;

	[Header("Battle Select Cards")]
	public CardDisplay attackCard;
	public CardDisplay targetCard;
	private bool usingAssassin;

	private void Start() {
		//AddCardsToHand("4_6_8_13");
		var cardObj = battleField.enemyAce.transform.GetChild(0);
		usingAssassin = false;
		skipTurn.SetActive(false);
		cardObj.GetComponent<CardDisplay>().OnSelect.AddListener(OnCardSelect);
	}

	public void AddCardsToHand(string hand) {
		waitingTurn.SetActive(false);
		confirmation.SetActive(false);
		readyButton.SetActive(true);

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
		//atm only bonus card, because they will have flipping functionality IF they didn't battle
		for (int i = 0; i < bonusField.transform.childCount; i++) {
			bonusField.transform.GetChild(i).GetComponent<CardDisplay>().OnBattlePhase();
		}

		waitingTurn.SetActive(true);
	}

	public void StartTurn() {
		skipTurn.SetActive(true);
		waitingTurn.SetActive(false);
	}

	//Don't look if hacky code makes you sick
	public void OnCardSelect(CardDisplay card) {
		if (GameState.currentState == GameState.BattlePhase) {
			//To make things a bit quicker, we automatically toggle the selection
			card.ToggleSelection(true);

			//If we card we're clicking is in the attack field, make that our ATTACK card
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
			//If we card we're clicking is in the enemy's defense field, make that our TARGET card
			else if (card.transform.IsChildOf(battleField.enemyDefense.transform) && (attackCard && attackCard.power != 11)) {
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
			//If there's no defense cards on the enemy, we can select the Ace instead
			else if (battleField.enemyDefense.transform.childCount == 0 && card.transform.IsChildOf(battleField.enemyAce.transform) && 
					(attackCard && attackCard.power != 11)) {
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
			//Bonus/Joker/Boer assassinations
			else if (card.power == 11 && !card.faceDown) {
				if ((targetCard && targetCard.transform.IsChildOf(battleField.enemyDefense.transform)) || (targetCard && targetCard.transform.IsChildOf(battleField.enemyAce.transform))) {
					//Clearing the target card in case the player still had that one selected that wouldn't work with 
					targetCard.ToggleSelection(false);
					targetCard = null;
				}
				//No need to include the else statement (with every other similar example) because there will only be one assassination card
				if (attackCard && card == attackCard) {
					attackCard.ToggleSelection(false);
					attackCard = null;
				} else {
					attackCard = card;
				}
				
			} else if (card.transform.IsChildOf(battleField.enemyBonus.transform) && attackCard.power == 11) {
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
			usingAssassin = (attackCard.power == 11);

			confirmation.SetActive(true);
		}
	}

	public void SkipTurn() {
		OnTurnSkip.Invoke();
		waitingTurn.SetActive(true);
		skipTurn.SetActive(false);
	}


	public void ConfirmTurn() {
		int attackCardPos = Extensions.FindObjectIndexWithinParent(attackField.transform, attackCard.transform);
		int defenseCardPos = Extensions.FindObjectIndexWithinParent(battleField.enemyDefense.transform, targetCard.transform);
		int bonusCardPos = -1;

		if (usingAssassin)
			attackCardPos = -7;

		//If there's no defense card selected, check the bonus one
		if (defenseCardPos == -1)
			bonusCardPos = Extensions.FindObjectIndexWithinParent(battleField.enemyBonus.transform, targetCard.transform);

		OnConfirmationAttack.Invoke(attackCardPos, defenseCardPos, bonusCardPos, battleField.enemyName.text);
		confirmation.SetActive(false);
	}

	public void ProcessBattleTurn(TurnResultMessage msg, string ourName) {
		//We divide this up in two parts, as the mobile variant can't witness the complete battle between two enemies
		if (msg.attackerName == ourName || msg.defenderName == ourName) {
			//Make sure the defender is looking at the attacker
			if (msg.defenderName == ourName)
				battleField.DisplayEnemy(msg.attackerName);

			if (msg.attackingAce) {
				if (msg.attackerName == ourName) {
					//Destroy(battleField.enemyAce.transform.GetChild(0));
					CancelTurn();
					if (msg.gameEnder) {
						gameOverText.text = "GOTTEM! You've offically {i}saved your ace{/i}!";
						gameOver.SetActive(true);
						OnGameLoss.Invoke();
					} else {
						waitingTurn.SetActive(true);
						skipTurn.SetActive(false);
					}
					
				} else {
					gameOverText.text = "Too bad! " + msg.attackerName + " de_stroyed your ace! \n\nF in chat boys";
					gameOver.SetActive(true);
					OnGameLoss.Invoke();
				}

				return;
			}
			
			//Display the relevant cards with the info
			List<CardDisplay> losingCards = new List<CardDisplay>();
			//Sequence if we're the attacker
			if (msg.attackerName == ourName) {
				//Check whether we've just used a Joker/Boer/Assassin
				if (msg.attackCardPosition == -7) {
					//Get both bonus cards and flag them for removal
					losingCards.Add(GetAssassin(false));
					losingCards.Add(battleField.GetBonusCard(msg.bonusCardPosition));
				} else {
					//Otherwise check for both cards, removing both if its a tie OR only the losing one
					var defCard = battleField.GetDefenseCard(msg.defenseCardPosition);
					defCard.SetCard(msg.defenderCard);
					if (msg.tie) {
						losingCards.Add(defCard);
						losingCards.Add(attackField.transform.GetChild(msg.attackCardPosition).GetComponent<CardDisplay>());
					}
					else {
						losingCards.Add(msg.attackerWon ? defCard : attackField.transform.GetChild(msg.attackCardPosition).GetComponent<CardDisplay>());
					}
				}
			} else {
				if (msg.attackCardPosition == -7) {
					losingCards.Add(GetAssassin(true));
					losingCards.Add(bonusField.transform.GetChild(msg.bonusCardPosition).GetComponent<CardDisplay>());
				} else {
					var attCard = battleField.GetAttackCard(msg.attackCardPosition);
					attCard.SetCard(msg.attackerCard);
					if (msg.tie) {
						losingCards.Add(attCard);
						losingCards.Add(defenseField.transform.GetChild(msg.defenseCardPosition).GetComponent<CardDisplay>());
					}
					else {
						losingCards.Add(msg.attackerWon ? defenseField.transform.GetChild(msg.defenseCardPosition).GetComponent<CardDisplay>() : attCard);
					}
				}
			}
			
			//Let the player see the relevant cards before removing them
			StartCoroutine(DelayedBattleResults(losingCards));
		} else {
			//TODO: Process the actual changes into the OtherPlayers classes?
			//TODO: Make a logger
			if (msg.attackerWon)
				Debug.Log(msg.attackerName + " attacked " + msg.defenderName + " and won! " + msg.defenderName + " loses their card in position " + msg.defenseCardPosition + "!");
			else
				Debug.Log(msg.attackerName + " attacked " + msg.defenderName + " and lost! " + msg.attackerName + " loses their card in position " + msg.attackCardPosition + "!");
		}
	}

	private CardDisplay GetAssassin(bool enemy) {
		var carrier = enemy ? battleField.enemyBonus.transform : bonusField.transform;

		for (int i = 0; i < carrier.childCount; i++) {
			var child = carrier.GetChild(i).GetComponent<CardDisplay>();
			if (child.power == 11) {
				return child;
			}
		}
		return null;
	}

	private IEnumerator DelayedBattleResults(List<CardDisplay> losingCards) {
		var state = true;
		while (state) {
			yield return new WaitForSeconds(AceRules.Duration_Client_Before_Deletion_Cards);
			losingCards.ForEach(x => Destroy(x.gameObject));
			CancelTurn();
			waitingTurn.SetActive(true);
			skipTurn.SetActive(false);
			state = false;
			yield return null;
		}
		yield return null;
	}

	public void CancelTurn() {
		if (attackCard) {
			attackCard.ToggleSelection(false);
			attackCard = null;
		}
		
		if (targetCard) {
			targetCard.ToggleSelection(false);
			targetCard = null;
		}
	}




}
