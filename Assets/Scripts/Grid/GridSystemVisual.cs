using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystemVisual : MonoBehaviour
{

    public static GridSystemVisual Instance {get; private set;}
    [Serializable] public struct GridVisualTypeMaterial
    {
        public GridVisualType gridVisualType;
        public Material material;
    }

    public enum GridVisualType
    {
        White,
        Blue,
        Red,
        RedSoft,
        Yellow
    }

    [SerializeField] private Transform gridSystemVisualPrefab;
    [SerializeField] private List<GridVisualTypeMaterial> gridVisualTypeMaterialList;

    private GridSystemVisualSingle[,] gridSystemVisualSingleArray;

    private void Awake() 
    {
        if(Instance != null)
        {
            Debug.LogError("There are more than ine GridSystemVisual!" + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Update() 
    {

    }

    private void Start() 
    {
        gridSystemVisualSingleArray = new GridSystemVisualSingle[LevelGrid.Instance.GetWidth(),LevelGrid.Instance.GetHight()];
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for(int z = 0; z < LevelGrid.Instance.GetHight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x,z);
                Transform gridSystemVisualSingleTransform = Instantiate(gridSystemVisualPrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity);

                gridSystemVisualSingleArray[x,z] = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>();
            }
        }

        UnitActionSystem.Instance.OnSelcetedActionChanged += UnitActionSystem_OnSelcetedActionChanged;
        LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMovedGridPosition;

        UpdateGridVisual();
    }

    private void LevelGrid_OnAnyUnitMovedGridPosition(object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    private void UnitActionSystem_OnSelcetedActionChanged(object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    public void HideAllGridPositions()
    {
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for(int z = 0; z < LevelGrid.Instance.GetHight(); z++)
            {
                gridSystemVisualSingleArray[x,z].Hide();
            }
        }
    }
    public void ShowGridPositionList(List<GridPosition> gridPositionList, GridVisualType gridVisualType)
    {
        foreach(GridPosition gridPosition in gridPositionList)
        {
            gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].Show(GetGridVisualTypeMaterial(gridVisualType));
        }
    }

    private void ShowGridPositionRange(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();
        for (int x = -range; x <= range; x++)
        {
            for(int z =  -range; z <= range; z++)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x,z);

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if(testDistance > range)
                {
                    continue;
                }
                gridPositionList.Add(testGridPosition);

            }
        }
        ShowGridPositionList(gridPositionList,gridVisualType);
    }

    private void UpdateGridVisual()
    {
            HideAllGridPositions();

            Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
            BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();

            GridVisualType gridVisualType;
            switch(selectedAction)
            {
                case MoveAction moveAction:
                gridVisualType = GridVisualType.White;
                break;
                case ShootAction shootAction:
                gridVisualType = GridVisualType.Red;

                ShowGridPositionRange(selectedUnit.GetGridPosition(), shootAction.GetMaxShootDistance(),GridVisualType.RedSoft);
                break;
                case SpinAction spinAction:
                gridVisualType = GridVisualType.Blue;
                break;
                default:
                gridVisualType = GridVisualType.White;
                break;

            }
            ShowGridPositionList(
                selectedAction.GetValidActionGridPositionList(), gridVisualType);
    }

    private Material GetGridVisualTypeMaterial(GridVisualType gridVisualType)
    {
        foreach (GridVisualTypeMaterial gridVisualTypeMaterial in gridVisualTypeMaterialList)
        {
            if(gridVisualTypeMaterial.gridVisualType == gridVisualType)
            {
                return gridVisualTypeMaterial.material;
            }
        }
        Debug.LogError("Could not find GridVisualTypeMaterial for GridVisualType " + gridVisualType);
        return null;
    }

}
