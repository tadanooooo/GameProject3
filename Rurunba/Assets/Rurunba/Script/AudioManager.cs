using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    // スライダー
    public Slider BGMVolumeSlider;
    public Slider SEVolumeSlider;
    public GameObject AudioUI;

    // オーディオソース
    public AudioSource BGMaudioSource;
    public AudioSource SEaudioSource;

    // BGM・SEリスト
    public List<AudioClip> BGMs;
    public List<AudioClip> SEs;

    public bool Opened_Audio_Setting = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        // 保存された音量をスライダーに反映
        if (BGMVolumeSlider != null) BGMVolumeSlider.value = AudioUpdate.BGMsliderValue;
        if (SEVolumeSlider != null) SEVolumeSlider.value = AudioUpdate.SEsliderValue;

        // 現在の音量を実際のAudioSourceに即座に反映
        if (BGMaudioSource != null) BGMaudioSource.volume = AudioUpdate.BGMsliderValue;
        if (SEaudioSource != null) SEaudioSource.volume = AudioUpdate.SEsliderValue;

        // スライダーの値が変更された時の処理を登録
        if (BGMVolumeSlider != null) BGMVolumeSlider.onValueChanged.AddListener(ChangeVolumeBGM);
        if (SEVolumeSlider != null) SEVolumeSlider.onValueChanged.AddListener(ChangeVolumeSE);

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

    public void ToggleAudioSetting()
    {
        Opened_Audio_Setting = !Opened_Audio_Setting;
        if (AudioUI != null) AudioUI.SetActive(Opened_Audio_Setting);
    }

    void PauseGame() { Time.timeScale = 0f; }
    void ResumeGame() { Time.timeScale = 1f; }
}