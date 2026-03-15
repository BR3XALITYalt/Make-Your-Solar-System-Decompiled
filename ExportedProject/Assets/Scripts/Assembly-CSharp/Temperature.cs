using UnityEngine;

public class Temperature : MonoBehaviour
{
	private Quaternion tempRotation;

	private void Start()
	{
		tempRotation = Quaternion.Euler(270f, 0f, 0f);
	}

	private void LateUpdate()
	{
		base.transform.rotation = tempRotation;
	}
}
