using SmartLocalization;
using UnityEngine;
using UnityEngine.UI;

public class ObjectTypeFromMass : MonoBehaviour
{
	public void TypeFromMass(float value)
	{
		float num = (0f - value) * (value - 2f);
		num *= 10f;
		num = Mathf.Pow(10f, num - 4f);
		string text = LanguageManager.Instance.GetTextValue("Mass") + "\n";
		if ((double)num <= 0.0002)
		{
			text += LanguageManager.Instance.GetTextValue("Meteoroid");
		}
		else if ((double)num > 0.0002 && (double)num <= 0.003)
		{
			text += LanguageManager.Instance.GetTextValue("Dwarf planet");
		}
		else if ((double)num > 0.003 && (double)num <= 0.04)
		{
			text += LanguageManager.Instance.GetTextValue("Mercury");
		}
		else if ((double)num > 0.04 && (double)num <= 0.3)
		{
			text += LanguageManager.Instance.GetTextValue("Sub Earth");
		}
		else if ((double)num > 0.3 && num <= 2f)
		{
			text += LanguageManager.Instance.GetTextValue("Earth");
		}
		else if (num > 2f && num <= 7f)
		{
			text += LanguageManager.Instance.GetTextValue("Super Earth");
		}
		else if (num > 7f && num <= 70f)
		{
			text += LanguageManager.Instance.GetTextValue("Neptune");
		}
		else if (num > 70f && num <= 700f)
		{
			text += LanguageManager.Instance.GetTextValue("Jupiter");
		}
		else if (num > 700f && num <= 5000f)
		{
			text += LanguageManager.Instance.GetTextValue("Super Jupiter");
		}
		else if (num > 5000f && num <= 16000f)
		{
			text += LanguageManager.Instance.GetTextValue("Brown dwarf");
		}
		else if (num > 16000f && num <= 42500f)
		{
			text += LanguageManager.Instance.GetTextValue("Red dwarf star");
		}
		else if (num > 42500f && num <= 125000f)
		{
			text += LanguageManager.Instance.GetTextValue("Orange star");
		}
		else if (num > 105000f && num <= 260000f)
		{
			text += LanguageManager.Instance.GetTextValue("Yellow star");
		}
		else if (num > 260000f && num <= 440000f)
		{
			text += LanguageManager.Instance.GetTextValue("White star");
		}
		else if (num > 440000f && num <= 600000f)
		{
			text += LanguageManager.Instance.GetTextValue("Bluish white star");
		}
		else if (num > 600000f && num <= 800000f)
		{
			text += LanguageManager.Instance.GetTextValue("Blue giant star");
		}
		else if (num > 800000f)
		{
			text += LanguageManager.Instance.GetTextValue("Neutron star");
		}
		GetComponent<Text>().text = text;
	}
}
