using System;
using System.Collections;
using System.IO;
using SmartLocalization;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
	private const float G = 6.674f;

	public GameObject TopMessagePanel;

	private bool thereIsNewMessageToDisplay;

	public GameObject leftSidebar;

	public GameObject leftSidebarForSliders;

	public GameObject leftPanelCover;

	public GameObject MapSelectorPanel;

	public GameObject pausePanel;

	public GameObject howToPlayPanel;

	public GameObject aboutPanel;

	public GameObject gameoverPanel;

	public GameObject ScorePanel;

	public GameObject NumberObjectsPanel;

	public GameObject TimePanel;

	public GameObject maps;

	private GameObject currentMap;

	public GameObject currentSystemSandboxMap;

	private string mapNameToPlay = string.Empty;

	public Text mapDescription;

	private Text previousMapButtonText;

	private string previousMapTitle = string.Empty;

	private string currentMode = "timeAttack";

	private int currentTotalObjectsNumber;

	public GameObject drawSpeedVector;

	public GameObject planetPrefab;

	public GameObject ultraDensePrefab;

	private bool creatingPlanet;

	private bool canCreatePlanetAfterCoolDown = true;

	private Vector2 planetOriginPoint;

	private Vector2 planetSpeedPoint;

	public int planetSpeedFactorOnCreate;

	public float planetSpeedPowOnCreate;

	public int planetMassFactor;

	private int gravityFactor;

	private float totalMassOfSystem;

	public GameObject barycenter;

	private Vector2 barycenter2D;

	public bool centerObjectsInWorld;

	private bool gamePaused;

	private bool gameOver;

	public GameObject timerText;

	private int secondsForGameOver;

	public int secondsInTimerMods;

	public GameObject scoreText;

	public GameObject plusScoreText;

	public GameObject minusScoreText;

	private int score;

	public int scoreDivider;

	public int scoringRate;

	public int scoreLostOnImpactFactor;

	public int scoreLostOnPlanetLostFactor;

	private GPGSCalls gpgs;

	private bool triedToLogInGoogleGames;

	private bool isLeaderboardLoaded;

	private bool noInternetOrLoggedIntoGoogleGamesMessage;

	private Text[] ranksText = new Text[25];

	private Text[] playerNamesText = new Text[25];

	private Text[] scoresText = new Text[25];

	public GameObject GameOverPanel;

	public GameObject ScoresUI;

	public Text statusText;

	private int currentPlayerPosition = -1;

	public AudioClip[] musicClips;

	private AudioSource cantCreatePlanetSound;

	private Texture2D[] planetTextures;

	private int numPlanetTextures;

	private Color32[] planetFlatColors;

	public string activeAction;

	private bool canShareScreenshot = true;

	private bool canStepShareAchievement = true;

	private int maxPlayerScoreForCurrentMode = -1;

	private bool isAppInBackground;

	private bool showInterstitialGoingToMenu;

	private bool goingToShowInterstitial;

	private DateTime adOpenedLastDateTime;

	public GameObject optionsUI;

	public Text optionDescription;

	public bool minimalistic;

	public GameObject frameMinimalistic;

	public GameObject frameRealistic;

	public Material flatBackground;

	public Material realisticBackground;

	public GameObject visibleFrontier;

	public Slider[] optionsSliders;

	private float soundVolume;

	private bool playingSoundSampleForConfig;

	public AudioSource supernovaSound;

	public AudioSource menuClickSound;

	public GameObject optionButtonLoginGG;

	public GameObject socialContent;

	private string langCode = string.Empty;

	public GameObject tempAux;

	public GameObject saveButton;

	public GameObject deleteSavedGameButton;

	public GameObject confirmationPanel;

	private bool? isActionConfirmed;

	private GameObject savedMaps;

	private GameObject newSavedMapPrototype;

	private GameObject savedGamesRowPrototype;

	private float scrollableHeight;

	private RectTransform scrollable;

	private int amountToTranslateNewRows;

	private bool savingSystem;

	private int fileNumberToSave;

	private Vector2 partialForce;

	private void Awake()
	{
		ColorsForPlanetTextures();
	}

	private void Start()
	{
		Screen.sleepTimeout = -1;
		cantCreatePlanetSound = GetComponents<AudioSource>()[1];
		StartCoroutine(LoopMusic());
		gameOver = true;
		LoadPlayerPrefs();
		if (langCode == string.Empty)
		{
			SystemLanguage systemLanguage = Application.systemLanguage;
			if (systemLanguage == SystemLanguage.Spanish)
			{
				langCode = "es";
			}
			else
			{
				langCode = "en";
			}
		}
		LanguageManager.Instance.ChangeLanguage(langCode);
		optionsUI.transform.Find("OptionsPanel/Visuals Scrollable/Scrollable content/Visuals/" + langCode + "_Button/Frame_" + langCode).gameObject.SetActive(true);
		optionDescription.text = LanguageManager.Instance.GetTextValue("OptionDescription");
		StartCoroutine("DisplayNewTopMessage");
		StartCoroutine("GooglePlayGamesAPI");
		Time.timeScale = 0f;
		LoadSavedSystemsForSandbox();
		LoadNewObjectsFromMap("mapStart");
		Time.timeScale = 1f;
		mapDescription.text = LanguageManager.Instance.GetTextValue("Welcome");
		for (int i = 0; i < 25; i++)
		{
			ranksText[i] = ScoresUI.transform.Find("Rank" + (i + 1)).GetComponent<Text>();
			playerNamesText[i] = ScoresUI.transform.Find("PlayerName" + (i + 1)).GetComponent<Text>();
			scoresText[i] = ScoresUI.transform.Find("Score" + (i + 1)).GetComponent<Text>();
		}
		adOpenedLastDateTime = DateTime.MinValue;
		RequestBanner();
		StartCoroutine(ShowInterstitial());
	}

	private void LoadPlayerPrefs()
	{
		if (PlayerPrefs.HasKey("Minimalistic") && PlayerPrefs.GetInt("Minimalistic") == 1)
		{
			minimalistic = true;
		}
		frameRealistic.SetActive(!minimalistic);
		frameMinimalistic.SetActive(minimalistic);
		GameObject.Find("GameBackground").GetComponent<MeshRenderer>().material = ((!minimalistic) ? realisticBackground : flatBackground);
		optionsSliders[0].value = ((!PlayerPrefs.HasKey("TrailLength")) ? 5 : PlayerPrefs.GetInt("TrailLength"));
		optionsSliders[1].value = ((!PlayerPrefs.HasKey("TrailSize")) ? 50 : PlayerPrefs.GetInt("TrailSize"));
		optionsSliders[2].value = ((!PlayerPrefs.HasKey("TrailOpacity")) ? 50 : PlayerPrefs.GetInt("TrailOpacity"));
		optionsSliders[3].value = ((!PlayerPrefs.HasKey("FrontierOpacity")) ? 100 : PlayerPrefs.GetInt("FrontierOpacity"));
		optionsSliders[4].value = (float)((!PlayerPrefs.HasKey("SoundVolume")) ? 80 : PlayerPrefs.GetInt("SoundVolume")) / 100f;
		optionsSliders[5].value = (float)((!PlayerPrefs.HasKey("MusicVolume")) ? 20 : PlayerPrefs.GetInt("MusicVolume")) / 100f;
		langCode = ((!PlayerPrefs.HasKey("LangCode")) ? string.Empty : PlayerPrefs.GetString("LangCode"));
	}

	public void ChangeLanguage(string languageCode)
	{
		LanguageManager.Instance.ChangeLanguage(languageCode);
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(translationUI));
		for (int i = 0; i < array.Length; i++)
		{
			translationUI translationUI2 = (translationUI)array[i];
			translationUI2.TranslateUItext();
		}
		UnityEngine.Object[] array2 = Resources.FindObjectsOfTypeAll(typeof(Slider));
		for (int j = 0; j < array2.Length; j++)
		{
			Slider slider = (Slider)array2[j];
			float value = slider.value;
			slider.value = value + 0.01f;
			slider.value = value - 0.01f;
			slider.value = value;
		}
		if (NumberObjectsPanel.transform.Find("NumberObjectsText") != null)
		{
			NumberObjectsPanel.transform.Find("NumberObjectsText").GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("Objects") + " " + currentTotalObjectsNumber + "/" + currentMap.GetComponent<MapLoader>().MaxObjects;
		}
		if (timerText != null)
		{
			string empty = string.Empty;
			empty = ((!(currentMode == "timeAttack")) ? GetFormatedStringFromMiliseconds(secondsInTimerMods * 10) : GetFormatedStringFromMiliseconds((secondsForGameOver + 1) * 1000, false));
			timerText.GetComponent<Text>().text = string.Format(LanguageManager.Instance.GetTextValue("Time"), empty);
		}
		if (scoreText != null)
		{
			scoreText.GetComponent<Text>().text = string.Format(LanguageManager.Instance.GetTextValue("Score"), score);
		}
		optionDescription.text = LanguageManager.Instance.GetTextValue("VisualsOptionDescription");
		mapDescription.text = LanguageManager.Instance.GetTextValue("Welcome");
		ResetMapSelectionButton();
		Text[] componentsInChildren = savedMaps.GetComponentsInChildren<Text>(true);
		foreach (Text text in componentsInChildren)
		{
			string oldValue = text.text.Split('\n')[0];
			text.text = text.text.Replace(oldValue, LanguageManager.Instance.GetTextValue("Game"));
		}
		PlayerPrefs.SetString("LangCode", languageCode);
	}

	private void LoadSavedSystemsForSandbox()
	{
		amountToTranslateNewRows = -150;
		if (newSavedMapPrototype == null)
		{
			newSavedMapPrototype = UnityEngine.Object.Instantiate(currentSystemSandboxMap);
		}
		if (savedGamesRowPrototype == null)
		{
			savedGamesRowPrototype = GameObject.Find("SavedMapsPrototype");
			savedGamesRowPrototype.SetActive(false);
		}
		deleteSavedGameButton.transform.SetParent(savedGamesRowPrototype.transform.parent, false);
		deleteSavedGameButton.SetActive(false);
		if (savedMaps == null)
		{
			savedMaps = GameObject.Find("SavedMaps");
		}
		foreach (Transform item in savedMaps.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in savedGamesRowPrototype.transform)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(item2.gameObject);
			gameObject.transform.SetParent(savedMaps.GetComponent<Transform>(), false);
		}
		if (scrollable == null)
		{
			scrollable = savedMaps.transform.parent.GetComponent<RectTransform>();
		}
		if (scrollableHeight == 0f)
		{
			scrollableHeight = scrollable.offsetMin.y;
		}
		string[] files = Directory.GetFiles(Application.persistentDataPath, "sys*", SearchOption.TopDirectoryOnly);
		int num = -1;
		if (files.Length > 0)
		{
			int num2 = -1;
			string[] array = files;
			foreach (string text in array)
			{
				num2 = int.Parse(text.Replace(Application.persistentDataPath, string.Empty).Replace("/", string.Empty).Replace("\\", string.Empty)
					.Replace("sys", string.Empty));
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		for (int j = 0; j <= num && j < 100000; j++)
		{
			if (j % 3 == 0)
			{
				foreach (Transform item3 in savedGamesRowPrototype.transform)
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate(item3.gameObject);
					RectTransform component = gameObject2.GetComponent<RectTransform>();
					component.SetParent(savedMaps.GetComponent<Transform>(), false);
					component.localPosition += new Vector3(0f, amountToTranslateNewRows, 0f);
				}
				scrollable.offsetMin = new Vector2(scrollable.offsetMin.x, scrollable.offsetMin.y - 150f);
				amountToTranslateNewRows -= 150;
			}
			GameObject savedSystemButtonAux = GetSavedSystemButtonAux();
			if (File.Exists(Path.Combine(Application.persistentDataPath, "sys" + j)))
			{
				if (maps.transform.Find("ModeSavedSystem" + j + "Sandbox") == null)
				{
					GameObject gameObject3 = UnityEngine.Object.Instantiate(newSavedMapPrototype);
					gameObject3.name = "ModeSavedSystem" + j + "Sandbox";
					gameObject3.transform.parent = maps.transform;
					gameObject3.SetActive(true);
					LoadSystem("sys" + j);
					gameObject3.SetActive(false);
				}
				CreateButtonToSaveGame(j, savedSystemButtonAux);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(savedSystemButtonAux);
			}
		}
		GameObject savedSystemButtonAux2 = GetSavedSystemButtonAux();
		int num3 = 0;
		while (savedSystemButtonAux2 != null && num3 < 100000)
		{
			UnityEngine.Object.DestroyImmediate(savedSystemButtonAux2);
			savedSystemButtonAux2 = GetSavedSystemButtonAux();
			num3++;
		}
		if (currentMode != "sandbox")
		{
			scrollable.parent.gameObject.SetActive(false);
		}
	}

	private GameObject GetSavedSystemButtonAux()
	{
		Transform transform = savedMaps.transform.Find("ModeSavedSystemButton") ?? savedMaps.transform.Find("ModeSavedSystemButton(Clone)");
		return (!(transform != null)) ? null : transform.gameObject;
	}

	private void CreateButtonToSaveGame(int numSave, GameObject buttonAux)
	{
		buttonAux.name = "ModeSavedSystem" + numSave + "SandboxButton";
		buttonAux.SetActive(true);
		string mapName = "ModeSavedSystem" + numSave + "Sandbox";
		buttonAux.GetComponent<Button>().onClick.AddListener(delegate
		{
			HandleMapsButton(mapName);
		});
		buttonAux.transform.Find("Text").GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("Game") + "\n" + (numSave + 1);
	}

	public void LoadSystem(string sysName)
	{
		if (!(sysName != string.Empty))
		{
			return;
		}
		string text = Path.Combine(Application.persistentDataPath, sysName);
		if (!File.Exists(text))
		{
			return;
		}
		int[] progress = new int[1] { 1 };
		string text2 = Path.Combine(Application.persistentDataPath, "toLoadTemp");
		lzip.decompress_File(text, text2, progress);
		string text3 = File.ReadAllText(Path.Combine(text2, "s"));
		Directory.Delete(text2, true);
		string[] array = text3.Replace("N", "N_").Replace("U", "U_").Split('_');
		string[] array2 = array;
		foreach (string text4 in array2)
		{
			if (text4 != string.Empty)
			{
				char[] separator = new char[8] { 'M', 'X', 'Y', 'A', 'B', 'N', 'I', 'U' };
				string[] array3 = text4.Split(separator);
				int num = int.Parse(array3[0]);
				float mass = float.Parse(array3[1]);
				Vector2 vector = new Vector2(float.Parse(array3[2]), float.Parse(array3[3]));
				Vector2 initialMovement = new Vector2(float.Parse(array3[4]), float.Parse(array3[5]));
				bool initialObject = (text4.Contains("I") ? true : false);
				bool flag = (text4.Contains("U") ? true : false);
				GameObject gameObject = UnityEngine.Object.Instantiate((!flag) ? planetPrefab : ultraDensePrefab, vector, base.transform.rotation) as GameObject;
				GameObject gameObject2 = GameObject.Find("ModeSavedSystem" + sysName.Replace("sys", string.Empty) + "Sandbox");
				gameObject.transform.parent = gameObject2.transform;
				GravitationalObject component = gameObject.GetComponent<GravitationalObject>();
				if (!flag)
				{
					gameObject.transform.Find("Surface").GetComponent<MeshRenderer>().material.mainTexture = planetTextures[num];
				}
				gameObject.GetComponent<Rigidbody2D>().mass = mass;
				component.initialMovement = initialMovement;
				component.initialObject = initialObject;
				component.randomTexture = false;
				component.insidePlayableArea = true;
				component.addsScore = false;
				gameObject.transform.position = new Vector3(vector.x, vector.y);
			}
		}
	}

	private IEnumerator SavingSystemMessage()
	{
		saveButton.GetComponent<Button>().interactable = false;
		Text saveButtonText = saveButton.GetComponentInChildren<Text>();
		saveButtonText.text = LanguageManager.Instance.GetTextValue("Saving") + "...";
		showInterstitialGoingToMenu = true;
		yield return new WaitWhile(() => savingSystem);
		saveButtonText.text = LanguageManager.Instance.GetTextValue("Game") + " " + (fileNumberToSave + 1);
		saveButtonText.color = new Color(0.7490196f, 0.7490196f, 0.7490196f);
	}

	public void SaveSystem(Text savingText)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Planet");
		if (array.Length > 0)
		{
			fileNumberToSave = 0;
			savingSystem = true;
			StartCoroutine(SavingSystemMessage());
			string path = "s";
			string text = Path.Combine(Application.persistentDataPath, path);
			Debug.Log(text);
			string text2 = string.Empty;
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				GravitationalObject component = gameObject.GetComponent<GravitationalObject>();
				Rigidbody2D component2 = gameObject.GetComponent<Rigidbody2D>();
				text2 += component.GetNumTexture();
				text2 = text2 + "M" + component2.mass;
				string text3 = text2;
				text2 = text3 + "X" + gameObject.transform.position.x + "Y" + gameObject.transform.position.y;
				text3 = text2;
				text2 = text3 + "A" + component2.velocity.x / Time.fixedDeltaTime + "B" + component2.velocity.y / Time.fixedDeltaTime;
				if (component.initialObject)
				{
					text2 += "I";
				}
				text2 = ((!(component.objectType == "neutron") && !(component.objectType == "blackHole")) ? (text2 + "N") : (text2 + "U"));
			}
			File.WriteAllText(text, text2);
			while (File.Exists(text + "ys" + fileNumberToSave))
			{
				fileNumberToSave++;
			}
			lzip.compress_File(9, text + "ys" + fileNumberToSave, text, false, string.Empty, string.Empty);
			File.Delete(text);
			scrollable.offsetMin = new Vector2(scrollable.offsetMin.x, scrollable.offsetMin.y - (float)amountToTranslateNewRows - 150f);
			LoadSavedSystemsForSandbox();
			savingSystem = false;
		}
		else
		{
			saveButton.GetComponent<Button>().interactable = false;
			Text componentInChildren = saveButton.GetComponentInChildren<Text>();
			componentInChildren.text = "Empty";
			componentInChildren.color = new Color(0.7490196f, 0.7490196f, 0.7490196f);
		}
	}

	public void DeleteSavedSystem()
	{
		confirmationPanel.SetActive(true);
		StartCoroutine(DeleteSystemAfterConfirmation());
	}

	private IEnumerator DeleteSystemAfterConfirmation()
	{
		yield return new WaitUntil(delegate
		{
			bool? flag = isActionConfirmed;
			return flag.HasValue;
		});
		if (isActionConfirmed.Value)
		{
			GameObject parentButton = deleteSavedGameButton.transform.parent.gameObject;
			int numberSavedSystem = int.Parse(parentButton.name.Replace("ModeSavedSystem", string.Empty).Replace("SandboxButton", string.Empty));
			string filePath = Path.Combine(Application.persistentDataPath, "sys" + numberSavedSystem);
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
			UnityEngine.Object.Destroy(maps.transform.Find("ModeSavedSystem" + numberSavedSystem + "Sandbox").gameObject);
			deleteSavedGameButton.transform.SetParent(deleteSavedGameButton.transform.parent.transform.parent.transform.parent, false);
			deleteSavedGameButton.SetActive(false);
			UnityEngine.Object.Destroy(parentButton);
			mapDescription.text = LanguageManager.Instance.GetTextValue("SystemDeleted");
		}
		isActionConfirmed = null;
	}

	public void SetConfirmation(bool confirmation)
	{
		isActionConfirmed = confirmation;
		confirmationPanel.SetActive(false);
	}

	private void RequestBanner()
	{
	}

	private IEnumerator GooglePlayGamesAPI()
	{
		yield return null;
		gpgs = GameObject.Find("GPGS").GetComponent<GPGSCalls>();
		gpgs.LogInGooglePlayGames();
	}

	public GPGSCalls GetGoogleGames()
	{
		return gpgs;
	}

	public void setActiveAction(string newActiveAction)
	{
		activeAction = newActiveAction;
	}

	public string getActiveAction()
	{
		return activeAction;
	}

	public void HandleMapsButton(string selectedMapName)
	{
		if (selectedMapName != mapNameToPlay || previousMapButtonText.color != Color.white)
		{
			mapNameToPlay = selectedMapName;
			ResetMapSelectionButton();
			if (mapNameToPlay.Contains("SavedSystem"))
			{
				mapDescription.text = string.Empty;
				StartCoroutine(GetDynamicSavedSystemDescription());
				deleteSavedGameButton.SetActive(true);
				GameObject gameObject = GameObject.Find(mapNameToPlay + "Button");
				deleteSavedGameButton.transform.SetParent(gameObject.transform, false);
				deleteSavedGameButton.transform.position = gameObject.transform.position;
				deleteSavedGameButton.transform.localPosition += new Vector3(0f, -71.35f, 0f);
			}
			else
			{
				deleteSavedGameButton.SetActive(false);
				mapDescription.text = LanguageManager.Instance.GetTextValue((!mapNameToPlay.Contains("Mode")) ? "Welcome" : mapNameToPlay);
			}
			Text text = (previousMapButtonText = GameObject.Find(mapNameToPlay + "Button").transform.Find("Text").GetComponent<Text>());
			previousMapTitle = text.text;
			text.text = LanguageManager.Instance.GetTextValue("CurrentPressedButtonText");
			previousMapButtonText.fontSize = 30;
			previousMapButtonText.color = Color.white;
			LoadNewObjectsFromMap(mapNameToPlay, true);
		}
		else
		{
			LoadNewPlayableMap(mapNameToPlay);
			ResetMapSelectionButton();
		}
	}

	private IEnumerator GetDynamicSavedSystemDescription()
	{
		yield return null;
		mapDescription.text = GetObjectsText().Replace("#", string.Empty).Replace("neutronStar", "neutron star").Replace("blackHole", "black hole")
			.Replace("brownDwarf", "brown dwarf");
	}

	public void ResetMapSelectionButton()
	{
		if (previousMapTitle != string.Empty && previousMapButtonText != null)
		{
			previousMapButtonText.text = previousMapTitle;
			previousMapButtonText.fontSize = 20;
			previousMapButtonText.color = new Color(0.2509804f, 64f / 85f, 0.95686275f);
		}
		deleteSavedGameButton.SetActive(false);
		switch (currentMode)
		{
		case "sandbox":
			mapDescription.text = LanguageManager.Instance.GetTextValue("SandboxDescription");
			break;
		case "timeAttack":
			mapDescription.text = LanguageManager.Instance.GetTextValue("TimeAttackDescription");
			break;
		case "destroyer":
			mapDescription.text = LanguageManager.Instance.GetTextValue("DestroyerDescription");
			break;
		case "supernova":
			mapDescription.text = LanguageManager.Instance.GetTextValue("SupernovaDescription");
			break;
		default:
			mapDescription.text = LanguageManager.Instance.GetTextValue("Welcome");
			break;
		}
	}

	public void SetMode(string modeName)
	{
		currentMode = modeName;
		ResetMapSelectionButton();
	}

	public string GetMode()
	{
		return currentMode;
	}

	public string GetMapName()
	{
		return mapNameToPlay;
	}

	public void HandleOptionsButton(string selectedOptionName)
	{
		switch (selectedOptionName)
		{
		case "VisualsOption":
			optionDescription.text = LanguageManager.Instance.GetTextValue("VisualsOptionDescription");
			break;
		case "Realistic":
		{
			optionDescription.text = LanguageManager.Instance.GetTextValue("RealisticDescription");
			minimalistic = false;
			GameObject.Find("GameBackground").GetComponent<MeshRenderer>().material = realisticBackground;
			GameObject[] array6 = GameObject.FindGameObjectsWithTag("Planet");
			foreach (GameObject gameObject6 in array6)
			{
				GravitationalObject component2 = gameObject6.GetComponent<GravitationalObject>();
				StartCoroutine(component2.delayedColorUpdate());
				component2.GetSurface().GetComponent<MeshRenderer>().enabled = true;
				if (component2.objectType == "star" || component2.objectType == "brownDwarf")
				{
					gameObject6.transform.Find("starSurface").GetComponent<MeshRenderer>().enabled = true;
				}
				if (component2.objectType == "star")
				{
					gameObject6.transform.Find("Crown").gameObject.SetActive(true);
					gameObject6.transform.Find("StarLight").gameObject.SetActive(true);
				}
				gameObject6.transform.Find("Surface2D").GetComponent<MeshRenderer>().enabled = false;
				PlayerPrefs.SetInt("Minimalistic", 0);
			}
			break;
		}
		case "Minimalistic":
		{
			optionDescription.text = LanguageManager.Instance.GetTextValue("MinimalisticDescription");
			minimalistic = true;
			GameObject.Find("GameBackground").GetComponent<MeshRenderer>().material = flatBackground;
			GameObject[] array5 = GameObject.FindGameObjectsWithTag("Planet");
			foreach (GameObject gameObject5 in array5)
			{
				GravitationalObject component = gameObject5.GetComponent<GravitationalObject>();
				gameObject5.transform.Find("Surface2D").GetComponent<MeshRenderer>().enabled = true;
				StartCoroutine(component.delayedColorUpdate());
				component.GetSurface().GetComponent<MeshRenderer>().enabled = false;
				if (component.objectType == "star" || component.objectType == "brownDwarf")
				{
					gameObject5.transform.Find("starSurface").GetComponent<MeshRenderer>().enabled = false;
				}
				if (component.objectType == "star")
				{
					gameObject5.transform.Find("Crown").gameObject.SetActive(false);
					gameObject5.transform.Find("StarLight").gameObject.SetActive(false);
				}
				PlayerPrefs.SetInt("Minimalistic", 1);
			}
			break;
		}
		case "TrailLength":
		{
			int num = (int)planetPrefab.GetComponentInChildren<TrailRenderer>().time;
			optionDescription.text = string.Format(LanguageManager.Instance.GetTextValue("TrailLengthDescription"), num);
			GameObject[] array2 = GameObject.FindGameObjectsWithTag("Planet");
			foreach (GameObject gameObject2 in array2)
			{
				gameObject2.GetComponentInChildren<TrailRenderer>().time = num;
			}
			PlayerPrefs.SetInt("TrailLength", num);
			break;
		}
		case "TrailSize":
		{
			int trailSizeFactor = planetPrefab.GetComponent<GravitationalObject>().GetTrailSizeFactor();
			optionDescription.text = string.Format(LanguageManager.Instance.GetTextValue("TrailSizeDescription"), trailSizeFactor);
			GameObject[] array4 = GameObject.FindGameObjectsWithTag("Planet");
			foreach (GameObject gameObject4 in array4)
			{
				gameObject4.GetComponent<GravitationalObject>().SetTrailSizeFactor(trailSizeFactor);
				StartCoroutine(gameObject4.GetComponent<GravitationalObject>().delayedColorUpdate());
			}
			PlayerPrefs.SetInt("TrailSize", trailSizeFactor);
			break;
		}
		case "TrailOpacity":
		{
			int trailOpacityFactor = planetPrefab.GetComponent<GravitationalObject>().GetTrailOpacityFactor();
			optionDescription.text = string.Format(LanguageManager.Instance.GetTextValue("TrailOpacityDescription"), 100 - trailOpacityFactor);
			GameObject[] array = GameObject.FindGameObjectsWithTag("Planet");
			foreach (GameObject gameObject in array)
			{
				gameObject.GetComponent<GravitationalObject>().SetTrailOpacityFactor(trailOpacityFactor);
				StartCoroutine(gameObject.GetComponent<GravitationalObject>().delayedColorUpdate());
			}
			PlayerPrefs.SetInt("TrailOpacity", trailOpacityFactor);
			break;
		}
		case "FrontierOpacity":
		{
			int opacityFactor = visibleFrontier.GetComponent<VisibleGeometry>().GetOpacityFactor();
			optionDescription.text = string.Format(LanguageManager.Instance.GetTextValue("FrontierOpacityDescription"), 100 - opacityFactor);
			PlayerPrefs.SetInt("FrontierOpacity", opacityFactor);
			break;
		}
		case "AudioOption":
			optionDescription.text = LanguageManager.Instance.GetTextValue("AudioOptionDescription");
			break;
		case "SoundVolume":
		{
			Camera.main.GetComponent<AudioSource>().volume = 0.8f * soundVolume;
			GetComponents<AudioSource>()[1].volume = 0.7f * soundVolume;
			AudioSource[] componentsInChildren = planetPrefab.GetComponentsInChildren<AudioSource>();
			componentsInChildren[0].volume = 0.4f * soundVolume;
			componentsInChildren[1].volume = 0.7f * soundVolume;
			componentsInChildren[2].volume = soundVolume;
			componentsInChildren[3].volume = soundVolume;
			GameObject[] array3 = GameObject.FindGameObjectsWithTag("Planet");
			foreach (GameObject gameObject3 in array3)
			{
				AudioSource[] componentsInChildren2 = gameObject3.GetComponentsInChildren<AudioSource>();
				componentsInChildren2[0].volume = 0.4f * soundVolume;
				componentsInChildren2[1].volume = 0.7f * soundVolume;
				componentsInChildren2[2].volume = soundVolume;
				componentsInChildren2[3].volume = soundVolume;
			}
			barycenter.GetComponentInChildren<AudioSource>().volume = soundVolume;
			supernovaSound.volume = soundVolume;
			menuClickSound.volume = 0.4f * soundVolume;
			optionDescription.text = string.Format(LanguageManager.Instance.GetTextValue("SoundVolumeDescription"), (int)(soundVolume * 100f));
			if (!playingSoundSampleForConfig)
			{
				StartCoroutine(SoundSampleForConfig());
			}
			PlayerPrefs.SetInt("SoundVolume", (int)(soundVolume * 100f));
			break;
		}
		case "MusicVolume":
		{
			int num2 = (int)(GetComponents<AudioSource>()[0].volume * 100f);
			optionDescription.text = string.Format(LanguageManager.Instance.GetTextValue("MusicVolumeDescription"), num2);
			PlayerPrefs.SetInt("MusicVolume", num2);
			break;
		}
		case "SocialOption":
			if (!gpgs.GetLoggedIntoGoogleGames())
			{
				optionDescription.text = LanguageManager.Instance.GetTextValue("SocialOptionDescriptionNotLogged");
				optionButtonLoginGG.SetActive(true);
				socialContent.SetActive(false);
			}
			else
			{
				optionDescription.text = LanguageManager.Instance.GetTextValue("SocialOptionDescriptionLogged");
				optionButtonLoginGG.SetActive(false);
				socialContent.SetActive(true);
			}
			break;
		case "ShowLeaderboards":
			gpgs.ShowLeaderboard();
			break;
		case "ShowAchievements":
			gpgs.ShowAchievements();
			break;
		case "InfoOption":
			optionDescription.text = LanguageManager.Instance.GetTextValue("InfoOptionDescription");
			break;
		default:
			optionDescription.text = LanguageManager.Instance.GetTextValue("OptionDescription");
			break;
		}
	}

	private IEnumerator SoundSampleForConfig()
	{
		playingSoundSampleForConfig = true;
		Camera.main.GetComponent<AudioSource>().Play();
		yield return new WaitForSeconds(0.5f);
		playingSoundSampleForConfig = false;
	}

	public void SetSoundVolume(float newVolume)
	{
		soundVolume = newVolume;
	}

	public float GetSoundVolume()
	{
		return soundVolume;
	}

	public void TryToLogInGGFromSocialOption()
	{
		StartCoroutine(ResultAfterTryLogInGoogleGamesInOptions());
		gpgs.LogInGooglePlayGames();
	}

	private IEnumerator ResultAfterTryLogInGoogleGamesInOptions()
	{
		triedToLogInGoogleGames = false;
		optionDescription.text = LanguageManager.Instance.GetTextValue("TryingLogin");
		yield return new WaitUntil(() => triedToLogInGoogleGames);
		triedToLogInGoogleGames = false;
		if (gpgs.GetLoggedIntoGoogleGames())
		{
			optionDescription.text = LanguageManager.Instance.GetTextValue("SocialOptionDescriptionLogged");
			optionButtonLoginGG.SetActive(false);
			socialContent.SetActive(true);
		}
		else
		{
			optionDescription.text = LanguageManager.Instance.GetTextValue("SocialUnableLogin");
		}
	}

	public void BackToMainMenu()
	{
		gameOver = true;
		pausePanel.SetActive(false);
		MapSelectorPanel.SetActive(true);
		Camera.main.GetComponent<CameraControl>().objectToLookAt = null;
		Camera.main.transform.position = new Vector3(784f, -1443f, Camera.main.transform.position.z);
		activeAction = "creatingObjects";
		showInterstitialGoingToMenu = true;
		if (!goingToShowInterstitial && DateTime.UtcNow - adOpenedLastDateTime > TimeSpan.FromMinutes(30.0))
		{
			adOpenedLastDateTime = DateTime.MinValue;
		}
		LoadNewObjectsFromMap("mapStart");
	}

	public void BackFromGameOverPanel()
	{
		ScorePanel.SetActive(false);
		TimePanel.SetActive(false);
		barycenter.transform.Find("Image").gameObject.SetActive(false);
		NumberObjectsPanel.SetActive(false);
		GameOverPanel.SetActive(false);
		MapSelectorPanel.SetActive(true);
		LoadNewObjectsFromMap("mapStart");
		showInterstitialGoingToMenu = true;
	}

	private IEnumerator ShowInterstitial()
	{
		while (true)
		{
			goingToShowInterstitial = false;
			yield return new WaitForSeconds(120f);
			RequestInterstitial();
			yield return new WaitForSeconds(120f);
			yield return new WaitUntil(() => DateTime.UtcNow - adOpenedLastDateTime > TimeSpan.FromMinutes(30.0));
			adOpenedLastDateTime = DateTime.MinValue;
			goingToShowInterstitial = true;
			if (showInterstitialGoingToMenu)
			{
				showInterstitialGoingToMenu = false;
			}
			yield return new WaitUntil(() => showInterstitialGoingToMenu);
		}
	}

	public void BackFromOptions()
	{
		optionsUI.SetActive(false);
		if (gameOver)
		{
			MapSelectorPanel.SetActive(true);
		}
		else
		{
			pausePanel.SetActive(true);
		}
	}

	public void BackFromHowToPlay()
	{
		howToPlayPanel.SetActive(false);
		optionsUI.SetActive(true);
	}

	public void BackFromAbout()
	{
		aboutPanel.SetActive(false);
		optionsUI.SetActive(true);
	}

	public void LoadNewPlayableMap(string newMapName = "mapStart")
	{
		LoadNewObjectsFromMap(newMapName);
		activeAction = "creatingObjects";
		creatingPlanet = false;
		gameoverPanel.SetActive(false);
		MapSelectorPanel.SetActive(false);
		leftPanelCover.SetActive(false);
		leftSidebarForSliders.transform.Find("MassSlider").gameObject.SetActive(true);
		Transform transform = leftSidebar.transform.Find("SlideObjectsMenuButton");
		transform.Find("objectsImage").gameObject.SetActive(false);
		transform.Find("MassFrame").gameObject.SetActive(true);
		transform.Find("MassFrame").Find("ObjectTypeText").gameObject.SetActive(true);
		leftSidebarForSliders.transform.Find("CameraSlider").gameObject.SetActive(false);
		Transform transform2 = leftSidebar.transform.Find("SlideCameraMenuButton");
		transform2.Find("cameraImage").gameObject.SetActive(true);
		transform2.Find("ZoomFrame").gameObject.SetActive(false);
		transform2.Find("ZoomFrame").Find("CameraInfoText").gameObject.SetActive(false);
		barycenter.transform.Find("Image").gameObject.SetActive(false);
		StopCoroutine("UpdateScore");
		StopCoroutine("TimerForGameOver");
		CancelInvoke("TimerCounter");
		gameOver = false;
		if (currentMap.GetComponent<MapLoader>().addsScore)
		{
			ScorePanel.SetActive(true);
			StartCoroutine("UpdateScore");
		}
		maxPlayerScoreForCurrentMode = -1;
		score = 0;
		scoreText.GetComponent<Text>().text = string.Format(LanguageManager.Instance.GetTextValue("Score"), 0);
		plusScoreText.GetComponent<Text>().text = string.Empty;
		minusScoreText.GetComponent<Text>().text = string.Empty;
		if (currentMap.GetComponent<MapLoader>().hasCountdown)
		{
			TimePanel.SetActive(true);
			secondsForGameOver = currentMap.GetComponent<MapLoader>().time;
			StartCoroutine("TimerForGameOver");
		}
		else
		{
			secondsInTimerMods = 0;
			TimePanel.SetActive(true);
			InvokeRepeating("TimerCounter", 0f, 0.01f);
			if (currentMode == "sandbox")
			{
				StartCoroutine(TimeInSandboxAchievement());
			}
		}
		SetNumberObjects();
		NumberObjectsPanel.SetActive(true);
		Camera.main.GetComponent<CameraControl>().objectToLookAt = barycenter;
	}

	private IEnumerator TimeInSandboxAchievement()
	{
		yield return new WaitUntil(() => secondsInTimerMods >= 360000);
		gpgs.UnlockAchievement("SolarSystemLover");
	}

	private void LoadNewObjectsFromMap(string newMapName, bool mapZoom = false)
	{
		DeleteObjects();
		if (currentMap != null)
		{
			UnityEngine.Object.Destroy(currentMap);
		}
		currentMap = UnityEngine.Object.Instantiate(maps.transform.Find(newMapName).gameObject);
		CopyMapToSandbox();
		DelayedNormalizePlanetsSpeed();
		currentMap.SetActive(true);
		Camera.main.GetComponent<CameraControl>().minZoom = currentMap.GetComponent<MapLoader>().minimumZoom;
		if (mapZoom)
		{
			Camera.main.GetComponent<CameraControl>().zoomSlider.GetComponent<Slider>().value = currentMap.GetComponent<MapLoader>().initialSliderZoom;
			Camera.main.transform.position = new Vector3(0f, 0f, Camera.main.transform.position.z);
		}
		gravityFactor = currentMap.GetComponent<MapLoader>().mapGravityFactor;
		Transform transform = barycenter.transform.Find("Playable Region");
		int playableRegionRadius = currentMap.GetComponent<MapLoader>().playableRegionRadius;
		transform.localScale = new Vector3(playableRegionRadius, playableRegionRadius);
		float num = currentMap.GetComponent<MapLoader>().initialSliderZoom / 200f;
		transform.GetComponentInChildren<LineRenderer>().SetWidth(num, num);
		Camera.main.GetComponent<CameraControl>().SetCurrentPlayableRegionScale(transform.localScale);
		barycenter.transform.Find("Outer Playable Region").localScale = transform.localScale * 1.4f;
		if (gamePaused)
		{
			ContinueGame();
		}
	}

	private static void DeleteObjects()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Planet");
		foreach (GameObject obj in array)
		{
			UnityEngine.Object.Destroy(obj);
		}
		GameObject[] array2 = GameObject.FindGameObjectsWithTag("Effect");
		foreach (GameObject obj2 in array2)
		{
			UnityEngine.Object.Destroy(obj2);
		}
	}

	private void CopyMapToSandbox()
	{
		currentSystemSandboxMap.SetActive(false);
		foreach (Transform item in currentSystemSandboxMap.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in currentMap.transform)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(item2.gameObject);
			gameObject.transform.rotation = item2.rotation;
			gameObject.transform.position = item2.position;
			gameObject.transform.parent = currentSystemSandboxMap.transform;
			gameObject.name = item2.name;
		}
	}

	private void RequestInterstitial()
	{
	}

	private void OnAdLeavingApplication(object sender, EventArgs e)
	{
		adOpenedLastDateTime = DateTime.UtcNow;
	}

	public int GetNumberObjectsCreatedByPlayer()
	{
		int num = 0;
		GameObject[] array = GameObject.FindGameObjectsWithTag("Planet");
		foreach (GameObject gameObject in array)
		{
			if (!gameObject.GetComponent<GravitationalObject>().initialObject)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberObjectsNotCreatedByPlayer()
	{
		int num = 0;
		GameObject[] array = GameObject.FindGameObjectsWithTag("Planet");
		foreach (GameObject gameObject in array)
		{
			if (gameObject.GetComponent<GravitationalObject>().initialObject)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberObjects()
	{
		return GameObject.FindGameObjectsWithTag("Planet").Length;
	}

	public void ReloadNewMap()
	{
		gameoverPanel.SetActive(false);
		LoadNewPlayableMap(mapNameToPlay);
	}

	public bool currentMapAddsScore()
	{
		return currentMap.GetComponent<MapLoader>().addsScore;
	}

	public void SetPlanetLostGoogleEvent()
	{
		gpgs.SetPlanetLostEvent();
	}

	private IEnumerator LoopMusic()
	{
		AudioSource music = GetComponents<AudioSource>()[0];
		while (true)
		{
			music.clip = musicClips[UnityEngine.Random.Range(0, musicClips.Length)];
			music.Play();
			yield return new WaitForSeconds(music.clip.length + 5f);
		}
	}

	public int GetScore()
	{
		return score;
	}

	private void Update()
	{
		if (CanCreateNewObject())
		{
			if (Input.GetButtonDown("Fire1") && !creatingPlanet && canCreatePlanetAfterCoolDown)
			{
				if (IsInPlayableRegion(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
				{
					StartCoroutine(CoolDownForCreateAnotherPlanet());
					planetOriginPoint = Input.mousePosition;
					if (planetOriginPoint.x > (float)Camera.main.pixelWidth * GetLeftSideBarRelativeWidth())
					{
						creatingPlanet = true;
						UnityEngine.Object.Instantiate(drawSpeedVector);
					}
				}
				else if (Input.mousePosition.x > (float)Camera.main.pixelWidth * GetLeftSideBarRelativeWidth())
				{
					cantCreatePlanetSound.Play();
				}
			}
			else
			{
				if (!creatingPlanet || !Input.GetButtonUp("Fire1"))
				{
					return;
				}
				creatingPlanet = false;
				if (IsInPlayableRegion(Camera.main.ScreenToWorldPoint(planetOriginPoint)))
				{
					planetSpeedPoint = Input.mousePosition;
					GameObject gameObject = UnityEngine.Object.Instantiate((!(planetPrefab.GetComponent<Rigidbody2D>().mass < (float)GravitationalObject.maxStarMass)) ? ultraDensePrefab : planetPrefab, ScreenPointToWorld(planetOriginPoint), base.transform.rotation) as GameObject;
					if (planetPrefab.GetComponent<Rigidbody2D>().mass >= (float)GravitationalObject.maxStarMass)
					{
						gameObject.GetComponent<Rigidbody2D>().mass = planetPrefab.GetComponent<Rigidbody2D>().mass / 2f;
					}
					Vector2 vector = ScreenPointToWorld(planetSpeedPoint) - ScreenPointToWorld(planetOriginPoint);
					Vector2 initialMovement = vector * Mathf.Pow(planetSpeedFactorOnCreate, planetSpeedPowOnCreate);
					gameObject.GetComponent<GravitationalObject>().initialMovement = initialMovement;
				}
				else
				{
					cantCreatePlanetSound.Play();
				}
			}
		}
		else if (Input.GetButtonDown("Fire1") && !gamePaused && !gameOver && activeAction == "creatingObjects" && currentTotalObjectsNumber >= currentMap.GetComponent<MapLoader>().MaxObjects && Input.mousePosition.x > (float)Camera.main.pixelWidth * GetLeftSideBarRelativeWidth())
		{
			cantCreatePlanetSound.Play();
		}
	}

	private bool CanCreateNewObject()
	{
		return !gamePaused && !gameOver && activeAction == "creatingObjects" && currentTotalObjectsNumber < currentMap.GetComponent<MapLoader>().MaxObjects;
	}

	private bool IsInPlayableRegion(Vector3 point)
	{
		LayerMask layerMask = 256;
		return Physics2D.Raycast(point, Vector2.zero, float.PositiveInfinity, layerMask);
	}

	private IEnumerator CoolDownForCreateAnotherPlanet()
	{
		canCreatePlanetAfterCoolDown = false;
		yield return new WaitForSeconds(0.1f);
		canCreatePlanetAfterCoolDown = true;
	}

	public void SetNumberObjects(int difference = 0)
	{
		currentTotalObjectsNumber += difference;
		NumberObjectsPanel.transform.Find("NumberObjectsText").GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("Objects") + " " + currentTotalObjectsNumber + "/" + currentMap.GetComponent<MapLoader>().MaxObjects;
		if (currentTotalObjectsNumber == 20 && !gameOver)
		{
			gpgs.UnlockAchievement("MoreThan20objects");
		}
	}

	public bool GetCreatingPlanet()
	{
		return creatingPlanet;
	}

	public Vector2 GetPlanetOriginPoint()
	{
		return planetOriginPoint;
	}

	public Vector2 GetBarycenter2D()
	{
		return barycenter2D;
	}

	public bool GetGameOver()
	{
		return gameOver;
	}

	public void PauseGame()
	{
		if (!gameOver)
		{
			gamePaused = true;
			Time.timeScale = 0f;
			pausePanel.SetActive(true);
		}
	}

	public bool IsGamePaused()
	{
		return gamePaused;
	}

	public void ContinueGame()
	{
		gamePaused = false;
		Time.timeScale = 1f;
		pausePanel.SetActive(false);
		if (!saveButton.GetComponent<Button>().interactable)
		{
			saveButton.GetComponent<Button>().interactable = true;
			Text componentInChildren = saveButton.GetComponentInChildren<Text>();
			componentInChildren.text = LanguageManager.Instance.GetTextValue("SAVE");
			Color color = new Color(0.2509804f, 64f / 85f, 0.95686275f);
			componentInChildren.color = color;
		}
	}

	public void ShareScreenshotButton()
	{
		if (canShareScreenshot)
		{
			canShareScreenshot = false;
			StartCoroutine("ShareScreenshot");
		}
	}

	private void OnApplicationFocus(bool focusStatus)
	{
		isAppInBackground = !focusStatus;
	}

	private IEnumerator ShareScreenshot()
	{
		if(Application.isMobilePlatform)
		{
            if (gamePaused)
            {
                pausePanel.SetActive(false);
                leftPanelCover.SetActive(false);
            }
            yield return new WaitForEndOfFrame();
            string fileName = "Make Your Solar System - screenshot.png";
            ScreenCapture.CaptureScreenshot(fileName);
            if (gamePaused)
            {
                pausePanel.SetActive(true);
                leftPanelCover.SetActive(true);
            }
            yield return new WaitForEndOfFrame();
            string destination = Path.Combine(Application.persistentDataPath, fileName);
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
            intentObject.Call<AndroidJavaObject>("setAction", new object[1] { intentClass.GetStatic<string>("ACTION_SEND") });
            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", new object[1] { "file://" + destination });
            intentObject.Call<AndroidJavaObject>("setType", new object[1] { "image/jpeg" });
            intentObject.Call<AndroidJavaObject>("putExtra", new object[2]
            {
            intentClass.GetStatic<string>("EXTRA_STREAM"),
            uriObject
            });
            intentObject.Call<AndroidJavaObject>("putExtra", new object[2]
            {
            intentClass.GetStatic<string>("EXTRA_SUBJECT"),
            "Check out this system!"
            });
            intentObject.Call<AndroidJavaObject>("putExtra", new object[2]
            {
            intentClass.GetStatic<string>("EXTRA_TEXT"),
            GetExtraTextForSharing()
            });
            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            currentActivity.Call("startActivity", intentObject);
            canShareScreenshot = true;
            StartCoroutine("ShareAchievementRestart");
        }
	}

	private IEnumerator ShareAchievementRestart()
	{
		yield return new WaitWhile(() => isAppInBackground);
		if (gamePaused && canStepShareAchievement && GameObject.FindGameObjectsWithTag("Planet").Length > 0)
		{
			gpgs.UnlockIncrementalAchievement("ShareFiveTimes");
		}
		canStepShareAchievement = false;
		yield return new WaitWhile(() => gamePaused);
		yield return new WaitForSeconds(5f);
		canStepShareAchievement = true;
	}

	private string GetExtraTextForSharing()
	{
		string text = string.Empty;
		if (gamePaused)
		{
			text = GetObjectsText();
		}
		else if (gameOver)
		{
			if (currentMap.GetComponent<MapLoader>().isDestroyMode)
			{
				string text2 = text;
				text = text2 + "I completed the " + MapButton.getMapNameInGameOver() + " - Destroyer mode in " + GetFormatedStringFromMiliseconds(secondsInTimerMods * 10) + ".";
				if (maxPlayerScoreForCurrentMode > -1)
				{
					text = ((secondsInTimerMods * 10 >= maxPlayerScoreForCurrentMode) ? (text + " My best time is " + GetFormatedStringFromMiliseconds(maxPlayerScoreForCurrentMode) + ".") : (text + " A new personal best time!"));
				}
			}
			else if (currentMap.GetComponent<MapLoader>().isSupernovaMode)
			{
				string text2 = text;
				text = text2 + "My stars were safe in " + MapButton.getMapNameInGameOver() + " - Supernova mode for " + GetFormatedStringFromMiliseconds(secondsInTimerMods * 10) + ".";
				if (maxPlayerScoreForCurrentMode > -1)
				{
					text = ((maxPlayerScoreForCurrentMode >= secondsInTimerMods * 10) ? (text + " My record is " + GetFormatedStringFromMiliseconds(maxPlayerScoreForCurrentMode) + ".") : (text + " A new personal time record!"));
				}
			}
			else
			{
				string text2 = text;
				text = text2 + "I scored " + score + " points in the " + MapButton.getMapNameInGameOver() + " - Time Attack mode.";
				if (maxPlayerScoreForCurrentMode > -1)
				{
					if (maxPlayerScoreForCurrentMode < score)
					{
						text += " A new personal record!";
					}
					else
					{
						text2 = text;
						text = text2 + " My record is " + maxPlayerScoreForCurrentMode + " points!";
					}
				}
			}
			text += " Can you beat me?";
		}
		return text + " #MakeYourSolarSystem";
	}

	private static string GetObjectsText()
	{
		string text = string.Empty;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		string empty = string.Empty;
		GameObject[] array = GameObject.FindGameObjectsWithTag("Planet");
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			empty = gameObject.GetComponent<GravitationalObject>().GetObjectType();
			if (empty == string.Empty)
			{
				num++;
				continue;
			}
			switch (empty)
			{
			case "brownDwarf":
				num2++;
				break;
			case "star":
				num3++;
				break;
			case "neutron":
				num4++;
				break;
			case "blackHole":
				num5++;
				break;
			}
		}
		if (array.Length == 0)
		{
			text = "Just empty #space";
		}
		else
		{
			if (num5 > 0)
			{
				string text2 = text;
				text = text2 + num5 + " #blackHole" + ((num5 <= 1) ? string.Empty : "s") + ((num4 + num3 + num2 + num <= 0) ? string.Empty : " with ");
			}
			if (num4 > 0)
			{
				string text2 = text;
				text = text2 + num4 + " #neutronStar" + ((num4 <= 1) ? string.Empty : "s");
				if ((num3 > 0 && num2 + num == 0) || (num2 > 0 && num3 + num == 0) || (num > 0 && num2 + num3 == 0))
				{
					text += " and ";
				}
				else if (num3 + num2 + num > 0)
				{
					text += ((num5 != 0) ? ", " : " with ");
				}
			}
			if (num3 > 0)
			{
				string text2 = text;
				text = text2 + num3 + " #star" + ((num3 <= 1) ? string.Empty : "s");
				if ((num2 > 0 && num == 0) || (num > 0 && num2 == 0))
				{
					text += " and ";
				}
				else if (num2 + num > 0)
				{
					text += ((num5 + num4 != 0) ? ", " : " with ");
				}
			}
			if (num2 > 0)
			{
				string text2 = text;
				text = text2 + num2 + " #brownDwarf" + ((num2 <= 1) ? string.Empty : "s");
				if (num > 0)
				{
					text += " and ";
				}
				else if (num5 + num4 + num3 == 0 && num > 0)
				{
					text += " with ";
				}
			}
			if (num > 0)
			{
				string text2 = text;
				text = text2 + num + " #planet" + ((num <= 1) ? string.Empty : "s");
			}
		}
		return text + ".";
	}

	public void DelayedNormalizePlanetsSpeed()
	{
		StartCoroutine("DelayedNormalizePlanetsSpeedCoroutine");
	}

	private IEnumerator DelayedNormalizePlanetsSpeedCoroutine()
	{
		yield return new WaitForFixedUpdate();
		NormalizePlanetsSpeed();
	}

	private void NormalizePlanetsSpeed()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Planet");
		Vector2 vector = Vector2.zero;
		float num = 0f;
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (gameObject.GetComponent<Rigidbody2D>().mass > num)
			{
				vector = gameObject.GetComponent<Rigidbody2D>().velocity;
				num = gameObject.GetComponent<Rigidbody2D>().mass;
			}
		}
		GameObject[] array3 = array;
		foreach (GameObject gameObject2 in array3)
		{
			gameObject2.GetComponent<Rigidbody2D>().velocity -= vector;
		}
	}

	public void ClearAllTrails()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Planet");
		foreach (GameObject gameObject in array)
		{
			TrailRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<TrailRenderer>();
			foreach (TrailRenderer trailRenderer in componentsInChildren)
			{
				trailRenderer.Clear();
			}
		}
	}

	public void ClearAllTrailsTwice()
	{
		StartCoroutine(ClearAllTrailsTwiceRoutine());
	}

	public IEnumerator ClearAllTrailsTwiceRoutine()
	{
		ClearAllTrails();
		yield return null;
		ClearAllTrails();
	}

	public Vector2 ScreenPointToWorld(Vector2 screenPoint)
	{
		return Camera.main.ScreenPointToRay(screenPoint).origin;
	}

	private void FixedUpdate()
	{
		SetObjectsPosition();
	}

	private void SetObjectsPosition()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Planet");
		if (array.Length <= 0)
		{
			return;
		}
		partialForce = Vector2.zero;
		barycenter2D = new Vector2(0f, 0f);
		totalMassOfSystem = 0f;
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			partialForce = Vector2.zero;
			GameObject[] array3 = array;
			foreach (GameObject gameObject2 in array3)
			{
				if (gameObject != gameObject2 && (gameObject.GetComponent<GravitationalObject>().insidePlayableArea || gameObject2.GetComponent<GravitationalObject>().insidePlayableArea))
				{
					ApplyGravity(gameObject.GetComponent<Rigidbody2D>(), gameObject2.GetComponent<Rigidbody2D>());
				}
			}
			gameObject.GetComponent<Rigidbody2D>().AddForce(partialForce);
			partialForce = Vector2.zero;
			barycenter2D += (Vector2)(gameObject.GetComponent<Rigidbody2D>().mass * gameObject.GetComponent<Rigidbody2D>().transform.position);
			totalMassOfSystem += gameObject.GetComponent<Rigidbody2D>().mass;
		}
		barycenter2D /= totalMassOfSystem;
		barycenter2D = new Vector2((float)Mathf.RoundToInt(barycenter2D.x * 100f) / 100f, (float)Mathf.RoundToInt(barycenter2D.y * 100f) / 100f);
		barycenter.transform.position = new Vector3(barycenter2D.x, barycenter2D.y, 0f);
		if (centerObjectsInWorld)
		{
			DoCenterObjectsInWorld(array);
		}
	}

	private void DoCenterObjectsInWorld(GameObject[] objects)
	{
		foreach (GameObject gameObject in objects)
		{
			gameObject.transform.position -= barycenter.transform.position;
			gameObject.transform.position = new Vector2((float)Mathf.RoundToInt(gameObject.transform.position.x * 100f) / 100f, (float)Mathf.RoundToInt(gameObject.transform.position.y * 100f) / 100f);
		}
		barycenter.transform.position = Vector3.zero;
	}

	private void ApplyGravity(Rigidbody2D A, Rigidbody2D B)
	{
		Vector2 vector = B.transform.position - A.transform.position;
		float magnitude = vector.magnitude;
		vector = vector.normalized;
		float num = 6.674f * (A.mass * B.mass * (float)planetMassFactor) / (magnitude * magnitude) * (float)gravityFactor;
		partialForce += 2f * (vector * num);
	}

	private IEnumerator TimerForGameOver()
	{
		while (!gameOver)
		{
			if (secondsForGameOver <= 0)
			{
				timerText.GetComponent<Text>().text = string.Format(LanguageManager.Instance.GetTextValue("Time"), 0);
				GameOver();
			}
			else
			{
				timerText.GetComponent<Text>().text = string.Format(LanguageManager.Instance.GetTextValue("Time"), GetFormatedStringFromMiliseconds(secondsForGameOver * 1000, false));
				secondsForGameOver--;
				yield return new WaitForSeconds(1f);
			}
		}
	}

	private void TimerCounter()
	{
		secondsInTimerMods++;
		timerText.GetComponent<Text>().text = string.Format(LanguageManager.Instance.GetTextValue("Time"), GetFormatedStringFromMiliseconds(secondsInTimerMods * 10));
	}

	public void SetScore(int newScore)
	{
		score = newScore;
	}

	public void GameOver()
	{
		gameOver = true;
		CancelInvoke("TimerCounter");
		activeAction = "creatingObjects";
		creatingPlanet = false;
		GameOverPanel.transform.Find("LeaderBoard").gameObject.SetActive(false);
		leftPanelCover.SetActive(true);
		gameoverPanel.SetActive(true);
		TryToLoadLeaderBoard();
	}

	public void TryToLoadLeaderBoard()
	{
		ResetLeaderboard();
		LoadingLeaderBoardTextOn(true);
		statusText.GetComponent<Text>().text = string.Empty;
		StartCoroutine("ShowScoresWhenLoaded");
		StartCoroutine("NoInternetOrLoggedIntoGoogleGamesMessage");
		if (gpgs.GetLoggedIntoGoogleGames())
		{
			if (currentMap.GetComponent<MapLoader>().hasCountdown)
			{
				gpgs.PutScoreAndGetLeaderBoard(score, mapNameToPlay);
			}
			else
			{
				gpgs.PutScoreAndGetLeaderBoard(secondsInTimerMods * 10, mapNameToPlay);
			}
		}
		else
		{
			noInternetOrLoggedIntoGoogleGamesMessage = true;
		}
	}

	public void TryToLogInAndLoadLeaderBoard()
	{
		LoadingLeaderBoardTextOn(true);
		StartCoroutine("NoInternetOrLoggedIntoGoogleGamesMessage");
		StartCoroutine("ResultAfterTryLogInGoogleGames");
		gpgs.LogInGooglePlayGames();
	}

	private void LoadingLeaderBoardTextOn(bool loading)
	{
		GameOverPanel.transform.Find("LoadingText").gameObject.SetActive(loading);
		GameOverPanel.transform.Find("NotLoggedItems").gameObject.SetActive(!loading);
	}

	public void SetTryedToLogInGoogleGames(bool tried)
	{
		triedToLogInGoogleGames = tried;
	}

	private IEnumerator ResultAfterTryLogInGoogleGames()
	{
		triedToLogInGoogleGames = false;
		yield return new WaitUntil(() => triedToLogInGoogleGames);
		triedToLogInGoogleGames = false;
		if (gpgs.GetLoggedIntoGoogleGames())
		{
			TryToLoadLeaderBoard();
		}
		else
		{
			noInternetOrLoggedIntoGoogleGamesMessage = true;
		}
	}

	internal void SetLeaderboardLoaded()
	{
		isLeaderboardLoaded = true;
	}

	private IEnumerator ShowScoresWhenLoaded()
	{
		isLeaderboardLoaded = false;
		yield return new WaitUntil(() => isLeaderboardLoaded);
		isLeaderboardLoaded = false;
		string[] ranks = gpgs.GetRanks();
		string[] playerNames = gpgs.GetPlayerNames();
		string[] scores = gpgs.GetScores();
		maxPlayerScoreForCurrentMode = gpgs.GetPrevScore();
		for (int i = 0; i < ranks.Length; i++)
		{
			if (ranks[i] == null)
			{
				ranksText[i].text = string.Empty;
				playerNamesText[i].text = string.Empty;
				scoresText[i].text = string.Empty;
				continue;
			}
			ranksText[i].text = ranks[i];
			playerNamesText[i].text = playerNames[i];
			if (currentMap.GetComponent<MapLoader>().isDestroyMode || currentMap.GetComponent<MapLoader>().isSupernovaMode)
			{
				scoresText[i].text = GetFormatedStringFromMiliseconds(int.Parse(scores[i]));
			}
			else
			{
				scoresText[i].text = scores[i];
			}
		}
		currentPlayerPosition = gpgs.GetCurrentPlayerPosition();
		if (currentPlayerPosition >= 0)
		{
			ranksText[currentPlayerPosition].color = Color.green;
			playerNamesText[currentPlayerPosition].color = Color.green;
			scoresText[currentPlayerPosition].color = Color.green;
			float scrollPosition = Mathf.Clamp01(-0.055556f * (float)currentPlayerPosition + 1.17f);
			GameOverPanel.transform.Find("LeaderBoard").Find("Scrollable Scores").GetComponent<ScrollRect>()
				.verticalNormalizedPosition = scrollPosition;
			DisplayNewRecordOrNot(maxPlayerScoreForCurrentMode);
		}
		GameOverPanel.transform.Find("LoadingText").gameObject.SetActive(false);
		GameOverPanel.transform.Find("LeaderBoard").gameObject.SetActive(true);
		if (ranksText[24] != null && currentPlayerPosition == 0 && ranksText[24].text != string.Empty && ranksText[currentPlayerPosition].text == "#1")
		{
			gpgs.UnlockAchievement("TheBest");
		}
	}

	private void DisplayNewRecordOrNot(int currentRecordScore)
	{
		int num = ((!currentMap.GetComponent<MapLoader>().isDestroyMode && !currentMap.GetComponent<MapLoader>().isSupernovaMode) ? score : (secondsInTimerMods * 10));
		if (currentMap.GetComponent<MapLoader>().isDestroyMode)
		{
			if (num < currentRecordScore)
			{
				statusText.GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("NewRecord");
				statusText.GetComponent<Text>().color = Color.green;
				scoresText[currentPlayerPosition].text = GetFormatedStringFromMiliseconds(num);
			}
			else
			{
				statusText.GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("NotNewRecord");
				statusText.GetComponent<Text>().color = Color.white;
			}
		}
		else if (num > currentRecordScore)
		{
			statusText.GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("NewRecord");
			statusText.GetComponent<Text>().color = Color.green;
			if (currentMap.GetComponent<MapLoader>().isSupernovaMode)
			{
				scoresText[currentPlayerPosition].text = GetFormatedStringFromMiliseconds(num);
			}
			else
			{
				scoresText[currentPlayerPosition].text = num.ToString();
			}
		}
		else
		{
			statusText.GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("NotNewRecord");
			statusText.GetComponent<Text>().color = Color.white;
		}
		if (mapNameToPlay == "ModeTinyTimeAttack" && (num > 9000 || currentRecordScore > 9000))
		{
			gpgs.UnlockAchievement("Over9000");
		}
	}

	public void SetNewTopMessage()
	{
		thereIsNewMessageToDisplay = true;
	}

	private IEnumerator DisplayNewTopMessage()
	{
		thereIsNewMessageToDisplay = false;
		yield return new WaitUntil(() => thereIsNewMessageToDisplay);
		thereIsNewMessageToDisplay = false;
		TopMessagePanel.GetComponentInChildren<Text>().text = gpgs.getMessage();
		TopMessagePanel.SetActive(true);
	}

	public void setNoInternetOrLoggedIntoGoogleGames()
	{
		noInternetOrLoggedIntoGoogleGamesMessage = true;
	}

	private IEnumerator NoInternetOrLoggedIntoGoogleGamesMessage()
	{
		noInternetOrLoggedIntoGoogleGamesMessage = false;
		yield return new WaitUntil(() => noInternetOrLoggedIntoGoogleGamesMessage);
		noInternetOrLoggedIntoGoogleGamesMessage = false;
		LoadingLeaderBoardTextOn(false);
	}

	public void ResetLeaderboard()
	{
		StopCoroutine("ShowScoresWhenLoaded");
		StopCoroutine("NoInternetOrLoggedIntoGoogleGamesMessage");
		if (currentPlayerPosition >= 0)
		{
			ranksText[currentPlayerPosition].color = Color.white;
			playerNamesText[currentPlayerPosition].color = Color.white;
			scoresText[currentPlayerPosition].color = Color.white;
		}
		gpgs.Reset();
	}

	public void ShowCurrentLeaderboardInGoogleGames()
	{
		gpgs.ShowSpecificLeaderboard(mapNameToPlay);
	}

	private IEnumerator UpdateScore()
	{
		while (!gameOver)
		{
			GameObject[] Objects = GameObject.FindGameObjectsWithTag("Planet");
			int totalScoreThisSecond = 0;
			GameObject[] array = Objects;
			foreach (GameObject planet in array)
			{
				if (planet.GetComponent<GravitationalObject>().addsScore)
				{
					totalScoreThisSecond += GetPlanetScore(planet.GetComponent<Rigidbody2D>().mass);
				}
			}
			AddScorePoints(totalScoreThisSecond);
			yield return new WaitForSeconds(scoringRate);
		}
	}

	public GameObject GetCurrentMap()
	{
		return currentMap;
	}

	public int GetPlanetScore(float baseMass)
	{
		float num = 0f;
		num = ((baseMass <= 0.01f) ? (baseMass * 1000f + 9.9f) : ((baseMass <= 0.1f) ? (baseMass * 111f + 18.9f) : ((baseMass <= 1f) ? (baseMass * 11.1f + 28.9f) : ((baseMass <= 50f) ? (baseMass * 0.408f + 39.6f) : ((baseMass <= 1000f) ? (baseMass * 0.0421f + 57.9f) : ((baseMass <= 16000f) ? (baseMass * 0.0267f + 73.3f) : ((!(baseMass <= 400000f)) ? (baseMass * 0.000167f + 833f) : (baseMass * 0.00104f + 483f))))))));
		return Mathf.CeilToInt(num);
	}

	public void AddScorePoints(int eventScore)
	{
		if (eventScore < 0)
		{
			Text component = minusScoreText.GetComponent<Text>();
			component.text = eventScore.ToString();
			component.CrossFadeAlpha(1f, 0f, true);
			component.CrossFadeAlpha(0f, 3f, false);
		}
		else if (eventScore > 0)
		{
			Text component2 = plusScoreText.GetComponent<Text>();
			component2.text = "+" + eventScore;
			component2.CrossFadeAlpha(1f, 0f, true);
			component2.CrossFadeAlpha(0f, 1f, false);
		}
		score += eventScore;
		if (score < 0)
		{
			score = 0;
		}
		scoreText.GetComponent<Text>().text = string.Format(LanguageManager.Instance.GetTextValue("Score"), score);
	}

	private void ColorsForPlanetTextures()
	{
		planetTextures = Resources.LoadAll<Texture2D>("Textures/Planets");
		numPlanetTextures = planetTextures.Length;
		planetFlatColors = new Color32[numPlanetTextures];
		for (int i = 0; i < numPlanetTextures; i++)
		{
			Color32 color = AverageColorFromTexture(planetTextures[i]);
			float h = 0f;
			float s = 0f;
			float v = 0f;
			ColorToHSV(color, out h, out s, out v);
			h += (float)UnityEngine.Random.Range(-20, 30);
			s -= 0.2f;
			if (s < 0.1f)
			{
				s = 0.1f;
			}
			s *= (float)UnityEngine.Random.Range(3, 9);
			v *= (float)(UnityEngine.Random.Range(15, 25) / 10);
			planetFlatColors[i] = ColorFromHSV(h, s, v, 1f);
		}
	}

	public Texture2D GetPlanetTexture(int numTexture)
	{
		return planetTextures[numTexture];
	}

	private Color32 AverageColorFromTexture(Texture2D tex)
	{
		Color32[] pixels = tex.GetPixels32();
		int num = pixels.Length;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		for (int i = 0; i < num; i++)
		{
			num2 += (float)(int)pixels[i].r;
			num3 += (float)(int)pixels[i].g;
			num4 += (float)(int)pixels[i].b;
		}
		return new Color32((byte)(num2 / (float)num), (byte)(num3 / (float)num), (byte)(num4 / (float)num), 0);
	}

	public int GetNumPlanetTextures()
	{
		return numPlanetTextures;
	}

	public Color32 GetColorForPlanet(int number)
	{
		return planetFlatColors[number];
	}

	public static Color ColorFromHSV(float h, float s, float v, float a = 1f)
	{
		if (s == 0f)
		{
			return new Color(v, v, v, a);
		}
		float num = h / 60f;
		int num2 = (int)num;
		float num3 = num - (float)num2;
		float num4 = v * (1f - s);
		float num5 = v * (1f - s * num3);
		float num6 = v * (1f - s * (1f - num3));
		Color result = new Color(0f, 0f, 0f, a);
		switch (num2)
		{
		case 0:
			result.r = v;
			result.g = num6;
			result.b = num4;
			break;
		case 1:
			result.r = num5;
			result.g = v;
			result.b = num4;
			break;
		case 2:
			result.r = num4;
			result.g = v;
			result.b = num6;
			break;
		case 3:
			result.r = num4;
			result.g = num5;
			result.b = v;
			break;
		case 4:
			result.r = num6;
			result.g = num4;
			result.b = v;
			break;
		default:
			result.r = v;
			result.g = num4;
			result.b = num5;
			break;
		}
		return result;
	}

	public static void ColorToHSV(Color color, out float h, out float s, out float v)
	{
		float num = Mathf.Min(Mathf.Min(color.r, color.g), color.b);
		float num2 = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
		float num3 = num2 - num;
		v = num2;
		if (!Mathf.Approximately(num2, 0f))
		{
			s = num3 / num2;
			if (Mathf.Approximately(num, num2))
			{
				v = num2;
				s = 0f;
				h = -1f;
				return;
			}
			if (color.r == num2)
			{
				h = (color.g - color.b) / num3;
			}
			else if (color.g == num2)
			{
				h = 2f + (color.b - color.r) / num3;
			}
			else
			{
				h = 4f + (color.r - color.g) / num3;
			}
			h *= 60f;
			if (h < 0f)
			{
				h += 360f;
			}
		}
		else
		{
			s = 0f;
			h = -1f;
		}
	}

	public float GetLeftSideBarRelativeWidth()
	{
		return leftSidebar.GetComponent<RectTransform>().anchorMax.x;
	}

	public string GetFormatedStringFromMiliseconds(int miliseconds, bool showCenths = true)
	{
		string text = Mathf.Floor(miliseconds / 60000).ToString("00");
		string text2 = Mathf.Floor(miliseconds / 1000 % 60).ToString("00");
		string text3 = Mathf.Floor(miliseconds / 10 % 100).ToString("00");
		return ((!(text != "00")) ? string.Empty : (text + ":")) + text2 + ((!showCenths) ? string.Empty : ("," + text3));
	}
}
