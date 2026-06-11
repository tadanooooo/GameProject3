using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    // Singleton
    public static AudioManager Instance;

    [Header("Slider (無いシーンでは空っぽでOK)")]
    public Slider bgmSlider;
    public Slider seSlider;

    [Header("AudioSource")]
    public AudioSource bgmSource;
    public AudioSource seSource;

    [Header("UI (無いシーンでは空っぽでOK)")]
    public GameObject audioUI;

    [Header("Audio List")]
    public List<AudioClip> bgms;
    public List<AudioClip> ses;

    private bool isOpen = false;

    void Awake()
    {
        // 複雑な処理は無し！シンプルに自分を登録
        Instance = this;
    }

    void Start()
    {
        // 【音量の維持】保存されたデータを読み込む（無ければ初期値 1.0）
        float savedBGM = PlayerPrefs.GetFloat("SavedBGMVolume", 1.0f);
        float savedSE = PlayerPrefs.GetFloat("SavedSEVolume", 1.0f);

        // スピーカーに音量を適用
        bgmSource.volume = savedBGM;
        seSource.volume = savedSE;

        // 【エラー対策】もし画面にスライダーが存在する場合だけ、スライダーの設定を行う
        if (bgmSlider != null)
        {
            bgmSlider.value = savedBGM;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (seSlider != null)
        {
            seSlider.value = savedSE;
            seSlider.onValueChanged.AddListener(SetSEVolume);
        }

        // 【エラー対策】もし画面にUIオブジェクトがある場合だけ非表示にする
        if (audioUI != null)
        {
            audioUI.SetActive(false);
        }
    }

    // BGM音量変更（スライダーを動かした時に自動で保存される）
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat("SavedBGMVolume", volume); // スマホ内へ保存
        PlayerPrefs.Save();
    }

    // SE音量変更（スライダーを動かした時に自動で保存される）
    public void SetSEVolume(float volume)
    {
        seSource.volume = volume;
        PlayerPrefs.SetFloat("SavedSEVolume", volume); // スマホ内へ保存
        PlayerPrefs.Save();
    }

    // UI表示切替
    public void ToggleAudioUI()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }
        if (audioUI == null) return; // UIが無いシーンなら無視
        isOpen = !isOpen;
        audioUI.SetActive(isOpen);
    }

    // BGM再生
    public void PlayBGM(int index)
    {
        if (index < 0 || index >= bgms.Count || bgms[index] == null) return;
        bgmSource.clip = bgms[index];
        bgmSource.Play();
    }

    // SE再生
    public void PlaySE(int index)
    {
        if (index < 0 || index >= ses.Count || ses[index] == null) return;
        seSource.PlayOneShot(ses[index]);
    }
}