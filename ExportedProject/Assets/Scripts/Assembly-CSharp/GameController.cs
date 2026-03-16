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

	// Leaderboard UI references (kept for possible future use)
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

	private int maxPlayerScoreForCurrentMode = -1;

	private bool isAppInBackground;

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
		AudioSource[] audioSources = GetComponents<AudioSource>();
		if (audioSources.Length > 1)
		{
			cantCreatePlanetSound = audioSources[1];
		}
		if (musicClips != null && musicClips.Length > 0 && audioSources.Length > 0)
		{
			StartCoroutine(LoopMusic());
		}
		gameOver = true;
		LoadPlayerPrefs();
		if (langCode == string.Empty)
		{
			SystemLanguage systemLanguage = Application.systemLanguage;
			if (systemLanguage == SystemLanguage.Spanish)
				langCode = "es";
			else
				langCode = "en";
		}
		LanguageManager.Instance.ChangeLanguage(langCode);
		optionsUI.transform.Find("OptionsPanel/Visuals Scrollable/Scrollable content/Visuals/" + langCode + "_Button/Frame_" + langCode).gameObject.SetActive(true);
		optionDescription.text = LanguageManager.Instance.GetTextValue("OptionDescription");
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
	}

	private void LoadPlayerPrefs()
	{
		if (PlayerPrefs.HasKey("Minimalistic") && PlayerPrefs.GetInt("Minimalistic") == 1)
			minimalistic = true;
		frameRealistic.SetActive(!minimalistic);
		frameMinimalistic.SetActive(minimalistic);
		GameObject.Find("GameBackground").GetComponent<MeshRenderer>().material = ((!minimalistic) ? realisticBackground : flatBackground);
		SetSliderValue(0, (!PlayerPrefs.HasKey("TrailLength")) ? 5f : PlayerPrefs.GetInt("TrailLength"));
		SetSliderValue(1, (!PlayerPrefs.HasKey("TrailSize")) ? 50f : PlayerPrefs.GetInt("TrailSize"));
		SetSliderValue(2, (!PlayerPrefs.HasKey("TrailOpacity")) ? 50f : PlayerPrefs.GetInt("TrailOpacity"));
		SetSliderValue(3, (!PlayerPrefs.HasKey("FrontierOpacity")) ? 100f : PlayerPrefs.GetInt("FrontierOpacity"));
		SetSliderValue(4, (float)((!PlayerPrefs.HasKey("SoundVolume")) ? 80 : PlayerPrefs.GetInt("SoundVolume")) / 100f);
		SetSliderValue(5, (float)((!PlayerPrefs.HasKey("MusicVolume")) ? 20 : PlayerPrefs.GetInt("MusicVolume")) / 100f);
		langCode = ((!PlayerPrefs.HasKey("LangCode")) ? string.Empty : PlayerPrefs.GetString("LangCode"));
	}

	private void SetSliderValue(int index, float value)
	{
		if (optionsSliders != null && index >= 0 && index < optionsSliders.Length && optionsSliders[index] != null)
		{
			optionsSliders[index].value = value;
		}
	}

	public void ChangeLanguage(string languageCode)
	{
		LanguageManager.Instance.ChangeLanguage(languageCode);
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(translationUI));
		for (int i = 0; i < array.Length; i++)
			((translationUI)array[i]).TranslateUItext();
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
			string empty = (currentMode == "timeAttack") ? GetFormatedStringFromMiliseconds((secondsForGameOver + 1) * 1000, false) : GetFormatedStringFromMiliseconds(secondsInTimerMods * 10);
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
			newSavedMapPrototype = UnityEngine.Object.Instantiate(currentSystemSandboxMap);
		if (savedGamesRowPrototype == null)
		{
			savedGamesRowPrototype = GameObject.Find("SavedMapsPrototype");
			savedGamesRowPrototype.SetActive(false);
		}
		deleteSavedGameButton.transform.SetParent(savedGamesRowPrototype.transform.parent, false);
		deleteSavedGameButton.SetActive(false);
		if (savedMaps == null)
			savedMaps = GameObject.Find("SavedMaps");
		foreach (Transform item in savedMaps.transform)
			UnityEngine.Object.Destroy(item.gameObject);
		foreach (Transform item2 in savedGamesRowPrototype.transform)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(item2.gameObject);
			gameObject.transform.SetParent(savedMaps.GetComponent<Transform>(), false);
		}
		if (scrollable == null)
			scrollable = savedMaps.transform.parent.GetComponent<RectTransform>();
		if (scrollableHeight == 0f)
			scrollableHeight = scrollable.offsetMin.y;
		string[] files = Directory.GetFiles(Application.persistentDataPath, "sys*", SearchOption.TopDirectoryOnly);
		int num = -1;
		if (files.Length > 0)
		{
			foreach (string text in files)
			{
				int num2 = int.Parse(text.Replace(Application.persistentDataPath, string.Empty).Replace("/", string.Empty).Replace("\\", string.Empty).Replace("sys", string.Empty));
				if (num2 > num)
					num = num2;
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
			scrollable.parent.gameObject.SetActive(false);
	}

	private GameObject GetSavedSystemButtonAux()
	{
		Transform transform = savedMaps.transform.Find("ModeSavedSystemButton") ?? savedMaps.transform.Find("ModeSavedSystemButton(Clone)");
		return (transform != null) ? transform.gameObject : null;
	}

	private void CreateButtonToSaveGame(int numSave, GameObject buttonAux)
	{
		buttonAux.name = "ModeSavedSystem" + numSave + "SandboxButton";
		buttonAux.SetActive(true);
		string mapName = "ModeSavedSystem" + numSave + "Sandbox";
		buttonAux.GetComponent<Button>().onClick.AddListener(delegate { HandleMapsButton(mapName); });
		buttonAux.transform.Find("Text").GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("Game") + "\n" + (numSave + 1);
	}

	public void LoadSystem(string sysName)
	{
		if (!(sysName != string.Empty))
			return;
		string text = Path.Combine(Application.persistentDataPath, sysName);
		if (!File.Exists(text))
			return;
		int[] progress = new int[1] { 1 };
		string text2 = Path.Combine(Application.persistentDataPath, "toLoadTemp");
		lzip.decompress_File(text, text2, progress);
		string text3 = File.ReadAllText(Path.Combine(text2, "s"));
		Directory.Delete(text2, true);
		string[] array = text3.Replace("N", "N_").Replace("U", "U_").Split('_');
		foreach (string text4 in array)
		{
			if (text4 == string.Empty)
				continue;
			char[] separator = new char[8] { 'M', 'X', 'Y', 'A', 'B', 'N', 'I', 'U' };
			string[] array3 = text4.Split(separator);
			int num = int.Parse(array3[0]);
			float mass = float.Parse(array3[1]);
			Vector2 vector = new Vector2(float.Parse(array3[2]), float.Parse(array3[3]));
			Vector2 initialMovement = new Vector2(float.Parse(array3[4]), float.Parse(array3[5]));
			bool initialObject = text4.Contains("I");
			bool flag = text4.Contains("U");
			GameObject gameObject = UnityEngine.Object.Instantiate(flag ? ultraDensePrefab : planetPrefab, vector, base.transform.rotation) as GameObject;
			GameObject gameObject2 = GameObject.Find("ModeSavedSystem" + sysName.Replace("sys", string.Empty) + "Sandbox");
			gameObject.transform.parent = gameObject2.transform;
			GravitationalObject component = gameObject.GetComponent<GravitationalObject>();
			if (!flag)
				gameObject.transform.Find("Surface").GetComponent<MeshRenderer>().material.mainTexture = planetTextures[num];
			gameObject.GetComponent<Rigidbody2D>().mass = mass;
			component.initialMovement = initialMovement;
			component.initialObject = initialObject;
			component.randomTexture = false;
			component.insidePlayableArea = true;
			component.addsScore = false;
			gameObject.transform.position = new Vector3(vector.x, vector.y);
		}
	}

	private IEnumerator SavingSystemMessage()
	{
		saveButton.GetComponent<Button>().interactable = false;
		Text saveButtonText = saveButton.GetComponentInChildren<Text>();
		saveButtonText.text = LanguageManager.Instance.GetTextValue("Saving") + "...";
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
			string text2 = string.Empty;
			foreach (GameObject gameObject in array)
			{
				GravitationalObject component = gameObject.GetComponent<GravitationalObject>();
				Rigidbody2D component2 = gameObject.GetComponent<Rigidbody2D>();
				text2 += component.GetNumTexture();
				text2 = text2 + "M" + component2.mass;
				text2 = text2 + "X" + gameObject.transform.position.x + "Y" + gameObject.transform.position.y;
				text2 = text2 + "A" + component2.velocity.x / Time.fixedDeltaTime + "B" + component2.velocity.y / Time.fixedDeltaTime;
				if (component.initialObject)
					text2 += "I";
				text2 += (component.objectType == "neutron" || component.objectType == "blackHole") ? "U" : "N";
			}
			File.WriteAllText(text, text2);
			while (File.Exists(text + "ys" + fileNumberToSave))
				fileNumberToSave++;
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
		yield return new WaitUntil(() => isActionConfirmed.HasValue);
		if (isActionConfirmed.Value)
		{
			GameObject parentButton = deleteSavedGameButton.transform.parent.gameObject;
			int numberSavedSystem = int.Parse(parentButton.name.Replace("ModeSavedSystem", string.Empty).Replace("SandboxButton", string.Empty));
			string filePath = Path.Combine(Application.persistentDataPath, "sys" + numberSavedSystem);
			if (File.Exists(filePath))
				File.Delete(filePath);
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

	public void setActiveAction(string newActiveAction) { activeAction = newActiveAction; }
	public string getActiveAction() { return activeAction; }

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
				mapDescription.text = LanguageManager.Instance.GetTextValue(mapNameToPlay.Contains("Mode") ? mapNameToPlay : "Welcome");
			}
			previousMapButtonText = GameObject.Find(mapNameToPlay + "Button").transform.Find("Text").GetComponent<Text>();
			previousMapTitle = previousMapButtonText.text;
			previousMapButtonText.text = LanguageManager.Instance.GetTextValue("CurrentPressedButtonText");
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
		mapDescription.text = GetObjectsText().Replace("#", string.Empty).Replace("neutronStar", "neutron star").Replace("blackHole", "black hole").Replace("brownDwarf", "brown dwarf");
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
			case "sandbox": mapDescription.text = LanguageManager.Instance.GetTextValue("SandboxDescription"); break;
			case "timeAttack": mapDescription.text = LanguageManager.Instance.GetTextValue("TimeAttackDescription"); break;
			case "destroyer": mapDescription.text = LanguageManager.Instance.GetTextValue("DestroyerDescription"); break;
			case "supernova": mapDescription.text = LanguageManager.Instance.GetTextValue("SupernovaDescription"); break;
			default: mapDescription.text = LanguageManager.Instance.GetTextValue("Welcome"); break;
		}
	}

	public void SetMode(string modeName) { currentMode = modeName; ResetMapSelectionButton(); }
	public string GetMode() { return currentMode; }
	public string GetMapName() { return mapNameToPlay; }

	public void HandleOptionsButton(string selectedOptionName)
	{
		switch (selectedOptionName)
		{
			case "VisualsOption":
				optionDescription.text = LanguageManager.Instance.GetTextValue("VisualsOptionDescription");
				break;
			case "Realistic":
				optionDescription.text = LanguageManager.Instance.GetTextValue("RealisticDescription");
				minimalistic = false;
				GameObject.Find("GameBackground").GetComponent<MeshRenderer>().material = realisticBackground;
				foreach (GameObject go in GameObject.FindGameObjectsWithTag("Planet"))
				{
					GravitationalObject g = go.GetComponent<GravitationalObject>();
					StartCoroutine(g.delayedColorUpdate());
					g.GetSurface().GetComponent<MeshRenderer>().enabled = true;
					if (g.objectType == "star" || g.objectType == "brownDwarf")
						go.transform.Find("starSurface").GetComponent<MeshRenderer>().enabled = true;
					if (g.objectType == "star")
					{
						go.transform.Find("Crown").gameObject.SetActive(true);
						go.transform.Find("StarLight").gameObject.SetActive(true);
					}
					go.transform.Find("Surface2D").GetComponent<MeshRenderer>().enabled = false;
				}
				PlayerPrefs.SetInt("Minimalistic", 0);
				break;
			case "Minimalistic":
				optionDescription.text = LanguageManager.Instance.GetTextValue("MinimalisticDescription");
				minimalistic = true;
				GameObject.Find("GameBackground").GetComponent<MeshRenderer>().material = flatBackground;
				foreach (GameObject go in GameObject.FindGameObjectsWithTag("Planet"))
				{
					GravitationalObject g = go.GetComponent<GravitationalObject>();
					go.transform.Find("Surface2D").GetComponent<MeshRenderer>().enabled = true;
					StartCoroutine(g.delayedColorUpdate());
					g.GetSurface().GetComponent<MeshRenderer>().enabled = false;
					if (g.objectType == "star" || g.objectType == "brownDwarf")
						go.transform.Find("starSurface").GetComponent<MeshRenderer>().enabled = false;
					if (g.objectType == "star")
					{
						go.transform.Find("Crown").gameObject.SetActive(false);
						go.transform.Find("StarLight").gameObject.SetActive(false);
					}
				}
				PlayerPrefs.SetInt("Minimalistic", 1);
				break;
			case "TrailLength":
				int len = (int)planetPrefab.GetComponentInChildren<TrailRenderer>().time;
				optionDescription.text = string.Format(LanguageManager.Instance.GetTextValue("TrailLengthDescription"), len);
				foreach (GameObject go in GameObject.FindGameObjectsWithTag("Planet"))
					go.GetComponentInChildren<TrailRenderer>().time = len;
				PlayerPrefs.SetInt("TrailLength", len);
				break;
			case "TrailSize":
				int size = planetPrefab.GetComponent<GravitationalObject>().GetTrailSizeFactor();
				optionDescription.text = string.Format(LanguageManager.Instance.GetTextValue("TrailSizeDescription"), size);
				foreach (GameObject go in GameObject.FindGameObjectsWithTag("Planet"))
				{
					go.GetComponent<GravitationalObject>().SetTrailSizeFactor(size);
					StartCoroutine(go.GetComponent<GravitationalObject>().delayedColorUpdate());
				}
				PlayerPrefs.SetInt("TrailSize", size);
				break;
			case "TrailOpacity":
				int opacity = planetPrefab.GetComponent<GravitationalObject>().GetTrailOpacityFactor();
				optionDescription.text = string.Format(LanguageManager.Instance.GetTextValue("TrailOpacityDescription"), 100 - opacity);
				foreach (GameObject go in GameObject.FindGameObjectsWithTag("Planet"))
				{
					go.GetComponent<GravitationalObject>().SetTrailOpacityFactor(opacity);
					StartCoroutine(go.GetComponent<GravitationalObject>().delayedColorUpdate());
				}
				PlayerPrefs.SetInt("TrailOpacity", opacity);
				break;
			case "FrontierOpacity":
				int front = visibleFrontier.GetComponent<VisibleGeometry>().GetOpacityFactor();
				optionDescription.text = string.Format(LanguageManager.Instance.GetTextValue("FrontierOpacityDescription"), 100 - front);
				PlayerPrefs.SetInt("FrontierOpacity", front);
				break;
			case "AudioOption":
				optionDescription.text = LanguageManager.Instance.GetTextValue("AudioOptionDescription");
				break;
			case "SoundVolume":
				Camera.main.GetComponent<AudioSource>().volume = 0.8f * soundVolume;
				GetComponents<AudioSource>()[1].volume = 0.7f * soundVolume;
				AudioSource[] prefabSrc = planetPrefab.GetComponentsInChildren<AudioSource>();
				prefabSrc[0].volume = 0.4f * soundVolume;
				prefabSrc[1].volume = 0.7f * soundVolume;
				prefabSrc[2].volume = soundVolume;
				prefabSrc[3].volume = soundVolume;
				foreach (GameObject go in GameObject.FindGameObjectsWithTag("Planet"))
				{
					AudioSource[] src = go.GetComponentsInChildren<AudioSource>();
					src[0].volume = 0.4f * soundVolume;
					src[1].volume = 0.7f * soundVolume;
					src[2].volume = soundVolume;
					src[3].volume = soundVolume;
				}
				barycenter.GetComponentInChildren<AudioSource>().volume = soundVolume;
				supernovaSound.volume = soundVolume;
				menuClickSound.volume = 0.4f * soundVolume;
				optionDescription.text = string.Format(LanguageManager.Instance.GetTextValue("SoundVolumeDescription"), (int)(soundVolume * 100f));
				if (!playingSoundSampleForConfig)
					StartCoroutine(SoundSampleForConfig());
				PlayerPrefs.SetInt("SoundVolume", (int)(soundVolume * 100f));
				break;
			case "MusicVolume":
				int musicVol = (int)(GetComponents<AudioSource>()[0].volume * 100f);
				optionDescription.text = string.Format(LanguageManager.Instance.GetTextValue("MusicVolumeDescription"), musicVol);
				PlayerPrefs.SetInt("MusicVolume", musicVol);
				break;
			case "SocialOption":
			case "ShowLeaderboards":
			case "ShowAchievements":
				// Social features removed
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

	public void SetSoundVolume(float newVolume) { soundVolume = newVolume; }
	public float GetSoundVolume() { return soundVolume; }

	public void BackToMainMenu()
	{
		gameOver = true;
		pausePanel.SetActive(false);
		MapSelectorPanel.SetActive(true);
		Camera.main.GetComponent<CameraControl>().objectToLookAt = null;
		Camera.main.transform.position = new Vector3(784f, -1443f, Camera.main.transform.position.z);
		activeAction = "creatingObjects";
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
	}

	public void BackFromOptions()
	{
		optionsUI.SetActive(false);
		if (gameOver)
			MapSelectorPanel.SetActive(true);
		else
			pausePanel.SetActive(true);
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
		string scoreFormat = LanguageManager.Instance.GetTextValue("Score");
		if (string.IsNullOrEmpty(scoreFormat))
		{
			scoreFormat = "Score: {0}";
		}
		scoreText.GetComponent<Text>().text = string.Format(scoreFormat, 0);
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
		}
		SetNumberObjects();
		NumberObjectsPanel.SetActive(true);
		Camera.main.GetComponent<CameraControl>().objectToLookAt = barycenter;
	}

	private void LoadNewObjectsFromMap(string newMapName, bool mapZoom = false)
	{
		DeleteObjects();
		if (currentMap != null)
			UnityEngine.Object.Destroy(currentMap);
		Transform mapTransform = maps.transform.Find(newMapName);
		if (mapTransform == null)
		{
			Debug.LogError("Map not found: " + newMapName);
			return;
		}
		currentMap = UnityEngine.Object.Instantiate(mapTransform.gameObject);
		CopyMapToSandbox();
		DelayedNormalizePlanetsSpeed();
		currentMap.SetActive(true);
		CameraControl cameraControl = Camera.main.GetComponent<CameraControl>();
		MapLoader mapLoader = currentMap.GetComponent<MapLoader>();
		cameraControl.minZoom = mapLoader.minimumZoom;
		if (mapZoom)
		{
			if (cameraControl.zoomSlider != null)
			{
				Slider zoomSliderComponent = cameraControl.zoomSlider.GetComponent<Slider>();
				if (zoomSliderComponent != null)
				{
					zoomSliderComponent.value = mapLoader.initialSliderZoom;
				}
			}
			Camera.main.transform.position = new Vector3(0f, 0f, Camera.main.transform.position.z);
		}
		gravityFactor = mapLoader.mapGravityFactor;
		Transform transform = barycenter.transform.Find("Playable Region");
		int playableRegionRadius = mapLoader.playableRegionRadius;
		transform.localScale = new Vector3(playableRegionRadius, playableRegionRadius);
		float num = mapLoader.initialSliderZoom / 200f;
		transform.GetComponentInChildren<LineRenderer>().SetWidth(num, num);
		Camera.main.GetComponent<CameraControl>().SetCurrentPlayableRegionScale(transform.localScale);
		barycenter.transform.Find("Outer Playable Region").localScale = transform.localScale * 1.4f;
		if (gamePaused)
			ContinueGame();
	}

	private static void DeleteObjects()
	{
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Planet"))
			UnityEngine.Object.Destroy(obj);
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Effect"))
			UnityEngine.Object.Destroy(obj);
	}

	private void CopyMapToSandbox()
	{
		currentSystemSandboxMap.SetActive(false);
		foreach (Transform item in currentSystemSandboxMap.transform)
			UnityEngine.Object.Destroy(item.gameObject);
		foreach (Transform item2 in currentMap.transform)
		{
			GameObject go = UnityEngine.Object.Instantiate(item2.gameObject);
			go.transform.rotation = item2.rotation;
			go.transform.position = item2.position;
			go.transform.parent = currentSystemSandboxMap.transform;
			go.name = item2.name;
		}
	}

	public int GetNumberObjectsCreatedByPlayer()
	{
		int count = 0;
		foreach (GameObject go in GameObject.FindGameObjectsWithTag("Planet"))
			if (!go.GetComponent<GravitationalObject>().initialObject)
				count++;
		return count;
	}

	public int GetNumberObjectsNotCreatedByPlayer()
	{
		int count = 0;
		foreach (GameObject go in GameObject.FindGameObjectsWithTag("Planet"))
			if (go.GetComponent<GravitationalObject>().initialObject)
				count++;
		return count;
	}

	public int GetNumberObjects() { return GameObject.FindGameObjectsWithTag("Planet").Length; }
	public void ReloadNewMap()
	{
		gameoverPanel.SetActive(false);
		LoadNewPlayableMap(mapNameToPlay);
	}
	public bool currentMapAddsScore() { return currentMap.GetComponent<MapLoader>().addsScore; }

	private IEnumerator LoopMusic()
	{
		AudioSource[] audioSources = GetComponents<AudioSource>();
		if (audioSources.Length == 0 || musicClips == null || musicClips.Length == 0)
		{
			yield break;
		}
		AudioSource music = audioSources[0];
		while (true)
		{
			music.clip = musicClips[UnityEngine.Random.Range(0, musicClips.Length)];
			music.Play();
			yield return new WaitForSeconds(music.clip.length + 5f);
		}
	}

	public int GetScore() { return score; }

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
						if (cantCreatePlanetSound != null)
						{
							cantCreatePlanetSound.Play();
						}
					}
			}
			else if (creatingPlanet && Input.GetButtonUp("Fire1"))
			{
				creatingPlanet = false;
				if (IsInPlayableRegion(Camera.main.ScreenToWorldPoint(planetOriginPoint)))
				{
					planetSpeedPoint = Input.mousePosition;
					GameObject go = UnityEngine.Object.Instantiate((planetPrefab.GetComponent<Rigidbody2D>().mass < GravitationalObject.maxStarMass) ? planetPrefab : ultraDensePrefab,
						ScreenPointToWorld(planetOriginPoint), base.transform.rotation) as GameObject;
					if (planetPrefab.GetComponent<Rigidbody2D>().mass >= GravitationalObject.maxStarMass)
						go.GetComponent<Rigidbody2D>().mass = planetPrefab.GetComponent<Rigidbody2D>().mass / 2f;
					Vector2 delta = ScreenPointToWorld(planetSpeedPoint) - ScreenPointToWorld(planetOriginPoint);
					go.GetComponent<GravitationalObject>().initialMovement = delta * Mathf.Pow(planetSpeedFactorOnCreate, planetSpeedPowOnCreate);
				}
				else
				{
					cantCreatePlanetSound.Play();
				}
			}
		}
		else if (Input.GetButtonDown("Fire1") && !gamePaused && !gameOver && activeAction == "creatingObjects" &&
				 currentTotalObjectsNumber >= currentMap.GetComponent<MapLoader>().MaxObjects &&
				 Input.mousePosition.x > (float)Camera.main.pixelWidth * GetLeftSideBarRelativeWidth())
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
		return Physics2D.Raycast(point, Vector2.zero, float.PositiveInfinity, 1 << 8); // layer 8
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
		NumberObjectsPanel.transform.Find("NumberObjectsText").GetComponent<Text>().text =
			LanguageManager.Instance.GetTextValue("Objects") + " " + currentTotalObjectsNumber + "/" + currentMap.GetComponent<MapLoader>().MaxObjects;
	}

	public bool GetCreatingPlanet() { return creatingPlanet; }
	public Vector2 GetPlanetOriginPoint() { return planetOriginPoint; }
	public Vector2 GetBarycenter2D() { return barycenter2D; }
	public bool GetGameOver() { return gameOver; }

	public void PauseGame()
	{
		if (!gameOver)
		{
			gamePaused = true;
			Time.timeScale = 0f;
			pausePanel.SetActive(true);
		}
	}
	public bool IsGamePaused() { return gamePaused; }

	public void ContinueGame()
	{
		gamePaused = false;
		Time.timeScale = 1f;
		pausePanel.SetActive(false);
		if (!saveButton.GetComponent<Button>().interactable)
		{
			saveButton.GetComponent<Button>().interactable = true;
			Text t = saveButton.GetComponentInChildren<Text>();
			t.text = LanguageManager.Instance.GetTextValue("SAVE");
			t.color = new Color(0.2509804f, 64f / 85f, 0.95686275f);
		}
	}

	public void ShareScreenshotButton()
	{
		// Android sharing removed
	}

	private void OnApplicationFocus(bool focusStatus)
	{
		isAppInBackground = !focusStatus;
	}

	private string GetExtraTextForSharing()
	{
		// Original sharing text builder, kept as is
		string text = string.Empty;
		if (gamePaused)
		{
			text = GetObjectsText();
		}
		else if (gameOver)
		{
			if (currentMap.GetComponent<MapLoader>().isDestroyMode)
			{
				text += "I completed the " + MapButton.getMapNameInGameOver() + " - Destroyer mode in " + GetFormatedStringFromMiliseconds(secondsInTimerMods * 10) + ".";
				if (maxPlayerScoreForCurrentMode > -1)
				{
					text = (secondsInTimerMods * 10 >= maxPlayerScoreForCurrentMode) ?
						text + " My best time is " + GetFormatedStringFromMiliseconds(maxPlayerScoreForCurrentMode) + "." :
						text + " A new personal best time!";
				}
			}
			else if (currentMap.GetComponent<MapLoader>().isSupernovaMode)
			{
				text += "My stars were safe in " + MapButton.getMapNameInGameOver() + " - Supernova mode for " + GetFormatedStringFromMiliseconds(secondsInTimerMods * 10) + ".";
				if (maxPlayerScoreForCurrentMode > -1)
				{
					text = (maxPlayerScoreForCurrentMode >= secondsInTimerMods * 10) ?
						text + " My record is " + GetFormatedStringFromMiliseconds(maxPlayerScoreForCurrentMode) + "." :
						text + " A new personal time record!";
				}
			}
			else
			{
				text += "I scored " + score + " points in the " + MapButton.getMapNameInGameOver() + " - Time Attack mode.";
				if (maxPlayerScoreForCurrentMode > -1)
				{
					if (maxPlayerScoreForCurrentMode < score)
						text += " A new personal record!";
					else
						text += " My record is " + maxPlayerScoreForCurrentMode + " points!";
				}
			}
			text += " Can you beat me?";
		}
		return text + " #MakeYourSolarSystem";
	}

	private static string GetObjectsText()
	{
		string text = string.Empty;
		int planets = 0, brownDwarfs = 0, stars = 0, neutrons = 0, blackHoles = 0;
		foreach (GameObject go in GameObject.FindGameObjectsWithTag("Planet"))
		{
			string type = go.GetComponent<GravitationalObject>().GetObjectType();
			if (type == string.Empty) planets++;
			else if (type == "brownDwarf") brownDwarfs++;
			else if (type == "star") stars++;
			else if (type == "neutron") neutrons++;
			else if (type == "blackHole") blackHoles++;
		}
		if (planets + brownDwarfs + stars + neutrons + blackHoles == 0)
			return "Just empty #space.";
		if (blackHoles > 0)
		{
			text += blackHoles + " #blackHole" + (blackHoles > 1 ? "s" : "");
			if (neutrons + stars + brownDwarfs + planets > 0)
				text += " with ";
		}
		if (neutrons > 0)
		{
			text += neutrons + " #neutronStar" + (neutrons > 1 ? "s" : "");
			if ((stars > 0 && brownDwarfs + planets == 0) || (brownDwarfs > 0 && stars + planets == 0) || (planets > 0 && stars + brownDwarfs == 0))
				text += " and ";
			else if (stars + brownDwarfs + planets > 0)
				text += (blackHoles != 0 ? ", " : " with ");
		}
		if (stars > 0)
		{
			text += stars + " #star" + (stars > 1 ? "s" : "");
			if ((brownDwarfs > 0 && planets == 0) || (planets > 0 && brownDwarfs == 0))
				text += " and ";
			else if (brownDwarfs + planets > 0)
				text += (blackHoles + neutrons != 0 ? ", " : " with ");
		}
		if (brownDwarfs > 0)
		{
			text += brownDwarfs + " #brownDwarf" + (brownDwarfs > 1 ? "s" : "");
			if (planets > 0)
				text += " and ";
			else if (blackHoles + neutrons + stars == 0 && planets > 0)
				text += " with ";
		}
		if (planets > 0)
			text += planets + " #planet" + (planets > 1 ? "s" : "");
		return text + ".";
	}

	public void DelayedNormalizePlanetsSpeed() { StartCoroutine("DelayedNormalizePlanetsSpeedCoroutine"); }
	private IEnumerator DelayedNormalizePlanetsSpeedCoroutine()
	{
		yield return new WaitForFixedUpdate();
		NormalizePlanetsSpeed();
	}

	private void NormalizePlanetsSpeed()
	{
		GameObject[] objects = GameObject.FindGameObjectsWithTag("Planet");
		Vector2 vel = Vector2.zero;
		float maxMass = 0f;
		foreach (GameObject go in objects)
		{
			if (go.GetComponent<Rigidbody2D>().mass > maxMass)
			{
				vel = go.GetComponent<Rigidbody2D>().velocity;
				maxMass = go.GetComponent<Rigidbody2D>().mass;
			}
		}
		foreach (GameObject go in objects)
			go.GetComponent<Rigidbody2D>().velocity -= vel;
	}

	public void ClearAllTrails()
	{
		foreach (GameObject go in GameObject.FindGameObjectsWithTag("Planet"))
			foreach (TrailRenderer tr in go.GetComponentsInChildren<TrailRenderer>())
				tr.Clear();
	}
	public void ClearAllTrailsTwice() { StartCoroutine(ClearAllTrailsTwiceRoutine()); }
	public IEnumerator ClearAllTrailsTwiceRoutine()
	{
		ClearAllTrails();
		yield return null;
		ClearAllTrails();
	}

	public Vector2 ScreenPointToWorld(Vector2 screenPoint) { return Camera.main.ScreenPointToRay(screenPoint).origin; }

	private void FixedUpdate() { SetObjectsPosition(); }

	private void SetObjectsPosition()
	{
		GameObject[] objects = GameObject.FindGameObjectsWithTag("Planet");
		if (objects.Length == 0)
			return;
		barycenter2D = Vector2.zero;
		totalMassOfSystem = 0f;
		foreach (GameObject A in objects)
		{
			partialForce = Vector2.zero;
			foreach (GameObject B in objects)
			{
				if (A != B && (A.GetComponent<GravitationalObject>().insidePlayableArea || B.GetComponent<GravitationalObject>().insidePlayableArea))
					ApplyGravity(A.GetComponent<Rigidbody2D>(), B.GetComponent<Rigidbody2D>());
			}
			A.GetComponent<Rigidbody2D>().AddForce(partialForce);
			barycenter2D += (Vector2)(A.GetComponent<Rigidbody2D>().mass * A.GetComponent<Rigidbody2D>().transform.position);
			totalMassOfSystem += A.GetComponent<Rigidbody2D>().mass;
		}
		barycenter2D /= totalMassOfSystem;
		barycenter2D = new Vector2(Mathf.RoundToInt(barycenter2D.x * 100f) / 100f, Mathf.RoundToInt(barycenter2D.y * 100f) / 100f);
		barycenter.transform.position = new Vector3(barycenter2D.x, barycenter2D.y, 0f);
		if (centerObjectsInWorld)
			DoCenterObjectsInWorld(objects);
	}

	private void DoCenterObjectsInWorld(GameObject[] objects)
	{
		foreach (GameObject go in objects)
		{
			go.transform.position -= barycenter.transform.position;
			go.transform.position = new Vector2(Mathf.RoundToInt(go.transform.position.x * 100f) / 100f, Mathf.RoundToInt(go.transform.position.y * 100f) / 100f);
		}
		barycenter.transform.position = Vector3.zero;
	}

	private void ApplyGravity(Rigidbody2D A, Rigidbody2D B)
	{
		Vector2 dir = B.transform.position - A.transform.position;
		float dist = dir.magnitude;
		dir.Normalize();
		float force = 6.674f * (A.mass * B.mass * planetMassFactor) / (dist * dist) * gravityFactor;
		partialForce += 2f * (dir * force);
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

	public void SetScore(int newScore) { score = newScore; }

	public void GameOver()
	{
		gameOver = true;
		CancelInvoke("TimerCounter");
		activeAction = "creatingObjects";
		creatingPlanet = false;
		GameOverPanel.transform.Find("LeaderBoard").gameObject.SetActive(false);
		leftPanelCover.SetActive(true);
		gameoverPanel.SetActive(true);
		// Leaderboard loading removed
	}

	public GameObject GetCurrentMap() { return currentMap; }

	public int GetPlanetScore(float baseMass)
	{
		float s;
		if (baseMass <= 0.01f) s = baseMass * 1000f + 9.9f;
		else if (baseMass <= 0.1f) s = baseMass * 111f + 18.9f;
		else if (baseMass <= 1f) s = baseMass * 11.1f + 28.9f;
		else if (baseMass <= 50f) s = baseMass * 0.408f + 39.6f;
		else if (baseMass <= 1000f) s = baseMass * 0.0421f + 57.9f;
		else if (baseMass <= 16000f) s = baseMass * 0.0267f + 73.3f;
		else if (baseMass <= 400000f) s = baseMass * 0.00104f + 483f;
		else s = baseMass * 0.000167f + 833f;
		return Mathf.CeilToInt(s);
	}

	public void AddScorePoints(int eventScore)
	{
		if (eventScore < 0)
		{
			Text t = minusScoreText.GetComponent<Text>();
			t.text = eventScore.ToString();
			t.CrossFadeAlpha(1f, 0f, true);
			t.CrossFadeAlpha(0f, 3f, false);
		}
		else if (eventScore > 0)
		{
			Text t = plusScoreText.GetComponent<Text>();
			t.text = "+" + eventScore;
			t.CrossFadeAlpha(1f, 0f, true);
			t.CrossFadeAlpha(0f, 1f, false);
		}
		score += eventScore;
		if (score < 0) score = 0;
		scoreText.GetComponent<Text>().text = string.Format(LanguageManager.Instance.GetTextValue("Score"), score);
	}

	private void ColorsForPlanetTextures()
	{
		planetTextures = Resources.LoadAll<Texture2D>("Textures/Planets");
		numPlanetTextures = planetTextures.Length;
		planetFlatColors = new Color32[numPlanetTextures];
		for (int i = 0; i < numPlanetTextures; i++)
		{
			Color32 avg = AverageColorFromTexture(planetTextures[i]);
			float h, s, v;
			ColorToHSV(avg, out h, out s, out v);
			h += UnityEngine.Random.Range(-20f, 30f);
			s -= 0.2f;
			if (s < 0.1f) s = 0.1f;
			s *= UnityEngine.Random.Range(3, 9);
			v *= UnityEngine.Random.Range(15, 25) / 10f;
			planetFlatColors[i] = ColorFromHSV(h, s, v, 1f);
		}
	}

	public Texture2D GetPlanetTexture(int num) { return planetTextures[num]; }
	private Color32 AverageColorFromTexture(Texture2D tex)
	{
		Color32[] pixels = tex.GetPixels32();
		int n = pixels.Length;
		float r = 0, g = 0, b = 0;
		for (int i = 0; i < n; i++)
		{
			r += pixels[i].r;
			g += pixels[i].g;
			b += pixels[i].b;
		}
		return new Color32((byte)(r / n), (byte)(g / n), (byte)(b / n), 0);
	}
	public int GetNumPlanetTextures() { return numPlanetTextures; }
	public Color32 GetColorForPlanet(int num) { return planetFlatColors[num]; }

	public static Color ColorFromHSV(float h, float s, float v, float a = 1f)
	{
		if (s == 0f) return new Color(v, v, v, a);
		float hue = h / 60f;
		int i = (int)hue;
		float f = hue - i;
		float p = v * (1f - s);
		float q = v * (1f - s * f);
		float t = v * (1f - s * (1f - f));
		switch (i)
		{
			case 0: return new Color(v, t, p, a);
			case 1: return new Color(q, v, p, a);
			case 2: return new Color(p, v, t, a);
			case 3: return new Color(p, q, v, a);
			case 4: return new Color(t, p, v, a);
			default: return new Color(v, p, q, a);
		}
	}

	public static void ColorToHSV(Color color, out float h, out float s, out float v)
	{
		float min = Mathf.Min(color.r, color.g, color.b);
		float max = Mathf.Max(color.r, color.g, color.b);
		float delta = max - min;
		v = max;
		if (max != 0f)
		{
			s = delta / max;
			if (Mathf.Approximately(min, max))
			{
				h = -1f;
				return;
			}
			if (color.r == max)
				h = (color.g - color.b) / delta;
			else if (color.g == max)
				h = 2f + (color.b - color.r) / delta;
			else
				h = 4f + (color.r - color.g) / delta;
			h *= 60f;
			if (h < 0f) h += 360f;
		}
		else
		{
			s = 0f;
			h = -1f;
		}
	}

	public float GetLeftSideBarRelativeWidth() { return leftSidebar.GetComponent<RectTransform>().anchorMax.x; }

	public string GetFormatedStringFromMiliseconds(int miliseconds, bool showCenths = true)
	{
		string minutes = Mathf.Floor(miliseconds / 60000).ToString("00");
		string seconds = Mathf.Floor(miliseconds / 1000 % 60).ToString("00");
		string cenths = Mathf.Floor(miliseconds / 10 % 100).ToString("00");
		return (minutes != "00" ? minutes + ":" : "") + seconds + (showCenths ? "," + cenths : "");
	}
}
