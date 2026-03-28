using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmartLocalization
{
	public class LoadAllLanguages : MonoBehaviour
	{
		private List<string> currentLanguageKeys;

		private List<SmartCultureInfo> availableLanguages;

		private LanguageManager languageManager;

		private Vector2 valuesScrollPosition = Vector2.zero;

		private Vector2 languagesScrollPosition = Vector2.zero;

		private void Start()
		{
			languageManager = LanguageManager.Instance;
			SmartCultureInfo deviceCultureIfSupported = languageManager.GetDeviceCultureIfSupported();
			if (deviceCultureIfSupported != null)
			{
				languageManager.ChangeLanguage(deviceCultureIfSupported);
			}
			else
			{
				Debug.Log("The device language is not available in the current application. Loading default.");
			}
			if (languageManager.NumberOfSupportedLanguages > 0)
			{
				currentLanguageKeys = languageManager.GetAllKeys();
				availableLanguages = languageManager.GetSupportedLanguages();
			}
			else
			{
				Debug.LogError("No languages are created!, Open the Smart Localization plugin at Window->Smart Localization and create your language!");
			}
			LanguageManager instance = LanguageManager.Instance;
			instance.OnChangeLanguage = (ChangeLanguageEventHandler)Delegate.Combine(instance.OnChangeLanguage, new ChangeLanguageEventHandler(OnLanguageChanged));
		}

		private void OnDestroy()
		{
			if (LanguageManager.HasInstance)
			{
				LanguageManager instance = LanguageManager.Instance;
				instance.OnChangeLanguage = (ChangeLanguageEventHandler)Delegate.Remove(instance.OnChangeLanguage, new ChangeLanguageEventHandler(OnLanguageChanged));
			}
		}

		private void OnLanguageChanged(LanguageManager languageManager)
		{
			currentLanguageKeys = languageManager.GetAllKeys();
		}

		private void OnGUI()
		{
			if (languageManager.NumberOfSupportedLanguages <= 0)
			{
				return;
			}
			if (languageManager.CurrentlyLoadedCulture != null)
			{
				GUILayout.Label("Current Language:" + languageManager.CurrentlyLoadedCulture.ToString());
			}
			GUILayout.BeginHorizontal();
			GUILayout.Label("Keys:", GUILayout.Width(460f));
			GUILayout.Label("Values:", GUILayout.Width(460f));
			GUILayout.EndHorizontal();
			valuesScrollPosition = GUILayout.BeginScrollView(valuesScrollPosition);
			foreach (string currentLanguageKey in currentLanguageKeys)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(currentLanguageKey, GUILayout.Width(460f));
				GUILayout.Label(languageManager.GetTextValue(currentLanguageKey), GUILayout.Width(460f));
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
			languagesScrollPosition = GUILayout.BeginScrollView(languagesScrollPosition);
			foreach (SmartCultureInfo availableLanguage in availableLanguages)
			{
				if (GUILayout.Button(availableLanguage.nativeName, GUILayout.Width(960f)))
				{
					languageManager.ChangeLanguage(availableLanguage);
				}
			}
			GUILayout.EndScrollView();
		}
	}
}
