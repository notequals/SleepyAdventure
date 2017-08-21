﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PathSquare
{
    public Square square;

    public IPosition pos;

    public double gScore;
    public double fScore;

    public PathSquare parent = null;

    public Dictionary<PathSquare, double> neighbours;

    public PathSquare(Square square)
    {
        this.square = square;
        this.pos = square.position;

        this.neighbours = new Dictionary<PathSquare, double>();

        gScore = 9999;
        fScore = 9999;
    }

    public double Distance(PathSquare sqr)
    {
        return sqr.square.position.Distance(this.square.position);
    }

    public override bool Equals(object obj)
    {
        if (obj is PathSquare)
        {
            return Equals((PathSquare)this);
        }

        return false;
    }

    public bool Equals(PathSquare obj)
    {
        return obj.square.position == this.square.position;
    }

    public override int GetHashCode()
    {
        return this.square.position.GetHashCode();
    }
}
