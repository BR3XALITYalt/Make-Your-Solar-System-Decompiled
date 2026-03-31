using System;
using UnityEngine;

public class VisibleGeometry : MonoBehaviour
{
	public int segments;

	public float xradius;

	public float yradius;

	public LineRenderer line;

	private int frontierOpacityFactor;

	private void Start()
	{
		line.SetVertexCount(segments + 1);
		line.useWorldSpace = false;
		CreatePoints();
	}

	private void CreatePoints()
	{
		float z = 0f;
		float num = 20f;
		for (int i = 0; i < segments + 1; i++)
		{
			float x = Mathf.Sin((float)Math.PI / 180f * num) * xradius;
			float y = Mathf.Cos((float)Math.PI / 180f * num) * yradius;
			line.SetPosition(i, new Vector3(x, y, z));
			num += 360f / (float)segments;
		}
	}

	public void SetOpacityFactor(float newFactor)
	{
		frontierOpacityFactor = (int)newFactor;
		Color red = Color.red;
		red.a = (float)frontierOpacityFactor / 100f;
		line.SetColors(red, red);
	}

	public int GetOpacityFactor()
	{
		return frontierOpacityFactor;
	}
}
