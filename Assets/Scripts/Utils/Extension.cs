using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class Extension
    {
        public static List<T> Shuffle<T>(this IList<T> list)
        {
            var result = new List<T>(list);
            int n = result.Count;
    
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                (result[k], result[n]) = (result[n], result[k]);
            }
    
            return result;
        }
    }
}