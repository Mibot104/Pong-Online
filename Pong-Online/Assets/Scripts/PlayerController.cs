using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
	private Vector3 movement;
	private PlayerInput input;
	[SerializeField] private float movementSpeed;
	[SerializeField] private float minY;
	[SerializeField] private float maxY;
	
	private void Awake()
	{
		input = new PlayerInput();
	}

	private void Start()
	{		
		if (IsLocalPlayer)
		{
			input.Player.Movement.performed += OnMove;
			input.Player.Movement.canceled += CancelMove;
		}

		if (FindObjectOfType<Chat>())
		{
			input.Player.SendChatMessage.performed += FindObjectOfType<Chat>().CheckInputToSend;
		}
	}

	private void OnMove(InputAction.CallbackContext context)
	{
		MoveRPC(context.ReadValue<Vector2>());
	}

	private void CancelMove(InputAction.CallbackContext context)
	{
		MoveRPC(new Vector2());
	}
	
	public void ResetPosition()
	{
		if (IsServer)
		{
			Vector3 reset = transform.position;
			reset.y = 0;
			transform.position = reset;
		}
	}

	private void Update()
	{
		if (IsServer)
		{
			transform.position += movementSpeed * Time.deltaTime * movement;
			if (transform.position.y < minY || transform.position.y > maxY)
				transform.position -= movementSpeed * Time.deltaTime * movement;
		}
	}

	[Rpc(SendTo.Server)]
	private void MoveRPC(Vector2 data)
	{
		movement = data;
	}

	private void OnEnable()
	{
		input.Player.Movement.Enable();
		input.Player.SendChatMessage.Enable();
	}

	private void OnDisable()
	{
		input.Player.Movement.Disable();
		input.Player.SendChatMessage.Disable();
	}
}
