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
		GameState.SetState(GameState.PlanningPhase);

		players = lobby.GetParticipatingPlayers();


		ShuffleCards(players);
		OnStartGame.Invoke();

		LogInfo("Game has started lads!!!");
	}

	/// <summary>
	/// Shuffles the cards for each AcePlayer and tells Server to send it to the respective Clients
	/// </summary>
	private void ShuffleCards(List<AcePlayer> players) {
		string[] playerHands = new string[4];
		for (int i = 0; i < players.Count; i++) {
			List<int> playerHand = players[i].ShuffleCards(3);

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
			if (!player.isDonePlanning)
				someoneNotReady = true;
		}

		if (!someoneNotReady) {
			//Start battle phase!!!
			LogInfo("O SHIT LETS GO BOYS");

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

}
