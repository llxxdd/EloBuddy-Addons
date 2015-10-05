using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;

namespace SimplisticDiana
{
    internal class Spells
    {
        private static SpellSlot ignite;

        public static readonly Spell.Skillshot SpellQ = new Spell.Skillshot(SpellSlot.Q, 830, SkillShotType.Cone, 500,
            1600, 195);

        public static readonly Spell.Active SpellE = new Spell.Active(SpellSlot.E, 350);
        public static readonly Spell.Active SpellW = new Spell.Active(SpellSlot.W, 200);
        public static readonly Spell.Targeted SpellR = new Spell.Targeted(SpellSlot.R, 825);

        public static readonly Spell.Targeted Ignite =
            new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static void Start()
        {
            SpellQ.AllowedCollisionCount = int.MaxValue;

            Orbwalker.ForcedTarget = null;
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();
            if (Config.ComboMenu["misaya"].Cast<KeyBind>().CurrentValue) MisayaCombo();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) Harass();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) WaveClear();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) JungleClear();
            SChoose();
        }

        private static void MisayaCombo()
        {
            var target = TargetSelector.GetTarget(1500, DamageType.Magical);
            if (target == null || !target.IsValid())
            {
                return;
            }

            if (Config.ComboMenu["useQ"].Cast<CheckBox>().CurrentValue &&
                Config.ComboMenu["useR"].Cast<CheckBox>().CurrentValue && SpellQ.IsReady() && SpellR.IsReady() &&
                !target.HasBuff("dianamoonlight"))
            {
                SpellR.Cast(target);
                SpellQ.Cast(target);
            }

            Combo();
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1500, DamageType.Magical);
            if (target == null || !target.IsValid())
            {
                return;
            }

            if (Config.ComboMenu["GapKS"].Cast<CheckBox>().CurrentValue && SpellQ.IsReady() && SpellR.IsReady() && SpellE.IsReady() && target.Health < _Player.GetSpellDamage(target,SpellSlot.E) || target.Health < _Player.GetSpellDamage(target,SpellSlot.W))
            {
                var minions = EntityManager.GetLaneMinions().Where(a => a.Distance(Player.Instance) < 1500).OrderBy(a => a.Distance(target.ServerPosition) < SpellE.Range);
                var minion = minions.FirstOrDefault();
                if (minion != null && SpellQ.IsInRange(minion) && minion.Distance(target.ServerPosition) < SpellE.Range)
                {
                    var f = minion; // minion can change
                    var p = SpellQ.GetPrediction(f);
                    if (p.HitChance >= PredQ())
                    {
                        SpellQ.Cast(p.CastPosition);
                        if (f.HasBuff("dianamoonlight") && f.Distance(target.ServerPosition) < SpellE.Range)
                        {
                            SpellR.Cast(f);
                            if (SpellE.IsInRange(target)) SpellE.Cast();
                            if (SpellW.IsInRange(target)) SpellW.Cast();
                        }
                    }

                }
            }

            if (SpellQ.IsReady() && Config.ComboMenu["useQ"].Cast<CheckBox>().CurrentValue)
            {
                if (SpellQ.GetPrediction(target).HitChance >= PredQ())
                {
                    var predQ = SpellQ.GetPrediction(target);
                    SpellQ.Cast(predQ.CastPosition);
                }
            }

            if (SpellR.IsReady() && Config.ComboMenu["useR"].Cast<CheckBox>().CurrentValue &&
                target.HasBuff("dianamoonlight"))
            {
                if (target.HasBuff("dianamoonlight"))
                    SpellR.Cast(target);
            }
            else if (SpellR.IsReady() && Config.ComboMenu["useR2"].Cast<CheckBox>().CurrentValue &&
                     Config.ComboMenu["useR"].Cast<CheckBox>().CurrentValue)
            {
                if (_Player.CountEnemiesInRange(SpellQ.Range * 2) <= Config.ComboMenu["useR2Count"].Cast<Slider>().CurrentValue && SpellR.IsReady() &&
                    _Player.GetSpellDamage(target,SpellSlot.R) >= target.Health)
                {
                    SpellR.Cast(target);
                }
            }

            if (SpellW.IsReady() && SpellW.IsInRange(target) && Config.ComboMenu["useW"].Cast<CheckBox>().CurrentValue)
            {
                SpellW.Cast();
            }

            if (SpellE.IsReady() && SpellE.IsInRange(target) && Config.ComboMenu["useE"].Cast<CheckBox>().CurrentValue)
            {
                SpellE.Cast();
            }

            if (SpellR.IsReady() && Config.ComboMenu["useR2"].Cast<CheckBox>().CurrentValue)
            {
                var e = _Player.CountEnemiesInRange(SpellQ.Range*2);

                if (e <= Config.ComboMenu["useR2Count"].Cast<Slider>().CurrentValue && SpellR.IsReady() &&
                   _Player.GetSpellDamage(target,SpellSlot.R) > target.Health)
                {
                    SpellR.Cast(target);
                }
            }

            if (Ignite.IsInRange(target) && target.Health < 50 + 20*_Player.Level - (target.HPRegenRate/5*3) &&
                Config.ComboMenu["useI"].Cast<CheckBox>().CurrentValue)
            {
                Ignite.Cast(target);
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(1500, DamageType.Magical);
            if (target == null || !target.IsValid())
            {
                return;
            }

            if (_Player.ManaPercent < Config.HarassMenu["Mana"].Cast<Slider>().CurrentValue) return;

            if (SpellQ.IsReady() && Config.ComboMenu["useQ"].Cast<CheckBox>().CurrentValue)
            {
                if (SpellQ.GetPrediction(target).HitChance >= PredQ())
                {
                    var predQ = SpellQ.GetPrediction(target);
                    SpellQ.Cast(predQ.CastPosition);
                }
            }

            if (SpellW.IsReady() && SpellW.IsInRange(target) && Config.ComboMenu["useW"].Cast<CheckBox>().CurrentValue)
            {
                SpellW.Cast();
            }

            if (SpellE.IsReady() && SpellE.IsInRange(target) && Config.ComboMenu["useE"].Cast<CheckBox>().CurrentValue)
            {
                SpellE.Cast();
            }
        }

        private static void WaveClear()
        {
            if (_Player.ManaPercent <= Config.FarmMenu["Mana"].Cast<Slider>().CurrentValue) return;

            var minions = ObjectManager.Get<Obj_AI_Base>()
                .Where(
                    a =>
                        a.IsEnemy && a.Distance(_Player) <= _Player.AttackRange &&
                        a.Health <= _Player.GetAutoAttackDamage(a)*1.1);

            var minions2 = ObjectManager.Get<Obj_AI_Base>()
                .Where(
                    a =>
                        a.IsEnemy && a.Distance(_Player) <= _Player.AttackRange &&
                        a.Health <= _Player.GetAutoAttackDamage(a)*1.1);

            var qminion = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                Player.Instance.ServerPosition.To2D(), SpellQ.Range);

            var wminion = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                Player.Instance.ServerPosition.To2D(), SpellW.Range);

            var eminion = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                Player.Instance.ServerPosition.To2D(), SpellE.Range);

            var qinrange = qminion.Where(m => SpellQ.IsInRange(m)).ToArray();
            var winrange = qminion.Where(m => SpellW.IsInRange(m)).ToArray();
            var einrange = qminion.Where(m => SpellE.IsInRange(m)).ToArray();

            if (Config.FarmMenu["qlc"].Cast<CheckBox>().CurrentValue && SpellQ.IsReady())
            {
                foreach (var minion in qinrange)
                {
                    var mline = SpellQ.GetPrediction(minion);
                    if (mline.GetCollisionObjects<Obj_AI_Minion>().Count() >=
                        Config.FarmMenu["qct"].Cast<Slider>().CurrentValue)
                    {
                        SpellQ.Cast(minion);
                    }
                }
            }

            if (Config.FarmMenu["wlc"].Cast<CheckBox>().CurrentValue && SpellW.IsReady())
            {
                foreach (var minion in winrange)
                {
                    SpellW.Cast();
                }
            }

            if (Config.FarmMenu["elc"].Cast<CheckBox>().CurrentValue && SpellE.IsReady())
            {
                foreach (var minion in einrange)
                {
                    SpellE.Cast();
                }
            }


            var min = minions.OrderByDescending(a => minions2.Count(b => b.Distance(a) <= 200)).FirstOrDefault();
            Orbwalker.ForcedTarget = min;
        }

        private static void JungleClear()
        {
            if (_Player.ManaPercent <= Config.JungleMenu["Mana"].Cast<Slider>().CurrentValue) return;

            var qminion = EntityManager.GetJungleMonsters(Player.Instance.ServerPosition.To2D(), SpellQ.Range);
            var wminion = EntityManager.GetJungleMonsters(Player.Instance.ServerPosition.To2D(), SpellW.Range);
            var eminion = EntityManager.GetJungleMonsters(Player.Instance.ServerPosition.To2D(), SpellE.Range);

            if (Config.JungleMenu["useQ"].Cast<CheckBox>().CurrentValue && SpellQ.IsReady())
            {
                foreach (var minion in qminion)
                {
                    SpellQ.Cast(minion);
                }
            }

            if (Config.JungleMenu["useW"].Cast<CheckBox>().CurrentValue && SpellW.IsReady())
            {
                foreach (var minion in wminion)
                {
                    if (_Player.Distance(minion) <= SpellW.Range)
                    {
                        SpellW.Cast();
                    }
                }
            }

            if (Config.JungleMenu["useE"].Cast<CheckBox>().CurrentValue && SpellE.IsReady())
            {
                foreach (var minion in eminion)
                {
                    if (_Player.Distance(minion) <= SpellE.Range)
                    {
                        SpellE.Cast();
                    }
                }
            }
        }

        private static HitChance PredQ()
        {
            var mode = Config.PredMenu["hQ"].DisplayName;
            switch (mode)
            {
                case "Low (Fast Casting)":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High (Slow Casting)":
                    return HitChance.High;
            }
            return HitChance.High;
        }

        private static void SChoose()
        {
            var style = Config.SkinMenu["sID"].DisplayName;


            switch (style)
            {
                case "Classic":
                    Player.SetSkinId(0);
                    break;
                case "Dark Valkyrie":
                    Player.SetSkinId(1);
                    break;
                case "Lunar Goddess":
                    Player.SetSkinId(2);
                    break;
            }
        }

        public static void g(AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloser)
        {
            if (!Config.GapMenu["GapW"].Cast<CheckBox>().CurrentValue &&
                _Player.Distance(gapcloser.Sender, true) < SpellW.Range && sender.IsValidTarget() && SpellW.IsReady() &&
                Config.GapSMenu[gapcloser.SpellName].Cast<CheckBox>().CurrentValue)
            {
                SpellW.Cast();

            }

            if (!Config.GapMenu["GapE"].Cast<CheckBox>().CurrentValue &&
                _Player.Distance(gapcloser.Sender, true) < SpellE.Range && sender.IsValidTarget() && SpellE.IsReady() &&
                Config.GapSMenu[gapcloser.SpellName].Cast<CheckBox>().CurrentValue)
            {
                SpellE.Cast();
                
            }

            


        }

        public static void i(Obj_AI_Base s,
            Interrupter.InterruptableSpellEventArgs g)
        {
            if (Config.GapMenu["IntR"].Cast<CheckBox>().CurrentValue && _Player.Distance(g.Sender, true) < SpellR.Range &&
                s.IsValidTarget() && SpellR.IsReady())
            {
                SpellR.Cast(g.Sender);
            }

            if (Config.GapMenu["IntE"].Cast<CheckBox>().CurrentValue && _Player.Distance(g.Sender, true) < SpellE.Range &&
                s.IsValidTarget() && SpellE.IsReady())
            {
                SpellE.Cast();
            }

           
        }

       
    }
}