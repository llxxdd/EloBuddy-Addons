#region

using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;
using SimplisticTemplate.Champion.Fizz.Utils;

#endregion

namespace SimplisticTemplate.Champion.Fizz.Modes
{
    internal static class Combo
    {
        public static AIHeroClient Me
        {
            get { return ObjectManager.Player; }
        }

        public static void Execute()
        {
            var target = TargetSelector.GetTarget(Fizz.R.Range, DamageType.Magical);
            if (!target.IsValidTarget()) return;

            var useRGap = GameMenu.ComboMenu["useRGap"].Cast<CheckBox>().CurrentValue;
            var useQ = GameMenu.ComboMenu["useQ"].Cast<CheckBox>().CurrentValue;
            var useW = GameMenu.ComboMenu["useW"].Cast<CheckBox>().CurrentValue;
            var useE = GameMenu.ComboMenu["useE"].Cast<CheckBox>().CurrentValue;
            var useR = GameMenu.ComboMenu["useR"].Cast<CheckBox>().CurrentValue;
            var useWMode = GameMenu.MiscMenu["useWMode"].Cast<Slider>().CurrentValue;

            if (useRGap && Fizz.Q.IsReady() && Fizz.W.IsReady() && Fizz.E.IsReady() && Fizz.R.IsReady() &&
                Me.Distance(target) < (Fizz.Q.Range - 50) + (Fizz.E.Range + 350) && ComboDamage(target) > target.Health)
            {

                CastR(target);
                Fizz.E.Cast(Me.ServerPosition.Extend(target.ServerPosition, Fizz.E.Range - 1).To3D());
                Fizz.E.Cast(Me.ServerPosition.Extend(target.ServerPosition, Fizz.E.Range - 1).To3D());

                Fizz.W.Cast();
                Fizz.Q.Cast(target);
            }
               
                if (useR && Fizz.R.IsReady())
                {

                    if (Me.GetSpellDamage(target, SpellSlot.R) > target.Health)
                    {
                        CastR(target);
                        return;
                    }

                    if (ComboDamage(target) > target.Health)
                    {
                        CastR(target);
                    }
                }

                if (useW && useWMode == 0 && Fizz.W.IsReady() && Me.IsInAutoAttackRange(target))
                {
                    Fizz.W.Cast();
                }

                if (Fizz.Q.IsReady() && useQ)
                {
                    Fizz.Q.Cast(target);
                }

                if (Fizz.E.IsReady() && useE)
                {
                    Fizz.E.Cast(target);
                }

                if (useW && useWMode == 1 && Fizz.W.IsReady() && Me.IsInAutoAttackRange(target))
                {
                    Fizz.W.Cast();
                }
            }

        private static void CastR(Obj_AI_Base target)
        {
            if (Fizz.R.GetPrediction(target).HitChance >= HitChance.High)
            {

                var pred = Fizz.R.GetPrediction(target).CastPosition;
                Fizz.R.Cast((Me.ServerPosition.Extend(pred, Fizz.R.Range).To3D()));

            }
        }

        private static float ComboDamage(Obj_AI_Base target)
        {
            var damage = 0d;

            if (Fizz.Q.IsReady())
            {
                damage += Me.GetSpellDamage(target, SpellSlot.Q);
            }

            if (Fizz.W.IsReady())
            {
                damage += Me.GetSpellDamage(target, SpellSlot.W);
            }

            if (Fizz.E.IsReady())
            {
                damage += Me.GetSpellDamage(target, SpellSlot.E);
            }

            if (Fizz.R.IsReady())
            {
                damage += Me.GetSpellDamage(target, SpellSlot.R);
            }

            return (float) damage;
        }

        
    }
}