using System.Collections;
using UnityEngine;

// Based on http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html
namespace Steering.Pathfinding
{
    public abstract class PathFinding : PathFollowing {
        
        // Different heuristics used for determining h values
        public enum Heuristic {
            Manhattan,
            Chebychev,
            Euclidean
        }
        [Header("Path Finding")]
        [SerializeField]
        // The grid upon which this pathfinding is going to work
        protected GridMap _grid;

        [SerializeField]
        // The heuristic to be used by this pathfinding
        protected Heuristic _heuristic;
        
        // Each unit has a type, influencing its movement throughout the terrain
        protected NPC.UnitType _type;

        // Each unit belongs to one of two available teams, influencing its movement depending on other units around
        protected NPC.UnitTeam _team;
        
        public NPC.UnitType Type {
            get => _type;
            set => _type = value;
        }

        public NPC.UnitTeam Team {
            get => _team;
            set => _team = value;
        }

        // Multipliers for each type of cost considered during tactical pathfinding
        [SerializeField] protected float _terrainCostMultiplier;
        [SerializeField] protected float _influenceCostMultiplier;
        [SerializeField] protected float _visibilityCostMultiplier;

        // Minimal straight distance between two tiles
        protected float _minimalStraightDistance;

        // Minimal diagonal distance between two tiles
        protected float _minimalDiagonalDistance;
        
        // Used by subclasses during algorithm execution
        protected Tile _currentTile;
        protected Tile _goalTile;

        public float TerrainCostMultiplier {
            get => _terrainCostMultiplier;
            set => _terrainCostMultiplier = value;
        }

        public float InfluenceCostMultiplier {
            get => _influenceCostMultiplier;
            set => _influenceCostMultiplier = value;
        }

        public float VisibilityCostMultiplier {
            get => _visibilityCostMultiplier;
            set => _visibilityCostMultiplier = value;
        }

        private void Awake() {
            Initialize();
        }
        
        private void Initialize() {
            _minimalStraightDistance = _grid.CellSize; // The minimal distance between two tiles is always the size of a cell
            _minimalDiagonalDistance = Mathf.Sqrt(2) * _grid.CellSize; // Diagonal distance
        }


        protected float ManhattanDistance(Tile start, Tile end) {
            int dx = Mathf.Abs(start.x - end.x);
            int dy = Mathf.Abs(start.z - end.z);

            // D * (dx + dy)
            return _minimalStraightDistance * (dx + dy);
        }

        protected float ChebychevDistance(Tile start, Tile end) {
            int dx = Mathf.Abs(start.x - end.x);
            int dy = Mathf.Abs(start.z - end.z);

            // D * (dx + dy) + (D2 - 2 * D) * min(dx, dy)
            return _minimalStraightDistance * (dx + dy) + (_minimalDiagonalDistance - 2 * _minimalStraightDistance) * Mathf.Min(dx, dy);
        }

        protected float EuclideanDistance(Tile start, Tile end) {
            int dx = Mathf.Abs(start.x - end.x);
            int dy = Mathf.Abs(start.z - end.z);

            // return D * sqrt(dx^2 + dy^2)
            return _minimalStraightDistance * Mathf.Sqrt(dx * dx + dy * dy);
        }

        protected abstract IEnumerator FindPath();

        protected float SetInitialHeuristic(Tile start, Tile goal) {
            switch (_heuristic) {
                case Heuristic.Manhattan:
                    return ManhattanDistance(start, goal);
                case Heuristic.Chebychev:
                    return ChebychevDistance(start, goal);
                case Heuristic.Euclidean:
                    return EuclideanDistance(start, goal);
                default:
                    Debug.LogWarning("Unknown heuristic '"+ _heuristic +"', whose fault is this?");
                    return 0;
            }
        }    


        public void FindPathToPosition(Vector3 currentPosition, Vector3 targetPosition) {
            Tile start = _grid.GetTile(currentPosition);
            if (start == null) {
                Debug.Log("Current position is outside the grid, doing nothing...");
                return;
            }
            Tile goal = _grid.GetTile(targetPosition);
            if (goal == null) {
                Debug.Log("Requested position is outside the grid, doing nothing...");
                return;
            }
            if (!start.isWalkable) {
                Debug.Log("Can't walk from that position, doing nothing...");
                return;
            }
            
            if (!goal.isWalkable) {
                Debug.Log("Can't walk to that position, doing nothing...");
                return;
            }
            StopCoroutine("FindPath");
            _currentTile = start;
            _goalTile = goal;

            _path.ClearPath();
            _currentParam = 0;
            _currentPos = 0;
            StartCoroutine(FindPath());
        }

    }
}
