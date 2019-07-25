using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConnectedPlayer {
	public int id;
	public string name;
	public bool isPlayer;

	public ConnectedPlayer(int id, string name, bool isPlayer) {
		this.id = id;
		this.name = name;
		this.isPlayer = isPlayer;
	}
}

public class LobbyManager : MonoBehaviour {
	public List<ConnectedPlayer> connectedClients;
	private List<AcePlayer> participatingPlayers;
	public TMPro.TextMeshProUGUI playerList, ipText;

	private void Start() {
		connectedClients = new List<ConnectedPlayer>();
		ShowIP();
	}

	public void AddPlayer(ConnectedPlayer player, int id) {
		connectedClients.Add(player);

		UpdatePlayerList();
	}

	private void UpdatePlayerList() {
		playerList.text = "";

		for (int i = 0; i < connectedClients.Count; i++) {
			playerList.text += (i + 1) + "  " + connectedClients[i].name + "\n";
		}
	}

	public void ShowIP() {
		ipText.text = IPManager.GetLocalIPAddress();
	}

	public List<AcePlayer> GetParticipatingPlayers() {
		if (participatingPlayers != null) {
			return participatingPlayers;
		}

		List<AcePlayer> players = new List<AcePlayer>();
		/*int playerCount = 0;
		connectedClients.ForEach(x => { if (x.isPlayer) playerCount++; });

		//Create dummies
		for (int i = 0; i < playerCount; i++) {
			players.Add(new AcePlayer(-1, ""));
		}


		for (int i = 0; i < playerCount; i++) {
			if (connectedClients[i].isPlayer) {

			}
			
		}*/

		foreach (ConnectedPlayer player in connectedClients) {
			if (player.isPlayer) {
				var acePlayer = new AcePlayer(player.id, player.name);
				players.Add(acePlayer);
			}
		}
		participatingPlayers = players;

		return participatingPlayers;
	}


	public string ParseToString() {
		string playerList = "";
		for (int i = 0; i < connectedClients.Count; i++) {
			if (i > 0)
				playerList += "_";

			playerList += connectedClients[i].name;
		}

		return playerList;
	}

	public AcePlayer GetPlayerByName(string name) {
		for (int i = 0; i < participatingPlayers.Count; i++) {
			if (participatingPlayers[i].playerName == name) {
				return participatingPlayers[i];
			}
		}
		return null;
	}
}
