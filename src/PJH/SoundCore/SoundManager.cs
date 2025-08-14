using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum BgmType
{
    None,
    EntryScene,
    LobbyScene,
    BattleScene,
    BattleBoss,
    BattleReady,
    BlackSmith,
    Gacha
}

public enum SoundType
{
    Bgm,
    Sfx
}

public class SoundManager : Singleton<SoundManager>
{
    protected override bool ShouldDontDestroyOnLoad => true;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private string currentBgmType;

    private void Reset()
    {
        bgmSource = transform.FindChildByName<AudioSource>("AudioSource_Bgm");
        sfxSource = transform.FindChildByName<AudioSource>("AudioSource_Sfx");
    }

    public void Initialize()
    {
        foreach (SoundType soundType in Enum.GetValues(typeof(SoundType)))
        {
            SetVolume(soundType, GetVolume(soundType));
        }
    }
    
    public void PlayButtonSound()
    {
        AudioClip clip = ResourceManager.Instance.GetResource<AudioClip>(StringAdrAudioSfx.ButtonClick);
        sfxSource.PlayOneShot(clip); //PlayOneShot으로 한번 실행
    }

    public void PlaySfx(string sfxName)
    {
        AudioClip clip = ResourceManager.Instance.GetResource<AudioClip>(sfxName);
        sfxSource.PlayOneShot(clip);
    }

    public void PlayBgm(string bgmName)
    {
        if (currentBgmType == bgmName) return; // 중복 재생 방지
        currentBgmType = bgmName;
        
        bgmSource.clip = ResourceManager.Instance.GetResource<AudioClip>(bgmName);
        bgmSource.Play();
    }

    public void StopBGM() => bgmSource.Stop();

    public void SetVolume(SoundType soundType, float volume)
    {
        switch (soundType)
        {
            case SoundType.Bgm:
                bgmSource.volume = volume;
                break;
            case SoundType.Sfx:
                sfxSource.volume = volume;
                break;
        }
        
        PlayerPrefs.SetFloat(StringPrefs.SoundTypeDict[soundType], volume);
    }
    
    public float GetVolume(SoundType soundType)
    {
        return PlayerPrefs.GetFloat(StringPrefs.SoundTypeDict[soundType], 0.5f);
    }

    public void MuteBGM(bool isMuted)
    {
        bgmSource.mute = isMuted;     
    }

    public void MuteSoundEffect(bool isMuted)
    {
        sfxSource.mute = isMuted;
    }
    
    private void PlayRandomSfx(string[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        int randomIndex = UnityEngine.Random.Range(0, clips.Length);
        PlaySfx(clips[randomIndex]);
    }

    public void PlayRandomUnitAttackSfx() => PlayRandomSfx(StringAdrAudioSfx.RandomUnitAttackSfx);
    
    public void PlayRandomUnitHitSfx() =>  PlayRandomSfx(StringAdrAudioSfx.RandomUnitHitSfx);
    
     /*
      
    private readonly Dictionary<string, AudioClip> bgmDict = new();
    private readonly Dictionary<string, AudioClip> sfxDict = new();
    
    public void Initialization(SceneType sceneType)
    {
        // InitializationBGM(sceneType);
        // InitializationSfx(sceneType);
    }
    
    public void InitializationBGM(SceneType sceneType)
    {
        var sceneBgmMappings = new Dictionary<SceneType, BgmType[]>
        {
            { SceneType.Entry, new[] { BgmType.EntryScene } },
            { SceneType.Lobby, new[] { BgmType.LobbyScene, BgmType.BlackSmith, BgmType.Gacha } },
            { SceneType.Game, new[] { BgmType.BattleScene, BgmType.BattleBoss, BgmType.BattleReady } }
        };

        if (sceneBgmMappings.TryGetValue(sceneType, out var bgmTypes))
        {
            foreach (var bgmType in bgmTypes)
            {
                LoadBgm(bgmType);
            }
        }
    }
    private void LoadBgm(BgmType bgmType)
    {
        if (bgmDict.ContainsKey(bgmType)) return; // 이미 로드됨

        string bgmPath = bgmType switch
        {
            BgmType.EntryScene => StringAdrAudio.BgmEntryScene,
            BgmType.LobbyScene => StringAdrAudio.BgmLobbyScene,
            BgmType.BattleScene => StringAdrAudio.BgmBattleScene,
            BgmType.BattleBoss => StringAdrAudio.BgmBattleBoss,
            BgmType.BattleReady => StringAdrAudio.BgmBattleReady,
            BgmType.BlackSmith => StringAdrAudio.BgmBlackSmith,
            BgmType.Gacha => StringAdrAudio.BgmGacha,
            _ => throw new Exception("잘못된 BgmType입니다")
        };
       
        AudioClip clip = ResourceManager.Instance.GetResource<AudioClip>(bgmPath);
        
        if (clip != null) 
        {
            bgmDict[bgmType] = clip;
        }
    }

    public void InitializationSfx(SceneType sceneType)
    {
        var sceneSfxMappings = new Dictionary<SceneType, string[]>
        {
            [SceneType.Entry] = new[] 
            {
                StringAdrAudio.ButtonClickSfx,
            },
            [SceneType.Lobby] = new[] 
            {
                StringAdrAudio.ButtonClickSfx,
                StringAdrAudio.OpenSfx,
                StringAdrAudio.EquipmentSfx,
                StringAdrAudio.EnforceSfx,
                StringAdrAudio.ResolutionSfx,
                StringAdrAudio.BreakThroughSfx,
                StringAdrAudio.EquipmentSfx,
            },
            [SceneType.Game] = new[] 
            {
                StringAdrAudio.ButtonClickSfx,
                StringAdrAudio.AttackMonsterSfx,
                StringAdrAudio.AttackRangeSfx,
                StringAdrAudio.AttackMeleeSfx,
                StringAdrAudio.BattleLoseSfx,
                StringAdrAudio.BattleWinSfx,
                StringAdrAudio.UsherSkillSfx,
                StringAdrAudio.MomoSkillSfx,
                StringAdrAudio.RuruSkillSfx,
                StringAdrAudio.GetRewardSfx
            }
        };
        
        if (sceneSfxMappings.TryGetValue(sceneType, out var sfxList))
        {
            foreach (var sfxPath in sfxList)
            {
                LoadSfx(sfxPath);
            }
        }
    }
    private void LoadSfx(string sfxPath)
    {
        if (sfxDict.ContainsKey(sfxPath)) return;
        AudioClip clip = ResourceManager.Instance.GetResource<AudioClip>(sfxPath);
        if (clip != null)
        {
            sfxDict[sfxPath] = clip;
        }
    }
    */
}



