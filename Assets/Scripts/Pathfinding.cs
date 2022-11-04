using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public static Pathfinding Instance { get; private set; }
    private const int MOVE_STRAIGT_COST = 10;
    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private LayerMask obstacleLayerMask;
    private int width;
    private int height;
    private float cellSize;
    private GridSystemHex<PathNode> gridSystem;

    private void Awake() 
    {
        if (Instance != null)
        {
            Debug.LogError("There are more than ine Pathfinding!" + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;


    }

    public void Setup(int width, int height, float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridSystem = new GridSystemHex<PathNode>(width,height,cellSize, 
                (GridSystemHex<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition));

        //gridSystem.CreateDegubObjects(gridDebugObjectPrefab);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x,z);
                Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
                float raycastOffsetDistance = 5f;
                if(Physics.Raycast(
                    worldPosition + Vector3.down * raycastOffsetDistance,
                    Vector3.up,
                    raycastOffsetDistance * 2,
                    obstacleLayerMask))
                    {
                        GetNode(x,z).SetIsWalkable(false);
                    }
            }
        }
    }


    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength)
    {
        List<PathNode> openList = new List<PathNode>(); // A List for all Nodes that needs to be searched
        List<PathNode> closeList = new List<PathNode>(); // A List for all Nodes that has been searched

        PathNode startNode = gridSystem.GetGridObject(startGridPosition);
        PathNode endNode = gridSystem.GetGridObject(endGridPosition);
        openList.Add(startNode);

        for (int x = 0; x < gridSystem.GetWidth(); x++)
        {
            for (int z = 0; z < gridSystem.GetHeight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x,z);
                PathNode pathNode = gridSystem.GetGridObject(gridPosition);

                pathNode.SetGCost(int.MaxValue);
                pathNode.SetHCost(0);
                pathNode.CalculateFCost();
                pathNode.ResetCameFromPathNode();

            }
        }

        startNode.SetGCost(0);
        startNode.SetHCost(CalculateHeuristicDistance(startGridPosition, endGridPosition));
        startNode.CalculateFCost();

        while(openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostPathNode(openList);

            if(currentNode == endNode)
            {
                //Reached final Node
                pathLength = endNode.GetFCost();
                return CalculatePath(endNode);
            }
            openList.Remove(currentNode);
            closeList.Add(currentNode);

            foreach(PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if(closeList.Contains(neighbourNode))
                {
                    continue;
                }
                if(!neighbourNode.IsWalkable())
                {
                    closeList.Add(neighbourNode);
                    continue;
                }
                int tentativeGCost = 
                    currentNode.GetGCost() + MOVE_STRAIGT_COST;
                
                if(tentativeGCost < neighbourNode.GetGCost())
                {
                    neighbourNode.SetCameFromPathNode(currentNode);
                    neighbourNode.SetGCost(tentativeGCost);
                    neighbourNode.SetHCost(CalculateHeuristicDistance(neighbourNode.GetGridPositon(), endGridPosition));
                    neighbourNode.CalculateFCost();
                    if(!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }

            }
        }
        // No path found
        pathLength = 0;
        return null;
    }


    public int CalculateHeuristicDistance(GridPosition gridPositionA, GridPosition gridPositionB)
    {
        return Mathf.RoundToInt(MOVE_STRAIGT_COST * Vector3.Distance(gridSystem.GetWorldPosition(gridPositionA), gridSystem.GetWorldPosition(gridPositionB)));
    }

    private PathNode GetLowestFCostPathNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostPathNode = pathNodeList[0];
        for (int i = 0; i < pathNodeList.Count; i++)
        {
            if(pathNodeList[i].GetFCost() < lowestFCostPathNode.GetFCost())
            {
                lowestFCostPathNode = pathNodeList[i];
            }
        }
        return lowestFCostPathNode;
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        GridPosition gridPostion = currentNode.GetGridPositon();
        if(gridPostion.x - 1 >= 0)
        {
        //Left Node
        neighbourList.Add(GetNode(gridPostion.x -1, gridPostion.z));
            
        }
        if(gridPostion.x + 1 < gridSystem.GetWidth())
        {
        //Right Node
        neighbourList.Add(GetNode(gridPostion.x +1, gridPostion.z));
        
        }
        if(gridPostion.z - 1 >= 0)
        {
            //Down Node
            neighbourList.Add(GetNode(gridPostion.x, gridPostion.z - 1));
        }
        if(gridPostion.z + 1< gridSystem.GetHeight())
        {      
            //UP Node
            neighbourList.Add(GetNode(gridPostion.x, gridPostion.z + 1));
        }

        bool oddRow = gridPostion.z % 2 == 1;

        if(oddRow)
        {
            if(gridPostion.x + 1 < gridSystem.GetWidth())
            {
                if(gridPostion.z - 1 >= 0)
                {
                neighbourList.Add(GetNode(gridPostion.x + 1, gridPostion.z - 1));
                }
                if(gridPostion.z + 1< gridSystem.GetHeight())
                {
                neighbourList.Add(GetNode(gridPostion.x + 1, gridPostion.z + 1));
                }
            }
        }
        else
        {
            if(gridPostion.x - 1 >= 0)
            {
                if(gridPostion.z - 1 >= 0)
                {
                    neighbourList.Add(GetNode(gridPostion.x - 1, gridPostion.z - 1));
                }
                if(gridPostion.z + 1< gridSystem.GetHeight())
                {
                    neighbourList.Add(GetNode(gridPostion.x - 1, gridPostion.z + 1));
                }
            }
        }

        return neighbourList;
    }

    private PathNode GetNode(int x, int z)
    {
        return gridSystem.GetGridObject(new GridPosition(x,z));
    }
    private List<GridPosition> CalculatePath(PathNode endNode)
    {
        List<PathNode> pathNodeList = new List<PathNode>();
        pathNodeList.Add(endNode);
        PathNode currentNode = endNode;
        while( currentNode.GetCameFromPathNode() != null)
        {
             pathNodeList.Add(currentNode.GetCameFromPathNode());
             currentNode = currentNode.GetCameFromPathNode();
        }

         pathNodeList.Reverse();
         List<GridPosition> gridPositionList = new List<GridPosition>();
         foreach (PathNode pathNode in pathNodeList)
         {
            gridPositionList.Add(pathNode.GetGridPositon());
         }
         return gridPositionList;
    }
    public bool IsWalkableGridPosition(GridPosition gridPosition)
    {
        return gridSystem.GetGridObject(gridPosition).IsWalkable();
    }
    public void SetIsWalkableGridPosition(GridPosition gridPosition,bool isWalkable)
    {
        gridSystem.GetGridObject(gridPosition).SetIsWalkable(isWalkable);
    }
    public bool HasPath(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        return FindPath(startGridPosition, endGridPosition, out int pathLength) != null;
    }
    public int GetPathLength(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        FindPath(startGridPosition, endGridPosition, out int pathLength);
        return pathLength;
    }
}
