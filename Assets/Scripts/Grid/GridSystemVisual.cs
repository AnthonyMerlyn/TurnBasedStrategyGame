using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystemVisual : MonoBehaviour
{
    
    public static GridSystemVisual Instance {get; private set;}
    [SerializeField] private Transform gridSystemVisualPrefab;

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
        UpdateGridVisual();
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
    public void ShowGridPositionList(List<GridPosition> gridPositionList)
    {
        foreach(GridPosition gridPosition in gridPositionList)
        {
            gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].Show();
        }
    }

    private void UpdateGridVisual()
    {
            HideAllGridPositions();
            Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();

            ShowGridPositionList(selectedUnit.GetMoveAction().GetValidActionGridPositionList());
    }

}
