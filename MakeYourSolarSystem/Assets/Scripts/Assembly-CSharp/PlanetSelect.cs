using System.Collections;
using UnityEngine;

public class PlanetSelect : MonoBehaviour
{
	private GameController gc;

	public static GameObject previousPlanetSelector;

	private GameObject objectToMove;

	public static bool panning;

	private void Start()
	{
		getGameController();
	}

	private void OnMouseDown()
	{
		if (gc.getActiveAction() == "camera" && !gc.IsGamePaused())
		{
			float num = (float)Camera.main.pixelWidth * gc.GetLeftSideBarRelativeWidth();
			if (Input.mousePosition.x > num && !Camera.main.GetComponent<CameraControl>().GetMovingToNewObject())
			{
				panning = false;
				objectToMove = base.transform.parent.gameObject;
				StartCoroutine(GoToObject());
			}
		}
	}

	private IEnumerator GoToObject()
	{
		yield return new WaitUntil(() => Input.touchCount == 0);
		if (!panning)
		{
			Camera.main.GetComponent<CameraControl>().MoveToNewObject(objectToMove);
			base.gameObject.SetActive(false);
			if (previousPlanetSelector != null)
			{
				previousPlanetSelector.SetActive(true);
			}
			previousPlanetSelector = base.gameObject;
		}
		objectToMove = null;
	}

	public void SetPanning(bool isPanning = true)
	{
		panning = isPanning;
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
