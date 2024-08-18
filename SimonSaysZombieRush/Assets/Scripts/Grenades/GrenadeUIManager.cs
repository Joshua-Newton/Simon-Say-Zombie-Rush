using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GrenadeUIManager : MonoBehaviour
{
    [SerializeField] private Image grenadeIcon; // Icono de la granada
    [SerializeField] private TMP_Text grenadeCountText; // Texto para mostrar el número de granadas
    private int currentGrenades;
    private float cooldownTime;

    public void Initialize(int maxGrenades, float cooldown)
    {
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
        currentGrenades++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        grenadeCountText.text = currentGrenades.ToString();
        grenadeIcon.fillAmount = (float)currentGrenades / 2; // Si tienes un máximo de 2 granadas
    }
}