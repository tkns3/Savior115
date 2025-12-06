using System;
using System.Reflection;
using HarmonyLib;
using TMPro;

namespace Savior115.Patches
{
    [HarmonyPatch]
    internal static class EndOfLevelViewControllerPatch
    {
        // internal 型なので string で Type を取得する
        static MethodBase TargetMethod()
        {
            // "BeatSaviorUI.UI.EndOfLevelViewController.SetData(PlayData, BeatmapLevel)"
            var type = AccessTools.TypeByName("BeatSaviorUI.UI.EndOfLevelViewController");
            return AccessTools.Method(type, "SetData");
        }

        // Postfix
        //  - playData: SetData の第 1 引数（型は object で受ける）
        //  - ___leftAverage / ___rightAverage: private フィールドに Harmony でアクセス
        static void Postfix(
            object playData,
            TextMeshProUGUI ___leftAverage,
            TextMeshProUGUI ___rightAverage
        )
        {
            if (playData == null) return;

            try
            {
                var playDataType = playData.GetType();

                float GetHandAccuracy(string handPropertyName)
                {
                    // playData.Left / playData.Right
                    var handProp = playDataType.GetProperty(
                        handPropertyName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                    var hand = handProp?.GetValue(playData);
                    if (hand == null) return 0f;

                    // hand.Accuracy
                    var accProp = hand.GetType().GetProperty(
                        "Accuracy",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                    var accObj = accProp?.GetValue(hand);
                    if (accObj == null) return 0f;

                    // Accuracy.Sum()
                    var sumMethod = accObj.GetType().GetMethod(
                        "Sum",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                    if (sumMethod == null) return 0f;

                    var result = sumMethod.Invoke(accObj, null);
                    return Convert.ToSingle(result);
                }

                // PlayData.Left.Accuracy.Sum() / PlayData.Right.Accuracy.Sum()
                float leftAcc = GetHandAccuracy("Left");
                float rightAcc = GetHandAccuracy("Right");

                // 元コードと同じ ratio 計算: Sum() / 115f
                float leftRatio = leftAcc / 115f;
                float rightRatio = rightAcc / 115f;

                // 1行目: percentage（切り捨て 2 桁）+ "%"
                string leftPercentText = FormatUtil.Truncate2(leftRatio * 100f) + "%";
                string rightPercentText = FormatUtil.Truncate2(rightRatio * 100f) + "%";

                // 2行目: Accuracy（切り捨て 2 桁）
                string leftAccuracyText = FormatUtil.Truncate2(leftAcc);
                string rightAccuracyText = FormatUtil.Truncate2(rightAcc);

                // 改行して 2 行表示
                ___leftAverage.text = $"{leftPercentText}\n{leftAccuracyText}";
                ___rightAverage.text = $"{rightPercentText}\n{rightAccuracyText}";
            }
            catch (Exception e)
            {
                // ログがあるならここで出す。なければ削除して問題ない。
                // Plugin.Log?.Warn($"EndOfLevelViewControllerPatch.Postfix failed: {e}");
            }
        }
    }
}
