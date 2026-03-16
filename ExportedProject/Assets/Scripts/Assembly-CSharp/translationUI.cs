using System.Collections;
using SmartLocalization;
using UnityEngine;
using UnityEngine.UI;

public class translationUI : MonoBehaviour
{
	public string keyForLocalization = string.Empty;

	private void Awake()
	{
		StartCoroutine(TranslateText());
	}

	public IEnumerator TranslateText()
	{
		yield return null;
		TranslateUItext();
	}

	private void OnEnable()
	{
		TranslateUItext();
	}

	public void TranslateUItext()
	{
		Text textComponent = GetComponent<Text>();
		if (textComponent == null)
		{
			return;
		}
		if (keyForLocalization == string.Empty)
		{
			keyForLocalization = textComponent.text;
		}
		if (LanguageManager.Instance == null)
		{
			return;
		}
		string translatedValue = LanguageManager.Instance.GetTextValue(keyForLocalization);
		if (!string.IsNullOrEmpty(translatedValue))
		{
			textComponent.text = translatedValue;
		}
	}
}
