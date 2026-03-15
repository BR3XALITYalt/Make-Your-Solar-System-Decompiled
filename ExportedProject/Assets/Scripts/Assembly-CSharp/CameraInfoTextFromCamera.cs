using SmartLocalization;
using UnityEngine;
using UnityEngine.UI;

public class CameraInfoTextFromCamera : MonoBehaviour
{
	private GameController gc;

	private void Start()
	{
		getGameController();
	}

	private void Update()
	{
		if (gc.getActiveAction() == "camera")
		{
			Vector2 vector = Camera.main.transform.position;
			float xcorrectionFromSidebar = Camera.main.GetComponent<CameraControl>().GetXcorrectionFromSidebar();
			float num = (2f * Camera.main.orthographicSize * Camera.main.aspect - xcorrectionFromSidebar) / 564f;
			string text = "  X: " + ((vector.x + xcorrectionFromSidebar) / 564f).ToString("F1") + "\n  Y: " + (vector.y / 564f).ToString("F1") + "\n" + LanguageManager.Instance.GetTextValue("Width") + " " + num.ToString("F1");
			GetComponent<Text>().text = text;
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
