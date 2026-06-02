using System;
using UnityEngine;

namespace BadWizards.ChunkData {
[Serializable]
/// <summary>
/// Structure for a road that is populated on the grid
/// </summary>
public struct RoadObject
{
    public ChunkObject.RoadIdentity identity;
    public GameObject road;
}}
