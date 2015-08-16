using Hectic7;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class HecticHelper
{
    public static int FirstIndex<T>(this IEnumerable<T> col, Func<T, bool> match = null)
    {
        var index = 0;
        foreach (var item in col)
        {
            if (match(item))
                return index;
            index++;
        }

        return -1;
    }

    public static int WrapToEnd(this int val, int max)
    {
        if (max > 0)
        {
            if (val < 0)
                return max - 1;

            if (val >= max)
                return 0;
        }

        return val;
    }

    /// <summary>
    /// Rounds each axis to nearest int
    /// </summary>
    public static Vector3 Snap(this Vector3 src)
    {
        //return src;
        return new Vector3(Mathf.Round(src.x), Mathf.Round(src.y), Mathf.Round(src.z));
    }

    /// <summary>
    /// Sets z to zero
    /// </summary>
    public static Vector3 Flatten(this Vector3 src)
    {
        return new Vector3(src.x, src.y, 0);
    }

    public static T[] GetRandomVals<T>(this List<T> src, int count)
    {
        var result = new T[count];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = src[UnityEngine.Random.Range(0, src.Count)];
        }
        return result;
    }

    public static T GetRandomVal<T>(this List<T> src)
    {
        return src[UnityEngine.Random.Range(0, src.Count)];
    }

    public static T AddNew<T>(this List<T> list, T item)
    {
        list.Add(item);
        return item;
    }

    public static T EnumFromFields<T>(this List<FieldEntry> fields)
    {
        var choice = fields.First(f => f.EnumType == typeof(T)).Choice.Value;
        return (T)(object)choice;
    }
}
   
