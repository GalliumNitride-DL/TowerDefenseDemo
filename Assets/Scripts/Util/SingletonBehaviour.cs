
using UnityEngine;

namespace TowerDefenseDemo
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject(typeof(T).Name).AddComponent<T>();
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning($"[Singleton] More than 1 instances of {GetType()} exist.");
                Destroy(this); return;
            }
            instance = this as T;
        }

        protected virtual void OnEnable()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning($"[Singleton] More than 1 instances of {GetType()} exist.");
                Destroy(this); return;
            }
            instance = this as T;
        }

        protected virtual void OnDisable()
        {
            instance = null;
        }

    }
}