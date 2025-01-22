using UnityEngine;
using System.Collections;

namespace MagicMod
{
    public class CoroutineHandler : MonoBehaviour
    {
        private static CoroutineHandler instance;

        public static CoroutineHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("MagicMod_CoroutineHandler");
                    instance = obj.AddComponent<CoroutineHandler>();
                    DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }

        public void RunCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}