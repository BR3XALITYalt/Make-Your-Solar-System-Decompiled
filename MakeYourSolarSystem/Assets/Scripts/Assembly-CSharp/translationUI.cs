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
		if (keyForLocalization == string.Empty)
		{
			keyForLocalization = GetComponent<Text>().text;
		}
		GetComponent<Text>().text = LanguageManager.Instance.GetTextValue(keyForLocalization);
	}
}
