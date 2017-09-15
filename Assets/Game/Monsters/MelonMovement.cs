﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

public class MelonMovement : AppleMovement
{
    public CircularSpell explosion;

    void Update()
    {
        if (isDead)
        {
            return;
        }

        Move();
        Attack();

        if (GameManager.time - lastUpdate > updateFrequency)
        {
            GetDestination();
            Idle();
            lastUpdate = GameManager.time;
        }

    }

    public new void Attack()
    {
        if (GameManager.time - lastAttack > attackFrequency)
        {
            var pos = GameManager.instance.player.pos2d;

            if (pos2d.Distance(pos) < 2)
            {
                anim.SetTrigger("Attack");
                
                GameManager.instance.CreateCircularSpell(this, explosion, this.transform.position);                
                lastAttack = GameManager.time;

                Death();
            }
        }
    }
}
