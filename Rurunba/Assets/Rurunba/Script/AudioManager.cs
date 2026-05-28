using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Slider")]
    public Slider bgmSlider;
    public Slider seSlider;

    [Header("AudioSource")]
    public AudioSource bgmSource;
    public AudioSource seSource;

    [Header("UI")]
    public GameObject audioUI;

    [Header("Audio List")]
    public List<AudioClip> bgms;
    public List<AudioClip> ses;

    private bool isOpen = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 保存された音量を反映
        bgmSlider.value = AudioUpdate.BGMsliderValue;
        seSlider.value = AudioUpdate.SEsliderValue;

        // 音量設定
        bgmSource.volume = bgmSlider.value;
        seSource.volume = seSlider.value;

        // スライダー変更時
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        seSlider.onValueChanged.AddListener(SetSEVolume);

        // UI非表示
        audioUI.SetActive(false);
    }

    // BGM音量変更
    public void SetBGMVolume(float volume)
    {
        AudioUpdate.BGMsliderValue = volume;
        bgmSource.volume = volume;
    }

    // SE音量変更
    public void SetSEVolume(float volume)
    {
        AudioUpdate.SEsliderValue = volume;
        seSource.volume = volume;
    }

    // 音量UI表示切替
    public void ToggleAudioUI()
    {
        isOpen = !isOpen;
        audioUI.SetActive(isOpen);
    }

    // SE再生
    public void PlaySE(int index)
    {
        seSource.PlayOneShot(ses[index]);
    }

    // BGM再生
    public void PlayBGM(int index)
    {
        bgmSource.clip = bgms[index];
        bgmSource.Play();
    }

}