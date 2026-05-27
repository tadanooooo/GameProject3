using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    // スライダー
    public Slider BGMVolumeSlider;
    public Slider SEVolumeSlider;
    public GameObject AudioUI;

    // オーディオソース
    public GameObject BGMaudioSource;
    public GameObject SEaudioSource;

    // BGM・SEリスト
    public List<AudioClip> BGMs;
    public List<AudioClip> SEs;

    // 音量管理システムが開かれているかどうか
    public bool Opened_Audio_Setting = false;

    void Start()
    {
        // シーン内からSliderを探して取得
        //BGMvolumeSlider = GameObject.Find("BGMVolumeSlider").GetComponent<Slider>();
        //SEvolumeSlider = GameObject.Find("SEVolumeSlider").GetComponent<Slider>();
        //AudioUI = GameObject.Find("AudioUI").GetComponent<Canvas>();

        // 保存された音量を反映
        BGMVolumeSlider.value = AudioUpdate.BGMsliderValue;
        SEVolumeSlider.value = AudioUpdate.SEsliderValue;

        AudioUI.SetActive(false);
        //gameObject.GetComponent<GraphicRaycaster>().enabled = false;
    }

    void Update()
    {
        // スライダーの値を取得
        AudioUpdate.BGMsliderValue = BGMVolumeSlider.value;
        AudioUpdate.SEsliderValue = SEVolumeSlider.value;
        // オーディオの音量を設定
        BGMaudioSource.GetComponent<AudioSource>().volume = AudioUpdate.BGMsliderValue;
        SEaudioSource.GetComponent<AudioSource>().volume = AudioUpdate.SEsliderValue;

        // スライダーの値が変更された時の処理を登録
        BGMVolumeSlider.onValueChanged.AddListener(ChangeVolumeBGM);
        SEVolumeSlider.onValueChanged.AddListener(ChangeVolumeSE);
    }

    void ChangeVolumeBGM(float newVolume)
    {
        // スライダーの値によって音量を変更
        BGMaudioSource.GetComponent<AudioSource>().volume = newVolume;
    }
    void ChangeVolumeSE(float newVolume)
    {
        // スライダーの値によって音量を変更
        SEaudioSource.GetComponent<AudioSource>().volume = newVolume;
    }

    void PauseGame()
    {
        Time.timeScale = 0f; // ゲームの時間を停止
    }
    void ResumeGame()
    {
        Time.timeScale = 1f; // ゲームの時間を再開
    }

    public void ToggleAudioSetting()
    {
        Opened_Audio_Setting = !Opened_Audio_Setting;

        AudioUI.SetActive(Opened_Audio_Setting);
        //gameObject.GetComponent<GraphicRaycaster>().enabled = Opened_Audio_Setting;
    }

    //音量調節画面を開く
    //public void Open_Audio_Setting()
    //{
    //    if (!Opened_Audio_Setting)
    //    {
    //        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 8f, gameObject.transform.position.z);
    //        Debug.Log("a");
    //    }
    //}

    ////音量調節画面を閉じる
    //public void Close_Audio_Setting()
    //{
    //    if (Opened_Audio_Setting)
    //    {
    //        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 8f, gameObject.transform.position.z);
    //    }
    //}

    //SEを鳴らす
    //public void Play_SE(int seIndex)
    //{
    //    SEaudioSource.GetComponent<AudioSource>().clip = SEs[seIndex];
    //    SEaudioSource.GetComponent<AudioSource>().Play();
    //}

    ////BGMを鳴らす
    //public void Play_BGM(int bgmIndex)
    //{
    //    BGMaudioSource.GetComponent<AudioSource>().clip = BGMs[bgmIndex];
    //    BGMaudioSource.GetComponent<AudioSource>().Play();
    //}
}

