using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UI : NetworkBehaviour
{
    private GameManager gameManager;
	[SerializeField] private TMP_Text playerOneGoalText;
	[SerializeField] private TMP_Text playerTwoGoalText;
	[SerializeField] private TMP_Text victoryText;
	[SerializeField] private TMP_InputField goalInput;
	[SerializeField] private GameObject chatObject;
	[SerializeField] private Button hostOnline;
	[SerializeField] private Button readyButton;
	[SerializeField] private Button joinOnline;

	public void Start()
	{
		gameManager = FindAnyObjectByType<GameManager>();
		playerOneGoalText.enabled = false;
		playerTwoGoalText.enabled = false;
		goalInput.gameObject.SetActive(false);
		readyButton.gameObject.SetActive(false);
	}

	public void UpdatePlayerOneUI(int previousValue, int newValue)
	{
		playerOneGoalText.text = newValue.ToString();
	}

	public void UpdatePlayerTwoUI(int previousValue, int newValue)
	{
		playerTwoGoalText.text = newValue.ToString();
	}

	private void Victory(bool isPlayerOne)
	{
		chatObject.SetActive(true);
		readyButton.gameObject.SetActive(true);
		if (IsServer)
			goalInput.gameObject.SetActive(true);
		victoryText.text = isPlayerOne ? "Player One Wins" : "Player Two Wins";
	}


	private void StartLobby()
	{
		readyButton.gameObject.SetActive(true);
		chatObject.SetActive(true);
		hostOnline.gameObject.SetActive(false);
		joinOnline.gameObject.SetActive(false);

		if (IsServer)
			goalInput.gameObject.SetActive(true);
	}
	
	private void StartGame(int goalsToWin)
	{
		chatObject.SetActive(false);
		playerOneGoalText.enabled = true;
		playerTwoGoalText.enabled = true;
		victoryText.text = "First to " + goalsToWin;
	}
	

	[Rpc(SendTo.Everyone)]
	public void StartLobbyRPC()
	{
		StartLobby();
	}

	[Rpc(SendTo.Everyone)]
	public void StartGameRPC(int goalsToWin)
	{
		StartGame(goalsToWin);
	}

	[Rpc(SendTo.Everyone)]
	public void VictoryRPC(bool isPlayerOne)
	{
		Victory(isPlayerOne);
	}
	
	public void Ready()
	{
		readyButton.gameObject.SetActive(false);
		victoryText.text = "Waiting for other player";
		if (IsServer)
		{
			goalInput.gameObject.SetActive(false);
			if (goalInput.text.Equals(""))
			{
				goalInput.text = "3";
			}
			gameManager.SetGoalsRPC(int.Parse(goalInput.text));
		}
		gameManager.ReadyRPC();
	}
}