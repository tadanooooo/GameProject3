using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    // Singleton
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
        // 自分をInstanceに登録
        Instance = this;
    }

    void Start()
    {
        // 保存音量反映
        bgmSlider.value = AudioUpdate.BGMsliderValue;
        seSlider.value = AudioUpdate.SEsliderValue;

        // 初期音量設定
        bgmSource.volume = bgmSlider.value;
        seSource.volume = seSlider.value;

        // Slider変更時イベント
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

    // UI表示切替
    public void ToggleAudioUI()
    {
        isOpen = !isOpen;
        audioUI.SetActive(isOpen);
    }

    // BGM再生
    public void PlayBGM(int index)
    {
        bgmSource.clip = bgms[index];
        bgmSource.Play();
    }

    // SE再生
    public void PlaySE(int index)
    {
        seSource.PlayOneShot(ses[index]);
    }
}