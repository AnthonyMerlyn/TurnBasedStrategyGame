using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAction : BaseAction
{   
    private enum State
    {
        SwiningSwordBeforHit,
        SwingingSwordAfterHit,
        
    }
    private int maxSwordDistance = 1;
    private State state;
    private float stateTimer;
    private Unit targetUnit;
    public event EventHandler OnSwordActionStarted;
    public event EventHandler OnSwordActionComplete;
    public static event EventHandler OnAnySwordHit;

    private void Update() 
    {
        if( !isActive)
        {
            return;
        }

        stateTimer -= Time.deltaTime;
        switch(state)
        {
            case State.SwiningSwordBeforHit:
            float rotateSpeed = 10f;
            Vector3 aimDir = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
            transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * rotateSpeed);
            break;
            case State.SwingingSwordAfterHit:
              
            break;
        }

        if(stateTimer <= 0f)
        {
            NextState();
        }

    }

    private void NextState()
    {
        switch(state)
        {
            case State.SwiningSwordBeforHit:
                state = State.SwingingSwordAfterHit;
                float afterHitStateTime = 0.5f;
                stateTimer = afterHitStateTime;
                targetUnit.Damage(100);
                OnAnySwordHit?.Invoke(this,EventArgs.Empty);
            break;
            case State.SwingingSwordAfterHit:
                OnSwordActionComplete?.Invoke(this,EventArgs.Empty);
                ActionComplete();
            break;
        }
    }

    public override string GetActionName()
    {
        return "Sword";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction{
            gridPosition = gridPosition,
            actionValue = 200,
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -maxSwordDistance; x <= maxSwordDistance; x++)
        {
            for (int z = -maxSwordDistance; z <= maxSwordDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x,z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                if(!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    continue;
                }
                Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
                if(targetUnit.IsEnemy() == unit.IsEnemy()) //Both Units on the same Team
                {
                    continue;
                }

                validGridPositionList.Add(testGridPosition);
            }
        }
        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);

        state = State.SwiningSwordBeforHit;
        float bevorHitStateTime = 0.7f;
        stateTimer = bevorHitStateTime;
        OnSwordActionStarted?.Invoke(this, EventArgs.Empty);
        ActionStart(onActionComplete);
    }
    public int GetMaxSwordDistance()
    {
        return maxSwordDistance;
    }
}
