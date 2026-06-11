using UnityEngine;
using System.Collections;

public interface IPropagator
{
	Vector2I GridPosition { get; }
	float Value { get; }
}

public class SimplePropagator : MonoBehaviour, IPropagator
{
	[SerializeField]
	float _value;
	public float Value {
		get => _value;
		set => _value = value;
	}

	[SerializeField]
	InfluenceMapControl _map;

	public Vector2I GridPosition => _map.GetGridPosition(transform.position);
	
}
