using System;
using System.Collections.Generic;

namespace BoxVRPlaylistManagerNETCore.FitXr.Utility
{
    public static class ShuffleList
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            Random random = new Random();
            int count = list.Count;
            while(count > 1)
            {
                --count;
                int index = random.Next(count + 1);
                T obj = list[index];
                list[index] = list[count];
                list[count] = obj;
            }
        }
    }
}
