using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TurnSystemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private Button endTurnBtn;
    [SerializeField] private GameObject enemyTurnVisualGameObject;
    private TurnSystem turnSystem;
    private void Awake() 
    {
        TurnSystem turnSystem = GetComponent<TurnSystem>();    
    }
    
    void Start()
    {
        endTurnBtn.onClick.AddListener(() => 
        {
            TurnSystem.Instance.NextTurn();
        });
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        UpdateTurnText();
        UpdateEnemyTurnVisual();
        UpdateEndTurnButtonVisibility();
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        UpdateEndTurnButtonVisibility();
        UpdateEnemyTurnVisual();
        UpdateTurnText();
    }

    private void UpdateTurnText()
    {
        textMeshPro.text = "Current Turn: " + TurnSystem.Instance.GetTurnNumber();
    }

    private void UpdateEnemyTurnVisual()
    {
        enemyTurnVisualGameObject.SetActive(!TurnSystem.Instance.IsPlayerTurn());
    }
    private void UpdateEndTurnButtonVisibility()
    {
        endTurnBtn.gameObject.SetActive(TurnSystem.Instance.IsPlayerTurn());
    }

}
