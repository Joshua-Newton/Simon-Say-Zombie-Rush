using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemTimer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI timerLabel;

    GameObject item;
    float remainingTime;
    bool expired;
    int timerIndex;

    private void Start()
    {

    }

    private void Update()
    {
        if (!GameManager.instance.isPaused && !expired)
        {
            remainingTime -= Time.deltaTime;
            UpdateTime();
        }
    }

    public TextMeshProUGUI GetTimerText() { return timerText; }
    public TextMeshProUGUI GetTimerLabel() { return timerLabel; }
    
    public void SetItem(GameObject item)
    {
        this.item = item;
        timerLabel.text = this.item.name;
    }

    public void SetRemainingTime(float remainingTime) { this.remainingTime = remainingTime; }
    public float GetRemainingTime() { return remainingTime;}

    public void InitializeTimer(int index)
    {
        timerIndex = index;
        timerLabel.text = item.name;
        UpdateTime();
    }

    private void UpdateTime()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = $"{minutes:0}:{seconds:00}";
        if (remainingTime <= 0)
        {
            gameObject.SetActive(false);
            item.SetActive(false);
            expired = true;
            TimeTrialModeManager.instance.HandleExpiredItem(item, timerIndex);
        }
    }


}
