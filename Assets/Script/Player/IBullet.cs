using UnityEngine;

namespace CareerQuest.Player
{
    public interface IBullet
    {
        byte Damage { get; }
        bool IsActive { get; }
        //  ’e‚ÌŒú‚Ý(“–‚½‚è”»’è—p)
        float Tickness { get; }
        Vector3 Position { get; }
    }
}