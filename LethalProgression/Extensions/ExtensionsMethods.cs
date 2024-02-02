using UnityEngine;

namespace LethalProgression.Extensions
{
    internal static class ExtensionsMethods
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }
        public static T GetOrAddComponentInChildren<T>(this GameObject go) where T : Component
        {
            T component = go.GetComponentInChildren<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }
        public static T GetOrAddComponentInParent<T>(this GameObject go) where T : Component
        {
            T component = go.GetComponentInParent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }
    }
}
