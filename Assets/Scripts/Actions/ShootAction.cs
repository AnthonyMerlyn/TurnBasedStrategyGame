using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAction : BaseAction
{
    private int maxShootDistance = 7;
    private enum State
    {
        Aiming,
        Shooting,
        Cooloff,
    }
    private Unit targetUnit;
    private State state;
    private float stateTimer;
    private bool canShootBullet;
    public event EventHandler<OnShootEventArgs> OnShoot;

    public class OnShootEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit shootingUnit;
    }
    private void Update() 
    {
        if (!isActive)
        {
            return;
        }
        stateTimer -= Time.deltaTime;
        switch(state)
        {
            case State.Aiming:
            float rotateSpeed = 10f;
            Vector3 aimDir = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
            transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * rotateSpeed);
            break;
            case State.Shooting:
               if(canShootBullet)
               {
                    Shoot();
                    canShootBullet = false;
               }
            break;
            case State.Cooloff:
                
            break;
        }

        if(stateTimer <= 0f)
        {
            NextState();
        }

    }

    private void Shoot()
    {
        OnShoot?.Invoke(this,new OnShootEventArgs {
            targetUnit = targetUnit,
            shootingUnit = unit
        });
        targetUnit.Damage(40);
    }

    private void NextState()
    {
        switch(state)
        {
            case State.Aiming:
                if(stateTimer <= 0f)
                {
                    
                    state = State.Shooting;
                    float shootingStateTime = 0.1f;
                    stateTimer = shootingStateTime;
                }
            break;
            case State.Shooting:
                if(stateTimer <= 0f)
                {
                    state = State.Cooloff;
                    float coolOffStateTime = 0.5f;
                    stateTimer = coolOffStateTime;
                }
            break;
            case State.Cooloff:
                ActionComplete();
            break;
        }
    }

    public override string GetActionName()
    {
        return "Shoot";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {

        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -maxShootDistance; x <= maxShootDistance; x++)
        {
            for (int z = -maxShootDistance; z <= maxShootDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x,z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }
                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if(testDistance > maxShootDistance)
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
        ActionStart(onActionComplete);

        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        
        state = State.Aiming;
        float animingStateTime = 1f;
        stateTimer = animingStateTime;

        
        canShootBullet = true;

    }
}
