using UnityEngine;

namespace Savior115
{
    internal static class FormatUtil
    {
        // 例: 98.478 -> "98.47" / 113.259 -> "113.25"
        public static string Truncate2(float value)
        {
            float truncated = Mathf.Floor(value * 100f) / 100f;
            return truncated.ToString("0.00");
        }
    }
}
