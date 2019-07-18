using UnityEngine;
using System.Collections;

public class AceMsgTypes {
	public const short LobbyJoinedSuccessful		= 101; //Server -> Client
	public const short LobbyInfoUpdate				= 102; //Server -> Client
	public const short GameStart					= 103; //Server -> All Clients
	public const short PlayerHandUpdate				= 104; //Server -> Client
	public const short PlayerPlanningPhaseDone		= 105; //Client -> Server
	public const short ObscuredPlayerInfo			= 106; //Server -> All Clients
	public const short BattlePhaseStart				= 107; //Server -> All Clients

	public const short BattlePhase_PlayerTurn		= 108; //Server -> Client
	public const short BattlePhase_PlayerTurnFinish = 109; //Client -> Server
	public const short BattlePhase_TurnResult		= 110; //Server -> All Clients

}
