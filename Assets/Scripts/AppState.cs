using UnityEngine;

public class AppState : MonoBehaviour
{
    public static AppState I { get; private set; }
    public string selectedDeckId = "";

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }
}
