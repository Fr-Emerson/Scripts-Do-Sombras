using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DayNightSystem : MonoBehaviour
{
    public static DayNightSystem Instance { get; private set; }

    [Header("Light Settings")]
    public Light directionalLight;

    [Header("Time Settings")]
    public float dayDurationInSeconds = 24.0f;

    [Header("UI Elements")]
    public TextMeshProUGUI timeUI;
    public TextMeshProUGUI dayUI;

    [Header("Skybox Settings")]
    public List<SkyBoxTimeMapping> timeMapping;

    // Private variables
    private int currentHour = 0;
    private int currentWeek = 1;
    private float currentTimeOfDay = 0.35f;
    private float blendValue = 0.0f;
    private bool lockNextDayTrigger = false;
    private int lastHour = -1; // Para detectar mudanças de hora

    // Public properties (read-only)
    public int CurrentHour => currentHour;
    public float CurrentTimeOfDay => currentTimeOfDay;
    public int CurrentWeek => currentWeek;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        UpdateTime();
        UpdateLighting();
        UpdateUI();
        UpdateSkybox();
        CheckDayTransition();
    }

    private void UpdateTime()
    {
        currentTimeOfDay += Time.deltaTime / dayDurationInSeconds;
        currentTimeOfDay %= 1.0f;

        int newHour = Mathf.FloorToInt(currentTimeOfDay * 24.0f);

        // Detectar mudança de hora para debug
        if (newHour != currentHour)
        {
            //Debug.Log($"Hour changed from {currentHour} to {newHour}");
            currentHour = newHour;
        }
    }

    private void UpdateLighting()
    {
        // Update the directional light rotation
        directionalLight.transform.rotation = Quaternion.Euler(
            new Vector3((currentTimeOfDay * 360) - 90, 170, 0)
        );
    }

    private void UpdateUI()
    {
        if (timeUI != null)
            timeUI.text = $"{currentHour:00}:00";

        if (dayUI != null && TimeManager.Instance != null)
            dayUI.text = $"Dia {TimeManager.Instance.getCurrentDay()}";
    }

    private void CheckDayTransition()
    {
        if (currentHour == 0 && !lockNextDayTrigger)
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.TriggerNextDay();
                Debug.Log("New day triggered!");
            }
            lockNextDayTrigger = true;
        }

        if (currentHour != 0)
        {
            lockNextDayTrigger = false;
        }
    }

    public int GetCurrentHour()
    {
        return currentHour; // Agora retorna o valor já calculado
    }

    public string GetCurrentPhaseName()
    {
        foreach (SkyBoxTimeMapping mapping in timeMapping)
        {
            if (mapping.hour == currentHour)
            {
                return mapping.PhaseName;
            }
        }
        return "Unknown Phase";
    }

    private void UpdateSkybox()
    {
        // Find the appropriate skybox material based on the current hour
        Material currentSkybox = null;

        foreach (SkyBoxTimeMapping mapping in timeMapping)
        {
            if (mapping.hour == currentHour)
            {
                currentSkybox = mapping.skyboxMaterial;

                if (currentSkybox != null && currentSkybox.shader != null)
                {
                    if (currentSkybox.shader.name == "Custom/SkyboxTransition")
                    {
                        blendValue += Time.deltaTime;
                        blendValue = Mathf.Clamp01(blendValue);
                        currentSkybox.SetFloat("_TransitionFactor", blendValue);
                    }
                    else
                    {
                        blendValue = 0.0f;
                    }
                }
                break;
            }
        }

        if (currentSkybox != null)
        {
            RenderSettings.skybox = currentSkybox;
        }
        else
        {
            //Debug.LogWarning($"No skybox material found for hour {currentHour}");
        }
    }

    // Métodos utilitários para debug
    [ContextMenu("Debug Current Time")]
    public void DebugCurrentTime()
    {
        Debug.Log($"Current Hour: {currentHour}, Time of Day: {currentTimeOfDay:F2}, Phase: {GetCurrentPhaseName()}");
    }

    [ContextMenu("Set Time to 6 AM")]
    public void SetTimeTo6AM()
    {
        currentTimeOfDay = 6.0f / 24.0f;
    }

    [ContextMenu("Set Time to 6 PM")]
    public void SetTimeTo6PM()
    {
        currentTimeOfDay = 18.0f / 24.0f;
    }

    public void SetTimeOfDay(float timeOfDay)
    {
        currentTimeOfDay = Mathf.Clamp01(timeOfDay);
    }

    public void SetHour(int hour)
    {
        hour = Mathf.Clamp(hour, 0, 23);
        currentTimeOfDay = hour / 24.0f;
    }
}

[System.Serializable]
public class SkyBoxTimeMapping
{
    public string PhaseName;
    public int hour;
    public Material skyboxMaterial;
}