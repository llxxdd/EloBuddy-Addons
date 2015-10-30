#region

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
            var useRMax = GameMenu.ComboMenu["useWMode"].Cast<Slider>().CurrentValue;
            var hasManaE = Me.Mana >= Me.Spellbook.GetSpell(SpellSlot.E).SData.Mana;
            var hasManaW = Me.Mana >= Me.Spellbook.GetSpell(SpellSlot.W).SData.Mana;
            var hasManaQ = Me.Mana >= Me.Spellbook.GetSpell(SpellSlot.Q).SData.Mana;


            var targetGap = TargetSelector.GetTarget(Diana.Q.Range + Diana.R.Range, DamageType.Magical);
            if (Diana.R.IsReady() && useR && useRGap &&
                (targetGap.Health < Me.GetSpellDamage(targetGap, SpellSlot.E) +
                 Me.GetSpellDamage(targetGap, SpellSlot.W) +
                 Me.GetSpellDamage(targetGap, SpellSlot.Q)) && (hasManaE || hasManaW || hasManaQ) &&
                targetGap.IsValidTarget())
            {
                var minions =
                    EntityManager.MinionsAndMonsters.GetLaneMinions()
                        .Where(a => a.Distance(Player.Instance) < Diana.Q.Range + Diana.R.Range)
                        .OrderBy(a => a.Distance(targetGap.ServerPosition) < Diana.E.Range);
                var minion = minions.FirstOrDefault();
                if (minion != null && Diana.R.IsInRange(minion) &&
                    minion.Distance(target.ServerPosition) < Diana.Q.Range)
                {
                    Diana.R.Cast(minion);


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

            if (Diana.R.IsReady() && useRKS && target.CountEnemiesInRange(Diana.Q.Range) <= useRMax ||
                (Diana.R.IsReady() && useRnoBuff && ComboDamage(target) > target.Health) ||
                (Diana.R.IsReady() && target.HasBuff("dianamoonlight")) && Diana.R.IsInRange(target))
            {
                Diana.R.Cast(target);
            }

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
            if (Diana.Q.GetPrediction(target).HitChance < HitChance.Medium && !Diana.Q.IsInRange(target)) return;

            var pos = Diana.Q.GetPrediction(target).CastPosition;
            Diana.Q.Cast(pos);
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

            damage += Me.GetAutoAttackDamage(target)*2;
            return (float) damage;
        }
    }
}