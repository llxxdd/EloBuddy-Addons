#region

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;
using SimplisticTemplate.Champion.Diana.Utils;

#endregion

namespace SimplisticTemplate.Champion.Diana.Modes
{
    // ReSharper disable once InconsistentNaming
    internal static class Combo
    {
        private static AIHeroClient Me
        {
            get { return ObjectManager.Player; }
        }

        public static void Execute()
        {
            var target = TargetSelector.GetTarget(Diana.Q.Range, DamageType.Magical);
            if (!target.IsValidTarget()) return;
            var useRGap = GameMenu.ComboMenu["useRGap"].Cast<CheckBox>().CurrentValue;
            var useRKS = GameMenu.ComboMenu["useRKS"].Cast<CheckBox>().CurrentValue;
            var useRnoBuff = GameMenu.ComboMenu["useRKS"].Cast<CheckBox>().CurrentValue;
            var useQ = GameMenu.ComboMenu["useQ"].Cast<CheckBox>().CurrentValue;
            var useW = GameMenu.ComboMenu["useW"].Cast<CheckBox>().CurrentValue;
            var useE = GameMenu.ComboMenu["useE"].Cast<CheckBox>().CurrentValue;
            var useR = GameMenu.ComboMenu["useR"].Cast<CheckBox>().CurrentValue;
            var useRMax = GameMenu.ComboMenu["useRMax"].Cast<Slider>().CurrentValue;
            var hasManaR = Me.Mana >= Me.Spellbook.GetSpell(SpellSlot.R).SData.Mana;
            var hasManaQ = Me.Mana >= Me.Spellbook.GetSpell(SpellSlot.Q).SData.Mana;

            var targetGap = TargetSelector.GetTarget(2000, DamageType.Magical);
            if (targetGap.IsValidTarget() && Diana.R.IsReady() && Diana.Q.IsReady() && useRGap &&
                Me.GetSpellDamage(targetGap, SpellSlot.Q) >= targetGap.Health && hasManaQ && hasManaR &&
                targetGap.Distance(Me.Position) > Diana.Q.Range)
            {
                var minions =
                    EntityManager.MinionsAndMonsters.GetLaneMinions()
                        .Where(
                            a =>
                                a.Distance(targetGap.ServerPosition) < Diana.Q.Range &&
                                a.Distance(Me.Position) < Diana.R.Range)
                        .OrderBy(a => a.Distance(targetGap.ServerPosition))
                        .FirstOrDefault();
                // ReSharper disable once PossibleNullReferenceException
                if (!minions.IsValidTarget() || !Diana.R.IsInRange(minions) ||
                    targetGap.Distance(minions.ServerPosition) >= Diana.Q.Range || !Diana.Q.IsReady() ||
                    !Diana.R.IsReady()) return;

                Diana.R.Cast(minions);

                if (Diana.Q.IsReady() && useQ &&
                    Diana.Q.IsInRange(target) && Diana.Q.GetPrediction(target).HitChance >= HitChance.High)

                {
                    var pos = Diana.Q.GetPrediction(target).CastPosition;
                    Diana.Q.Cast(pos);
                    return;
                }
            }

            if ((Diana.R.IsReady() && target.HasBuff("dianamoonlight") && Diana.R.IsInRange(target)) ||
                (Diana.R.IsReady() && useRKS && target.CountEnemiesInRange(Diana.Q.Range) <= useRMax &&
                 Me.GetSpellDamage(target, SpellSlot.R) - 20 > target.Health) ||
                (Diana.R.IsReady() && useRnoBuff && ComboDamage(target) - 100 > target.Health + 50) && useR)
            {
                Diana.R.Cast(target);
            }

            if (Diana.Q.IsReady() && useQ &&
                Diana.Q.IsInRange(target) && Diana.Q.GetPrediction(target).HitChance >= HitChance.High)

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

        [SuppressMessage("ReSharper", "InvertIf")]
        public static void MisayaCombo()
        {
            Orbwalker.OrbwalkTo(Game.CursorPos);

            var target = TargetSelector.GetTarget(Diana.R.Range, DamageType.Magical);
            var targetwithbuff =
                EntityManager.Heroes.Enemies.Where(a => a.HasBuff("dianamoonlight"))
                    .OrderBy(a => a.Distance(Me.Position) <= Diana.R.Range)
                    .FirstOrDefault();
            if (targetwithbuff.IsValidTarget()) target = targetwithbuff;
            if (!target.IsValidTarget() || !Diana.R.IsReady() || !Diana.Q.IsReady()) return;

            if (Diana.R.IsInRange(target)) Diana.R.Cast(target);
            if (Diana.W.IsInRange(target) && Diana.E.IsReady()) Diana.W.Cast(target);
            if (Diana.E.IsInRange(target) && Diana.E.IsReady()) Diana.E.Cast(target);
            if (Diana.Q.IsReady() &&
                Diana.Q.IsInRange(target) && Diana.Q.GetPrediction(target).HitChance >= HitChance.Medium)

            {
                var pos = Diana.Q.GetPrediction(target).CastPosition;
                Diana.Q.Cast(pos);
            }
        }

        public static float ComboDamage(Obj_AI_Base target)
        {
            var damage = 0d;

            if (Diana.Q.IsReady())
            {
                damage += Me.GetSpellDamage(target, SpellSlot.Q);
            }

            if (Diana.W.IsReady())
            {
                damage += Me.GetSpellDamage(target, SpellSlot.W);
            }

            if (Diana.E.IsReady())
            {
                damage += Me.GetSpellDamage(target, SpellSlot.E);
            }

            if (Diana.R.IsReady())
            {
                damage += Me.GetSpellDamage(target, SpellSlot.R);
            }

            if (Me.Health > Me.GetAutoAttackDamage(target)*4)
            {
                damage += Me.GetAutoAttackDamage(target)*4;
            }
            else
            {
                damage += Me.GetAutoAttackDamage(target)*2;
            }

            return (float) damage;
        }
    }
}