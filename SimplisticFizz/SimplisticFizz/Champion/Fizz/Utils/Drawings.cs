#region

using System;
using System.Drawing;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;

#endregion

namespace SimplisticTemplate.Champion.Fizz.Utils
{
    internal static class Drawings
    {
        public static AIHeroClient Me
        {
            get { return ObjectManager.Player; }
        }

        public static void OnDraw(EventArgs args)
        {
            if (GameMenu.DrawMenu["disable"].Cast<CheckBox>().CurrentValue) return;
            var drawQ = GameMenu.DrawMenu["drawQ"].Cast<CheckBox>().CurrentValue;
            var drawW = GameMenu.DrawMenu["drawW"].Cast<CheckBox>().CurrentValue;
            var drawE = GameMenu.DrawMenu["drawE"].Cast<CheckBox>().CurrentValue;
            var drawR = GameMenu.DrawMenu["drawR"].Cast<CheckBox>().CurrentValue;
            var drawRPred = GameMenu.DrawMenu["drawRPred"].Cast<CheckBox>().CurrentValue;

            if (drawQ && Fizz.Q.IsReady())
            {
                new Circle {Color = Color.White, Radius = Fizz.Q.Range, BorderWidth = 2f}.Draw(Me.Position);
            }

            if (drawW && Fizz.W.IsReady())
            {
                new Circle {Color = Color.White, Radius = Fizz.W.Range, BorderWidth = 2f}.Draw(Me.Position);
            }

            if (drawE && Fizz.E.IsReady())
            {
                new Circle {Color = Color.White, Radius = Fizz.E.Range, BorderWidth = 2f}.Draw(Me.Position);
            }

            if (drawR && Fizz.R.IsReady())
            {
                new Circle {Color = Color.Crimson, Radius = Fizz.R.Range, BorderWidth = 2f}.Draw(Me.Position);
            }
            var target = TargetSelector.GetTarget(Fizz.R.Range, DamageType.Magical);
            if (drawRPred && Fizz.R.IsReady() && target.IsValidTarget())
            {
                new Circle {Color = Color.Crimson, Radius = Fizz.R.Width, BorderWidth = 2f}.Draw(
                    Fizz.R.GetPrediction(target).CastPosition);
            }
        }
    }
}