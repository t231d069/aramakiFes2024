using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNewData : MonoBehaviour
{
    private SaveData saveData = new SaveData();

    void Start()
    {
        // Debug用データ
        CreateDebugSaveData();
    }

    public SaveData GetSaveData()
    {
        return saveData;
    }

    // デバッグ用のデータの作成
    private void CreateDebugSaveData()
    {
        ScoreData scoreData1 = new ScoreData(1000, "Test1");
        ScoreData scoreData2 = new ScoreData(2000, "Test2");
        ScoreData scoreData3 = new ScoreData(4000, "Test3");
        ScoreData scoreData4 = new ScoreData(3000, "Test4");
        List<ScoreData> debugScoreDataList = new List<ScoreData>();
        debugScoreDataList.Add(scoreData1);
        debugScoreDataList.Add(scoreData2);
        debugScoreDataList.Add(scoreData3);
        debugScoreDataList.Add(scoreData4);
        saveData.SetScoreDataList(debugScoreDataList);
    }
}
