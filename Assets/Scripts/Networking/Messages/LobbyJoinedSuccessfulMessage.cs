using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class LobbyJoinedSuccessfulMessage : MessageBase {
	public bool successful;
	public int clientConnectionID;
}
