using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public class GravitationalObject : MonoBehaviour
{
	public bool insidePlayableArea = true;

	public bool initialObject;

	public bool randomTexture;

	private int numTexture;

	public Vector2 initialMovement;

	public bool hasRandomRotation = true;

	public bool addsScore;

	public GameObject supernovaEffect;

	public GameObject collision;

	public GameObject collision2;

	private float size = 1f;

	public GameObject auxStar;

	private Color planetColor;

	private Color starColor;

	private float arrowLength = 20f;

	private bool imBeingDestroyed;

	private bool itWasPlanet;

	private bool canBeDestroyedByBoundary = true;

	private static int maxPlanetMass = 5000;

	private static int minFusionStarMass = 16000;

	public static int maxStarMass = 800000;

	public static float blackHoleMinMass = 942324.4f;

	public string objectType;

	private bool pulsates;

	private float starPulseTimer;

	private float pulsationAmount;

	private float pulsationSpeedSeconds;

	private AudioSource crashyExplosion;

	private AudioSource lowPitchFastExplosion;

	private AudioSource lowPitchSlowExplosion;

	private AudioSource objectOutOfSpace;

	private static int trailSizeFactor = 50;

	private static int trailOpacityFactor = 90;

	public Texture2D lightSpriteForSupernova;

	public GameObject temperature;

	private GameController gc;

	private Vector3 pos;

	private void Awake()
	{
		if (!initialObject)
		{
			pos = Camera.main.WorldToScreenPoint(base.transform.position);
		}
	}

	private void Start()
	{
		if (!initialObject)
		{
			base.transform.position = Camera.main.ScreenToWorldPoint(pos);
		}
		getGameController();
		if (gc == null)
		{
			enabled = false;
			return;
		}
		InitializeSounds();
		if (objectType != "neutron" && objectType != "blackHole")
		{
			objectType = string.Empty;
		}
		float mass = GetComponent<Rigidbody2D>().mass;
		Vector2 velocity = gc.GetBarycenter2D() / Time.fixedDeltaTime;
		GameObject objectToLookAt = Camera.main.GetComponent<CameraControl>().objectToLookAt;
		if (objectToLookAt == null)
		{
			GetComponent<Rigidbody2D>().velocity = velocity;
		}
		else if (objectToLookAt.tag == "Barycenter")
		{
			GetComponent<Rigidbody2D>().velocity = velocity;
		}
		else
		{
			GetComponent<Rigidbody2D>().velocity = objectToLookAt.GetComponent<Rigidbody2D>().velocity;
		}
		GetComponent<Rigidbody2D>().AddForce(initialMovement * mass);
		GameObject surface = GetSurface();
		if (surface == null)
		{
			Debug.LogWarning("GravitationalObject is missing surface object.");
			enabled = false;
			return;
		}
		setSize();
		planetColor = Color.gray;
		if ((initialMovement.magnitude > 200000f && size < 25f) || (initialMovement.magnitude > 500000f && size < 75f) || initialMovement.magnitude > 2000000f)
		{
			GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		}
		if (mass < (float)maxPlanetMass)
		{
			itWasPlanet = true;
		}
		if (mass < (float)minFusionStarMass)
		{
			if (randomTexture)
			{
				numTexture = UnityEngine.Random.Range(0, gc.GetNumPlanetTextures());
				surface.GetComponent<Renderer>().material.mainTexture = gc.GetPlanetTexture(numTexture);
			}
			else
			{
					Renderer surfaceRenderer = surface.GetComponent<Renderer>();
					string textureName = (surfaceRenderer != null && surfaceRenderer.material != null && surfaceRenderer.material.mainTexture != null) ? surfaceRenderer.material.mainTexture.name : string.Empty;
					string value = Regex.Match(textureName, "\\d+").Value;
				if (value != string.Empty)
				{
					numTexture = int.Parse(value);
				}
			}
			if (hasRandomRotation)
			{
				surface.transform.rotation = UnityEngine.Random.rotation;
			}
			planetColor = gc.GetColorForPlanet(numTexture);
		}
		if (mass > (float)maxPlanetMass)
		{
			SetObjectSurface(mass);
			if (objectType == "star")
			{
				ResizeStarCrown();
			}
		}
		SetSelectBoxScale();
		if (!initialObject)
		{
			gc.SetNumberObjects(1);
		}
		if (gc.minimalistic)
		{
			surface.GetComponent<MeshRenderer>().enabled = false;
			if (objectType != string.Empty)
			{
				base.transform.Find("starSurface").GetComponent<MeshRenderer>().enabled = false;
			}
		}
		else
		{
			base.transform.Find("Surface2D").GetComponent<MeshRenderer>().enabled = false;
		}
		gc.ClearAllTrailsTwice();
		StartCoroutine(delayedColorUpdate());
	}

	private void InitializeSounds()
	{
		AudioSource[] componentsInChildren = GetComponentsInChildren<AudioSource>();
		if (componentsInChildren.Length > 0)
		{
			crashyExplosion = componentsInChildren[0];
			crashyExplosion.volume = 0.3f * gc.GetSoundVolume();
		}
		if (componentsInChildren.Length > 1)
		{
			lowPitchFastExplosion = componentsInChildren[1];
			lowPitchFastExplosion.volume = 0.6f * gc.GetSoundVolume();
		}
		if (componentsInChildren.Length > 2)
		{
			lowPitchSlowExplosion = componentsInChildren[2];
			lowPitchSlowExplosion.volume = gc.GetSoundVolume();
		}
		if (componentsInChildren.Length > 3)
		{
			objectOutOfSpace = componentsInChildren[3];
			objectOutOfSpace.volume = gc.GetSoundVolume();
		}
	}

	public IEnumerator delayedColorUpdate()
	{
		yield return null;
		TrailRenderer trail = null;
		MeshRenderer surface2d = base.transform.Find("Surface2D").GetComponent<MeshRenderer>();
		Color color = starColor;
		if (objectType == "brownDwarf" || objectType == string.Empty)
		{
			trail = base.transform.Find("Surface").GetComponent<TrailRenderer>();
			trail.sortingOrder = -2 - (int)size;
			setTrailWidth(trail);
			Color trailColor = planetColor;
			trailColor.a = (float)trailOpacityFactor / 100f;
			trail.material.SetColor("_Color", trailColor);
			trail.time = ((!PlayerPrefs.HasKey("TrailLength")) ? 5 : PlayerPrefs.GetInt("TrailLength"));
			GetComponent<LineRenderer>().SetColors(planetColor, planetColor);
			surface2d.material.color = (2f * planetColor + Color.gray) / 3f;
		}
		if (objectType != string.Empty)
		{
			trail = base.transform.Find("starSurface").GetComponent<TrailRenderer>();
			trail.sortingOrder = -1 - (int)size;
			setTrailWidth(trail);
			if (objectType == "brownDwarf")
			{
				color.a = 1f - GetBrownDwarfPlanetAlpha(GetComponent<Rigidbody2D>().mass);
				GetComponent<LineRenderer>().SetColors(starColor, (planetColor + starColor) / 2f);
				surface2d.material.color = (planetColor + starColor) / 2f;
			}
			else
			{
				GetComponent<LineRenderer>().SetColors(starColor, starColor);
				surface2d.material.color = ((!(objectType == "star")) ? starColor : ((7f * starColor + Color.white) / 8f));
			}
			Color trailColor = color;
			trailColor.a = (float)trailOpacityFactor / 100f;
			trail.material.SetColor("_Color", trailColor);
		}
		if (objectType == "star")
		{
			GameObject starParticles = base.transform.Find("Crown").gameObject;
			starParticles.GetComponent<ParticleSystem>().startColor = starColor;
			foreach (Transform flow in starParticles.transform)
			{
				flow.GetComponent<ParticleSystem>().startColor = starColor;
			}
		}
		if (objectType != string.Empty)
		{
			base.transform.Find("starSurface").GetComponent<MeshRenderer>().materials[0].color = starColor;
			if (objectType == "star")
			{
				Light starLight = base.transform.Find("StarLight").GetComponent<Light>();
				starLight.intensity = 0.008139535f * size - 0.7f;
				starLight.range = starLight.intensity * 3000f;
				starLight.color = starColor;
			}
			else if (objectType == "neutron")
			{
				Light starLight2 = base.transform.Find("StarLight").GetComponent<Light>();
				starLight2.intensity = 0.015116279f * size + 1f;
				starLight2.range = starLight2.intensity * 1000f;
				starLight2.color = (starColor + Color.red) / 2f;
			}
			SpriteRenderer lightSprite = base.transform.Find("LightSprite").GetComponent<SpriteRenderer>();
			if (objectType == "neutron" || objectType == "blackHole")
			{
				lightSprite.color = (starColor + Color.red) / 2f;
			}
			else
			{
				lightSprite.color = (color * 2f + ((!gc.minimalistic) ? Color.clear : color)) / 3f;
			}
		}
	}

	private void Update()
	{
		LineRenderer componentInChildren = GetComponentInChildren<LineRenderer>();
		float num = (float)Screen.width * gc.GetLeftSideBarRelativeWidth();
		Vector2 vector = Camera.main.WorldToScreenPoint(base.transform.position);
		if (vector.x > 0f + num && vector.x < (float)Screen.width && vector.y > 0f && vector.y < (float)Screen.height)
		{
			componentInChildren.enabled = false;
			return;
		}
		Vector2 vector2 = new Vector2(((float)Screen.width - num) / 2f + num, Screen.height / 2);
		Vector2 vector3 = vector - vector2;
		vector3.x *= -1f;
		float num2 = Mathf.Atan2(vector3.y, vector3.x);
		num2 -= (float)Math.PI / 2f;
		float num3 = Mathf.Cos(num2);
		float num4 = Mathf.Sin(num2);
		vector3 = vector2 + new Vector2(num4 * 150f, num3 * 150f);
		float num5 = num3 / num4;
		Vector3 vector4 = new Vector2(((float)Screen.width - num) / 2f, Screen.height / 2) * 0.99f;
		vector3 = ((!(num3 > 0f)) ? ((Vector2)new Vector3((0f - vector4.y) / num5, 0f - vector4.y, 0f)) : ((Vector2)new Vector3(vector4.y / num5, vector4.y, 0f)));
		if (vector3.x > vector4.x)
		{
			vector3 = new Vector3(vector4.x, vector4.x * num5, 0f);
		}
		else if (vector3.x < 0f - vector4.x)
		{
			vector3 = new Vector3(0f - vector4.x, (0f - vector4.x) * num5, 0f);
		}
		vector3 += vector2;
		componentInChildren.enabled = true;
		vector3 = gc.ScreenPointToWorld(vector3);
		componentInChildren.SetPosition(0, vector3);
		arrowLength = (size / 4f + 20f) * Camera.main.orthographicSize / 1000f;
		Vector3 position = vector3 - (vector3 - (Vector2)Camera.main.ScreenToWorldPoint(vector2)).normalized * arrowLength;
		componentInChildren.SetPosition(1, position);
		componentInChildren.SetWidth(0f, arrowLength / 2f);
	}

	private Color32 getStarColor(float mass)
	{
		Texture2D texture2D = Resources.Load<Texture2D>("Textures/StarsColors");
		int x = (int)(-1.4f * Mathf.Pow(10f, -10f) * Mathf.Pow(mass, 2f) + 3.87676f * Mathf.Pow(10f, -4f) * mass - 2f);
		return texture2D.GetPixel(x, 0);
	}

	private void OnCollisionEnter2D(Collision2D otherPlanet)
	{
		if (!imBeingDestroyed && !otherPlanet.gameObject.GetComponent<GravitationalObject>().GetBeingDestroyed())
		{
			PlanetCollision(otherPlanet);
		}
	}

	private void OnCollisionStay2D(Collision2D otherPlanet)
	{
		if (!imBeingDestroyed && !otherPlanet.gameObject.GetComponent<GravitationalObject>().GetBeingDestroyed())
		{
			PlanetCollision(otherPlanet);
		}
	}

	private void PlanetCollision(Collision2D otherPlanet)
	{
		Rigidbody2D component = GetComponent<Rigidbody2D>();
		Rigidbody2D component2 = otherPlanet.gameObject.GetComponent<Rigidbody2D>();
		float mass = component.mass;
		float mass2 = component2.mass;
		GravitationalObject component3 = otherPlanet.gameObject.GetComponent<GravitationalObject>();
		if (!(mass > mass2) && (mass != mass2 || !(component.velocity.magnitude > component2.velocity.magnitude)) && (!(objectType == "neutron") || !(component3.objectType != "neutron") || !(component3.objectType != "blackHole")))
		{
			return;
		}
		GameObject surface = GetSurface();
		TrailRenderer component4 = surface.GetComponent<TrailRenderer>();
		component.mass += ((!(objectType == "star") && !(objectType == "neutron") && !(objectType == "blackHole")) ? (mass2 / 2f) : mass2);
		mass = component.mass;
		SetObjectSurface(mass);
		mass = component.mass;
		setSize();
		if (objectType == "star")
		{
			ResizeStarCrown();
		}
		if (objectType != string.Empty)
		{
			starColor = getStarColor(mass);
			StartCoroutine(delayedColorUpdate());
		}
		setTrailWidth(component4);
		lowPitchFastExplosion.Play();
		if ((objectType == "star" || objectType == "neutron") && otherPlanet.gameObject.GetComponent<GravitationalObject>().GetObjectType() != string.Empty)
		{
			lowPitchSlowExplosion.Play();
		}
		else if (objectType == "blackHole")
		{
			objectOutOfSpace.Play();
		}
		else
		{
			crashyExplosion.Play();
		}
		if (!gc.minimalistic)
		{
			if (objectType != "blackHole" && mass2 < blackHoleMinMass)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(collision, new Vector3(otherPlanet.contacts[0].point.x, otherPlanet.contacts[0].point.y, collision.transform.position.z), Quaternion.LookRotation((otherPlanet.transform.position - base.transform.position).normalized)) as GameObject;
				gameObject.transform.parent = base.transform;
				ParticleSystem component5 = gameObject.GetComponent<ParticleSystem>();
				component5.startSpeed += otherPlanet.relativeVelocity.magnitude / 20f + Mathf.Abs(component.angularVelocity) / 2f;
				component5.startColor = (otherPlanet.gameObject.GetComponent<GravitationalObject>().GetPlanetColor() * 2f + Color.white + GetPlanetColor()) / 4f;
				component5.startSize = otherPlanet.gameObject.GetComponent<GravitationalObject>().GetSize() / 2f;
				UnityEngine.Object.Destroy(gameObject, component5.startLifetime);
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(collision2, new Vector3(otherPlanet.contacts[0].point.x, otherPlanet.contacts[0].point.y, collision2.transform.position.z), Quaternion.LookRotation((base.transform.position - otherPlanet.transform.position).normalized)) as GameObject;
			gameObject2.transform.parent = base.transform;
			ParticleSystem component6 = gameObject2.GetComponent<ParticleSystem>();
			component6.startSpeed = otherPlanet.relativeVelocity.magnitude / 10f;
			component6.startColor = (otherPlanet.gameObject.GetComponent<GravitationalObject>().GetPlanetColor() * 2f + Color.white + GetPlanetColor()) / 4f;
			component6.startSize = otherPlanet.gameObject.GetComponent<GravitationalObject>().GetSize() / 2f;
			UnityEngine.Object.Destroy(gameObject2, component6.startLifetime);
		}
		if (!gc.GetGameOver() && gc.currentMapAddsScore())
		{
			gc.AddScorePoints(-gc.scoreLostOnImpactFactor * gc.GetPlanetScore(component2.mass));
		}
		SetSelectBoxScale();

		// Google Play Games achievements removed entirely
		GetComponent<Rigidbody2D>().velocity = (component.velocity * mass + component2.velocity * mass2) / (mass + mass2);
		UnityEngine.Object.Destroy(otherPlanet.gameObject);
	}

	public IEnumerator SetBeingDestroyed(bool beingDestroyed = true)
	{
		imBeingDestroyed = beingDestroyed;
		addsScore = false;
		GetComponent<Rigidbody2D>().mass = 0f;
		MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
		TrailRenderer[] trails = GetComponentsInChildren<TrailRenderer>();
		SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
		MeshRenderer[] array = meshes;
		foreach (MeshRenderer mesh in array)
		{
			mesh.enabled = false;
		}
		TrailRenderer[] array2 = trails;
		foreach (TrailRenderer trail in array2)
		{
			trail.enabled = false;
		}
		SpriteRenderer[] array3 = sprites;
		foreach (SpriteRenderer sprite in array3)
		{
			sprite.enabled = false;
		}
		for (int l = 0; l < 6; l++)
		{
			yield return new WaitForFixedUpdate();
		}
		UnityEngine.Object.Destroy(base.gameObject);
		gc.ClearAllTrailsTwice();
	}

	public bool GetBeingDestroyed()
	{
		return imBeingDestroyed;
	}

	private void SetSelectBoxScale()
	{
		float f = Camera.main.orthographicSize / 20f;
		float num = 15f * Mathf.Pow(f, 0.5f);
		num /= Mathf.Pow(size, 0.65f);
		base.transform.Find("planetSelectBox").localScale = new Vector3(num, 0.1f, num);
	}

	private void ResizeStarCrown()
	{
		GameObject gameObject = base.transform.Find("Crown").gameObject;
		gameObject.GetComponent<ParticleSystem>().startSize = size;
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.GetComponent<ParticleSystem>().startSize = 2.5f * size;
		}
	}

	private void SetObjectSurface(float mass)
	{
		if (auxStar == null)
		{
			return;
		}
		if (objectType != "brownDwarf" && mass < (float)minFusionStarMass && mass > (float)maxPlanetMass)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(auxStar, base.transform.position, base.transform.rotation) as GameObject;
			gameObject.transform.localScale = base.transform.localScale;
			Transform transform = gameObject.transform.Find("starSurface");
			transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 359), 0f);
			transform.parent = base.transform;
			gameObject.transform.Find("LightSprite").parent = base.transform;
			UnityEngine.Object.Destroy(gameObject);
			objectType = "brownDwarf";
		}
		if (objectType == "brownDwarf" && mass < (float)minFusionStarMass && mass > (float)maxPlanetMass)
		{
			base.transform.Find("Surface").GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, GetBrownDwarfPlanetAlpha(mass));
		}
		if ((objectType == string.Empty || objectType == "brownDwarf") && mass >= (float)minFusionStarMass)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(auxStar, base.transform.position, base.transform.rotation) as GameObject;
			gameObject2.transform.localScale = base.transform.localScale;
			if (objectType == string.Empty)
			{
				Transform transform2 = gameObject2.transform.Find("starSurface");
				transform2.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 359), 0f);
				transform2.parent = base.transform;
				gameObject2.transform.Find("LightSprite").parent = base.transform;
			}
			UnityEngine.Object.Destroy(base.transform.Find("Surface").gameObject);
			gameObject2.transform.Find("Crown").parent = base.transform;
			gameObject2.transform.Find("StarLight").parent = base.transform;
			if (gc.minimalistic)
			{
				base.transform.Find("Crown").gameObject.SetActive(false);
				base.transform.Find("StarLight").gameObject.SetActive(false);
			}
			UnityEngine.Object.Destroy(gameObject2);
			objectType = "star";
			setSize();
			// Google Play Games achievement removed
		}
		if (objectType == "star" && !pulsates && size >= 500f)
		{
			pulsationAmount = Mathf.Pow(size / 490f, 10f) * 15f;
			pulsationSpeedSeconds = Mathf.Pow(510f / size, 10f);
			StartCoroutine(StarPulsation());
			pulsates = true;
			// Google Play Games achievement removed
		}
		if (objectType == "star" && mass >= (float)maxStarMass)
		{
			gc.GetCurrentMap().GetComponent<MapLoader>().SetSupernovaHappened();
			GameObject gameObject3 = UnityEngine.Object.Instantiate(supernovaEffect, new Vector3(base.transform.position.x, base.transform.position.y, supernovaEffect.transform.position.z), UnityEngine.Random.rotation) as GameObject;
			if (gc.minimalistic)
			{
				gameObject3.GetComponent<Renderer>().material.mainTexture = lightSpriteForSupernova;
			}
			UnityEngine.Object.Destroy(gameObject3, 10f);
			// Google Play Games achievements removed
			GetComponent<Rigidbody2D>().mass /= 2f;
			mass = GetComponent<Rigidbody2D>().mass;
			GetComponent<Rigidbody2D>().angularVelocity *= 25f;
			GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
			UnityEngine.Object.Destroy(base.transform.Find("Crown").gameObject);
			base.transform.Find("LightSprite").transform.localPosition = Vector3.zero;
			base.transform.Find("LightSprite").transform.localScale = new Vector3(0.35f, 0.35f);
			base.transform.Find("starSurface").GetComponent<sun_luminance>().enabled = false;
			base.transform.Find("starSurface").GetComponent<Renderer>().material = Resources.Load("Materials/neutron") as Material;
			objectType = "neutron";
			setSize();
		}
		if (objectType == "neutron" && mass >= blackHoleMinMass)
		{
			UnityEngine.Object.Destroy(base.transform.Find("StarLight").gameObject);
			objectType = "blackHole";
			setSize();
			// Google Play Games achievement removed
		}
		starColor = getStarColor(mass);
		delayedColorUpdate();
	}

	private void OnDestroy()
	{
		if (gc != null && !initialObject)
		{
			gc.SetNumberObjects(-1);
		}
	}

	public GameObject GetSurface()
	{
		Transform transform = base.transform.Find("Surface");
		if (transform == null)
		{
			transform = base.transform.Find("starSurface");
		}
		if (transform == null)
		{
			return null;
		}
		return transform.gameObject;
	}

	private Color GetPlanetColor()
	{
		if (objectType == string.Empty)
		{
			return planetColor;
		}
		return starColor;
	}

	private float GetBrownDwarfPlanetAlpha(float mass)
	{
		return -0.0001f * mass + 1.45f;
	}

	private IEnumerator StarPulsation()
	{
		starPulseTimer = 0f;
		float pulsationRadius = size + pulsationAmount;
		Vector3 pulseSize = base.transform.localScale;
		while (objectType == "star")
		{
			if (starPulseTimer > pulsationSpeedSeconds)
			{
				pulsationAmount = 0f - pulsationAmount;
				pulsationRadius = size + pulsationAmount;
				pulseSize = new Vector3(pulsationRadius, pulsationRadius, pulsationRadius);
				starPulseTimer = 0f;
			}
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, pulseSize, Time.deltaTime);
			starPulseTimer += Time.deltaTime;
			yield return null;
		}
	}

	private void setSize()
	{
		float massForSize = GetComponent<Rigidbody2D>().mass;
		float massFactor = Mathf.Max(gc.planetMassFactor, 1);
		float safeMass = Mathf.Max(massForSize * massFactor, 0.0001f);
		size = Mathf.Log10(safeMass);
		size = Mathf.Pow(size, 3f);
		size /= 16f;
		size += 1.3f;
		size *= 7f;
		if (objectType == "star")
		{
			size += ((!(size - 175f > 0f)) ? 0f : (2f * (size - 175f)));
		}
		if (objectType == "neutron")
		{
			size /= 4f;
		}
		else if (objectType == "blackHole")
		{
			size /= 5f;
		}
		size = Mathf.Max(size, 0.1f);
		Vector3 localScale = new Vector3(size, size, size);
		base.transform.localScale = localScale;
		Transform transform = base.transform.Find("Surface");
		if (transform != null)
		{
			transform.localScale = new Vector3(1.02f, 1.02f, 1.02f);
		}
	}

	public float GetSize()
	{
		return size;
	}

	public void SetTrailSizeFactor(float newFactor)
	{
		trailSizeFactor = (int)newFactor;
	}

	public int GetTrailSizeFactor()
	{
		return trailSizeFactor;
	}

	public void SetTrailOpacityFactor(float newFactor)
	{
		trailOpacityFactor = (int)newFactor;
	}

	public int GetTrailOpacityFactor()
	{
		return trailOpacityFactor;
	}

	public void setTrailWidth(TrailRenderer trail)
	{
		trail.startWidth = Mathf.Clamp(size * (float)trailSizeFactor / 100f, 0.5f, size);
		trail.endWidth = Mathf.Max(size * (float)trailSizeFactor / 100f, 5f);
	}

	public void setMass(float baseMass)
	{
		baseMass = (0f - baseMass) * (baseMass - 2f);
		baseMass *= 10f;
		GetComponent<Rigidbody2D>().mass = Mathf.Pow(10f, baseMass - 4f);
	}

	public string GetObjectType()
	{
		return objectType;
	}

	public bool CanBeDestroyedByBoundary()
	{
		return canBeDestroyedByBoundary;
	}

	public void SetCanBeDestroyedByBoundary(bool canBeDestroyed = true)
	{
		canBeDestroyedByBoundary = canBeDestroyed;
	}

	public int GetNumTexture()
	{
		return numTexture;
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
