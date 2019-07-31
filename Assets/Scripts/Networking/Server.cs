using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Server : MonoBehaviour {
	public const short CONNECT = 1;
	public const short JOINLOBBY = 2;

	int port = 9999;
	int maxConnections = 5; //4 players & 1 potential observer

	public LobbyManager lobbyManager;
	public GameManager gameManager;

	//Should've been put in gameManager but since 90% of the logic happens here anyway... whops.
	public ServerObserver observer;

	private int playerTurn;

	//public PlayerPlanningPhaseDone[] plannedPlayers;

	// Use this for initialization
	void Start() {
		// Usually the server doesn't need to draw anything on the screen
		Application.runInBackground = true;
		CreateServer();
	}

	void CreateServer() {
		// Register handlers for the types of messages we can receive
		RegisterHandlers();

		var config = new ConnectionConfig();
		// There are different types of channels you can use, check the official documentation
		config.AddChannel(QosType.ReliableFragmented);
		config.AddChannel(QosType.UnreliableFragmented);

		var ht = new HostTopology(config, maxConnections);

		if (!NetworkServer.Configure(ht)) {
			Debug.Log("No server created, error on the configuration definition");
			return;
		}
		else {
			// Start listening on the defined port
			if (NetworkServer.Listen(port))
				Debug.Log("Server created, listening on port: " + port);
			else
				Debug.Log("No server created, could not listen to the port: " + port);
		}
	}

	void OnApplicationQuit() {
		NetworkServer.Shutdown();
	}

	private void RegisterHandlers() {
		// Unity have different Messages types defined in MsgType
		NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnected);
		NetworkServer.RegisterHandler(MsgType.Disconnect, OnClientDisconnected);

		// Our message use his own message type.
		NetworkServer.RegisterHandler(MsgType.AddPlayer, OnJoinLobby);
		NetworkServer.RegisterHandler(AceMsgTypes.PlayerPlanningPhaseDone, OnPlayerPlanningPhaseDone);
		NetworkServer.RegisterHandler(AceMsgTypes.BattlePhase_PlayerTurnFinish, OnPlayerTurnFinish);
		NetworkServer.RegisterHandler(AceMsgTypes.BattlePhase_PlayerTurnSkip, OnPlayerTurnSkip);
		//more here pls
	}

	private void RegisterHandler(short t, NetworkMessageDelegate handler) {
		NetworkServer.RegisterHandler(t, handler);
	}

	void OnClientConnected(NetworkMessage netMessage) {
		// Do stuff when a client connects to this server

		//Add dummy
		lobbyManager.AddPlayer(new ConnectedPlayer(netMessage.conn.connectionId, AceRules.Unnamed_Player, true), netMessage.conn.connectionId);
	}

	void OnClientDisconnected(NetworkMessage netMessage) {
		// Do stuff when a client dissconnects

		//TODO: Remove player from lobbyManager
	}

	void OnJoinLobby(NetworkMessage netMessage) {
		var player = netMessage.ReadMessage<JoinLobbyMessage>();
		//lobbyManager.AddPlayer(new ConnectedPlayer(netMessage.conn.connectionId, player.playerName, player.isPlayer), netMessage.conn.connectionId);
		lobbyManager.AdjustPlayer(netMessage.conn.connectionId, player.playerName);

		//Send confirmation back to client
		var confirmation = new LobbyJoinedSuccessfulMessage();
		confirmation.successful = true;
		confirmation.clientConnectionID = netMessage.conn.connectionId;
		NetworkServer.SendToClient(netMessage.conn.connectionId, AceMsgTypes.LobbyJoinedSuccessful, confirmation);

		//Send the updated lobby list to everyone connected
		var lobbyUpdate = new LobbyUpdateMessage();
		lobbyUpdate.connectedPlayers = lobbyManager.ParseToString();
		NetworkServer.SendToAll(AceMsgTypes.LobbyInfoUpdate, lobbyUpdate);
	}

	public void StartGame() {
		if (lobbyManager.connectedClients.Count < 1)
			return;

		//Send message to all connected that the game as started
		var message = new MyNetworkMessage();
		//Players - 1 because the clients expect the message to be the number of enemies
		message.message = (lobbyManager.GetParticipatingPlayers().Count -1).ToString();
		NetworkServer.SendToAll(AceMsgTypes.GameStart, message);

		//plannedPlayers = new PlayerPlanningPhaseDone[lobbyManager.GetParticipatingPlayers().Count];

		//Start the actual game, shuffling cards n shit
		gameManager.StartGame();
		observer.StartObserving(gameManager.players);
	}

	public void SendHandToClients(string[] playerHands) {
		for (int i = 0; i < lobbyManager.connectedClients.Count; i++) {
			//Create the message with their newly updated hand
			var message = new PlayerHandUpdateMessage();
			message.hand = playerHands[i];

			Debug.Log(i + ": " + message.hand);

			NetworkServer.SendToClient(lobbyManager.connectedClients[i].id, AceMsgTypes.PlayerHandUpdate, message);
		}

		observer.UpdatePlayers();
	}

	public void OnShuffleCards() {

	}

	public void OnPlayerPlanningPhaseDone(NetworkMessage netMessage) {
		var playerPlanning = netMessage.ReadMessage<PlayerPlanningPhaseDone>();

		int playerID = netMessage.conn.connectionId - 1;

		//Process the changes the player made inside the server > AcePlayer
		//Debug.Log("Done planning: " + playerPlanning.hand);
		gameManager.OnPlayerPlanningDone(playerID, playerPlanning);
		gameManager.LogInfo(gameManager.players[playerID].playerName + " has finished planning!");
	}

	public ObscuredPlayerInfoMessage GetObscurePlayerInfo(int playerID) {
		var obscuredInfo = new ObscuredPlayerInfoMessage();
		var player = gameManager.players[playerID];

		obscuredInfo.attackCount = GetAmountCardsOnField(player.attackField);
		obscuredInfo.defenseCount = GetAmountCardsOnField(player.defenseField);
		obscuredInfo.bonus = GetBonusCardsAsString(player.bonusField);
		obscuredInfo.handCount = player.hand.Count;
		obscuredInfo.drawPileCount = player.drawPile.Count;
		obscuredInfo.discardPileCount = player.discardPile.Count;
		obscuredInfo.playerName = player.playerName;
		obscuredInfo.hasAce = player.hasAce;
		obscuredInfo.connID = player.connectionID;

		return obscuredInfo;
	}

	public void StartBattlePhase() {
		//First send the planning changes to all the other players (but maybe only process them in counts?)
		var players = lobbyManager.GetParticipatingPlayers();
		for (int i = 0; i < players.Count; i++) {
			var playerInfo = GetObscurePlayerInfo(i);

			//info stuff
			NetworkServer.SendToAll(AceMsgTypes.ObscuredPlayerInfo, GetObscurePlayerInfo(i));
		}
		
		observer.UpdatePlayers();
		Invoke("SendBattlePhaseMessage", 1f);
	}

	public void SendBattlePhaseMessage() {
		//Declare battle phase has started with an empty message
		NetworkServer.SendToAll(AceMsgTypes.BattlePhaseStart, new MyNetworkMessage());
		GameState.SetState(GameState.BattlePhase);

		playerTurn = 0;
		StartTurnsForPlayers();
	}

	private void StartTurnsForPlayers() {
		if (playerTurn >= lobbyManager.GetParticipatingPlayers().Count && (GameState.currentState != GameState.PlanningPhase)) {
			//Time to start
			Debug.Log("Time to start planning again!");
			gameManager.StartPlanningPhase(AceRules.Player_Draw_Count);

		} else {
			var player = lobbyManager.GetParticipatingPlayers()[playerTurn];
			if ((player.HasBonusCard() && player.HasAttackCard()) || player.HasFaceUpAssassin()) {
				NetworkServer.SendToClient(playerTurn + 1, AceMsgTypes.BattlePhase_PlayerTurn, new MyNetworkMessage());
				playerTurn++;
			} else {
				//If the current player doesn't have a bonus card, we'll loop back to this method but go through the next player
				playerTurn++;
				StartTurnsForPlayers();
			}
		}
	}

	public void OnPlayerTurnFinish(NetworkMessage netMessage) {
		var turn = netMessage.ReadMessage<PlayerTurnFinishMessage>();

		ProcessTurn(turn);
		observer.UpdatePlayers();

		if (gameManager.PlayersAlive() > 1)
			Invoke("StartTurnsForPlayers", AceRules.Duration_Server_Wait_After_Attack);
	}

	public void OnPlayerTurnSkip(NetworkMessage netMessage) {
		StartTurnsForPlayers();
	}

	private void ProcessTurn(PlayerTurnFinishMessage turn) {
		var attacker = lobbyManager.GetPlayerByName(turn.attackerName);
		var defender = lobbyManager.GetPlayerByName(turn.enemyName);
		var message = new TurnResultMessage();

		int cardAttacking = GetCardAtPosition(attacker, turn.attackCardPosition, true);
		message.attackerCard = cardAttacking;
		message.attackerName = attacker.playerName;
		message.defenderName = defender.playerName;
		if (turn.attackingAce) {
			message.attackingAce = turn.attackingAce;
			defender.GameOver();

			if (gameManager.PlayersAlive() == 1) {
				message.gameEnder = true;
				observer.ShowEndScreen(attacker.playerName);
			}

			SendTurnResultsToClients(attacker, defender, message);
			return;
		} 

		message.attackCardPosition = turn.attackCardPosition;
		message.defenseCardPosition = turn.defenseCardPosition;
		message.bonusCardPosition = turn.bonusCardPosition;

		//atm hard-coded to be -7, cause it looks like a J
		if (turn.attackCardPosition == -7) {
			defender.discardPile.Add(defender.bonusField[turn.bonusCardPosition, 0]);
			defender.bonusField[turn.bonusCardPosition, 0] = 0;
			defender.bonusField[turn.bonusCardPosition, 1] = 0;

			attacker.discardPile.Add(attacker.GetAssassin());
			attacker.RemoveAssassin();
		} /*else if (turn.defenseCardPosition == -1 && turn.bonusCardPosition == -1) {
			if (!defender.HasAnyDefenseCards()) {
				defender.GameOver();
			} else {
				gameManager.LogInfo("HEY HEY, NO CHEATERS AROUND HERE, OK?");
				//attacker.GameOver();
			}
		}*/
		else {
			//Regular attack on defense card
			int cardDefending = GetCardAtPosition(defender, turn.defenseCardPosition, false);
			message.defenderCard = cardDefending;

			message.attackerWon = (GetCardStrength(attacker, turn.attackCardPosition, true) > GetCardStrength(defender, turn.defenseCardPosition, false));
			//gameManager.LogInfo(GetCardStrength(attacker, turn.attackCardPosition, true) + " == " + GetCardStrength(defender, turn.defenseCardPosition, false));
			message.tie = (GetCardStrength(attacker, turn.attackCardPosition, true) == GetCardStrength(defender, turn.defenseCardPosition, false));

			if (AceRules.Two_Kills_All) {
				if (cardAttacking == 2) {
					if (AceRules.Two_Suicide_Effect)
						message.tie = true;
					message.attackerWon = true;
				} else if (cardDefending == 2) {
					if (AceRules.Two_Suicide_Effect)
						message.tie = true;
					message.attackerWon = false;
				}
			} else {
				if (cardAttacking == 2 && cardDefending == 10) {
					message.attackerWon = true;
					if (AceRules.Two_Suicide_Effect)
						message.tie = true;
				}
				else if (cardDefending == 2 && cardAttacking == 10) {
					message.attackerWon = false;
					if (AceRules.Two_Suicide_Effect)
						message.tie = true;
				}
			}

			if (message.tie) {
				attacker.discardPile.Add(attacker.attackField[turn.attackCardPosition]);
				attacker.attackField[turn.attackCardPosition] = 0;
				defender.discardPile.Add(defender.defenseField[turn.defenseCardPosition]);
				defender.defenseField[turn.defenseCardPosition] = 0;
			} else if (defender.hasAce) {
				if (message.attackerWon) {
					defender.discardPile.Add(defender.defenseField[turn.defenseCardPosition]);
					defender.defenseField[turn.defenseCardPosition] = 0;
				} else {
					attacker.discardPile.Add(attacker.attackField[turn.attackCardPosition]);
					attacker.attackField[turn.attackCardPosition] = 0;
				}
			}
		}

		SendTurnResultsToClients(attacker, defender, message);
	}

	private void SendTurnResultsToClients(AcePlayer attacker, AcePlayer defender, TurnResultMessage message) {
		//Send stuff back to clients
		var players = lobbyManager.GetParticipatingPlayers();
		for (int i = 0; i < players.Count; i++) {
			var currPlayer = players[i];
			if (currPlayer.playerName == attacker.playerName || currPlayer.playerName == defender.playerName) {
				NetworkServer.SendToClient(currPlayer.connectionID, AceMsgTypes.BattlePhase_TurnResult, message);
			}
		}

		message.attackerCard = -1;
		//If it's 0, leave it at 0 to make it clear to clients that there was no card
		if (message.defenderCard != 0)
			message.defenderCard = -1;

		for (int j = 0; j < players.Count; j++) {
			var currPlayer = players[j];
			if (currPlayer.playerName != attacker.playerName && currPlayer.playerName != defender.playerName) {
				NetworkServer.SendToClient(currPlayer.connectionID, AceMsgTypes.BattlePhase_TurnResult, message);
			}
		}
	}


	/// <summary>
	/// Gets the cards total strength, including any active bonus effects on the field.
	/// </summary>
	private int GetCardStrength(AcePlayer player, int cardID, bool attacker) {
		int bonus = player.GetBonusEffects(attacker);

		return GetCardAtPosition(player, cardID, attacker) + bonus;
	}

	private int GetCardAtPosition(AcePlayer player, int cardPosition, bool attacker) {
		if (cardPosition < 0)
			return -7;

		if (attacker)
			return player.attackField[cardPosition];
		else
			return player.defenseField[cardPosition];
	}

	private string GetBonusCardsAsString(int[,] bonusField) {
		string converted = "";

		for (int i = 0; i < bonusField.GetLength(0); i++) {
			
			if (i >= 1)
				converted += "_";

			//If the card is face down we'll use 1 instead
			if (bonusField[i,1] == 1)	converted += 1 + "";
			else						converted += bonusField[i, 0];
		}

		return converted;
	}

	public int GetAmountCardsOnField(int[] list) {
		int count = 0;
		for (int i = 0; i < list.Length; i++) {
			if (list[i] == 0)
				continue;
			count++;
		}
		return count;
	}

	public int GetAmountCardsOnField(int[,] list) {
		int count = 0;
		for (int i = 0; i < list.GetLength(0); i++) {
			if (list[i,0] == 0)
				continue;
			count++;
		}
		return count;
	}

	public void Update() {
		if (Input.GetKey(KeyCode.R)) {
			NetworkServer.DisconnectAll();
			NetworkServer.Reset();
			SceneManager.LoadScene("ServerScene");
		}
	}
}