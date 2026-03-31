using System;
using UnityEngine;

[Serializable]
public class _3Visio_Shape : MonoBehaviour
{
	public int sides = 3;

	public string id = string.Empty;

	public bool Star;

	public bool Gear;

	public float raggioEsterno = 0.5f;

	public float apotema = 0.5f;

	public float sideLength;

	public float perimeter;

	public float area;

	public string Builded = string.Empty;

	public string Comments = string.Empty;

	public float starAmount = 0.5f;

	public GameObject parent;
}
