using SmartLocalization;
using UnityEngine;
using UnityEngine.UI;

public class MapButton : MonoBehaviour
{
	public Text mapName;

	public static string currentMapNiceName;

	public void SetMapNameInGameOver()
	{
		currentMapNiceName = GetComponentInChildren<Text>().text.Replace("\n", " ");
		mapName.text = currentMapNiceName;
		Text text = mapName;
		text.text = text.text + " - " + LanguageManager.Instance.GetTextValue("Records");
	}

	public static string getMapNameInGameOver()
	{
		return currentMapNiceName;
	}
}
