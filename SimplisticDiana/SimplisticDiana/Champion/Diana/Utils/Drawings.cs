#region

using System;
using System.Drawing;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using SimplisticTemplate.Champion.Diana.Modes;
using Color = System.Drawing.Color;

#endregion

namespace SimplisticTemplate.Champion.Diana.Utils
{
    internal static class Drawings
    {
        private static readonly Text KillableText = new Text("",
            new Font(FontFamily.GenericSansSerif, 9, FontStyle.Bold));

        private static AIHeroClient Me
        {
            get { return ObjectManager.Player; }
        }

        public static void OnDraw(EventArgs args)
        {
            var disable = GameMenu.DrawMenu["disable"].Cast<CheckBox>().CurrentValue;
            var drawQ = GameMenu.DrawMenu["drawQ"].Cast<CheckBox>().CurrentValue;
            var drawW = GameMenu.DrawMenu["drawW"].Cast<CheckBox>().CurrentValue;
            var drawE = GameMenu.DrawMenu["drawE"].Cast<CheckBox>().CurrentValue;
            var drawR = GameMenu.DrawMenu["drawR"].Cast<CheckBox>().CurrentValue;
            var drawRPred = GameMenu.DrawMenu["drawRPred"].Cast<CheckBox>().CurrentValue;
            if (disable) return;

            if (drawQ && Diana.Q.IsReady())
            {
                new Circle {Color = Color.White, Radius = Diana.Q.Range, BorderWidth = 2f}.Draw(Me.Position);
            }

            if (drawW && Diana.W.IsReady())
            {
                new Circle {Color = Color.White, Radius = Diana.W.Range, BorderWidth = 2f}.Draw(Me.Position);
            }

            if (drawE && Diana.E.IsReady())
            {
                new Circle {Color = Color.White, Radius = Diana.E.Range, BorderWidth = 2f}.Draw(Me.Position);
            }

            if (drawR && Diana.R.IsReady())
            {
                new Circle {Color = Color.Crimson, Radius = Diana.R.Range, BorderWidth = 2f}.Draw(Me.Position);
            }

            var target = TargetSelector.GetTarget(Diana.Q.Range, DamageType.Magical);
            if (drawRPred && Diana.Q.IsReady() && target.IsValidTarget())
            {
                new Circle {Color = Color.Crimson, Radius = 100, BorderWidth = 2f}.Draw(
                    Diana.Q.GetPrediction(target).CastPosition);
            }
        }

        public static void OnDamageDraw(EventArgs args)
        {
            var disable = GameMenu.DrawMenu["disable"].Cast<CheckBox>().CurrentValue;
            var drawDamage = GameMenu.DrawMenu["drawDamage"].Cast<CheckBox>().CurrentValue;
            var useFarm = GameMenu.FarmingMenu["useFarm"].Cast<KeyBind>().CurrentValue;
            if (disable) return;
            if (useFarm)
            {
                KillableText.Position = Drawing.WorldToScreen(Me.Position) - new Vector2(40, -40);
                KillableText.Color = Color.DarkGreen;
                KillableText.TextValue = "Spell Farming Enabled";
                KillableText.Draw();
            }
            else
            {
                KillableText.Position = Drawing.WorldToScreen(Me.Position) - new Vector2(40, -40);
                KillableText.Color = Color.Firebrick;
                KillableText.TextValue = "Spell Farming Disabled";
                KillableText.Draw();
            }
            if (drawDamage)
            {
                foreach (var ai in EntityManager.Heroes.Enemies)
                {
                    if (ai.IsValidTarget())
                    {
                        var drawn = 0;
                        if (Combo.ComboDamage(ai) >= ai.Health && drawn == 0)
                        {
                            KillableText.Position = Drawing.WorldToScreen(ai.Position) - new Vector2(40, -40);
                            KillableText.Color = Color.Firebrick;
                            KillableText.TextValue = "100% Killable";
                            KillableText.Draw();
                            drawn = 1;
                        }

                        if (Combo.ComboDamage(ai) + 300 >= ai.Health && drawn == 0)
                        {
                            KillableText.Position = Drawing.WorldToScreen(ai.Position) - new Vector2(40, -40);
                            KillableText.Color = Color.AntiqueWhite;
                            KillableText.TextValue = "50% Killable - HP Left: " +
                                                     Math.Abs((int) ai.Health - (int) Combo.ComboDamage(ai));
                            KillableText.Draw();
                            drawn = 1;
                        }

                        if (Combo.ComboDamage(ai) < ai.Health - 100 && drawn == 0)
                        {
                            KillableText.Position = Drawing.WorldToScreen(ai.Position) - new Vector2(40, -40);
                            KillableText.Color = Color.ForestGreen;
                            KillableText.TextValue = "Not Killable - HP Left: " +
                                                     Math.Abs((int) ai.Health - (int) Combo.ComboDamage(ai));
                            KillableText.Draw();
                        }
                    }
                }
            }
        }
    }
}