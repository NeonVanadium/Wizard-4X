using UnityEngine;

/// <summary>
/// A token is any game piece that is placed on a hex.
/// 
/// Units, structures, etc.
/// </summary>
public class Token : MonoBehaviour
{
    public Hex currentHex { get; set; }

    public Color color { get; private set; }

    public int sight { get => 2; } // how far this token can see

    public int maxHP { private get; set; } = 10;

    public int hp { get; private set; }

    public Player owner { get; private set; }

    private MeshRenderer rendy;

    public void Awake()
    {
        rendy = this.GetComponent<MeshRenderer>();

        this.hp = maxHP;

        Show(false);
    }

    public void AssignTo(Player p)
    {
        color = p.color;
        rendy.material.color = color;
        owner = p;
    }

    public void Show(bool val = true)
    {
        rendy.enabled = val;
    }

    #region Combat and Damage and Stuff
    public void Damage(int val)
    {
        ErrorIfNegative(val);

        hp -= val;

        if (hp <= 0)
        {
            Die();
        }
    }

    public void Heal(int val)
    {
        ErrorIfNegative(val);

        hp = Mathf.Min(hp + val, maxHP);
    }

    private void ErrorIfNegative(int val)
    {
        if (val < 0)
        {
            throw new System.Exception("Val must not be negative.");
        }
    }

    private void Die()
    {
        Show(false);
        this.owner.RemoveToken(this);
        this.currentHex.RemoveToken();
        print($"{this.name} dies.");
        Destroy(this.gameObject);
    }


    #endregion


}
