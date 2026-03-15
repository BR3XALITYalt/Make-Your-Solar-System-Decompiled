using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
	private GameController gc;

	public float minZoom = 5000f;

	public GameObject objectToLookAt;

	public GameObject Barycenter;

	private GameObject barycenterImage;

	private Vector3 playableRegionScale;

	public GameObject InfoCameraText;

	public GameObject zoomSlider;

	public float orthoZoomSpeed = 10f;

	public float mouseSensitivity = 1f;

	private float xCorrectionFromSidebar;

	public static bool panning;

	private bool switchingToNewObject;

	public float cameraTranslationTime = 10.3f;

	private void Start()
	{
		getGameController();
		barycenterImage = Barycenter.transform.Find("Image").gameObject;
	}

	private void Update()
	{
		if (gc.getActiveAction() == "camera" && !gc.GetGameOver() && !switchingToNewObject)
		{
			float num = (float)Camera.main.pixelWidth * gc.GetLeftSideBarRelativeWidth();
			if (Input.touchCount == 2)
			{
				Touch touch = Input.GetTouch(0);
				Touch touch2 = Input.GetTouch(1);
				if (touch.position.x > num && touch2.position.x > num)
				{
					Vector2 vector = touch.position - touch.deltaPosition;
					Vector2 vector2 = touch2.position - touch2.deltaPosition;
					float magnitude = (vector - vector2).magnitude;
					float magnitude2 = (touch.position - touch2.position).magnitude;
					float num2 = magnitude - magnitude2;
					zoomSlider.GetComponent<Slider>().value = Mathf.Clamp(Camera.main.orthographicSize + num2 * orthoZoomSpeed, 50f, minZoom);
				}
			}
			else if (Input.touchCount == 1)
			{
				Touch touch3 = Input.GetTouch(0);
				if (touch3.position.x > num && touch3.phase == TouchPhase.Moved)
				{
					Vector2 vector3 = FixTouchDelta(touch3);
					if (vector3.magnitude > 5f || panning)
					{
						panning = true;
						PlanetSelect.panning = true;
						objectToLookAt = null;
						if (PlanetSelect.previousPlanetSelector != null)
						{
							PlanetSelect.previousPlanetSelector.SetActive(true);
						}
						Vector3 vector4 = Camera.main.ScreenToWorldPoint(Vector2.zero);
						Vector3 vector5 = Camera.main.ScreenToWorldPoint(Vector2.right);
						float num3 = Vector2.Distance(vector4, vector5);
						float num4 = (0f - vector3.x) * num3 * mouseSensitivity;
						float num5 = (0f - vector3.y) * num3 * mouseSensitivity;
						Vector2 vector6 = new Vector2(Mathf.Abs(base.transform.position.x + num4), Mathf.Abs(base.transform.position.y + num5));
						if (vector6.magnitude < playableRegionScale.x)
						{
							base.transform.Translate(num4, num5, 0f);
						}
						else if (base.transform.position.magnitude > playableRegionScale.x)
						{
							base.transform.position = base.transform.position.normalized * playableRegionScale.x;
						}
					}
				}
				else if (touch3.phase == TouchPhase.Ended)
				{
					panning = false;
				}
			}
		}
		if (objectToLookAt != null && !switchingToNewObject)
		{
			CenterInObject(new Vector2(objectToLookAt.transform.position.x, objectToLookAt.transform.position.y));
		}
	}

	public void MoveToNewObject(GameObject objectWhereNewCamPos)
	{
		GetComponent<AudioSource>().Play();
		objectToLookAt = null;
		switchingToNewObject = true;
		StartCoroutine("MoveCameraSmoothly", objectWhereNewCamPos);
	}

	private IEnumerator MoveCameraSmoothly(GameObject objectWhereNewCamPos)
	{
		float elapsedTime = 0f;
		SetXcorrectionFromSidebar();
		Vector3 fixedPositionWhileLerping = objectWhereNewCamPos.transform.position - new Vector3(xCorrectionFromSidebar, 0f, 500f);
		while (elapsedTime < cameraTranslationTime)
		{
			if (objectWhereNewCamPos != null)
			{
				fixedPositionWhileLerping = objectWhereNewCamPos.transform.position - new Vector3(xCorrectionFromSidebar, 0f, 500f);
			}
			Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, fixedPositionWhileLerping, elapsedTime / cameraTranslationTime);
			elapsedTime += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}
		switchingToNewObject = false;
		objectToLookAt = objectWhereNewCamPos;
	}

	public bool GetMovingToNewObject()
	{
		return switchingToNewObject;
	}

	public void SetCurrentPlayableRegionScale(Vector3 scale)
	{
		playableRegionScale = scale;
	}

	public static Vector2 FixTouchDelta(Touch aT)
	{
		float num = Time.deltaTime / aT.deltaTime;
		if (float.IsNaN(num) || float.IsInfinity(num))
		{
			num = 0.5f;
		}
		return aT.deltaPosition * num;
	}

	private void CenterInObject(Vector2 object2dCoords)
	{
		SetXcorrectionFromSidebar();
		base.transform.position = new Vector3(object2dCoords.x - xCorrectionFromSidebar, object2dCoords.y, -500f);
	}

	private void SetXcorrectionFromSidebar()
	{
		xCorrectionFromSidebar = Camera.main.ViewportToWorldPoint(new Vector3(gc.GetLeftSideBarRelativeWidth() / 2f, 0f, 0f)).x - Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x;
	}

	public float GetXcorrectionFromSidebar()
	{
		return xCorrectionFromSidebar;
	}

	public void setBarycenterImageAndPlanetSelectSize(float zoom)
	{
		float num = zoom / 15f;
		barycenterImage.transform.localScale = new Vector3(num, num, 1f);
		float num2 = 25f * Mathf.Pow(num, 0.5f);
		GameObject[] array = GameObject.FindGameObjectsWithTag("Planet");
		float num3 = 1f;
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			num3 = num2 / Mathf.Pow(gameObject.GetComponent<GravitationalObject>().GetSize(), 0.65f);
			gameObject.transform.Find("planetSelectBox").localScale = new Vector3(num3, 0.1f, num3);
		}
		float num4 = zoom / 200f;
		Barycenter.GetComponentInChildren<LineRenderer>().SetWidth(num4, num4);
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
