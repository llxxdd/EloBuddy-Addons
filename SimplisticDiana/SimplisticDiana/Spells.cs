using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;

namespace SimplisticDiana
{
    internal class Spells
    {
        private static SpellSlot ignite;

        public static readonly Spell.Skillshot SpellQ = new Spell.Skillshot(SpellSlot.Q, 830, SkillShotType.Linear, 500,
            1600, 195);

        public static readonly Spell.Active SpellE = new Spell.Active(SpellSlot.E, 350);
        public static readonly Spell.Active SpellW = new Spell.Active(SpellSlot.W, 200);
        public static readonly Spell.Active SpellR = new Spell.Active(SpellSlot.R, 825);

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
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                if (Config.ComboMenu["mode"].Cast<Slider>().CurrentValue > 0)
                {
                    Combo();
                }
                else
                {
                    MisayaCombo();
                }
            }
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) Harass();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) WaveClear();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) JungleClear();
        }

        public static void MisayaCombo()
        {
            var target = TargetSelector.GetTarget(1500, DamageType.Magical);
            if (target == null || !target.IsValid())
            {
                return;
            }

            if (SpellR.IsReady() && SpellR.IsInRange(target))
            {
                return;
            }

            if (!Config.ComboMenu["useQ"].Cast<CheckBox>().CurrentValue ||
                !Config.ComboMenu["useR"].Cast<CheckBox>().CurrentValue)
            {
                Combo();
                return;
            }

            if (Config.ComboMenu["useQ"].Cast<CheckBox>().CurrentValue &&
                Config.ComboMenu["useR"].Cast<CheckBox>().CurrentValue && SpellQ.IsReady() && SpellR.IsReady())
            {
                SpellR.Cast(target);
                SpellQ.Cast(target);
            }

            // NOT POSSIBLE NORMAL COMBO

            if (SpellQ.IsReady() && Config.ComboMenu["useQ"].Cast<CheckBox>().CurrentValue)
            {
                if (SpellQ.GetPrediction(target).HitChance >= PredQ())
                {
                    var predQ = SpellQ.GetPrediction(target);
                    SpellQ.Cast(predQ.CastPosition);
                }
            }

            if (SpellR.IsReady() && Config.ComboMenu["useR"].Cast<CheckBox>().CurrentValue)
            {
                if (target.HasBuff("dianamoonlight"))
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
                    Damage(target, SpellSlot.R) >= target.Health)
                {
                    SpellR.Cast(target);
                }
            }

            if (Ignite.IsInRange(target) && target.Health < 50 + 20*_Player.Level - (target.HPRegenRate/5*3))
            {
                Ignite.Cast(target);
            }
        }

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(1500, DamageType.Magical);
            if (target == null || !target.IsValid())
            {
                return;
            }

            if (SpellQ.IsReady() && Config.ComboMenu["useQ"].Cast<CheckBox>().CurrentValue)
            {
                if (SpellQ.GetPrediction(target).HitChance >= PredQ())
                {
                    var predQ = SpellQ.GetPrediction(target);
                    SpellQ.Cast(predQ.CastPosition);
                }
            }

            if (SpellR.IsReady() && Config.ComboMenu["useR"].Cast<CheckBox>().CurrentValue)
            {
                if (target.HasBuff("dianamoonlight"))
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
                    Damage(target, SpellSlot.R) >= target.Health)
                {
                    SpellR.Cast(target);
                }
            }

            if (Ignite.IsInRange(target) && target.Health < 50 + 20*_Player.Level - (target.HPRegenRate/5*3))
            {
                Ignite.Cast(target);
            }
        }

        public static void Harass()
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

        public static void WaveClear()
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
                        SpellQ.Cast(minion);
                }
            }

            if (Config.FarmMenu["wlc"].Cast<CheckBox>().CurrentValue && SpellW.IsReady())
            {
                foreach (var minion in winrange)
                {
                    var mline = minion.CountEnemiesInRange(SpellW.Range);
                    if (mline >=
                        Config.FarmMenu["wct"].Cast<Slider>().CurrentValue)
                        SpellW.Cast();
                }
            }

            if (Config.FarmMenu["elc"].Cast<CheckBox>().CurrentValue && SpellE.IsReady())
            {
                foreach (var minion in einrange)
                {
                    var mline = minion.CountEnemiesInRange(SpellE.Range);
                    if (mline >=
                        Config.FarmMenu["ect"].Cast<Slider>().CurrentValue)
                        SpellE.Cast();
                }
            }


            var min = minions.OrderByDescending(a => minions2.Count(b => b.Distance(a) <= 200)).FirstOrDefault();
            Orbwalker.ForcedTarget = min;
        }

        public static void JungleClear()
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

        public static float Damage(Obj_AI_Base target, SpellSlot slot)
        {
            if (target.IsValidTarget())
            {
                if (slot == SpellSlot.Q)
                {
                    return
                        _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                            (float) 25*SpellQ.Level + 15 + 0.35f*_Player.FlatMagicDamageMod) +
                        _Player.CalculateDamageOnUnit(target, DamageType.True,
                            (float) 25*SpellQ.Level + 15 + 0.35f*_Player.FlatMagicDamageMod);
                }
                if (slot == SpellSlot.W)
                {
                    return 1.6f*
                           _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                               (float) 25*SpellW.Level + 15 + 0.4f*_Player.FlatMagicDamageMod);
                }
                if (slot == SpellSlot.E)
                {
                    return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                        (float) 35*SpellE.Level + 25 + 0.5f*_Player.FlatMagicDamageMod);
                }
                if (slot == SpellSlot.R)
                {
                    return 3*
                           _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                               (float) 40*SpellR.Level + 30 + 0.3f*_Player.FlatMagicDamageMod);
                }
            }
            return _Player.GetSpellDamage(target, slot);
        }

        private static HitChance PredQ()
        {
            var mode = Config.PredMenu["hE"].DisplayName;
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

        public static void SChoose()
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
    }
}