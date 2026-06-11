using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649 // Disable "variable not initialized" warnings due to serializefield
namespace Steering.Delegate {
    public class Path : MonoBehaviour {
        [SerializeField] private List<Vector3> _path;        // Path points
        [SerializeField] private float _radius;              //  Radius to consider that the next point has been reached
        [SerializeField] private bool _drawGizmos;

        public int GetParam(Vector3 characterPosition, int currentPosition) {

            // If there is no path return current position
            if (_path.Count <= 1) {
                return currentPosition;
            }
            // If current position is somehow outside the path, return the last point
            if (currentPosition >= _path.Count) {
                return _path.Count - 1;
            }
            
            // Find the closest point to the path between the current position, the previous and the next one
            float distanceToPreviousPoint, distanceToCurrentPoint, distanceToNextPoint; // a, b, c

            // If we're at the start, there's no previous point
            if (currentPosition == 0) {
                distanceToPreviousPoint = float.MaxValue;
                distanceToNextPoint = Vector3.Distance(characterPosition, _path[currentPosition + 1]);
            }
            // If we're at the end, there's no next point
            else if (currentPosition == _path.Count - 1) {
                distanceToPreviousPoint = Vector3.Distance(characterPosition, _path[currentPosition - 1]);
                distanceToNextPoint = float.MaxValue;
            }
            // Otherwise, both points exist
            else {
                distanceToNextPoint = Vector3.Distance(characterPosition, _path[currentPosition + 1]);
                distanceToPreviousPoint = Vector3.Distance(characterPosition, _path[currentPosition - 1]);
            }

            distanceToCurrentPoint = Vector3.Distance(characterPosition, _path[currentPosition]);

            
            // The following criteria is used to determine the closest point:
            // If the current distance is closest to the current point, return said point
            // If the distance is closest to either the previous or next point AND it's under the radius threshold,
            // return said point. Otherwise, return the current point
            
            if (distanceToCurrentPoint <= distanceToPreviousPoint && distanceToCurrentPoint <= distanceToNextPoint)
                return currentPosition;

            if (distanceToNextPoint <= distanceToPreviousPoint && distanceToNextPoint < _radius)
                return currentPosition + 1;

            if (distanceToPreviousPoint <= distanceToNextPoint && distanceToPreviousPoint < _radius)
                return currentPosition - 1;

            return currentPosition;
        }

        public Vector3 GetPosition(int param) {

            // Return the current position if the path is empty
            if (_path.Count == 0)
                return transform.position;
            
            // Return the last point if further inexistent points are requested
            if (param >= _path.Count)
                return _path[_path.Count - 1];

            // Return the first point if previous inexistent points are requested
            if (param < 0)
                return _path[0];


            return _path[param];
        }

        public void AppendPointToPath(Vector3 position) {
            _path.Add(position);
        }

        public void ClearPath() {
            _path.Clear();
            _path.Add(transform.position); 
        }

        public int Length() {
            return _path.Count;
        }

        public void OnDrawGizmos() {
            if (_drawGizmos) {
                foreach (var position in _path) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(position, _radius);
                }
            }
        }
    }
}