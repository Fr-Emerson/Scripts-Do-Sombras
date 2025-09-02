using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionManager : MonoBehaviour 
{
    [Header("UI Configuration")]
    public GameObject interaction_Info_UI;
    
    [Header("Custom Cursor")]
    [Tooltip("GameObject que será usado como cursor personalizado")]
    public GameObject customCursor;
    
    [Tooltip("Esconder o cursor padrão do sistema")]
    public bool hideSystemCursor = true;
    
    [Tooltip("Offset do cursor personalizado")]
    public Vector2 cursorOffset = Vector2.zero;
    
    [Header("UI Positioning")]
    [Tooltip("Offset do UI em relação ao cursor")]
    public Vector2 uiOffset = new Vector2(10, 30);
    
    [Tooltip("Manter UI dentro dos limites da tela")]
    public bool keepUIOnScreen = true;
    
    [Tooltip("Suavizar movimento do UI")]
    public bool smoothMovement = false;
    
    [Tooltip("Velocidade da suavização (apenas se smoothMovement estiver ativo)")]
    public float smoothSpeed = 10f;
    
    private TextMeshProUGUI interaction_text;
    private RectTransform uiRectTransform;
    private RectTransform customCursorRectTransform;
    
    void Start()
    {
        // Configuração do cursor personalizado
        if (hideSystemCursor)
        {
            Cursor.visible = false;
        }
        
        if (customCursor != null)
        {
            customCursorRectTransform = customCursor.GetComponent<RectTransform>();
            if (customCursorRectTransform == null)
            {
                Debug.LogError("Custom Cursor deve ter um componente RectTransform!");
            }
        }
        
        if (interaction_Info_UI == null)
        {
            Debug.LogError("Interaction_Info_UI não está atribuído no Inspector!");
            return;
        }

        // Busca automaticamente o TMP mesmo se for filho e mesmo que esteja desativado
        interaction_text = interaction_Info_UI.GetComponentInChildren<TextMeshProUGUI>(true);
        
        if (interaction_text == null)
        {
            Debug.LogError("Nenhum componente TextMeshProUGUI encontrado dentro de Interaction_Info_UI!");
            return;
        }
        
        // Pega o RectTransform para controle de posição
        uiRectTransform = interaction_Info_UI.GetComponent<RectTransform>();
        
        if (uiRectTransform == null)
        {
            Debug.LogError("Interaction_Info_UI deve ter um componente RectTransform!");
            return;
        }

        interaction_Info_UI.SetActive(false);
    }

    void Update()
    {
        // Atualiza posição do cursor personalizado
        UpdateCustomCursor();
        
        if (interaction_text == null || uiRectTransform == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var interactable = hit.transform.GetComponentInParent<InteractableObject>();

            if (interactable != null)
            {
                // Atualiza o texto
                interaction_text.text = interactable.GetItemName();
                
                // Calcula a posição do UI
                Vector3 targetPosition = CalculateUIPosition();
                
                // Move o UI
                if (smoothMovement)
                {
                    interaction_Info_UI.transform.position = Vector3.Lerp(
                        interaction_Info_UI.transform.position, 
                        targetPosition, 
                        Time.deltaTime * smoothSpeed
                    );
                }
                else
                {
                    interaction_Info_UI.transform.position = targetPosition;
                }
                
                // Ativa o UI se não estiver ativo
                if (!interaction_Info_UI.activeSelf)
                {
                    interaction_Info_UI.SetActive(true);
                }
            }
            else
            {
                interaction_Info_UI.SetActive(false);
            }
        }
        else
        {
            interaction_Info_UI.SetActive(false);
        }
    }
    
    private void UpdateCustomCursor()
    {
        if (customCursor != null && customCursorRectTransform != null)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.x += cursorOffset.x;
            mousePos.y += cursorOffset.y;
            
            customCursor.transform.position = mousePos;
            
            // Garante que o cursor personalizado esteja sempre ativo
            if (!customCursor.activeSelf)
            {
                customCursor.SetActive(true);
            }
        }
    }
    
    private Vector3 CalculateUIPosition()
    {
        // Posição base do mouse com offset
        Vector3 mousePos = Input.mousePosition;
        mousePos.x += uiOffset.x;
        mousePos.y += uiOffset.y;
        
        // Se deve manter na tela, aplica os limites
        if (keepUIOnScreen)
        {
            // Pega as dimensões do UI
            Vector2 uiSize = uiRectTransform.sizeDelta;
            
            // Calcula os limites da tela
            float minX = uiSize.x * 0.5f;
            float maxX = Screen.width - (uiSize.x * 0.5f);
            float minY = uiSize.y * 0.5f;
            float maxY = Screen.height - (uiSize.y * 0.5f);
            
            // Aplica os limites
            mousePos.x = Mathf.Clamp(mousePos.x, minX, maxX);
            mousePos.y = Mathf.Clamp(mousePos.y, minY, maxY);
        }
        
        return mousePos;
    }
    
    void OnDestroy()
    {
        // Restaura o cursor quando o objeto for destruído
        if (hideSystemCursor)
        {
            Cursor.visible = true;
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        // Gerencia visibilidade do cursor quando a aplicação ganha/perde foco
        if (hideSystemCursor)
        {
            Cursor.visible = !hasFocus;
        }
    }
}