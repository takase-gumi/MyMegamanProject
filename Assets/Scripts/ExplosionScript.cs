using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
    int damage = 0;

    public void SetDamageValue(int damage)
    {
        this.damage = damage;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (this.damage > 0)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                PlayerController2D player = other.gameObject.GetComponent<PlayerController2D>();
                player.HitSide(transform.position.x > player.transform.position.x);
                player.TakeDamage(this.damage);
            }
        }
    }
}
