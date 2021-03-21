using System;
using System.Collections.Generic;
using System.Linq;

namespace BoxVRPlaylistManagerNETCore.FitXr.Tools
{
    public class Format
    {
        public static string SecondsToString(float value) => string.Format("{0:0}m:{1:00}s", (object)Math.Floor(value / 60f), (object)(value % 60f));

        public static void FloatsFromCsvLine(string entry, out float val1, out float val2)
        {
            string[] strArray = entry.Split(',');
            float.TryParse(strArray[1], out val1);
            float.TryParse(strArray[2], out val2);
        }

        public static IEnumerable<KeyValuePair<int, T>> CreateSortedDictionary<T>(
          ReadOnlyDictionary<int, T> unsortedDic)
        {
            return unsortedDic.OrderBy<KeyValuePair<int, T>, int>((Func<KeyValuePair<int, T>, int>)(pair => pair.Key)).Take<KeyValuePair<int, T>>(unsortedDic.Count);
        }

        public static int[] Long2DoubleInt(long a) => new int[2]
        {
      (int) (a & (long) uint.MaxValue),
      (int) (a >> 32)
        };

        public static long DoubleIntToLong(int[] a) => (long)a[1] << 32 | (long)(uint)a[0];

        public static float LbsToKg(float lbs) => lbs * 0.453592f;

        public static float CmToFeet(float cm) => cm * 0.0328084f;

        public static float FeetToInch(float feet) => feet * 12f;

        public static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int num = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays((double)(-1 * num)).Date;
        }
    }
}
