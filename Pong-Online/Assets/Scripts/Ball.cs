using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{
	private Rigidbody2D rb2D;
	[SerializeField] private float movementSpeed;

	private void Awake()
	{
		rb2D = GetComponent<Rigidbody2D>();
	}

	[Rpc(SendTo.Server)]
	public void ResetBallRPC(bool continueGame)
	{
		transform.position = Vector2.zero;
		rb2D.velocity = Vector2.zero;
		if (continueGame)
		{
			StartCoroutine(nameof(WaitForReset));
		}
	}

	public IEnumerator WaitForReset()
	{
		while (!transform.position.Equals(Vector2.zero))
		{
			ResetBallRPC(false);
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(1.5f);
		StartRoundRPC();
	}

	[Rpc(SendTo.Server)]
	private void StartRoundRPC()
	{
		float rotationX = Random.value < 0.5f ? -1 : 1;
		float rotationY = Random.value < 0.5f ? Random.Range(-1, -0.3f) : Random.Range(0.3f, 1);
		rb2D.bodyType = RigidbodyType2D.Dynamic;
		rb2D.AddForce(new Vector2(rotationX, rotationY) * movementSpeed);
	}

	[Rpc(SendTo.Server)]
	public void AddForceRPC(Vector2 force)
	{
		rb2D.AddForce(force);
	}
}
