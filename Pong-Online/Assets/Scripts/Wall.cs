using Unity.Netcode;
using UnityEngine;

public class Wall : NetworkBehaviour
{
    [SerializeField] private float bouncyness;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (IsServer)
		{
			Ball ball = collision.gameObject.GetComponent<Ball>();
			if (ball != null)
			{
				Vector2 normal = collision.GetContact(0).normal;
				ball.AddForceRPC(-normal * bouncyness);
			}
		}
	}
}
