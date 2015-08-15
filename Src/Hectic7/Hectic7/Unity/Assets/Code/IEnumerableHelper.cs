﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class IEnumerableHelper
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
    public static Vector3 Snap(this Vector3 src)
    {
        //return src;
        return new Vector3(Mathf.Round(src.x), Mathf.Round(src.y), Mathf.Round(src.z));
    }
}
   
