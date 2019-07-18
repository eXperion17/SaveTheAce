using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerPlanningPhaseDone : MessageBase {
	public int connID;
	public string hand;
	public string attack;
	public string defense;
	public string bonus;
}
