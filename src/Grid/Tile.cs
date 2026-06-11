using Steering.Pathfinding;
using UnityEngine;

public class Tile {

    public enum TerrainType {
        Road,
        Grassland,
        Forest,
        RedCapturePoint,
        BluCapturePoint,
        BluBase,
        RedBase,
        Undefined,
        NotWalkable
    }
    // Cell coordinates
    public int x;
    public int y;
    public int z;
    // Equivalent world position. Stored to reduce computing time
    public Vector3 worldPosition;
    public TerrainType terrainType;
    
    // Can you pass through this tile?
    public bool isWalkable;
    
    // Are there any obstacles blocking the visibility on this tile?
    public bool seeThrough;
    
    public float influence;
    public float visibility;

    // Returns the cost of the tile
    public float Cost(NPC.UnitType unitType, NPC.UnitTeam team) {
        if (!isWalkable)
            return float.MaxValue;
        
        switch (terrainType) {
            case TerrainType.Forest:
                switch (unitType) {
                    case NPC.UnitType.Scout:
                        return 1.1f; 
                    case NPC.UnitType.Heavy: 
                        return 1.33f;
                    case NPC.UnitType.Medic:
                        return 1;
                    case NPC.UnitType.Sniper:
                        return 1;
                }
                break;
            case TerrainType.Grassland:
                switch (unitType) {
                    case NPC.UnitType.Scout:
                        return 1; 
                    case NPC.UnitType.Heavy:
                        return 1;
                    case NPC.UnitType.Medic:
                        return 1;
                    case NPC.UnitType.Sniper:
                        return 0.90f;
                }
                break;
            case TerrainType.Road:
                switch (unitType) {
                    case NPC.UnitType.Scout:
                        return 0.8f; 
                    case NPC.UnitType.Heavy:
                        return 0.66f;
                    case NPC.UnitType.Medic:
                        return 0.66f;
                    case NPC.UnitType.Sniper:
                        return 0.8f;
                }
                break;
            case TerrainType.BluBase:
                switch (team) {
                    case NPC.UnitTeam.Red:
                        return 1000;
                    case NPC.UnitTeam.Blu:
                        return 1;
                }
                break;
            case TerrainType.RedBase:
                switch (team) {
                    case NPC.UnitTeam.Red:
                        return 1;
                    case NPC.UnitTeam.Blu:
                        return 1000;
                }
                break;
            case TerrainType.NotWalkable:
                return float.MaxValue;
            default:
                return 1;
        }
        return 1;
    }

    // Returns the multiplying factor that each individual unit should apply while traversing this tile
    public float SpeedMultiplier(NPC.UnitType unitType) {
        switch (terrainType) {
            case TerrainType.Forest:
                switch (unitType) {
                    case NPC.UnitType.Scout:
                        return 0.9f; 
                    case NPC.UnitType.Heavy: 
                        return 0.75f;
                    case NPC.UnitType.Medic:
                        return 1;
                    case NPC.UnitType.Sniper:
                        return 1;
                }
                break;
            case TerrainType.Grassland:
                switch (unitType) {
                    case NPC.UnitType.Scout:
                        return 1; 
                    case NPC.UnitType.Heavy:
                        return 1;
                    case NPC.UnitType.Medic:
                        return 1;
                    case NPC.UnitType.Sniper:
                        return 1.1f;
                }
                break;
            case TerrainType.Road:
                switch (unitType) {
                    case NPC.UnitType.Scout:
                        return 1.25f; 
                    case NPC.UnitType.Heavy:
                        return 1.5f;
                    case NPC.UnitType.Medic:
                        return 1.5f;
                    case NPC.UnitType.Sniper:
                        return 1.25f;
                }
                break;
            default:
                return 1;
        }
        return 1;
    }

}
