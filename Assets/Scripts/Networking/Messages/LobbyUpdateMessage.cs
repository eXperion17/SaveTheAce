using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class LobbyUpdateMessage : MessageBase {
	public string connectedPlayers;
}
