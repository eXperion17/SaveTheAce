using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class AcePlayer {
	public int connectionID;
	public string playerName;

	public List<int> drawPile;
	public List<int> discardPile;
	public List<int> hand;

	public int[] attackField, defenseField;
	public int[,] bonusField;
	public bool hasAce;

	public bool isDonePlanning;

	public AcePlayer(int connID, string name) {
		connectionID = connID;
		playerName = name;

		drawPile = new List<int>(new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 });
		discardPile = new List<int>();
		hand = new List<int>();

		attackField = new int[4];
		defenseField = new int[4];
		bonusField = new int[3,2];
	}

	/// <summary>
	/// Shuffles the drawpile and pulls X amount of cards.
	/// </summary>
	/// <param name="startingHandCount"></param>
	public List<int> ShuffleCards(int startingHandCount) {
		//Shuffle cards
		drawPile = drawPile.OrderBy(x => UnityEngine.Random.value).ToList();

		//Draw 3 cards & remove those 3 from the drawPile
		List<int> handDraw;
		handDraw = drawPile.GetRange(0, startingHandCount);
		drawPile.RemoveRange(0, startingHandCount);

		//TODO: DEBUG PURPOSES ONLY
		handDraw.Add(12);

		return handDraw;
	}

	public void ProcessPlanningPhase(PlayerPlanningPhaseDone player) {
		hand = Extensions.ParseStringIntoList(player.hand);
		attackField = Extensions.ParseStringIntoIntArray(player.attack, 4);
		defenseField = Extensions.ParseStringIntoIntArray(player.defense, 4);
		bonusField = Extensions.ParseStringIntoDoubleIntArray(player.bonus, 3);

		//event?
		isDonePlanning = true;
	}

	public int GetBonusEffects(bool attack) {
		int bonus = 0;
		var faceUp = new List<int>();
		//Put all face-up cards into a separate array
		for (int i = 0; i < bonusField.GetLength(0); i++) {
			if (bonusField[i, 1] == 0) {
				faceUp.Add(bonusField[i, 0]);
			}
		}
		//If there are no faceup cards, just return already
		if (faceUp.Count == 0)
			return bonus;

		//If both king and queen are present, the bonus is always 1 regardless of mode
		if (faceUp.Contains(12) && faceUp.Contains(13)) {
			bonus += 1;
		} else {
			//If not we check whether the queen or king is present, and apply the bonuses in each scenario
			if (faceUp.Contains(12))
				bonus += (attack) ? -1 : 1;
			else if (faceUp.Contains(13))
				bonus += (attack) ? 1 : -1;
		}

		return bonus;
	}

	public bool HasAnyDefenseCards() {
		foreach (int power in defenseField) {
			if (power > 1)
				return true;
		}

		return false;
	}
}
