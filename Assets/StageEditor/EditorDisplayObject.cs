﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorDisplayObject : MonoBehaviour {

    public int cid;
    public int id;

    public IPosition pos;

    SquareObject sqrObject;

    public void RemoveObject()
    {
        Destroy(gameObject);
        sqrObject = null;
    }
}
