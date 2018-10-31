using UnityEngine;

/// <summary>
/// Scene singleton implementaion. A scene singleton is a singleton that generates a gameobject
/// when istanced.
/// <summary>
public class SceneSingleton<T> : MonoBehaviour where T : SceneSingleton<T> {

    private static T m_Instance = null;

    public static T Instance {
        get {
            if (m_Instance == null) {
                m_Instance = FindObjectOfType<T>();

                if (m_Instance == null) {
                    m_Instance = new GameObject(typeof(T).Name).AddComponent<T>();
                }

                DontDestroyOnLoad(m_Instance.gameObject);
            }
            return m_Instance;
        }
    }

    public static bool HasInstance() {
        return m_Instance != null;
    }

}