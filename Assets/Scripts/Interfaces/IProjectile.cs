﻿using UnityEngine;

public interface IProjectile
{
    int Damage { get; }
    GameObject gameObject { get; }
    MeshRenderer MeshRenderer { get; }
    void Spawn(Transform spawnPoint, ref IPlayer owner, Material material);
}