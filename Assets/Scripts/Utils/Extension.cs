using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class Extension
    {
        /// <summary>원본을 건드리지 않고 섞은 새 리스트를 돌려준다.</summary>
        public static List<T> Shuffled<T>(this IList<T> list)
        {
            var result = new List<T>(list);
            for (int n = result.Count - 1; n > 0; n--)
            {
                int k = Random.Range(0, n + 1);
                (result[k], result[n]) = (result[n], result[k]);
            }
            return result;
        }

        public static T FindChild<T>(this Transform transform, string name = null,
            bool recursive = true, bool includeInactive = true) where T : Component
        {
            int cnt = transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                Transform child = transform.GetChild(i);
                if (!includeInactive && !child.gameObject.activeInHierarchy) continue;

                T component = child.GetComponent<T>();
                if (component != null && (string.IsNullOrEmpty(name) || child.name == name))
                    return component;

                if (!recursive) continue;

                T found = child.FindChild<T>(name, true, includeInactive);
                if (found != null) return found;
            }

            return null;
        }

        public static List<T> FindChildren<T>(this Transform transform, string name = null,
            bool recursive = true, bool includeInactive = true) where T : Component
        {
            var result = new List<T>();
            Collect(transform, name, recursive, includeInactive, result);
            return result;
        }

        private static void Collect<T>(Transform transform, string name,
            bool recursive, bool includeInactive, List<T> result) where T : Component
        {
            int cnt = transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                Transform child = transform.GetChild(i);
                if (!includeInactive && !child.gameObject.activeInHierarchy) continue;

                T component = child.GetComponent<T>();
                if (component != null && (string.IsNullOrEmpty(name) || child.name == name))
                    result.Add(component);

                if (recursive) Collect(child, name, true, includeInactive, result);
            }
        }

        public static T FindParent<T>(this Transform transform, string name = null,
            bool recursive = true) where T : Component
        {
            Transform tr = transform.parent;
            while (tr != null)
            {
                T component = tr.GetComponent<T>();
                if (component != null && (string.IsNullOrEmpty(name) || tr.name == name))
                    return component;

                if (!recursive) return null;
                tr = tr.parent;
            }

            return null;
        }
    }
}
