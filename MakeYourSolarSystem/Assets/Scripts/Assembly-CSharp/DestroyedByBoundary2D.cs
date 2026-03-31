using System.Collections;
using UnityEngine;

public class DestroyedByBoundary2D : MonoBehaviour
{
	private GameController gc;

	private void Start()
	{
		getGameController();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (base.gameObject.name == "Playable Region" && other.GetComponent<GravitationalObject>().initialObject)
		{
			other.GetComponent<GravitationalObject>().insidePlayableArea = true;
			StartCoroutine(AvoidPlanetDestructionAfterEntering(other.gameObject));
		}
	}

	private IEnumerator AvoidPlanetDestructionAfterEntering(GameObject planetToavoidDestroying)
	{
		planetToavoidDestroying.GetComponent<GravitationalObject>().SetCanBeDestroyedByBoundary(false);
		yield return new WaitForSeconds(0.5f);
		if (planetToavoidDestroying != null)
		{
			planetToavoidDestroying.GetComponent<GravitationalObject>().SetCanBeDestroyedByBoundary();
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (base.gameObject.name != "Playable Region" || other.GetComponent<GravitationalObject>().CanBeDestroyedByBoundary())
		{
			other.GetComponent<GravitationalObject>().insidePlayableArea = false;
			DestroyPlanet(other.gameObject);
		}
	}

	private void DestroyPlanet(GameObject planetToDestroy)
	{
		if (!gc.GetGameOver() && planetToDestroy.tag == "Planet" && gc.currentMapAddsScore())
		{
			gc.AddScorePoints(-gc.scoreLostOnPlanetLostFactor * gc.GetPlanetScore(planetToDestroy.GetComponent<Rigidbody2D>().mass));
		}
		GetComponent<AudioSource>().Play();
		planetToDestroy.GetComponent<CircleCollider2D>().enabled = false;
		planetToDestroy.gameObject.SendMessage("SetBeingDestroyed", true);
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
