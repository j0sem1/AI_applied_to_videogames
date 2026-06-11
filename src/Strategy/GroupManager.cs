using System.Collections.Generic;

public static class GroupManager {
    private static Dictionary<int, List<int>> _dictionary = new Dictionary<int, List<int>>();
    
    public static void CreateGroup(List<int> units) {
        var copy = new List<int>(units);
        foreach (var unit in units) {
            // If the unit belongs to another group, delete it from said group
            if (_dictionary.TryGetValue(unit, out var oldGroup)) {
                oldGroup.Remove(unit);
                // if the old group only has one member after removing said unit, remove the group
                if (oldGroup.Count == 1) {
                    _dictionary.Remove(oldGroup[0]);
                    oldGroup.Clear();
                }

                
            }
            _dictionary[unit] = copy;
        }
    }

    public static List<int> GetGroup(int unit) {
        try {
            return _dictionary[unit];
        }
        catch {
            return null;
        }
    }

    public static bool RemoveMemberFromGroup(int unit) {
        try {
            if (_dictionary.TryGetValue(unit, out var oldGroup)) {
                oldGroup.Remove(unit);
                _dictionary.Remove(unit);
                
                // if the old group only has one member after removing said unit, remove the group
                if (oldGroup.Count == 1) {
                    _dictionary.Remove(oldGroup[0]);
                    oldGroup.Clear();
                }
                    
            }
            return true;
        }
        catch {
            return false;
        }
    }

    public static void ClearAllGroups() {
        _dictionary.Clear();
    }
}
