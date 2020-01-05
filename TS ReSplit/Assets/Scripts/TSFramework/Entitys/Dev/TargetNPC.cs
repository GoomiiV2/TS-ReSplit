using Assets.Scripts.TSFramework.Singletons;
using UnityEngine;

[RequireComponent(typeof(PlayerAnimController))]
public class TargetNPC : MonoBehaviour, IDamageable
{
    private PlayerAnimController PlayerAnimator = null;
    private AudioClip HitAudio;

    public void Start()
    {
        PlayerAnimator = GetComponent<PlayerAnimController>();
        HitAudio       = ReSplit.Audio.GetAudioClip("ts2/pak/sounds.pak/sfx/female_barbera22_23.vag");
    }

    public float ApplyDamage(float DamageAmount)
    {
        Debug.Log($"Hit for: {DamageAmount}");
        PlayerAnimator.PlayHitAnimation();
        AudioSource.PlayClipAtPoint(HitAudio, transform.position);
        return 0f;
    }

    public bool CanBeDamaged()
    {
        return true;
    }
}
