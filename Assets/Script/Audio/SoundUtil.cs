public static class SoundUtil
{
    /// <summary>
    /// BGM‚ğÄ¶‚·‚é
    /// </summary>
    public static void PlayBGM(BGMType type, float volume = 1, float pitch = 1, float delay = 0)
    {
        AudioManager.Instance.PlayBGM(type, volume, pitch, delay);
    }

    /// <summary>
    /// SE‚ğÄ¶‚·‚é
    /// </summary>
    public static void PlaySE(SEType type, float volume = 1, float pitch = 1, float delay = 0)
    {
        AudioManager.Instance.PlaySE(type, volume, pitch, delay);
    }

    /// <summary>
    /// BGM‚ğˆê’â~‚·‚é
    /// </summary>
    public static void PauseBGM()
    {
        AudioManager.Instance.PauseBGM();
    }

    /// <summary>
    /// BGM‚Ìˆê’â~‚ğ‰ğœ
    /// </summary>
    public static void UnPauseBGM()
    {
        AudioManager.Instance.UnPauseBGM();
    }

    /// <summary>
    /// BGM‚ğŠ®‘S‚É’â~
    /// </summary>
    public static void StopBGM()
    {
        AudioManager.Instance.StopBGM();
    }
}
