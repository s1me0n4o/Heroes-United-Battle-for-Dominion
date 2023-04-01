﻿using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Utils
{

    /// <summary>
    /// Pure Singleton implementation
    /// </summary>
    /// <typeparam name="T">Type of the singleton class.</typeparam>
    public class Singleton<T> where T : class, new()
    {
        private static T _instance;

        public static T Instance => _instance ??= new T();
    }

    /// <summary>
    /// MonoBehaviour Singleton implementation
    /// </summary>
    /// <typeparam name="T">Type of the singleton class.</typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        private static readonly object _lock = new();

        /// <summary>
        /// !!! Do NOT call this Instance method in Awake of any MonoBehaviour
        ///  - recreates the SingletonInstance from the Hierarchy
        /// </summary>
        public static T Instance
        {
            get
            {
                if (!Application.isPlaying)
                    return null;
                // if (_isApplicationQuitting) return null;
                if (_instance != null)
                    return _instance;

                lock (_lock)
                {
                    var instances = FindObjectsOfType(typeof(T));
                    if (instances == null || instances.Length == 0)
                    {
                        var delimited = typeof(T).ToString().Split(new char[] { '.' });
                        var gameObject = new GameObject(delimited[^1]);
                        _instance = gameObject.AddComponent<T>();
                    }
                    else if (instances.Length == 1)
                    {
                        _instance = instances[0] as T;
                    }
                    else if (instances.Length > 1)
                    {
                        Debug.LogError("[MonoSingleton] Something went really wrong " +
                                       " - there should never be more than 1 singleton!" +
                                       " Reopen the scene might fix it.");
                    }

                    if (_instance is { })
                        DontDestroyOnLoad(_instance.gameObject);

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance)
            {
                DestroyImmediate(gameObject);
                _isApplicationQuitting = false;
            }
            else
            {
                DontDestroyOnLoad(gameObject);
                _instance = gameObject.GetComponent<T>();
            }
        }

        private static bool _isApplicationQuitting = false;

        protected void OnDestroy() => _isApplicationQuitting = true;
    }

    /// <summary>
    /// Use as singleton only for objects active on current Unity scene.
    /// </summary>
    /// <typeparam name="T">Type of the singleton class.</typeparam>
    public class PseudoSingleton<T> : MonoBehaviour where T : MonoBehaviour, new()
    {
        private static T _instance;

        private static readonly object _lock = new();

        public static T Instance
        {
            get
            {
                if (!Application.isPlaying)
                    return null;
                if (_instance != null && !_instance.gameObject.IsDestroyed())
                    return _instance;

                lock (_lock)
                {
                    var instances = FindObjectsOfType(typeof(T));
                    if (instances != null && instances.Length > 1)
                    {
                        Debug.LogError("[MonoSingleton] Something went really wrong " +
                                       " - there should never be more than 1 singleton!" +
                                       " Reopen the scene might fix it.");
                    }

                    _instance = instances?.FirstOrDefault() as T;
                    return _instance;
                }
            }
        }

        protected void OnDestroy() => _instance = null;
    }
} // namespace Utils 