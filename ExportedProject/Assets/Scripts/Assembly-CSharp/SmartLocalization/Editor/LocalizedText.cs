using System;
using UnityEngine;
using UnityEngine.UI;

namespace SmartLocalization.Editor
{
	[RequireComponent(typeof(Text))]
	public class LocalizedText : MonoBehaviour
	{
		public string localizedKey = "INSERT_KEY_HERE";

		private Text textObject;

		private void Start()
		{
			textObject = GetComponent<Text>();
			LanguageManager instance = LanguageManager.Instance;
			instance.OnChangeLanguage = (ChangeLanguageEventHandler)Delegate.Combine(instance.OnChangeLanguage, new ChangeLanguageEventHandler(OnChangeLanguage));
			OnChangeLanguage(instance);
		}

		private void OnDestroy()
		{
			if (LanguageManager.HasInstance)
			{
				LanguageManager instance = LanguageManager.Instance;
				instance.OnChangeLanguage = (ChangeLanguageEventHandler)Delegate.Remove(instance.OnChangeLanguage, new ChangeLanguageEventHandler(OnChangeLanguage));
			}
		}

		private void OnChangeLanguage(LanguageManager languageManager)
		{
			textObject.text = LanguageManager.Instance.GetTextValue(localizedKey);
		}
	}
}
