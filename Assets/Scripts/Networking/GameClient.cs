using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameClient : Client {

	//More references
	public TMPro.TextMeshProUGUI playerList;
	public PlayingField playingField;
	public BattleField battleField;

	public UnityEvent OnLobbyJoinedEvent;
	public UnityEvent OnGameStartEvent;
	public UnityEvent OnBattlePhaseStartEvent;

	private int actualConnectionID;

	protected override void RegisterHandlers() {
		base.RegisterHandlers();
		//client.RegisterHandler(MsgType.Connect, OnConnected);
		client.RegisterHandler(AceMsgTypes.LobbyJoinedSuccessful, OnLobbyJoined);
		client.RegisterHandler(AceMsgTypes.LobbyInfoUpdate, OnLobbyUpdate);
		client.RegisterHandler(AceMsgTypes.GameStart, OnGameStart);
		client.RegisterHandler(AceMsgTypes.PlayerHandUpdate, OnHandUpdate);
		client.RegisterHandler(AceMsgTypes.ObscuredPlayerInfo, OnPlayerInfoRecieve);
		client.RegisterHandler(AceMsgTypes.BattlePhaseStart, OnBattlePhaseStart);
		client.RegisterHandler(AceMsgTypes.BattlePhase_PlayerTurn, OnPlayerTurn);
		client.RegisterHandler(AceMsgTypes.BattlePhase_TurnResult, OnBattleTurnResult);
	}

	public void JoinServerWithInfo() {
		//Fill up the special message type with the info the server needs
		var lobbyMessage = new JoinLobbyMessage();
		lobbyMessage.playerName = nameInput.text;
		lobbyMessage.isPlayer = isPlayerToggle.isOn;

		client.Send(MsgType.AddPlayer, lobbyMessage);
	}

	//

	public void OnLobbyJoined(NetworkMessage netMessage) {
		var msg = netMessage.ReadMessage<LobbyJoinedSuccessfulMessage>();
		if (msg.successful) {
			OnLobbyJoinedEvent.Invoke();
			actualConnectionID = msg.clientConnectionID;
		}
			
	}
	
	public void OnLobbyUpdate(NetworkMessage netMessage) {
		var msg = netMessage.ReadMessage<LobbyUpdateMessage>();

		Debug.Log(msg.connectedPlayers);
		//parsing list
		string[] parsedPlayers = msg.connectedPlayers.Split('_');
		
		RefreshPlayerList(parsedPlayers);
	}

	public void RefreshPlayerList(string[] players) {
		//Clear the text
		playerList.text = "";

		for (int i = 0; i < players.Length; i++) {
			playerList.text += (i+1) + "  " + players[i] + "\n";
		}
	}

	public void OnGameStart(NetworkMessage netMessage) {
		var msg = netMessage.ReadMessage<MyNetworkMessage>();
		OnGameStartEvent.Invoke();

		GameState.SetState(GameState.PlanningPhase);
		playingField.OnPlanningPhaseEndEvent.AddListener(OnPlanningPhaseEnd);
		playingField.OnConfirmationAttack.AddListener(OnConfirmationAttack);
		battleField.Init(int.Parse(msg.message));
	}

	public void OnHandUpdate(NetworkMessage netMessage) {
		var msg = netMessage.ReadMessage<PlayerHandUpdateMessage>();

		playingField.AddCardsToHand(msg.hand);
	}

	public void OnPlanningPhaseEnd(string hand, string attack, string defense, string bonus) {
		//oh shit son
		var msg = new PlayerPlanningPhaseDone();
		msg.hand	= hand;
		msg.attack	= attack;
		msg.defense = defense;
		msg.bonus	= bonus;
		msg.connID	= actualConnectionID;

		client.Send(AceMsgTypes.PlayerPlanningPhaseDone, msg);
	}

	private void OnPlayerInfoRecieve(NetworkMessage netMessage) {
		var msg = netMessage.ReadMessage<ObscuredPlayerInfoMessage>();
		
		if (actualConnectionID == msg.connID) {
			Debug.Log("This is me!!! Ignoring it tho");
			return;
		}

		//playingfield stuff, updating the newly added battlefield shit
		battleField.ProcessPlayer(msg);
	}

	private void OnBattlePhaseStart(NetworkMessage netMessage) {
		GameState.SetState(GameState.BattlePhase);
		playingField.OnBattlePhaseStart();

		OnBattlePhaseStartEvent.Invoke();
		battleField.DisplayEnemy(0);
	}

	private void OnPlayerTurn(NetworkMessage netMessage) {
		playingField.StartTurn();
	}

	public void OnConfirmationAttack(int attackID, int defenseID, int bonusID, string enemyName) {
		var message = new PlayerTurnFinishMessage();
		message.attackCardPosition = attackID;
		message.defenseCardPosition = defenseID;
		message.bonusCardPosition = bonusID;
		message.enemyName = enemyName;
		message.attackerName = nameInput.text;

		client.Send(AceMsgTypes.BattlePhase_PlayerTurnFinish, message);
	}

	public void OnBattleTurnResult(NetworkMessage netMessage) {
		var msg = netMessage.ReadMessage<TurnResultMessage>();

		Debug.Log("whelp");
	}



	public void Update() {
		if (Input.GetKey(KeyCode.R)) {
			ResetGame();
		}
	}

	public void ResetGame() {
		client.Disconnect();
		SceneManager.LoadScene("ClientScene");
	}

}
