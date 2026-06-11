using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct Vector2I
{
	public int x;
	public int y;
	public float d;

	public Vector2I(int nx, int ny)
	{
		x = nx;
		y = ny;
		d = 1;
	}

	public Vector2I(int nx, int ny, float nd)
	{
		x = nx;
		y = ny;
		d = nd;
	}
}

public class InfluenceMap : GridData
{
	List<IPropagator> _propagators = new List<IPropagator>();
	float[,] _influencesBuffer;
	public float Decay { get; set; }
	public float Momentum { get; set; }
	public int Width { get{ return _gridMap.Grid.GetLength(0); } }
	public int Height { get{ return _gridMap.Grid.GetLength(1); } }
	public Tile GetValue(int x, int y)
	{
		return _gridMap.Grid[x, y];
	}

	[SerializeField] 
	private GridMap _gridMap;
	
	public InfluenceMap(int size, float decay, float momentum, GridMap gridMap)
	{
		_influencesBuffer = new float[size, size];
		Decay = decay;
		Momentum = momentum;
		_gridMap = gridMap;
	}
	
	public InfluenceMap(int width, int height, float decay, float momentum, GridMap gridMap)
	{
		_influencesBuffer = new float[width, height];
		Decay = decay;
		Momentum = momentum;
		_gridMap = gridMap;
	}
	
	public void SetInfluence(int x, int y, float value)
	{
		if (x < Width && y < Height)
		{
			Tile tile = _gridMap.Grid[x, y];
			if (tile.isWalkable) {
				tile.influence = value;
			}
			_influencesBuffer[x, y] = value;
		}
	}

	public void SetInfluence(Vector2I pos, float value)
	{
		if (pos.x < Width && pos.y < Height)
		{
			Tile tile = _gridMap.Grid[pos.x, pos.y];
			if (tile.isWalkable) {
				tile.influence = value;
			}
			_influencesBuffer[pos.x, pos.y] = value;
		}
	}

	public void RegisterPropagator(IPropagator p)
	{
		_propagators.Add(p);
	}

	public void Propagate()
	{
		UpdatePropagators();
		UpdatePropagation();
		UpdateInfluenceBuffer();
	}

	void UpdatePropagators()
	{
		foreach (IPropagator p in _propagators)
		{
			SetInfluence(p.GridPosition, p.Value);
		}
	}

	void UpdatePropagation()
	{
		for (int xIdx = 0; xIdx < _gridMap.Grid.GetLength(0); ++xIdx)
		{
			for (int yIdx = 0; yIdx < _gridMap.Grid.GetLength(1); ++yIdx)
			{
				//Debug.Log("at " + xIdx + ", " + yIdx);
				float maxInf = 0.0f;
				float minInf = 0.0f;
				Vector2I[] neighbors = GetNeighbors(xIdx, yIdx);
				foreach (Vector2I n in neighbors)
				{
					//Debug.Log(n.x + " " + n.y);
					float inf = _influencesBuffer[n.x, n.y] * Mathf.Exp(-Decay * n.d); //* Decay;
					maxInf = Mathf.Max(inf, maxInf);
					minInf = Mathf.Min(inf, minInf);
				}
				
				Tile tile = _gridMap.Grid[xIdx, yIdx];
				if (tile.isWalkable) {
					if (Mathf.Abs(minInf) > maxInf)
					{
						tile.influence = Mathf.Lerp(_influencesBuffer[xIdx, yIdx], minInf, Momentum);
					}
					else
					{
						tile.influence = Mathf.Lerp(_influencesBuffer[xIdx, yIdx], maxInf, Momentum);
					}
				}
			}
		}
	}

	void UpdateInfluenceBuffer()
	{
		for (int xIdx = 0; xIdx < _gridMap.Grid.GetLength(0); ++xIdx)
		{
			for (int yIdx = 0; yIdx < _gridMap.Grid.GetLength(1); ++yIdx)
			{
				Tile tile = _gridMap.Grid[xIdx, yIdx];
				_influencesBuffer[xIdx, yIdx] = tile.influence;
			}
		}
	}
	
	Vector2I[] GetNeighbors(int x, int y)
	{
		List<Vector2I> retVal = new List<Vector2I>();
		
		// as long as not in left edge
		if (x > 0)
		{
			retVal.Add(new Vector2I(x-1, y));
		}

		// as long as not in right edge
		if (x < _gridMap.Grid.GetLength(0)-1)
		{
			retVal.Add(new Vector2I(x+1, y));
		}
		
		// as long as not in bottom edge
		if (y > 0)
		{
			retVal.Add(new Vector2I(x, y-1));
		}

		// as long as not in upper edge
		if (y < _gridMap.Grid.GetLength(1)-1)
		{
			retVal.Add(new Vector2I(x, y+1));
		}


		// diagonals

		// as long as not in bottom-left
		if (x > 0 && y > 0)
		{
			retVal.Add(new Vector2I(x-1, y-1, 1.4142f));
		}

		// as long as not in upper-right
		if (x < _gridMap.Grid.GetLength(0)-1 && y < _gridMap.Grid.GetLength(1)-1)
		{
			retVal.Add(new Vector2I(x+1, y+1, 1.4142f));
		}

		// as long as not in upper-left
		if (x > 0 && y < _gridMap.Grid.GetLength(1)-1)
		{
			retVal.Add(new Vector2I(x-1, y+1, 1.4142f));
		}

		// as long as not in bottom-right
		if (x < _gridMap.Grid.GetLength(0)-1 && y > 0)
		{
			retVal.Add(new Vector2I(x+1, y-1, 1.4142f));
		}
		return retVal.ToArray();
	}
}
