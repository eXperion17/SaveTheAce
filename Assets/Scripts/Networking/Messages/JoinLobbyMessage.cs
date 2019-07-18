using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class JoinLobbyMessage : MessageBase {
	public const short type = MsgType.AddPlayer;

	public string playerName;
	public bool isPlayer;
}
