using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Based on Heuristic Search - Theory and Applications by Stefan Edelkamp & Stefan Schrodl

namespace Steering.Pathfinding
{
    public class LRTA : PathFinding {

        private enum LocalSearchSpaceMethod {
            OneLookAhead,
            BreadthFirstSearch
        }

        // Defines the information for each tile used during this algorithm
        // By having this information in the LRTA* class we avoid manipulating directly each tile, allowing concurrent pathfinding agents
        private class TileInfo {
            public Tile tile; // The tile this information is referring to
            public float h; // Tile's current heuristic value. -1 if it's "infinite"
            public float temp; // Temporal heuristic value
            public TileInfo successor; // Best neighbour to move to
            public float successorCost; // Cost of said best neighbour
            public int timestamp; // Timestamp of the heuristic value to improve code efficiency
        }

        // A "lazy" copy of the map grid 
        private TileInfo[,] _gridInfo; 

        // Method used to establish each local search space
        [SerializeField] private LocalSearchSpaceMethod _localSearchSpaceMethod;

        // Number of tiles per local search space using breadthFirstSearch
        [SerializeField] private int _breadthFirstSize;

        // Should "tactical" information be used during the pathfinding?
        [SerializeField] private bool _tactical;
        
        [SerializeField] private bool _drawGizmos;

        // Current local search space
        private List<TileInfo> _localSearchSpace;

        // Should diagonal movements be considered?
        private bool _diagonalMovement;

        // Time in seconds (since start of the game) when the coroutine was started
        private int _timestamp;

        private void Start() {
            _localSearchSpace = new List<TileInfo>();
            _timestamp = 0;
        }

        // LRTA implementation
        protected override IEnumerator FindPath() {
            // Initialize elements
            _timestamp = Mathf.FloorToInt(Time.time);
            _diagonalMovement = _heuristic != Heuristic.Manhattan;
            
            // Set initial state
            if (_gridInfo == null) {
                _gridInfo = new TileInfo[_grid.XSize, _grid.ZSize];
            } 
            TileInfo currentTileInfo;

            // Do magic until the goal tile is achieved
            while (_currentTile != _goalTile) {
                GenerateLocalSearchSpace();
                ValueUpdateStep(); 
                currentTileInfo = GetTileInfo(_currentTile);
                // The selected action is the successor of the current tile 
                do {
                    // Execute said action while the successor is inside the local search space
                    _currentTile = currentTileInfo.successor.tile;
                    currentTileInfo = currentTileInfo.successor;
                    
                    // In our case, we just append a point to the path following steering
                    _path.AppendPointToPath(_currentTile.worldPosition);
                    
                    // Another alternative would be teleporting the agent
                    // transform.position = _currentTile.worldPosition;
                } while (_localSearchSpace.Contains(currentTileInfo));
                yield return null;
            }
        }

        private void GenerateLocalSearchSpace() {
            _localSearchSpace.Clear();
            switch (_localSearchSpaceMethod) {
                case LocalSearchSpaceMethod.BreadthFirstSearch:
                    _localSearchSpace.AddRange(BreadthFirstSearch());
                    break;
                case LocalSearchSpaceMethod.OneLookAhead:
                    _localSearchSpace.Add(GetTileInfo(_currentTile));
                    break;
            }
        }

        private void ValueUpdateStep() {
            var openSearchSpace = new List<TileInfo>(_localSearchSpace);

            // Store all h values into temp and set h to "infinite"
            foreach (TileInfo tileInfo in openSearchSpace) {
                tileInfo.temp = tileInfo.h;
                tileInfo.h = Mathf.Infinity;
            }

            // While there are no tiles with h = infinite
            while (openSearchSpace.Count > 0) {

                // Keep track of the minimum of all the local search space
                TileInfo min = openSearchSpace[0];

                // Find the minimum cost action for each tile in the local search space
                foreach (TileInfo tileInfo in openSearchSpace) {

                    // Clear previous data
                    tileInfo.successorCost = Mathf.Infinity;
                    tileInfo.successor = null;

                    // The minimum cost is the cheapest neighbour to move to
                    var adjacentTiles = _grid.GetAdjacentTiles(tileInfo.tile, _diagonalMovement);
                    foreach (Tile adjacentTile in adjacentTiles) {
                        var adjacentTileInfo = GetTileInfo(adjacentTile);
                        float adjacentCost = AdjacentTileCost(adjacentTile);
                        // find min w(u,a) + h(Succ(u,a))
                        if (tileInfo.successorCost > adjacentCost + adjacentTileInfo.h) {
                            tileInfo.successorCost = adjacentCost + adjacentTileInfo.h; 
                            tileInfo.successor = adjacentTileInfo;
                        }
                    }
                    
                    // Maximize between the previous h value and the new one
                    if (tileInfo.temp > tileInfo.successorCost) {
                        tileInfo.successorCost = tileInfo.temp;
                    }

                    // Update the minimum
                    if (min.successorCost > tileInfo.successorCost)
                        min = tileInfo;
                }

                // Save the new h value of the minimum and repeat the process with the remaining tiles
                min.h = min.successorCost; 
                min.temp = Mathf.Infinity;

                // Unless no improvement was made
                if (float.IsPositiveInfinity(min.h))
                    return;

                openSearchSpace.Remove(min); 
            }

        }
        
        private TileInfo GetTileInfo(Tile tile) {

            // Has the requested tile been initialized?
            if (_gridInfo[tile.x, tile.z] != null) {
                
                var requestedTile = _gridInfo[tile.x, tile.z];
                
                // Is the tile's heuristic updated or from a previous execution of the algorithm?
                if (requestedTile.timestamp == _timestamp)
                    return requestedTile;

                // If so, set the initial heuristic
                requestedTile.h = SetInitialHeuristic(requestedTile.tile, _goalTile);
                requestedTile.timestamp = _timestamp;
                return requestedTile;
            }
            // If not, initialize it
            TileInfo tileInfo = new TileInfo {tile = tile};

            if (tile.isWalkable)
                tileInfo.h = SetInitialHeuristic(tile, _goalTile);
            else
                tileInfo.h = Mathf.Infinity;
            tileInfo.temp = Mathf.Infinity;
            tileInfo.successor = null;
            tileInfo.successorCost = Mathf.Infinity;
            tileInfo.timestamp = _timestamp;

            _gridInfo[tile.x, tile.z] = tileInfo;
            return tileInfo;
        }

        private List<TileInfo> BreadthFirstSearch() {
            // Adds tiles to the output until _breadthFirstSize is reached
            List<Tile> queue = new List<Tile>();
            queue.Add(_currentTile);
            List<TileInfo> output = new List<TileInfo>();

            for (int i = 0; output.Count < _breadthFirstSize && i < queue.Count; i++) {
                var tile = queue[i];
                if (tile != _goalTile)
                    output.Add(GetTileInfo(tile));
                
                var add = _grid.GetAdjacentTiles(tile, _diagonalMovement).Except(queue).ToList();
                queue.AddRange(add);
            }
            return output;
        }

        private bool IsAdjacentTileDiagonal(Tile adjacentTile) {
            return _grid.IsDiagonal(_currentTile, adjacentTile);
        }

        private float AdjacentTileCost(Tile adjacentTile) {
            float finalCost;
            if (IsAdjacentTileDiagonal(adjacentTile))
                finalCost = _minimalDiagonalDistance;
            else
                finalCost = _minimalStraightDistance;
            
            if (_tactical) {
                // Account for terrain costs
                float terrainCost = _terrainCostMultiplier * (_currentTile.Cost(Type, Team) + adjacentTile.Cost(Type, Team)) / 2;

                // Account for influence costs
                float currentInfluence;
                float adjacentInfluence;
                if (Team == NPC.UnitTeam.Red) {
                    // Ignore friendly influence (positive values)
                    // Avoid enemy influence areas (negative values turned positive)
                    if (_currentTile.influence > 0)
                        currentInfluence = 0;
                    else 
                        currentInfluence = Mathf.Abs(_currentTile.influence);
                    
                    if (adjacentTile.influence > 0) 
                       adjacentInfluence = 0;
                    else 
                       adjacentInfluence = Mathf.Abs(adjacentTile.influence);
                }
                else {
                    // Ignore friendly influence (negative values)
                    // Avoid enemy influence areas (positive values)
                    if (_currentTile.influence < 0)
                        currentInfluence = 0;
                    else
                        currentInfluence = _currentTile.influence;

                    if (adjacentTile.influence < 0) 
                        adjacentInfluence = 0;
                    else 
                        adjacentInfluence = adjacentTile.influence;
                }

                float influenceCost = _influenceCostMultiplier * (currentInfluence + adjacentInfluence) / 2;
                float visibilityCost =
                    _visibilityCostMultiplier * (_currentTile.visibility + adjacentTile.visibility) / 2;

                finalCost += terrainCost + influenceCost + visibilityCost;
            }
            return finalCost;
        }
        
        private void OnDrawGizmos() {
            // Draws visited tiles and their last stored heuristic value
            if (_drawGizmos) {
                Vector3 extents = new Vector3(_grid.CellSize, 0, _grid.CellSize);
                Gizmos.color = Color.black;
                if (_gridInfo == null)
                    return;
                foreach (var tileInfo in _gridInfo) {
                    if (tileInfo == null)
                        continue;
                    var tile = tileInfo.tile;
                    var color = Color.gray;
                    Gizmos.color = color;
                    Gizmos.DrawWireCube(tile.worldPosition, extents);
                    GizmosUtils.DrawText(GUI.skin, "(" + tile.x + "," + tile.z + "," +  (int) tileInfo.h +")", tile.worldPosition, color, 14, 0);
                }
            }
        }
    }
}
