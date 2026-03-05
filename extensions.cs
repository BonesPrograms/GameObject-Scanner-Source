using System;
using System.Linq;
using System.Collections.Generic;
using XRL.World;



namespace DODTS
{
    public static class Extensions
    {
       
        public static void ForEach<T>(this IEnumerable<T> objs, Action<T> action)
        {
            foreach (T obj in objs)
                action(obj);
        }

    }
}