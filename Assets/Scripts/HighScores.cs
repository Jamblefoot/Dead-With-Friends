using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//NEEDED FOR SAVING
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class HighScores : MonoBehaviour
{
    public static HighScores instance;

    public struct Score
    {
        public string name;
        public float time;
    }

    public const int COUNT = 10;
    Score[] scores = new Score[COUNT];

    [SerializeField] Text scoreChart;
    [SerializeField] string fileName = "/HighScores.dat";

    // Start is called before the first frame update
    void Start()
    {
        if (HighScores.instance == null)
        {
            instance = this;
        }
        else DestroyImmediate(this);

        LoadData();
        PopulateScoreChart();
    }

    void SaveData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + fileName);
        ScoreData data = new ScoreData();
        data.names = new string[COUNT];
        data.times = new float[COUNT];
        for(int i = 0; i < COUNT; i++)
        {
            data.names[i] = scores[i].name;
            data.times[i] = scores[i].time;
        }
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Game Data Saved!");
    }

    void LoadData()
    {
        if(File.Exists(Application.persistentDataPath + fileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + fileName, FileMode.Open);
            ScoreData data = (ScoreData)bf.Deserialize(file);
            file.Close();
            for(int i = 0; i < COUNT; i++)
            {
                scores[i].name = data.names[i];
                scores[i].time = data.times[i];
            }
            Debug.Log("Game data loaded!");

            
        }
        else 
        {
            Debug.LogError("There is no save data!");
            scores[0].name = "MARK";
            scores[0].time = 219.7135f;
        }

        
    }

    public void ResetData()
    {
        if(File.Exists(Application.persistentDataPath + fileName))
        {
            File.Delete(Application.persistentDataPath + fileName);
            //Score[] scores = new Score[COUNT];
            scores[0].name = "MARK";
            scores[0].time = 219.7135f;
            for(int i = 1; i < COUNT; i++)
            {
                scores[i].name = "";
                scores[i].time = 0f;
            }
            Debug.Log("Data reset complete!");
        }
        else Debug.Log("No data to delete!");

        PopulateScoreChart();
    }

    void PopulateScoreChart()
    {
        if (scoreChart != null)
        {
            scoreChart.text = "";
            for (int i = 0; i < COUNT; i++)
            {
                scoreChart.text += (i+1).ToString() + ". " + scores[i].name + ": " + scores[i].time.ToString() + "\n";
            }
        }
    }

    public int CheckScorePlace(float n)
    {
        int pos = COUNT + 1;
        for (int i = 0; i < COUNT; i++)
        {
            if (n < scores[i].time || scores[i].time <= 0)
            {
                pos = i;
                break;
            }
        }
        return pos;
    }
    public bool AddNewScore(float n, string name)
    {
        int pos = CheckScorePlace(n);
        if(pos >= COUNT || pos < 0) 
            return false;

        for(int i = COUNT - 1; i > pos; i--)
        {
            scores[i].name = scores[i-1].name;
            scores[i].time = scores[i-1].time;
        }
        scores[pos].name = name;
        scores[pos].time = n;

        SaveData();
        PopulateScoreChart();

        return true;

    }

    public bool SaveNewScore()
    {
        int pos = CheckScorePlace(GameControl.instance.gameTime);
        if (pos >= COUNT || pos < 0)
            return false;

        for (int i = COUNT - 1; i > pos; i--)
        {
            scores[i].name = scores[i - 1].name;
            scores[i].time = scores[i - 1].time;
        }
        scores[pos].name = GameControl.instance.bestTimeName;
        scores[pos].time = GameControl.instance.gameTime;

        SaveData();
        PopulateScoreChart();

        return true;

    }
}

[Serializable]
class ScoreData
{
    public string[] names;
    public float[] times;
}
