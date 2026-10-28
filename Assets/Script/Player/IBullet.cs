using UnityEngine;

namespace CareerQuest.Player
{
    public interface IBullet
    {
        byte Damage { get; }
        Vector3 Position { get; }
    }
}