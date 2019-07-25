using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class IntEvent : UnityEvent<int> {}
[Serializable]
public class StringEvent : UnityEvent<string> {}
[Serializable]
public class PlayerCardsEvent : UnityEvent<string, string, string, string> {}
[Serializable]
public class PlayerAttackEvent : UnityEvent<int, int, int, string> {}
[Serializable]
public class CardEvent : UnityEvent<CardDisplay> {}

public class CardDisplay : MonoBehaviour {
	[SerializeField]
	private Image cardImage;
	[SerializeField]
	private Sprite[] cardSprites;
	[SerializeField]
	private Sprite backSide;
	[SerializeField]
	private GameObject lockedObj;
	[SerializeField]
	public UIElementDragger elementDragger;

	public int power;
	public bool faceDown;
	public bool locked;
	public bool selected;

	public bool haveBeenInBattle = false;

	//Events
	[HideInInspector]
	public IntEvent OnDrag;
	[HideInInspector]
	public UnityEvent OnDragEnd;
	[HideInInspector]
	public CardEvent OnSelect;

	public void SetCard(int power) {
		this.power = power;
		if (power < 2)
			return;

		//power - 2 due to the serverside running off of 2 to 14 and clientside (for sprites) 0 to 12.
		cardImage.sprite = cardSprites[power - 2];
	}
	
	public void FlipCard(bool playerOwnsCard = true) {
		if (GameState.currentState == GameState.PlanningPhase) {
			if (playerOwnsCard) {
				if (power <= 10)
					return;
			}

			if (haveBeenInBattle)
				return;

			faceDown = !faceDown;
			if (faceDown)
				cardImage.sprite = backSide;
			else
				cardImage.sprite = cardSprites[power - 2];
		} else {
			//ToggleSelection(!selected);
			//if (selected)
			OnSelect.Invoke(this);
		}
		
	}

	public void ForceFlipCard() {
		faceDown = !faceDown;
		if (faceDown)
			cardImage.sprite = backSide;
		else
			cardImage.sprite = cardSprites[power - 2];
	}


	internal void ToggleSelection(bool selected) {
		this.selected = selected;

		cardImage.color = selected ? Color.red : Color.white;
	}

	public void LockCard(bool showVisuals = true) {
		if (showVisuals)
			lockedObj.SetActive(true);

		locked = true;
		elementDragger.enableDragging = false;
	}

	public void OnBattlePhase() {
		if (!faceDown)
			haveBeenInBattle = true;
	}



	public void OnPickup(BaseEventData data) {
		if (!elementDragger.enableDragging)
			return;

		transform.SetParent(transform.parent.parent);
		OnDrag.Invoke(power);
	}

	public void OnDrop(BaseEventData data) {
		//sshhhh
		if (!elementDragger.enableDragging)
			return;

		OnDragEnd.Invoke();

		//Check where the fuck the player dropped it
		List<GameObject> objs = Extensions.GetGameObjectsOverPointer();
		transform.rotation = Quaternion.identity;

		foreach (GameObject obj in objs) {
			//TODO: Fix both issues, using name as comparison (ty Unity) and not defining the name
			switch (obj.name) {
				case "PlayerHand":
					obj.transform.parent.gameObject.GetComponent<PlayingField>().AddCardToHand(gameObject);
					break;
				case "AttackField":
					if (power <= 10)
						transform.SetParent(obj.transform);
					break;
				case "BonusField":
					if (power >= 11 && power <= 13)
						transform.SetParent(obj.transform);
					break;
				case "DefenseField":
					if (power <= 10) {
						transform.SetParent(obj.transform);
						transform.rotation = Quaternion.Euler(0, 0, 90);
					}
					break;
			}
		}
	}


}
