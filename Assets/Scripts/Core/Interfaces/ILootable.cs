using UnityEngine;

public interface ILootable
{
    void Pickup(ref IPlayer player);
    bool Exists();
    Transform transform { get; }
}