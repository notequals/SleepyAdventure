﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : Entity
{
    public float speed = 1.0f;
    public PathInfo path;
    
    public int health;
    public int maxHealth = 100;
    
    public Animator anim;

    [System.NonSerialized]
    public new Rigidbody rigidbody;

    //public new Renderer renderer;

    [System.NonSerialized]
    public new Collider collider;

    [System.NonSerialized]
    public IPosition nextPos = IPosition.zero;
    [System.NonSerialized]
    public bool canMove = true;

    protected virtual void Initialize()
    {
        //renderer = GetComponent<Renderer>();
        collider = GetComponent<Collider>();
        rigidbody = GetComponent<Rigidbody>();

        health = maxHealth;
    }

    public IEnumerator DisableUnitMovementHelper(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    public void DisableUnitMovement(float time)
    {
        StartCoroutine(DisableUnitMovementHelper(time));
    }

    public PathInfo UnitMoveTo(Vector3 to)
    {
        return GameManager.instance.UnitMoveTo(this, to);
    }

    public void DeleteUnit(float time = 0)
    {
        GameManager.instance.DeleteUnit(this, time);
    }

    public float GetPixelSize()
    {
        var distance = Vector3.Distance(this.transform.position, Camera.main.transform.position);
        float pixelSize = (collider.bounds.extents.magnitude * Mathf.Rad2Deg * Screen.height) 
                            / (distance * Camera.main.fieldOfView);
        return pixelSize;
    }

    public float GetBoundsSize()
    {
        Vector3 origin = Camera.main.WorldToScreenPoint(new Vector3(collider.bounds.min.x, collider.bounds.max.y, 0f));
        Vector3 extent = Camera.main.WorldToScreenPoint(new Vector3(collider.bounds.max.x, collider.bounds.min.y, 0f));

        return Vector3.Distance(extent, origin);
    }

    public float GetRelativeSizeRatio()
    {
        var distance = Vector3.Distance(this.transform.position, Camera.main.transform.position);
                                     
        return (15.5f * 55) / (distance * Camera.main.fieldOfView);
    }

    public void Stop()
    {
        rigidbody.velocity = Vector3.zero;

        path = null;
        canMove = false;
    }

    public void LookAt(Unit target)
    {
        LookAt(target.transform.position);
    }

    public void LookAt(Vector3 lookPos)
    {
        Vector3 dir = (lookPos.ConvertToIPosition().ToVector() 
            - transform.position.ConvertToIPosition().ToVector()).normalized;
        dir.y = 0;

        if(dir != Vector3.zero)
        {
            Quaternion newRotation = Quaternion.LookRotation(dir);
            rigidbody.MoveRotation(newRotation);
        }        
    }
}
