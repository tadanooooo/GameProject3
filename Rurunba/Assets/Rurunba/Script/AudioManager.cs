using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    // スライダー
    public Slider BGMVolumeSlider;
    public Slider SEVolumeSlider;
    public GameObject AudioUI;

    // オーディオソース（インスペクターで直接 AudioSource をアタッチできるように型を変更）
    public AudioSource BGMaudioSource;
    public AudioSource SEaudioSource;

    // BGM・SEリスト
    public List<AudioClip> BGMs;
    public List<AudioClip> SEs;

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
        // 保存された音量をスライダーに反映
        BGMVolumeSlider.value = AudioUpdate.BGMsliderValue;
        SEVolumeSlider.value = AudioUpdate.SEsliderValue;

        // 現在の音量を実際のAudioSourceに即座に反映
        if (BGMaudioSource != null) BGMaudioSource.volume = AudioUpdate.BGMsliderValue;
        if (SEaudioSource != null) SEaudioSource.volume = AudioUpdate.SEsliderValue;

        // スライダーの値が変更された時の処理を「最初の一回だけ」登録（Updateから引越し）
        BGMVolumeSlider.onValueChanged.AddListener(ChangeVolumeBGM);
        SEVolumeSlider.onValueChanged.AddListener(ChangeVolumeSE);

        // 初期状態では設定画面を隠す
        if (AudioUI != null) AudioUI.SetActive(false);
    }

    void ChangeVolumeBGM(float newVolume)
    {
        AudioUpdate.BGMsliderValue = newVolume; // 共有データを更新
        if (BGMaudioSource != null) BGMaudioSource.volume = newVolume; // 音量を変更
    }

    void ChangeVolumeSE(float newVolume)
    {
        AudioUpdate.SEsliderValue = newVolume; // 共有データを更新
        if (SEaudioSource != null) SEaudioSource.volume = newVolume; // 音量を変更
    }

    // BGM再生
    public void PlayBGM(int index)
    {
        Opened_Audio_Setting = !Opened_Audio_Setting;
        if (AudioUI != null) AudioUI.SetActive(Opened_Audio_Setting);

        // 設定を閉じた時のゲーム再開処理などが必要ならここにPause/Resumeを入れます
    }

    void PauseGame() { Time.timeScale = 0f; }
    void ResumeGame() { Time.timeScale = 1f; }
}