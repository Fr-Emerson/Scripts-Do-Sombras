using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TimedSpawn
{
    public GameObject targetObject;
    public int appearHour;
    public int disappearHour;
    public bool keepColliders = true; // Se true, mantém colisores ativos quando o objeto é desativado
    [HideInInspector] public bool isActive;

    // Cache dos componentes para performance
    [HideInInspector] public Renderer[] renderers;
    [HideInInspector] public Collider[] colliders;
    [HideInInspector] public bool componentsInitialized = false;

    // Método para inicializar componentes
    public void InitializeComponents()
    {
        if (targetObject == null || componentsInitialized) return;

        renderers = targetObject.GetComponentsInChildren<Renderer>();
        colliders = targetObject.GetComponentsInChildren<Collider>();
        componentsInitialized = true;
    }

    // Método para debug
    public override string ToString()
    {
        return $"{targetObject?.name}: {appearHour}h-{disappearHour}h (Active: {isActive})";
    }
}

public class SpawnTimeManager : MonoBehaviour
{
    [Header("Configuration")]
    public TimedSpawn[] spawns;
    public bool debugMode = true; // Para debug

    [Header("Performance")]
    [Tooltip("Intervalo em segundos entre verificações (0 = toda frame)")]
    public float checkInterval = 0f;

    private int lastCheckedHour = -1; // Para otimização
    private float timeSinceLastCheck = 0f;

    void Start()
    {
        // Inicializar componentes de todos os spawns
        foreach (var spawn in spawns)
        {
            spawn.InitializeComponents();
        }
    }

    void Update()
    {
        // Verificar intervalo de tempo
        if (checkInterval > 0)
        {
            timeSinceLastCheck += Time.deltaTime;
            if (timeSinceLastCheck < checkInterval) return;
            timeSinceLastCheck = 0f;
        }

        // Verificar se DayNightSystem está disponível
        if (DayNightSystem.Instance == null)
        {
            if (debugMode) Debug.LogWarning("DayNightSystem.Instance is null");
            return;
        }

        int currentHour = DayNightSystem.Instance.GetCurrentHour();

        // Otimização: só executa se a hora mudou
        if (currentHour == lastCheckedHour && checkInterval == 0) return;
        lastCheckedHour = currentHour;

        if (debugMode)
        {
            Debug.Log($"Checking spawns for hour: {currentHour}");
        }

        foreach (var spawn in spawns)
        {
            ProcessSpawn(spawn, currentHour);
        }
    }

    private void ProcessSpawn(TimedSpawn spawn, int currentHour)
    {
        // Validação de segurança
        if (spawn.targetObject == null)
        {
            if (debugMode) Debug.LogWarning($"Target object is null in spawn configuration");
            return;
        }

        bool shouldBeActive = IsHourInRange(currentHour, spawn.appearHour, spawn.disappearHour);

        if (debugMode)
        {
            Debug.Log($"{spawn.targetObject.name}: Should be active = {shouldBeActive}, Is active = {spawn.isActive}");
        }

        // Ativar objeto
        if (shouldBeActive && !spawn.isActive)
        {
            ActivateSpawn(spawn);
        }
        // Desativar objeto
        else if (!shouldBeActive && spawn.isActive)
        {
            DeactivateSpawn(spawn);
        }
    }

    private void ActivateSpawn(TimedSpawn spawn)
    {
        if (spawn.keepColliders)
        {
            // Ativar apenas renderers, colliders já estão ativos
            SetRenderersEnabled(spawn, true);
        }
        else
        {
            // Ativar o GameObject inteiro
            spawn.targetObject.SetActive(true);
        }

        spawn.isActive = true;

        if (debugMode) Debug.Log($"✓ Activated: {spawn.targetObject.name}");
    }

    private void DeactivateSpawn(TimedSpawn spawn)
    {
        if (spawn.keepColliders)
        {
            // Desativar apenas renderers, manter colliders
            SetRenderersEnabled(spawn, false);
        }
        else
        {
            // Desativar o GameObject inteiro
            spawn.targetObject.SetActive(false);
        }

        spawn.isActive = false;

        if (debugMode) Debug.Log($"✗ Deactivated: {spawn.targetObject.name}");
    }

    private void SetRenderersEnabled(TimedSpawn spawn, bool enabled)
    {
        if (!spawn.componentsInitialized)
        {
            spawn.InitializeComponents();
        }

        if (spawn.renderers != null)
        {
            foreach (var renderer in spawn.renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = enabled;
                }
            }
        }
    }

    bool IsHourInRange(int currentHour, int appearHour, int disappearHour)
    {
        // Normalizar horas para 0-23
        currentHour = Mathf.Clamp(currentHour, 0, 23);
        appearHour = Mathf.Clamp(appearHour, 0, 23);
        disappearHour = Mathf.Clamp(disappearHour, 0, 23);

        // Caso especial: aparecer e desaparecer na mesma hora
        if (appearHour == disappearHour)
        {
            return false; // Nunca ativo
        }

        // Caso normal: intervalo não cruza meia-noite (ex: 8h às 17h)
        if (appearHour < disappearHour)
        {
            return currentHour >= appearHour && currentHour < disappearHour;
        }
        // Caso cruza meia-noite: ex. 22h às 6h
        else
        {
            return currentHour >= appearHour || currentHour < disappearHour;
        }
    }

    // Método para forçar verificação manual (útil para debug)
    [ContextMenu("Force Check")]
    public void ForceCheck()
    {
        lastCheckedHour = -1; // Reset para forçar verificação
        timeSinceLastCheck = checkInterval; // Força verificação imediata
    }

    // Método para resetar todos os spawns
    [ContextMenu("Reset All Spawns")]
    public void ResetAllSpawns()
    {
        foreach (var spawn in spawns)
        {
            if (spawn.targetObject != null)
            {
                if (spawn.keepColliders)
                {
                    SetRenderersEnabled(spawn, false);
                }
                else
                {
                    spawn.targetObject.SetActive(false);
                }
                spawn.isActive = false;
            }
        }

        if (debugMode) Debug.Log("All spawns have been reset");
    }

    // Método para reinicializar componentes
    [ContextMenu("Reinitialize Components")]
    public void ReinitializeComponents()
    {
        foreach (var spawn in spawns)
        {
            spawn.componentsInitialized = false;
            spawn.InitializeComponents();
        }

        if (debugMode) Debug.Log("Components reinitialized for all spawns");
    }

    // Método para debug - mostrar estado atual
    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        if (DayNightSystem.Instance != null)
        {
            int currentHour = DayNightSystem.Instance.GetCurrentHour();
            Debug.Log($"=== Current Hour: {currentHour} ===");

            foreach (var spawn in spawns)
            {
                if (spawn.targetObject != null)
                {
                    bool shouldBeActive = IsHourInRange(currentHour, spawn.appearHour, spawn.disappearHour);
                    Debug.Log($"{spawn.targetObject.name}: " +
                             $"Range({spawn.appearHour}-{spawn.disappearHour}) " +
                             $"Should:{shouldBeActive} Is:{spawn.isActive} " +
                             $"KeepColliders:{spawn.keepColliders}");
                }
            }
        }
    }
}