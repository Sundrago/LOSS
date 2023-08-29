using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;

public enum SFX_tag { darkFX1, darkFX2, darkFX3, bgm }

public class AudioCtrl : SerializedMonoBehaviour
{
    public static AudioCtrl Instance;

    [SerializeField] AudioSource sfx_source, bgm_source;

    [TableList(ShowIndexLabels = true)]
    [SerializeField] AudioData[] audioDatas;

    private float sfxVolume = 0.8f;
    private float bgmVolume = 0.8f;
    private AudioData bgmPlaying = null;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("settings_sfx_voulume"))
        {
            PlayerPrefs.SetFloat("settings_sfx_voulume", 0.8f);
            PlayerPrefs.SetFloat("settings_bgm_voulume", 0.8f);
        }

        SetVolume();
    }

    public void PlaySFXbyTag(SFX_tag tag, float randomPercent = 15f)
    {
        sfx_source.pitch *= 1 + UnityEngine.Random.Range(-randomPercent / 100, randomPercent / 100);
        foreach (AudioData data in audioDatas)
        {
            if (data.tag == tag)
            {
                sfx_source.PlayOneShot(data.src, data.volume * sfxVolume);
            }
        }
    }

    public void SetVolume()
    {
        sfxVolume = PlayerPrefs.GetFloat("settings_sfx_voulume");
        bgmVolume = PlayerPrefs.GetFloat("settings_bgm_voulume");
        sfx_source.volume = PlayerPrefs.GetFloat("settings_sfx_voulume");

        if (bgmPlaying == null) bgm_source.volume = PlayerPrefs.GetFloat("settings_bgm_voulume");
        else bgm_source.volume = PlayerPrefs.GetFloat("settings_bgm_voulume") * bgmPlaying.volume;
    }

    public void PlayBGM(SFX_tag tag)
    {
        foreach (AudioData data in audioDatas)
        {
            if (data.tag == tag)
            {
                bgmPlaying = data;
                bgm_source.clip = data.src;
                bgm_source.volume = data.volume * bgmVolume;
                bgm_source.Play();
                return;
            }
        }
    }

    [Serializable]
    public class AudioData
    {
        public SFX_tag tag;
        public AudioClip src;
        [Range(0f, 1f)]
        public float volume = 0.8f;
    }

}