using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {
	public LobbyManager lobby;
	public Server gameServer;
	

	public List<AcePlayer> players;

	public UnityEvent OnStartGame;
	public TMPro.TextMeshProUGUI logger;


	public void StartGame() {
		StartPlanningPhase(AceRules.Player_StartingHand_Count);
		OnStartGame.Invoke();

		LogInfo("Game has started lads!!!");
	}

	public void StartPlanningPhase(int count) {
		LogInfo("Starting Planning Phase!");
		GameState.SetState(GameState.PlanningPhase);
		players = lobby.GetParticipatingPlayers();

		players.ForEach(x => x.isDonePlanning = false);

		DrawCards(players, count);
	}

	/// <summary>
	/// Shuffles the cards for each AcePlayer and tells Server to send it to the respective Clients
	/// </summary>
	private void DrawCards(List<AcePlayer> players, int count) {
		string[] playerHands = new string[count];
		for (int i = 0; i < players.Count; i++) {
			List<int> playerHand = players[i].DrawCards(count);

			playerHands[i] = ConvertToString(playerHand);
		}

		gameServer.SendHandToClients(playerHands);
	}


	private string ConvertToString(List<int> list) {
		string convertedString = "";
		for (int i = 0; i < list.Count; i++) {
			if (i > 0)
				convertedString += "_";

			convertedString += list[i];
		}

		return convertedString;
	}

	public void OnPlayerPlanningDone(int id, PlayerPlanningPhaseDone info) {
		players[id].ProcessPlanningPhase(info);

		//Check if all present players are done planning
		bool someoneNotReady = false;
		foreach(AcePlayer player in players) {
			if (!player.isDonePlanning && player.hasAce)
				someoneNotReady = true;
		}

		if (!someoneNotReady) {
			//Start battle phase!!!

			Invoke("StartBattlePhase", 1f);	
		}
	}

	private void StartBattlePhase() {
		gameServer.StartBattlePhase();
		GameState.SetState(GameState.BattlePhase);
	}

	public void LogInfo(string info) {
		logger.text = info + "\n" + logger.text;
	}

	internal int PlayersAlive() {
		int aliveCount = 0;

		foreach (var player in players) {
			if (player.hasAce) {
				aliveCount++;
			}
		}

		return aliveCount;
	}
}
