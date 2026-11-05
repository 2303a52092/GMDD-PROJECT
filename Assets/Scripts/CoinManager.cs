using UnityEngine;

public sealed class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    private int collected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void Collect(GameObject coin)
    {
        if (coin == null || !coin.activeInHierarchy)
            return;

        collected++;
        if (GameManager.Instance != null)
            GameManager.Instance.AddCoins(1);
        Destroy(coin);
    }

    public void ResetRun()
    {
        collected = 0;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
