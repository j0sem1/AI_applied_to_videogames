using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#pragma warning disable 0649 // Disable "variable not initialized" warnings due to serializefield

public class GridMap : MonoBehaviour {

    [SerializeField]
    // Defines the size of a cell in the x and z coordinate. Y is ignored.
    private float _cellSize;

    [SerializeField]
    // Extents used for the detection of obstacles inside each tile
    private Vector3 _cellExtents;

    [SerializeField]
    // Debug purposes. Shows the the grid on a given material
    private Material _gridShader; 

    [SerializeField]
    // Should gizmos be drawn?
    private bool _drawGizmos;

    // Array with all the tiles within the grid
    private Tile [,] _grid;

    [SerializeField]
    // x size of the grid
    private int _xSize;

    [SerializeField]
    // z size of the grid
    private int _zSize;

    [SerializeField]
    // Two markers are used to define the size of the grid, this is the first one
    private Transform _firstMarker;

    [SerializeField]
    // And this is the second one
    private Transform _secondMarker;

    [SerializeField]
    // Holds the cell position where the grid starts from
    private Vector3Int _offsetPosition;

    [SerializeField] 
    private InfluenceMapControl _influenceMapControl;

    // Used for the implementation of the visibility grid
    [SerializeField] 
    private bool _generateColliders;

    [SerializeField]
    private GameObject _colliderPrefab;
    private GameObject[,] _colliders;
    
    public float CellSize {
        get => _cellSize;
        set {
            _cellSize = value;
            if (_cellSize < 0.25f) 
                _cellSize = 0.25f; 
            if (_gridShader)
                _gridShader.SetFloat("_GridStep", _cellSize);
        }
    }

    public Tile[,] Grid => _grid;

    public int XSize => _xSize;

    public int ZSize => _zSize;

    private void Start() {
        ReadLevel();
    }

    private void OnValidate() {
        CellSize = CellSize; // Forces property changes on Inspector changes
    }

    // Returns the cell coordinates corresponding to a given world position
    public Vector3Int WorldToCell(Vector3 position) {
        int x = (int) Mathf.Round(position.x/_cellSize) - _offsetPosition.x;
        //float y = Mathf.Round(position.y/_cellSize) * _cellSize;
        int z = (int) Mathf.Round(position.z/_cellSize) - _offsetPosition.z;

        return new Vector3Int(x, (int) position.y, z);
    }

    // Returns the world position corresponding to given cell coordinates
    public Vector3 CellToWorld(Vector3Int position) {
        Vector3 worldPosition = _offsetPosition;
        worldPosition *= _cellSize;
        worldPosition.x += position.x * _cellSize;
        worldPosition.z += position.z * _cellSize;
        return worldPosition;
    }

    public Tile GetTile(Vector3 position) {
        Vector3Int cellPosition = WorldToCell(position);
        // Check if the requested tile is within bounds
        if (cellPosition.x >= 0 && cellPosition.x < _xSize && cellPosition.z >= 0 && cellPosition.z < _zSize) 
            return _grid[cellPosition.x, cellPosition.z];
        return null;
    }

    public bool IsDiagonal(Tile tile, Tile adjacentTile) {
        // Left and down
        if (tile.x - 1 == adjacentTile.x && tile.z - 1 == adjacentTile.z)
            return true;

        // Left and up
        if (tile.x - 1 == adjacentTile.x && tile.z + 1 == adjacentTile.z)
            return true;

        // Right and down
        if (tile.x + 1 == adjacentTile.x && tile.z - 1 == adjacentTile.z)
            return true;

        // Right and up
        if (tile.x + 1 == adjacentTile.x && tile.z + 1 == adjacentTile.z)
            return true;

        return false;
    }

    public List<Tile> GetAdjacentTiles(Tile tile, bool diagonal = false) {
        var output = new List<Tile>();

        // Left
        if (tile.x > 0 && Grid[tile.x-1, tile.z].isWalkable) 
            output.Add(Grid[tile.x-1, tile.z]);
        
        // Down
        if (tile.z > 0 && Grid[tile.x, tile.z-1].isWalkable)
            output.Add(Grid[tile.x, tile.z-1]);
        
        // Right
        if (tile.x < _xSize-1 && Grid[tile.x+1, tile.z].isWalkable)
            output.Add(Grid[tile.x+1, tile.z]);

        // Up
        if (tile.z < _zSize-1 && Grid[tile.x, tile.z+1].isWalkable)
            output.Add(Grid[tile.x, tile.z+1]);
        
        if (diagonal) {
            // Left and down
            if (tile.x > 0 && tile.z > 0 && Grid[tile.x-1, tile.z-1].isWalkable) 
                output.Add(Grid[tile.x-1, tile.z-1]);

            // Left and up
            if (tile.x > 0 && tile.z < _zSize-1 && Grid[tile.x-1, tile.z+1].isWalkable)
                output.Add(Grid[tile.x-1, tile.z+1]);

            // Right and down
            if (tile.x < _xSize-1 && tile.z > 0 && Grid[tile.x+1, tile.z-1].isWalkable)
                output.Add(Grid[tile.x+1, tile.z-1]);

            // Right and up
            if (tile.x < _xSize-1 && tile.z < _zSize-1 && Grid[tile.x+1, tile.z+1].isWalkable)
                output.Add(Grid[tile.x+1, tile.z+1]);
        }
        return output;
    }

    private void ReadLevel() {

        if (!_firstMarker || !_secondMarker) {
            Debug.LogError("Grid was attempted to be created but markers were not assigned");
            return;
        }
        
        // Find the bounds of our grid given the two markers
        float minX, maxX, minZ, maxZ;

        // x
        if (_firstMarker.position.x < _secondMarker.position.x) {
            minX = _firstMarker.position.x;
            maxX = _secondMarker.position.x;
        } else {
            maxX = _firstMarker.position.x;
            minX = _secondMarker.position.x;
        }  
        
        // z
        if (_firstMarker.position.z < _secondMarker.position.z) {
            minZ = _firstMarker.position.z;
            maxZ = _secondMarker.position.z;
        } else {
            maxZ = _firstMarker.position.z;
            minZ = _secondMarker.position.z;
        }  

        // Bounds include the cell where the max position lands in
        int x = Mathf.RoundToInt((maxX - minX) / _cellSize) + 1;
        int z = Mathf.RoundToInt((maxZ - minZ) / _cellSize) + 1;

        // Save the starting position of the grid
        _offsetPosition = Vector3Int.zero;
        var position = WorldToCell(new Vector3(minX, 0, minZ));
        _offsetPosition = position;
        CreateGrid(x, z);
        if (_influenceMapControl)
            _influenceMapControl.Initialize(x, z);
    }

    private void CreateGrid(int xSize, int zSize) {
        _xSize = xSize;
        _zSize = zSize;
        //Vector3 halfExtents = new Vector3(_cellSize/4, _cellSize, _cellSize/4);
        Vector3 halfExtents = _cellExtents/2;
        _grid = new Tile[xSize,zSize];
        for (int x = 0; x < xSize; x++) {
            for (int z = 0; z < zSize; z++) {
                // Initialize the tile
                Tile tile = new Tile();

                tile.x = x;
                tile.y = 0;
                tile.z = z;
                tile.influence = 0;
                tile.visibility = 0;
                tile.terrainType = Tile.TerrainType.Undefined;

                tile.worldPosition = CellToWorld(new Vector3Int(x, 0, z));

                // Find out if the tile is walkable or not
                Collider[] tileOverlap = Physics.OverlapBox(tile.worldPosition, halfExtents, Quaternion.identity);
                if (tileOverlap.Length > 0) {
                    bool isWalkable = true;
                    bool seeThrough = true;
                    for (int i = 0; i < tileOverlap.Length; i++) {
                        string tileTag = tileOverlap[i].transform.tag;
                        // If there are any objects tagged as obstacles within the tile, it's not walkable and you cannot see past it
                        if (tileTag == "Obstacle") {
                            isWalkable = false;
                            seeThrough = false;
                            tile.terrainType = Tile.TerrainType.NotWalkable;
                        }
                        if (tileTag == "NotWalkable") {
                            isWalkable = false;
                            tile.terrainType = Tile.TerrainType.NotWalkable;
                        }
                        if (tileTag == "Road" && isWalkable) {
                            tile.terrainType = Tile.TerrainType.Road;
                        }
                        if (tileTag == "Grassland" && isWalkable) {
                            tile.terrainType = Tile.TerrainType.Grassland;
                        }
                        if (tileTag == "Forest" && isWalkable) {
                            tile.terrainType = Tile.TerrainType.Forest;
                        }
                        if (tileTag == "BluCapturePoint" && isWalkable) {
                            tile.terrainType = Tile.TerrainType.BluCapturePoint;
                        }
                        if (tileTag == "RedCapturePoint" && isWalkable) {
                            tile.terrainType = Tile.TerrainType.RedCapturePoint;
                        }
                        if (tileTag == "RedBase" && isWalkable) {
                            tile.terrainType = Tile.TerrainType.RedBase;
                        }
                        if (tileTag == "BluBase" && isWalkable) {
                            tile.terrainType = Tile.TerrainType.BluBase;
                        }
                    }
                    tile.isWalkable = isWalkable;
                    tile.seeThrough = seeThrough;
                } else {
                    tile.isWalkable = true;
                    tile.seeThrough = true;
                }
                _grid[x, z] = tile;
            }
        }
        
        if (_generateColliders)
            GenerateColliders();
    }

    private void GenerateColliders() {
        // For each tile, create a box collider of the same size
        _colliders = new GameObject[_xSize, _zSize];
        for (int x = 0; x < _xSize; x++) {
            for (int z = 0; z < _zSize; z++) {
                var instance = Instantiate(_colliderPrefab);
                _colliders[x, z] = instance;
                var tile = _grid[x, z];
                if (!tile.seeThrough) 
                    instance.tag = "Obstacle";
                
                instance.transform.position = CellToWorld(new Vector3Int(x, 0, z));
                instance.transform.parent = transform;

                // Collider added during Start instead because physics do not update on Start
                var collider = instance.AddComponent<BoxCollider>();
                collider.size = new Vector3(_cellSize, 1, _cellSize);
                instance.layer = 10;
            }
        }
        GenerateVisibilityMap();
    }

    private void GenerateVisibilityMap() {
        int maxVisibility = 0;
        for (int x = 0; x < _xSize; x++) {
            for (int z = 0; z < _zSize; z++) {
                // Not walkable tiles do not need visibility values
                if (!_grid[x, z].isWalkable) {
                    _grid[x, z].visibility = 0;
                    continue;
                }
                // For each tile, shoot raysToShoot raycasts and see how many colliders it passes through
                // For each collider added, visibility++ and stop counting once you hit an obstacle
                int raysToShoot = 6;
                float angle = 0;
                int visibility = 0;
                var world = _colliders[x, z].transform.position;
                for (int i = 0; i < raysToShoot; i++) {
                    float xRay = Mathf.Sin(angle);
                    float zRay = Mathf.Cos(angle);
                    angle += 2 * Mathf.PI / raysToShoot;
                    
                    Vector3 dir = new Vector3 (world.x + xRay, 0, world.z + zRay);
                    RaycastHit[] hits = Physics.RaycastAll(world, dir, 15, 1 << 10);
                    // Sort hits by distance
                    List<RaycastHit> hitList = new List<RaycastHit>(hits);
                    hitList.Sort((a, b) => a.distance.CompareTo(b.distance));
                    foreach (var hit in hitList) {
                        if (hit.transform.CompareTag("Obstacle"))
                            break;
                        visibility++;
                    }
                }
                _grid[x, z].visibility = visibility;
                
                // Update the value of the tile with highest visibility
                if (visibility > maxVisibility)
                    maxVisibility = visibility;
            }
        }
        
        // Lastly, invert the values of visibility so the lowest value is the best one
        // Normalize to [0,2] range
        // It helps LRTA*
        for (int x = 0; x < _xSize; x++) {
            for (int z = 0; z < _zSize; z++) {
                _grid[x, z].visibility = (maxVisibility - _grid[x, z].visibility) / (maxVisibility * 0.5f);
            }
        }
        DestroyColliders();
    }
    
    private void DestroyColliders() {
        for (int x = 0; x < _xSize; x++) {
            for (int z = 0; z < _zSize; z++) {
                // Normalization of values could be done here to increase efficiency
                Destroy(_colliders[x, z]);
            }
        }
    }
    
    private void OnDrawGizmos() {
        // Draws every available tile
        if (_drawGizmos) {
            Vector3 extents = new Vector3(_cellSize, 0, _cellSize);
            Gizmos.color = Color.black;
            if (_grid == null)
                return;
            foreach (var tile in _grid) {
                if (tile == null)
                    continue;
                var color = Color.gray;
                if (!tile.isWalkable) {
                    if (!tile.seeThrough)
                        color = Color.red;
                    else
                        color = Color.blue;
                }
                Gizmos.color = color;
                Gizmos.DrawWireCube(tile.worldPosition, extents);
            }
        }
    }
}

