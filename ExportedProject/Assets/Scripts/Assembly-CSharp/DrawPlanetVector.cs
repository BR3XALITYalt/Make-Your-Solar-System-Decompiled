using UnityEngine;

public class DrawPlanetVector : MonoBehaviour
{
	public LineRenderer drawSpeedLine;

	public LineRenderer drawSpeedPointer;

	private GameController gc;

	private void Start()
	{
		getGameController();
		if (drawSpeedLine == null)
		{
			Transform vectorLine = base.transform.Find("VectorLine");
			if (vectorLine != null)
			{
				drawSpeedLine = vectorLine.GetComponent<LineRenderer>();
			}
		}
		if (drawSpeedPointer == null)
		{
			Transform vectorPointer = base.transform.Find("VectorPointer");
			if (vectorPointer != null)
			{
				drawSpeedPointer = vectorPointer.GetComponent<LineRenderer>();
			}
		}
		if (drawSpeedLine == null || drawSpeedPointer == null)
		{
			Debug.LogWarning("DrawPlanetVector is missing line renderer references.");
			Object.Destroy(base.gameObject);
			return;
		}
		float num = Camera.main.orthographicSize / 400f;
		drawSpeedLine.SetWidth(8f * num, 8f * num);
		drawSpeedPointer.SetWidth(23.1f * num, 0f);
		drawSpeedPointer.SetPosition(0, new Vector3(0f, -10f * num, 0f));
		drawSpeedPointer.SetPosition(1, new Vector3(0f, 10f * num, 0f));
	}

	private void Update()
	{
		if (gc == null || drawSpeedLine == null || drawSpeedPointer == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (!gc.GetGameOver())
		{
			if (gc.GetCreatingPlanet())
			{
				Vector3 vector = gc.ScreenPointToWorld(gc.GetPlanetOriginPoint());
				drawSpeedLine.SetPosition(0, vector);
				Vector3 vector2 = gc.ScreenPointToWorld(Input.mousePosition);
				drawSpeedLine.SetPosition(1, vector2);
				drawSpeedPointer.SetPosition(0, vector2);
				float num = Camera.main.orthographicSize / 400f;
				Vector3 position = vector2 + (vector2 - vector).normalized * 23.1f * num;
				drawSpeedPointer.SetPosition(1, position);
			}
			else if (Input.GetButtonUp("Fire1") && !gc.GetCreatingPlanet())
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void getGameController()
	{
		GameObject gameObject = GameObject.FindWithTag("GameController");
		if (gameObject != null)
		{
			gc = gameObject.GetComponent<GameController>();
		}
		else
		{
			Debug.Log("Cannot find 'GameController' script");
		}
	}
}
