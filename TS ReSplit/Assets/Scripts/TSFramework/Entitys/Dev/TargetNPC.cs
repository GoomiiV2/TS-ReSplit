using UnityEngine;

[RequireComponent(typeof(PlayerAnimController))]
public class TargetNPC : MonoBehaviour, IDamageable
{
    private PlayerAnimController PlayerAnimator = null;

    public void Start()
    {
        PlayerAnimator = GetComponent<PlayerAnimController>();
    }

    public float ApplyDamage(float DamageAmount)
    {
        Debug.Log($"Hit for: {DamageAmount}");
        PlayerAnimator.PlayHitAnimation();
        return 0f;
    }

    public bool CanBeDamaged()
    {
        return true;
    }
}
