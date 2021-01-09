using UnityEditor;
using UnityEngine;

namespace DefaultNamespace
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class ImguiUtills
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            
        }
        
        
    }
}