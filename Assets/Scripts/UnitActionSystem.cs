using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
public class UnitActionSystem : MonoBehaviour
{
    public static UnitActionSystem Instance { get; private set; }

    public event EventHandler OnSelectedUnitChanged;
    public event EventHandler OnSelcetedActionChanged;
    public event EventHandler<bool> OnBusyChanged;
    public event EventHandler OnActionStarted;

    [SerializeField] private Unit selectedUnit;
    [SerializeField] private LayerMask unitLayerMask;
    private BaseAction selectedAction;
    private bool isBusy;
    //private bool isAlive = true;
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There are more than ine UnitActionSystem!" + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }
    private void Start()
    {
        SetSelectedUnit(selectedUnit);
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        //Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;
        
    }
    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }

        if (selectedUnit != null)
        {
            return;
        }

        List<Unit> friendlyUnitList = UnitManager.Instance.GetFriendlyUnitList();
        if (friendlyUnitList.Count == 0)
        {
            // TODO: Update Unit Manager to check if friendlyUnitList.Count == 0
            //             and to implement GAME OVER scene if it is. Then remove this check. 
            Debug.Log("Your team is not responding! This can't be good.");
        }
        else
        {
            SetSelectedUnit(friendlyUnitList[0]);
        }
    }
    // private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    // {
    //     isAlive = false;
    // }

    private void Update()
    {
        if (isBusy) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (TryHandleUnitSelection()) return;
        if (!TurnSystem.Instance.IsPlayerTurn()) return;
        HandleSelectedAction();
    }

    private void HandleSelectedAction()
    {
        if (InputManager.Instance.IsMouseButtonDownThisFrame())
        {
            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());

            if (!selectedAction.IsValidActionGridPosition(mouseGridPosition)) return;
            if (!selectedUnit.TrySpendActionPointsToTakeAction(selectedAction)) return;

            SetBusy();
            selectedAction.TakeAction(mouseGridPosition, ClearBusy);
            OnActionStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool TryHandleUnitSelection()
    {
        if (InputManager.Instance.IsMouseButtonDownThisFrame())
        {
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
            {
                if (raycastHit.transform.TryGetComponent<Unit>(out Unit unit))
                {
                    if (unit == selectedUnit)
                    {
                        //Clicked on Same Unit
                        return false;
                    }
                    if (unit.IsEnemy())
                    {
                        //Clicked on Enemy unit
                        return false;
                    }
                    // if(!isAlive)
                    // {
                    //     return false;
                    // }
                    SetSelectedUnit(unit);
                    return true;
                }
            }
        }
        return false;
    }
    private void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
        SetSelectedAction(unit.GetAction<MoveAction>());
        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSelectedAction(BaseAction baseAction)
    {
        selectedAction = baseAction;

        OnSelcetedActionChanged?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }
    private void SetBusy()
    {
        isBusy = true;
        OnBusyChanged.Invoke(this, isBusy);
    }
    private void ClearBusy()
    {
        isBusy = false;
        OnBusyChanged.Invoke(this, isBusy);
    }
    public BaseAction GetSelectedAction()
    {
        return selectedAction;
    }
}
