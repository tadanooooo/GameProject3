using UnityEngine;

public static class StageSaveManager
{
    // 星の数を保存する（今までの最高記録を超えたときだけ保存する）
    public static void SaveStars(int stageNumber, int starCount)
    {
        string key = "Stage_" + stageNumber + "_Stars";
        int currentBest = PlayerPrefs.GetInt(key, 0);

        if (starCount > currentBest)
        {
            PlayerPrefs.SetInt(key, starCount);
            PlayerPrefs.Save(); // 確実に保存を確定させる
            Debug.Log($"ステージ {stageNumber} の星の数を更新: {starCount}");
        }
    }

    // 星の数を読み込む（まだクリアしていない時は 0 が返る）
    public static int LoadStars(int stageNumber)
    {
        string key = "Stage_" + stageNumber + "_Stars";
        return PlayerPrefs.GetInt(key, 0);
    }

    // ベストタイムを保存する（今までの記録より速いときだけ保存する）
    public static void SaveBestTime(int stageNumber, float newTime)
    {
        string key = "Stage_" + stageNumber + "_BestTime";
        // まだ記録がない（0秒）の時は、比較のために大きな数字（9999秒）をデフォルトにする
        float currentBest = PlayerPrefs.GetFloat(key, 9999f);

        if (newTime < currentBest)
        {
            PlayerPrefs.SetFloat(key, newTime);
            PlayerPrefs.Save();
            Debug.Log($"ステージ {stageNumber} のベストタイムを更新: {newTime:F2}秒");
        }
    }

    // ベストタイムを読み込む（まだ記録がない時は -1 が返る）
    public static float LoadBestTime(int stageNumber)
    {
        string key = "Stage_" + stageNumber + "_BestTime";
        return PlayerPrefs.GetFloat(key, -1f);
    }
}