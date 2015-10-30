#region

using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;
using SimplisticTemplate.Champion.Diana.Utils;

#endregion

namespace SimplisticTemplate.Champion.Diana.Modes
{
    internal static class Harass
    {
        public static void Execute()
        {
            var target = TargetSelector.GetTarget(Diana.Q.Range, DamageType.Magical);
            if (!target.IsValidTarget()) return;

            var useQ = GameMenu.ComboMenu["useQ"].Cast<CheckBox>().CurrentValue;
            var useW = GameMenu.ComboMenu["useW"].Cast<CheckBox>().CurrentValue;
            var useE = GameMenu.ComboMenu["useE"].Cast<CheckBox>().CurrentValue;

            if (Diana.Q.IsReady() && useQ && Diana.Q.GetPrediction(target).HitChance > HitChance.High &&
                Diana.Q.IsInRange(target))
            {
                var pos = Diana.Q.GetPrediction(target).CastPosition;
                Diana.Q.Cast(pos);
            }

            if (useW && Diana.W.IsReady() && Diana.W.IsInRange(target))
            {
                Diana.W.Cast();
            }

            if (useE && Diana.E.IsReady() && Diana.E.IsInRange(target))
            {
                Diana.E.Cast();
            }
        }
    }
}