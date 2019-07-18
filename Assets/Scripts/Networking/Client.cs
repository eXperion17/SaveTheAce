using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class Client : MonoBehaviour {
	protected int port = 9999;
	//string ip = "localhost";

	// The network client
	protected NetworkClient client;

	//Canvas
	[Header("Canvas References")]
	public TextMeshProUGUI logger;
	public TMP_InputField inputIP;
	public TMP_InputField nameInput;
	public Toggle isPlayerToggle;

	//Events
	public UnityEvent OnServerConnect;

	private void Start() {
		CreateClient();
	}

	public void CreateClient() {
		var config = new ConnectionConfig();

		// Config the Channels we will use
		config.AddChannel(QosType.ReliableFragmented);
		config.AddChannel(QosType.UnreliableFragmented);

		// Create the client ant attach the configuration
		client = new NetworkClient();
		client.Configure(config, 1);

		// Register the handlers for the different network messages
		RegisterHandlers();
	}

	public void ConnectToServer() {
		// Connect to the server
		client.Connect(inputIP.text, port);
	}

	// Register the handlers for the different message types
	protected virtual void RegisterHandlers() {

		// Unity have different Messages types defined in MsgType
		//client.RegisterHandler(Server.CONNECT, OnMessageReceived);

		client.RegisterHandler(MsgType.Connect, OnConnected);
		client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
	}

	void OnConnected(NetworkMessage message) {
		// Do stuff when connected to the server

		OnServerConnect.Invoke();
	}

	void OnDisconnected(NetworkMessage message) {
		// Do stuff when disconnected to the server
	}

	// Message received from the server
	void OnMessageReceived(NetworkMessage netMessage) {
		// You can send any object that inherence from MessageBase
		// The client and server can be on different projects, as long as the MyNetworkMessage or the class you are using have the same implementation on both projects
		// The first thing we do is deserialize the message to our custom type
		var objectMessage = netMessage.ReadMessage<MyNetworkMessage>();

		Debug.Log("Message received: " + objectMessage.message);
		logger.text += objectMessage.message + "\n";
	}



}