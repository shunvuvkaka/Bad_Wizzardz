
using System;
using UnityEngine;

[Serializable]
public struct BasePoints
{
    public Vector3 br;
    public Vector3 tr;
    public Vector3 bl;
    public Vector3 tl;

    public BasePoints(Vector3 bottomRight, Vector3 topRight, Vector3 bottomLeft, Vector3 topLeft)
    {
        br = bottomRight;
        bl = bottomLeft;
        tr = topRight;
        tl = topLeft;
    }
}

