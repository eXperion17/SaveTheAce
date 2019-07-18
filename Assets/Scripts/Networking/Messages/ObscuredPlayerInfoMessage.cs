using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ObscuredPlayerInfoMessage : MessageBase {
	public int connID;
	public string playerName;
	public int attackCount;
	public int defenseCount;
	public string bonus;
	public int handCount;
	public int drawPileCount;
	public int discardPileCount;
	public bool hasAce;
}
