using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
	private string nameOfPlayer;
	private NetworkVariable<int> playerOneGoalCount = new(0);
    private NetworkVariable<int> playerTwoGoalCount = new(0);
	private NetworkVariable<int> playersReady = new(0);
	private NetworkVariable<int> goalsToWin = new(0);
	private Ball ball;

	[SerializeField] private Chat chat;
    [SerializeField] private NetworkManager networkManager;
	[SerializeField] private GameObject ballPrefab;
	[SerializeField] private UI ui;
    [SerializeField] private Transform playerOneStartTransform;
	[SerializeField] private Transform playerTwoStartTransform;
	[SerializeField] private Goal playerOneGoal;
	[SerializeField] private Goal playerTwoGoal;

	private void Start()
	{
        networkManager.ConnectionApprovalCallback = ConnectionApprovalCallback;
		playerOneGoalCount.OnValueChanged += ui.UpdatePlayerOneUI;
		playerTwoGoalCount.OnValueChanged += ui.UpdatePlayerTwoUI;
	}

	private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
	{
        var players = FindObjectsOfType<PlayerController>();

		response.Approved = true;
		response.CreatePlayerObject = true;
		
		switch (players.Length)
        {
			case 0:
			response.Position = playerOneStartTransform.position;
			StartCoroutine(StartHost());
			break;

			case 1:
			response.Position = playerTwoStartTransform.position;
			chat.ClearMessages();
			StartCoroutine(nameof(StartServer));
			break;

            default:
            response.Approved = false;
			response.CreatePlayerObject = false;
			break;
		}
	}

	private IEnumerator StartHost()
	{
		yield return new WaitForSeconds(0.1f);
		chat.SendMessageServerRPC(GetComponent<Relay>().Code, "Server Code");
	}
	
	private IEnumerator StartServer()
	{
		yield return new WaitForSeconds(0.1f);
		SpawnBallRPC();
		ui.StartLobbyRPC();
	}
	

	[Rpc(SendTo.Server)]
	private void CheckGoalRPC(bool isPlayerOneGoal)
	{
        if (isPlayerOneGoal)
			playerTwoGoalCount.Value++;
        else
            playerOneGoalCount.Value++;
    
        if (playerOneGoalCount.Value >= goalsToWin.Value || playerTwoGoalCount.Value >= goalsToWin.Value)
        {
            ball.ResetBallRPC(false);
	        ui.VictoryRPC(playerOneGoalCount.Value > playerTwoGoalCount.Value);
        }
		else
			ball.ResetBallRPC(true);
	}
	
	[Rpc(SendTo.Server)]
	private void SpawnBallRPC()
	{
		NetworkObject networkBall = Instantiate(ballPrefab).GetComponent<NetworkObject>();
		networkBall.Spawn();
		ball = networkBall.GetComponent<Ball>();
	}

	private void OnGUI()
	{
		if (GUILayout.Button("Quit"))
		{
			Application.Quit();
		}
	}

	[Rpc(SendTo.Server)]
	public void SetGoalsRPC(int setGoals)
	{
		goalsToWin.Value = setGoals;
	}
	
	public void Goal(Goal goal)
    {
	    if (IsServer)
	    {
			if (goal == playerOneGoal || goal == playerTwoGoal)
				CheckGoalRPC(goal == playerOneGoal);
	    }
    }

	[Rpc(SendTo.Server)]
	public void ReadyRPC()
	{
		playersReady.Value++;
		if (playersReady.Value == 2)
		{
			playersReady.Value = 0;
			playerOneGoalCount.Value = 0;
			playerTwoGoalCount.Value = 0;
			foreach (var playerController in FindObjectsOfType<PlayerController>())
			{
				playerController.ResetPosition();
			}
			ui.StartGameRPC(goalsToWin.Value);
			ball.ResetBallRPC(true);
		}
	}
	
	public void SetName(string name)
	{
		nameOfPlayer = name;
	}

	public string GetName()
	{
		return nameOfPlayer;
	}
}
