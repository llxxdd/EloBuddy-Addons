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
        private static AIHeroClient Me
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
                Me.Distance(target) < (Fizz.Q.Range - 50) + (Fizz.E.Range + 350) && ComboDamage(target) > target.Health &&
                Fizz.R.GetPrediction(target).HitChance >= HitChance.High)
            {
                CastR(target, HitChance.High);
                Fizz.E.Cast(Me.ServerPosition.Extend(target.ServerPosition, Fizz.E.Range - 1).To3D());
                Fizz.E.Cast(Me.ServerPosition.Extend(target.ServerPosition, Fizz.E.Range - 1).To3D());
                Fizz.Q.Cast(target);
                Fizz.W.Cast();
            }

            if (useR && Fizz.R.IsReady())
            {
                if (ComboDamage(target) > target.Health)
                {
                    CastR(target, HitChance.Medium);
                }

                if (Me.GetSpellDamage(target, SpellSlot.R) >= target.Health)
                {
                    CastR(target, HitChance.High);
                }
            }

            if (Fizz.E.IsReady() && useE && useQ && Me.Distance(target) <= (Fizz.E.Range + 350) &&
                Me.Mana >= Me.Spellbook.GetSpell(SpellSlot.Q).SData.Mana + Me.Spellbook.GetSpell(SpellSlot.E).SData.Mana &&
                Fizz.Q.IsReady(2))
            {
                Fizz.E.Cast(Me.ServerPosition.Extend(target.ServerPosition, Fizz.E.Range - 1).To3D());
                Fizz.E.Cast(Me.ServerPosition.Extend(target.ServerPosition, Fizz.E.Range - 1).To3D());
            }

            if (Fizz.E.IsReady() && useE && useQ && Me.Distance(target) <= Fizz.E.Range &&
                Me.Mana >= Me.Spellbook.GetSpell(SpellSlot.Q).SData.Mana + Me.Spellbook.GetSpell(SpellSlot.E).SData.Mana &&
                Fizz.Q.IsReady(2))
            {
                Fizz.E.Cast(target);
            }

            if (!Fizz.Q.IsReady() && useE && Fizz.E.IsReady() && Me.GetSpellDamage(target, SpellSlot.E) > target.Health)
            {
                Fizz.E.Cast(target);
            }

            if (Fizz.Q.IsReady() && useQ)
            {
                Fizz.Q.Cast(target);
            }

            if (useW && useWMode == 0 && Fizz.W.IsReady() && Me.IsInAutoAttackRange(target))
            {
                Fizz.W.Cast();
            }


            if (useW && useWMode == 1 && Fizz.W.IsReady() && Me.IsInAutoAttackRange(target))
            {
                Fizz.W.Cast();
            }
        }

        private static void CastR(Obj_AI_Base target, HitChance h)
        {
            if (Fizz.R.GetPrediction(target).HitChance >= h)
            {
                var pred = Fizz.R.GetPrediction(target).CastPosition;
                Fizz.R.Cast((Me.ServerPosition.Extend(pred, Fizz.R.Range).To3D()));
            }
        }

        public static float ComboDamage(Obj_AI_Base target)
        {
            var damage = 0d;

            if (Fizz.Q.IsReady(3))
            {
                damage += Me.GetSpellDamage(target, SpellSlot.Q);
            }

            if (Fizz.W.IsReady(5))
            {
                damage += Me.GetSpellDamage(target, SpellSlot.W);
            }

            if (Fizz.E.IsReady(2))
            {
                damage += Me.GetSpellDamage(target, SpellSlot.E);
            }

            if (Fizz.R.IsReady(5))
            {
                damage += Me.GetSpellDamage(target, SpellSlot.R);
            }

            damage += Me.GetAutoAttackDamage(target)*3;
            return (float) damage;
        }
    }
}