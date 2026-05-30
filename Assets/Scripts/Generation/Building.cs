/*
* Structure for a building that is populated on the grid
* It contains a reference to the building GameObject and its size in grid units. 
*/

using System;
using UnityEngine;

[Serializable]
public struct Building
{
    [Tooltip("Reference to GameObject that will be the physical building")]
    public GameObject building;
    [Tooltip("Size of the building in grid units (e.g. 2x3)")]
    public Vector2Int size;
}
