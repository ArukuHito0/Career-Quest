using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "AudioSO/AudioDatabase")]
public class AudioDatabase : ScriptableObject
{
    [SerializeField] private List<BGMData> bgmList = new();
    [SerializeField] private List<SEData> seList = new();

    public List<BGMData> BgmList => bgmList;
    public List<SEData> SeList => seList;
}

public enum BGMType
{
    testBGM_1,
    testBGM_2,
    testBGM_3,
}

public enum SEType
{
    testSE_1,
    testSE_2,
    testSE_3,
}

[System.Serializable]
public class BGMData
{
    public BGMType type;
    public AudioClip clip;
}

[System.Serializable]
public class SEData
{
    public SEType type;
    public AudioClip clip;
}
