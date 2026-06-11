using UnityEngine;
using System.Collections.Generic;

public class InfluenceMapControl : MonoBehaviour
{
	[SerializeField]
	Transform _bottomLeft;
	
	[SerializeField]
	Transform _upperRight;
	
	[SerializeField]
	float _gridSize;
	
	[SerializeField]
	float _decay = 0.3f;
	
	[SerializeField]
	float _momentum = 0.8f;
	
	[SerializeField]
	float _updateFrequency = 3;
	
	InfluenceMap _influenceMap;

	[SerializeField]
	GridDisplay _display;

	[SerializeField] 
	private GridMap _gridMap;

	[SerializeField]
	private List<SimplePropagator> _propagators;

	void CreateMap(int x, int z) {
		// how many of gridsize is in Mathf.Abs(_upperRight.positon.x - _bottomLeft.position.x)
		int width = x;
		int height = z;
		
		//Debug.Log(width + " x " + height);
		
		_influenceMap = new InfluenceMap(width, height, _decay, _momentum, _gridMap);
		
		_display.SetGridData(_influenceMap);
		_display.CreateMesh(_bottomLeft.position, _gridSize);
	}

	public Vector2I GetGridPosition(Vector3 pos)
	{
		//int x = (int)((pos.x - _bottomLeft.position.x)/_gridSize);
		//int y = (int)((pos.z - _bottomLeft.position.z)/_gridSize);
		var cellPosition =_gridMap.WorldToCell(pos);
		

		return new Vector2I(cellPosition.x, cellPosition.z);
	}

	
	public void Initialize(int x, int z) {
		CreateMap(x, z);
		
		foreach (var propagator in _propagators) {
			_influenceMap.RegisterPropagator(propagator);
		}
		
		InvokeRepeating(nameof(PropagationUpdate), 0.001f, 1.0f/_updateFrequency);
	}

	void PropagationUpdate()
	{
		_influenceMap.Propagate();
	}

	void SetInfluence(int x, int y, float value)
	{
		_influenceMap.SetInfluence(x, y, value);
	}

	void SetInfluence(Vector2I pos, float value)
	{
		_influenceMap.SetInfluence(pos, value);
	}

	void Update()
	{
		_influenceMap.Decay = _decay;
		_influenceMap.Momentum = _momentum;
		
	}
}
