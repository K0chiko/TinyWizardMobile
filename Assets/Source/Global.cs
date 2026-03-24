using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quinn.Source 
{
    #pragma warning disable CS0414 
    
    public class Global : MonoBehaviour
    {
        public static Global Instance { get; private set; }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;

            Debug.Log("[Global] Автоматическая инициализация...");
            
            GameObject prefab = Resources.Load<GameObject>("Globals");

            if (prefab == null)
            {
                Debug.LogError("[Global] Error: No 'Globals' prefabs in 'Resources!' folder ");
                return;
            }
            
            GameObject instanceObject = Instantiate(prefab);
            
            instanceObject.name = "GlobalSystems (Auto-Created)";
            
            Instance = instanceObject.GetComponent<Global>();
            
            DontDestroyOnLoad(instanceObject);
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Debug.LogWarning($"[Global] На сцене обнаружен дубликат Global на объекте '{gameObject.name}'. Уничтожаю дубликат.");
                Destroy(gameObject);
                return; 
            }
            
            Physics2D.callbacksOnDisable = false;
            
            Debug.Log("[Global] Система готова.");
        }
    }
}