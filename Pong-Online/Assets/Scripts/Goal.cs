using Unity.Netcode;
using UnityEngine;

public class Goal : NetworkBehaviour
{
    [SerializeField] private GameManager gameManager;
    
    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

	private void OnCollisionEnter2D(Collision2D collision)
	{
        if (gameManager != null && IsServer)
            gameManager.Goal(this);
	}
}
