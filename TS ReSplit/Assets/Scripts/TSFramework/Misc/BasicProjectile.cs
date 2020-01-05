using UnityEngine;

// TODO: Object pooling or swap over to ECS
// this is pretty eh, replace!
public class BasicProjectile : MonoBehaviour
{
    public GameObject Owner;
    //public float Speed;
    public float Damage;
    //public Vector3 Dir;

    protected Rigidbody RB;
    protected Inventory OwnerInv;
    protected GameObject SparkGO = null;

    protected TimeLimitedAction RemoveActioner = null;
    protected bool ShouldRemove                = false;

    public void Start()
    {
        RB       = GetComponent<Rigidbody>();
        OwnerInv = Owner.GetComponent<Inventory>();

        RemoveActioner = new TimeLimitedAction(1f, Remove);
    }

    public void Shoot(Vector3 Dir, float Speed, float Damage)
    {
        RB      = GetComponent<Rigidbody>();
        var vel = (Dir * Speed) - RB.velocity;
        RB.AddForce(vel, ForceMode.VelocityChange);
        this.Damage = Damage;
    }

    public void FixedUpdate()
    {
        //RemoveActioner.Run();
    }

    public void OnCollisionEnter(Collision Coll)
    {
        // This is eh
        SparkGO                    = Instantiate(OwnerInv.AssestReferances.HitSpark);
        SparkGO.transform.position = Coll.contacts[0].point;

        var damagable = Coll.gameObject.GetComponent<IDamageable>();
        damagable?.ApplyDamage(Damage);

        Destroy(gameObject);
        Destroy(SparkGO, 0.5f);
    }

    protected void Remove()
    {
        if (ShouldRemove)
        {
            Destroy(SparkGO);
            Destroy(gameObject);
        }
    }
}
