using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// オーディオクリップを登録して提供するクラス
/// </summary>
public class AudioClipProvider
{
    private readonly Dictionary<BGMType, AudioClip> bgmDict = new();
    private readonly Dictionary<SEType, AudioClip> seDict = new();

    public AudioClip GetBGMClip(BGMType type)
    {
        if (bgmDict.ContainsKey(type))
        {
            return bgmDict[type];
        }
        else
        {
            Debug.LogError($"BGMType[{type}]が見つかりませんでした");
            return null;
        }
    }

    public AudioClip GetSEClip(SEType type)
    {
        if (seDict.ContainsKey(type))
        {
            return seDict[type];
        }
        else
        {
            Debug.LogError($"SEType[{type}]が見つかりませんでした");
            return null;
        }
    }

    private void SetAudioDictionaries(AudioDatabase database)
    {
        foreach (BGMData bgm in database.BgmList)
        {
            if (!bgmDict.ContainsKey(bgm.type))
            {
                bgmDict.Add(bgm.type, bgm.clip);
            }
        }

        foreach (SEData se in database.SeList)
        {
            if (!seDict.ContainsKey(se.type))
            {
                seDict.Add(se.type, se.clip);
            }
        }
    }

    public AudioClipProvider(AudioDatabase database)
    {
        SetAudioDictionaries(database);
    }
}
