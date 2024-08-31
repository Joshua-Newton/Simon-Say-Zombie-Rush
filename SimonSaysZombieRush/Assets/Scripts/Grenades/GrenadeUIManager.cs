using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GrenadeUIManager : MonoBehaviour
{
    [SerializeField] private Image grenadeIcon; // Icono de la granada
    [SerializeField] private TMP_Text grenadeCountText; // Texto para mostrar el número de granadas
    private int currentGrenades;
    private int maxGrenades;
    private float cooldownTime;

    public void Initialize(int maxGrenades, float cooldown)
    {
        this.maxGrenades = maxGrenades;
        currentGrenades = maxGrenades;
        cooldownTime = cooldown;
        UpdateUI();
    }

    public void UseGrenade()
    {
        if (currentGrenades > 0)
        {
            currentGrenades--;
            UpdateUI();
            StartCoroutine(RechargeGrenade());
        }
    }

    private IEnumerator RechargeGrenade()
    {
        yield return new WaitForSeconds(cooldownTime);
        currentGrenades = Mathf.Min(currentGrenades + 1, maxGrenades);
        UpdateUI();
    }

    private void UpdateUI()
    {
        grenadeCountText.text = currentGrenades.ToString();
        if (maxGrenades > 0)
        {
            grenadeIcon.fillAmount = (float)currentGrenades / maxGrenades;
        }
        else
        {
            grenadeIcon.fillAmount = 0;
        }
    }
}
