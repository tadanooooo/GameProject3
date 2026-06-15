using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Slider (無いシーンでは空っぽでOK)")]
    public Slider bgmSlider;
    public Slider seSlider;

    [Header("AudioSource")]
    public AudioSource bgmSource;
    public AudioSource seSource;

    [Tooltip("吸い込みループ用のAudioSource（Loopにチェックを入れたものをインスペクターで割り当ててください）")]
    public AudioSource suctionLoopSource;

    [Header("UI (無いシーンでは空っぽでOK)")]
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
        float savedBGM = PlayerPrefs.GetFloat("SavedBGMVolume", 1.0f);
        float savedSE = PlayerPrefs.GetFloat("SavedSEVolume", 1.0f);

        bgmSource.volume = savedBGM;
        seSource.volume = savedSE;

        if (suctionLoopSource != null) suctionLoopSource.volume = savedSE;

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

        if (audioUI != null)
        {
            audioUI.SetActive(false);
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat("SavedBGMVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSEVolume(float volume)
    {
        seSource.volume = volume;
        if (suctionLoopSource != null) suctionLoopSource.volume = volume;
        PlayerPrefs.SetFloat("SavedSEVolume", volume);
        PlayerPrefs.Save();
    }

    public void ToggleAudioUI()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(0);
        }
        if (audioUI == null) return;
        isOpen = !isOpen;
        audioUI.SetActive(isOpen);
    }

    public void PlayBGM(int index)
    {
        if (index < 0 || index >= bgms.Count || bgms[index] == null) return;
        bgmSource.clip = bgms[index];
        bgmSource.Play();
    }

    public void PlaySE(int index)
    {
        if (index < 0 || index >= ses.Count || ses[index] == null) return;
        seSource.PlayOneShot(ses[index]);
    }

    public void StartSuctionSE(int index)
    {
        if (suctionLoopSource == null) return;
        if (index < 0 || index >= ses.Count || ses[index] == null) return;

        // すでに同じ音が鳴っている場合は無視する（ダブり防止）
        if (suctionLoopSource.isPlaying && suctionLoopSource.clip == ses[index]) return;

        suctionLoopSource.clip = ses[index];
        suctionLoopSource.loop = true; // 強制的にループON
        suctionLoopSource.Play();
    }

    public void StopSuctionAndPlayEndSE(int endSEIndex)
    {
        if (suctionLoopSource == null) return;

        // 吸い込みループ音をストップする
        if (suctionLoopSource.isPlaying)
        {
            suctionLoopSource.Stop();
        }

        // 吸い込んだ後のSEを通常スピーカーで1回鳴らす
        PlaySE(endSEIndex);
    }
}