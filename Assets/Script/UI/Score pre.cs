using UnityEngine;
using TMPro; // ★ TextMeshPro を使うために必要;

public class Scorepre : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameoverText;
    private CountLine countLineInstance; // CountLineスクリプトのインスタンスを保持する変数
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 1. シーン内から CountLine スクリプトがアタッチされているオブジェクトを探す
        countLineInstance = Object.FindFirstObjectByType<CountLine>();

        if(countLineInstance == null)
        {
                       Debug.LogError("CountLine instance not found in the scene.");
        }
        if(scoreText == null)
        {
            Debug.LogError("Score Text is not assigned in the inspector.");
            enabled = false; // スクリプトを無効化してエラーを防ぐ
            return;
        }
        UpdateScoreDisplay(); // 初期スコアの表示を更新
    }

    // Update is called once per frame
    void Update()
    {
        if(countLineInstance != null)
        {
            UpdateScoreDisplay(); // スコアの更新を毎フレーム行う

        }
    }

    void UpdateScoreDisplay()
    {

        if(scoreText != null && countLineInstance != null)
        {
            int currentScore = countLineInstance.CrossCount;
            scoreText.text = "Score: " + currentScore;


        }



    }
}
