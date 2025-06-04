using UnityEngine;
using TMPro; // �� TextMeshPro ���g�����߂ɕK�v;

public class Scorepre : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameoverText;
    private CountLine countLineInstance; // CountLine�X�N���v�g�̃C���X�^���X��ێ�����ϐ�
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 1. �V�[�������� CountLine �X�N���v�g���A�^�b�`����Ă���I�u�W�F�N�g��T��
        countLineInstance = Object.FindFirstObjectByType<CountLine>();

        if(countLineInstance == null)
        {
                       Debug.LogError("CountLine instance not found in the scene.");
        }
        if(scoreText == null)
        {
            Debug.LogError("Score Text is not assigned in the inspector.");
            enabled = false; // �X�N���v�g�𖳌������ăG���[��h��
            return;
        }
        UpdateScoreDisplay(); // �����X�R�A�̕\�����X�V
    }

    // Update is called once per frame
    void Update()
    {
        if(countLineInstance != null)
        {
            UpdateScoreDisplay(); // �X�R�A�̍X�V�𖈃t���[���s��

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
