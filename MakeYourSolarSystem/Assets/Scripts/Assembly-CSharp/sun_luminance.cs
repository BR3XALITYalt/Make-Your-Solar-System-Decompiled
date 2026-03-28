using UnityEngine;

public class sun_luminance : MonoBehaviour
{
	private Object[] objects;

	private Texture[] textures;

	private float speed = 10f;

	private float time;

	private int prev_time;

	private void Awake()
	{
	}

	private void Start()
	{
		objects = Resources.LoadAll("luma", typeof(Texture));
		textures = new Texture[objects.Length];
		for (int i = 0; i < objects.Length; i++)
		{
			textures[i] = (Texture)objects[i];
		}
	}

	private void Update()
	{
		time += Time.deltaTime * speed;
		if ((float)prev_time != Mathf.Round(time))
		{
			prev_time = (int)Mathf.Round(time);
			if (prev_time >= objects.Length)
			{
				prev_time = 0;
				time = 0f;
			}
			GetComponent<Renderer>().material.SetTexture("_Illum", textures[prev_time]);
		}
	}
}
