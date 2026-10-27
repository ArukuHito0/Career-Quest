namespace CareerQuest.Enemy
{
    [System.Flags]
    public enum EnemyUniqueMoveFlag : uint
    {
        None = 0,
        Warping = 1 << 0,      // ’èŠú“I‚ÉÁ‚¦‚ÄŒ»‚ê‚é
        Shooter = 1 << 1,      // ’e‚ðŒ‚‚Â
        SwarmLeader = 1 << 2,  // ŒQ‚ê‚ÌƒŠ[ƒ_[
        Healer = 1 << 3,       // ŽüˆÍ‚ð‰ñ•œ‚·‚é
    }
}