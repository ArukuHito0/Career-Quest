namespace CareerQuest.Enemy
{
    //  “G‚Ìó‘Ô
    public enum EnemyState : byte
    {
        Idle      = 0,  // ‰½‚à‚µ‚Ä‚È‚¢
        Search    = 1,  // üˆÍ‚Ì’Tõ’†
        Move      = 2,  // ˆÚ“®’†
        Attack    = 3,  // UŒ‚’†
        Vanishing = 4,  // Á–Å’†
        Dead      = 5,  // €–S
    }
}