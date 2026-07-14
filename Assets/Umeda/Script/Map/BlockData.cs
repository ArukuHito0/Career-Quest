using UnityEngine;
using System.Collections.Generic;

public enum BlockType { Soil, Grass, Sand, Rock, Snow, Water, Magma }

[System.Serializable]
public class BlockData
{
    public string name;
    public GameObject surfacePrefab; // 表面用（水・溶岩など）
    public GameObject internalPrefab; // 内部用（通常のブロックならこちらに同じものを）
    public BlockType type;
}