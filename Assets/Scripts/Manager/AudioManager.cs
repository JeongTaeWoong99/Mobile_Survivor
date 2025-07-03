using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public enum Sfx {Dead , EnemyHit, LevelUp, Lose, Win ,Select, Resurrection, SkiilUse, Range, Slash, Landing, Engage , Acquisition, FireAttack}
    
    [Header("----- BGM -----")]
    public  List<AudioClip>     bgmClip;    // 0?? ???????, 1?? ?????
    public  float               bgmVolume;
    private AudioSource         bgmPlayer;
    private AudioHighPassFilter bgmEffect;  // ?????? ???? ??, BGM?? ?????? ????? ????? ??.
    
    [Header("----- SFX ------")]
    public  AudioClip[]   sfxClips;           
    public  float         sfxVolume;          
    public  int           channelNumber;        // ??? ??????? ?????? ????? ???? ?? ??????
    private AudioSource[] sfxPlayers;           // ??? ?? ???, ???????
    private int           currentChannelNumber; // ????, ????? ????
    

    void Awake()
    {
        instance = this;
        Init();
    }

    void Init()
    {
        // ????? ??????? ????
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer                  = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake      = false;
        bgmPlayer.loop             = true;
        bgmPlayer.volume           = bgmVolume;
        bgmEffect                  = Camera.main.GetComponent<AudioHighPassFilter>();
        PlayBgm(0,true);
        
        // ????? ??????? ????
        GameObject sfxObject       = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers                 = new AudioSource[channelNumber]; // ??? ?? ???, ????? ??? ??? ?????
        
        for (int index = 0; index < sfxPlayers.Length; index++) // ????? ??? ????
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake           = false;
            sfxPlayers[index].bypassListenerEffects = true;      // ??? ?????? true???, AudioHighPassFilter?? ?????? ???? ????.
            sfxPlayers[index].volume                = sfxVolume;
        }
    }
    
    public void PlayBgm(int clipNum, bool isPlay)
    {
        bgmPlayer.clip = bgmClip[clipNum];
        if (isPlay)
            bgmPlayer.Play();
        else
            bgmPlayer.Stop();
    }
    
    public void EffectBgm(bool isPlay)
    {
        bgmEffect.enabled = isPlay;
    }
    
    public void PlaySfx(Sfx sfx)
    {
        // sfxPlayers.Length?? channels ????? ????
        for (int index = 0; index < sfxPlayers.Length; index++) 
        {
            // currentChannelNumber????, ??? ??
            int loopIndex = (index + currentChannelNumber) % sfxPlayers.Length; // % sfxPlayers.Length?? ???????? ??? ?????? 
                                                                                // loopIndex???? ChannelNumber?? ???? ????? ??? ????.
           // ?????? ??????, ?????
            if (sfxPlayers[loopIndex].isPlaying)
                continue;

            // 2???? ???? ????? ???? ???
            // int ranIndex = 0;
            // if (sfx == Sfx.Hit || sfx == Sfx.Melee)
            //     ranIndex = Random.Range(0, 2);

            currentChannelNumber = loopIndex;                // currentChannelNumber ????
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx]; // ??? ????
            sfxPlayers[loopIndex].Play();                    // ???
            break;
        }
    }
}

