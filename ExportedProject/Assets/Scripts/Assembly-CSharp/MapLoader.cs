using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapLoader : MonoBehaviour
{
	public int MaxObjects = 10;

	public float initialSliderMass = 0.215f;

	public float maximumMass = 0.5f;

	public float minimumMass;

	public float initialSliderZoom = 1000f;

	public float minimumZoom = 5000f;

	public int playableRegionRadius = 10000;

	public bool hasCountdown;

	public int time = 1;

	public bool addsScore;

	public int mapGravityFactor = 5;

	public GameObject massSlider;

	public GameObject zoomSlider;

	public bool isDestroyMode;

	public string[] planetNamesToDestroy;

	private GameObject[] planetsToDestroy;

	public bool isSupernovaMode;

	private bool supernovaHappened;

	public GameObject planet;

	public float initialPlanetMass = 10f;

	public float massIncrementFactor = 1.4f;

	public float maxPlanetMass = 750000f;

	public float initialPlanetCreationDelay = 10f;

	public float planetCreationTimeReductionFactor = 0.9f;

	public float planetCreationMinDelay = 0.5f;

	public int maxNumInitialObjects = 20;

	public bool symmetrical;

	public int initialMovementFactor = 40;

	public int minMovement;

	private GameController gc;

	private void Start()
	{
		getGameController();
		if (massSlider == null)
		{
			Transform massSliderTransform = transform.Find("MassSlider");
			if (massSliderTransform != null)
			{
				massSlider = massSliderTransform.gameObject;
			}
		}
		if (zoomSlider == null)
		{
			Transform zoomSliderTransform = transform.Find("CameraSlider");
			if (zoomSliderTransform != null)
			{
				zoomSlider = zoomSliderTransform.gameObject;
			}
		}
		if (massSlider != null)
		{
			Slider massSliderComponent = massSlider.GetComponent<Slider>();
			if (massSliderComponent != null)
			{
				massSliderComponent.maxValue = maximumMass;
				massSliderComponent.value = initialSliderMass;
				massSliderComponent.minValue = minimumMass;
			}
		}
		if (zoomSlider != null)
		{
			Slider zoomSliderComponent = zoomSlider.GetComponent<Slider>();
			if (zoomSliderComponent != null)
			{
				zoomSliderComponent.maxValue = minimumZoom;
				zoomSliderComponent.value = initialSliderZoom;
			}
		}
		if (isDestroyMode || isSupernovaMode)
		{
			int num = planetNamesToDestroy.Count();
			planetsToDestroy = new GameObject[num];
			for (int i = 0; i < num; i++)
			{
				planetsToDestroy[i] = base.transform.Find(planetNamesToDestroy[i]).gameObject;
			}
		}
		if (isDestroyMode)
		{
			StartCoroutine(DestroyerModEndCondition());
		}
		else if (isSupernovaMode)
		{
			StartCoroutine(SupernovaModeCreatePlanets());
			StartCoroutine(SupernovaModeGameOverCondition());
		}
	}

	private IEnumerator DestroyerModEndCondition()
	{
		yield return new WaitUntil(() => planetsToDestroy.Count((GameObject planet) => planet != null) == 0 || gc.GetGameOver());
		if (!gc.GetGameOver())
		{
			gc.GameOver();
		}
	}

	private IEnumerator SupernovaModeCreatePlanets()
	{
		yield return new WaitForSeconds(1f);
		while (!gc.GetGameOver())
		{
			Vector2 objectPosition = Random.insideUnitCircle.normalized * playableRegionRadius * 1.2f;
			GameObject newPlanet = Object.Instantiate(planet, objectPosition, base.transform.rotation) as GameObject;
			newPlanet.GetComponent<GravitationalObject>().initialObject = true;
			newPlanet.GetComponent<GravitationalObject>().insidePlayableArea = false;
			int impulseX = Random.Range(minMovement * 100, initialMovementFactor * 100);
			impulseX = ((Random.Range(0, 2) != 0) ? (-impulseX) : impulseX);
			int impulseY = Random.Range(minMovement * 100, initialMovementFactor * 100);
			impulseY = ((Random.Range(0, 2) != 0) ? (-impulseY) : impulseY);
			Vector2 objectInitialMovement = new Vector2(impulseX, impulseY);
			if (base.gameObject.name == "ModeCrashOfTheTitansSupernova(Clone)")
			{
				objectInitialMovement = objectInitialMovement + 150f * ((Vector2)(Quaternion.Euler(0f, 0f, 90f) * objectPosition.normalized) * (float)minMovement) - 150f * objectPosition.normalized * minMovement;
			}
			else if (base.gameObject.name == "ModeChaosSupernova(Clone)")
			{
				objectInitialMovement += (gc.GetNumberObjectsNotCreatedByPlayer() - 1) * 90 * ((Vector2)(Quaternion.Euler(0f, 0f, 90f) * objectPosition.normalized) * (float)minMovement);
				objectInitialMovement = ((Random.Range(0, 2) != 0) ? (-objectInitialMovement) : objectInitialMovement);
			}
			newPlanet.GetComponent<GravitationalObject>().initialMovement = objectInitialMovement;
			if (initialPlanetMass < maxPlanetMass)
			{
				newPlanet.GetComponent<Rigidbody2D>().mass = initialPlanetMass;
				initialPlanetMass *= massIncrementFactor;
			}
			else
			{
				newPlanet.GetComponent<Rigidbody2D>().mass = maxPlanetMass;
			}
			if (symmetrical)
			{
				GameObject secondPlanet = Object.Instantiate(planet, -objectPosition, base.transform.rotation) as GameObject;
				secondPlanet.GetComponent<GravitationalObject>().initialObject = true;
				secondPlanet.GetComponent<GravitationalObject>().initialMovement = -objectInitialMovement;
				if (base.gameObject.name == "ModeCrashOfTheTitansSupernova(Clone)")
				{
					secondPlanet.GetComponent<GravitationalObject>().initialMovement = -Vector2.Reflect(objectInitialMovement, Quaternion.Euler(0f, 0f, 90f) * objectPosition.normalized);
				}
				secondPlanet.GetComponent<Rigidbody2D>().mass = newPlanet.GetComponent<Rigidbody2D>().mass;
			}
			if (gc.GetNumberObjectsNotCreatedByPlayer() < maxNumInitialObjects)
			{
				if (initialPlanetCreationDelay > planetCreationMinDelay)
				{
					yield return new WaitForSeconds(initialPlanetCreationDelay);
					initialPlanetCreationDelay *= planetCreationTimeReductionFactor;
				}
				else
				{
					yield return new WaitForSeconds(planetCreationMinDelay);
				}
			}
			else
			{
				initialPlanetCreationDelay *= 2f;
				yield return new WaitForSeconds(initialPlanetCreationDelay);
			}
		}
	}

	public void SetSupernovaHappened(bool nova = true)
	{
		supernovaHappened = nova;
	}

	private IEnumerator SupernovaModeGameOverCondition()
	{
		yield return new WaitUntil(() => supernovaHappened || planetsToDestroy.Count((GameObject planet) => planet == null) > 0 || gc.GetGameOver());
		if (!gc.GetGameOver())
		{
			gc.GameOver();
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
