using UnityEngine;
using UnityEditor;

public class PlayerPrefsEditor : EditorWindow
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    public static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("All PlayerPrefs have been cleared successfully!");

        // Показываем диалоговое окно с подтверждением
        EditorUtility.DisplayDialog("PlayerPrefs Cleared",
            "All PlayerPrefs data has been successfully cleared.", "OK");
    }

    // Альтернативный вариант с подтверждением
    [MenuItem("Tools/Clear PlayerPrefs with Confirmation")]
    public static void ClearPlayerPrefsWithConfirmation()
    {
        if (EditorUtility.DisplayDialog("Clear PlayerPrefs?",
            "Are you sure you want to clear all PlayerPrefs data? This action cannot be undone.",
            "Yes", "No"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("All PlayerPrefs have been cleared successfully!");
            EditorUtility.DisplayDialog("Success",
                "All PlayerPrefs data has been cleared.", "OK");
        }
    }

    // Продвинутая версия с окном для выборочной очистки
    [MenuItem("Tools/Advanced PlayerPrefs Cleaner")]
    public static void ShowAdvancedWindow()
    {
        GetWindow<AdvancedPlayerPrefsCleaner>("PlayerPrefs Cleaner");
    }
}

public class AdvancedPlayerPrefsCleaner : EditorWindow
{
    private string specificKey = "";
    private bool showAllKeys = false;
    private Vector2 scrollPosition;

    void OnGUI()
    {
        GUILayout.Label("PlayerPrefs Cleaner", EditorStyles.boldLabel);

        // Очистка всех данных
        if (GUILayout.Button("Clear ALL PlayerPrefs", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear ALL PlayerPrefs?",
                "This will delete EVERYTHING saved in PlayerPrefs. Are you sure?",
                "Yes, clear everything", "Cancel"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("All PlayerPrefs cleared");
                EditorUtility.DisplayDialog("Success",
                    "All PlayerPrefs have been cleared.", "OK");
            }
        }

        GUILayout.Space(10);

        // Очистка конкретного ключа
        GUILayout.Label("Clear Specific Key:", EditorStyles.label);
        specificKey = EditorGUILayout.TextField("Key:", specificKey);

        if (GUILayout.Button("Clear Specific Key"))
        {
            if (!string.IsNullOrEmpty(specificKey))
            {
                PlayerPrefs.DeleteKey(specificKey);
                PlayerPrefs.Save();
                Debug.Log($"PlayerPrefs key '{specificKey}' has been cleared");
                EditorUtility.DisplayDialog("Success",
                    $"Key '{specificKey}' has been cleared.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error",
                    "Please enter a key name to clear.", "OK");
            }
        }

        GUILayout.Space(10);

        // Показать все существующие ключи (только для демонстрации)
        showAllKeys = EditorGUILayout.Foldout(showAllKeys, "Show All PlayerPrefs (Debug)");
        if (showAllKeys)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

            // Примечание: невозможно просто перебрать все ключи в PlayerPrefs
            // Нужно заранее знать имена ключей для отображения
            EditorGUILayout.HelpBox(
                "Note: Unity doesn't provide a way to list all PlayerPrefs keys.\n" +
                "Use the Unity Editor's 'Edit > Clear All PlayerPrefs' or manually delete registry/plist files for full cleanup.",
                MessageType.Info);

            EditorGUILayout.EndScrollView();
        }
    }
}