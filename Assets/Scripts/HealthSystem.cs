using UnityEngine;

public sealed class HealthSystem : MonoBehaviour
{
    [SerializeField, Min(1)] private int maximumHealth = 3;

    public int CurrentHealth { get; private set; }
    public int MaximumHealth => maximumHealth;

    private void Awake()
    {
        maximumHealth = Mathf.Max(1, maximumHealth);
        CurrentHealth = maximumHealth;
    }

    public void Damage(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerHealthChanged(CurrentHealth);
    }

    public void Heal(int amount)
    {
        if (amount > 0)
            CurrentHealth = Mathf.Min(maximumHealth, CurrentHealth + amount);
    }

    public void ResetHealth()
    {
        CurrentHealth = maximumHealth;
    }
}
