using TMPro;
using UnityEngine;

public class DeckNameInput : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;

    void Awake()
    {
        inputField.characterLimit = 15; // 15ï∂éöêßå¿
    }

    public string GetDeckName()
    {
        return inputField.text;
    }
    public void SetDeckName(string name)
    {
        inputField.text = name;
    }
}
