using UnityEngine;
using System.Collections;

public class GameState {
	public const short PlanningPhase = 1;
	public const short BattlePhase = 2;

	private static short state;

	public static short currentState {
		get {
			return state;
		}
	}

	public static short GetCurrentState() {
		return state;
	}

	public static void SetState(short st) {
		state = st;
	}

}