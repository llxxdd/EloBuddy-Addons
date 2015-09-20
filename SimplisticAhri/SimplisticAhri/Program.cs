using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace SimplisticAhri
{
    internal class Program
    {
        public static Dictionary<SpellSlot, Spell.SpellBase> Spells = new Dictionary<SpellSlot, Spell.SpellBase>
        {
            {SpellSlot.Q, new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 250, 1600, 50)},
            {SpellSlot.W, new Spell.Active(SpellSlot.W, 700)},
            {SpellSlot.E, new Spell.Skillshot(SpellSlot.E, 1000, SkillShotType.Linear, 250, 1550, 60)},
            {SpellSlot.R, new Spell.Active(SpellSlot.R, 900)}
        };

        public static Dictionary<SpellSlot, int> Mana = new Dictionary<SpellSlot, int>
        {
            {SpellSlot.Q, new[] {65, 70, 75, 80, 85}[Spells[SpellSlot.Q].IsLearned ? Spells[SpellSlot.Q].Level - 1 : 0]},
            {SpellSlot.W, 50},
            {SpellSlot.E, 85},
            {SpellSlot.R, 100}
        };

        private static Vector3 mousePos { get { return Game.CursorPos; } }
        public static System.Version Version;

        private static readonly Spell.Skillshot SpellE = new Spell.Skillshot(SpellSlot.E, 1000, SkillShotType.Linear,
            250, 1550, 60);

        public static Menu menu, ComboMenu, HarassMenu, FarmMenu, KillStealMenu, JungleMenu, FleeMenu;
        public static CheckBox SmartMode;

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            Version = Assembly.GetExecutingAssembly().GetName().Version;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (_Player.Hero != Champion.Ahri)
            {
                Chat.Print("Champion not supported!");
                return;
            }
            Chat.Print("<b>Simplistic Ahri</b> - Loaded!");
            updater.UpdateCheck();

            Bootstrap.Init(null);

            menu = MainMenu.AddMenu("Simplistic Ahri1", "simplisticahri");
            menu.AddGroupLabel("Simplistic Ahri");
            menu.AddLabel("This project is being updated daily.");
            menu.AddLabel("Expect Bugs and bad Prediction!");
            menu.AddSeparator();
            SmartMode = menu.Add("smartMode", new CheckBox("Smart Mana Management", true));
            menu.AddLabel("Harass Smart Mana Mode");

            ComboMenu = menu.AddSubMenu("Combo", "ComboAhri");
            ComboMenu.Add("useQCombo", new CheckBox("Use Q"));
            ComboMenu.Add("useWCombo", new CheckBox("Use W"));
            ComboMenu.Add("useECombo", new CheckBox("Use E"));

            KillStealMenu = menu.AddSubMenu("Killsteal", "ksAhri");
            KillStealMenu.Add("useKS", new CheckBox("Killsteal on?"));
            KillStealMenu.Add("useQKS", new CheckBox("Use Q for KS"));
            KillStealMenu.Add("useWKS", new CheckBox("Use W for KS"));
            KillStealMenu.Add("useEKS", new CheckBox("Use E for KS"));
            KillStealMenu.Add("useRKS", new CheckBox("Use R for KS"));

            HarassMenu = menu.AddSubMenu("Harass", "HarassAhri");
            HarassMenu.Add("useQHarass", new CheckBox("Use Q"));
            HarassMenu.Add("useWHarass", new CheckBox("Use W"));
            HarassMenu.Add("useEHarass", new CheckBox("Use E"));

            FarmMenu = menu.AddSubMenu("Farm", "FarmAhri");
            FarmMenu.AddLabel("Coming Soon");
            // FarmMenu.Add("useQFarm", new CheckBox("Use Q"));
            // FarmMenu.Add("useWFarm", new CheckBox("Use W"));

            JungleMenu = menu.AddSubMenu("JungleClear", "JungleClear");
            JungleMenu.Add("Q", new CheckBox("Use Q", true));
            JungleMenu.Add("W", new CheckBox("Use W", true));
            JungleMenu.Add("E", new CheckBox("Use E", true));
            JungleMenu.Add("Mana", new Slider("Min. Mana Percent:", 20, 0, 100));

            FleeMenu = menu.AddSubMenu("Flee", "Flee");
            FleeMenu.Add("R", new CheckBox("Use R", true));

            SpellE.AllowedCollisionCount = 0;
            Events.Init();
            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapCloser += Events.Gapcloser_OnGapCloser;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
        }

        private static void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) Harass();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) WaveClear();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee)) Flee();
            KillSteal();
            JungleClear();
        }

        public static void WaveClear()
        {
            var minions =
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(
                        a =>
                            a.IsEnemy && a.Distance(_Player) <= _Player.AttackRange &&
                            a.Health <= _Player.GetAutoAttackDamage(a)*1.1);
            var minions2 =
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(
                        a =>
                            a.IsEnemy && a.Distance(_Player) <= _Player.AttackRange &&
                            a.Health <= _Player.GetAutoAttackDamage(a)*1.1);
            var minion = minions.OrderByDescending(a => minions2.Count(b => b.Distance(a) <= 200)).FirstOrDefault();
            Orbwalker.ForcedTarget = minion;
        }

        public static void KillSteal()
        {
            if (KillStealMenu["useKS"].Cast<CheckBox>().CurrentValue)
            {
                var kstarget = TargetSelector.GetTarget(2500, DamageType.Magical);

                if (ComboMenu["useRKS"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.R].IsReady() &&
                    kstarget.Distance(_Player) > 400 &&
                    kstarget.Health < (_Player.GetSpellDamage(kstarget, SpellSlot.R)) || Spells[SpellSlot.R].IsReady())
                {
                    Spells[SpellSlot.R].Cast(kstarget);
                }

                if (ComboMenu["useQKS"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.Q].IsReady() &&
                    kstarget.Distance(_Player) > Spells[SpellSlot.Q].Range &&
                    kstarget.Health < (_Player.GetSpellDamage(kstarget, SpellSlot.Q)) || Spells[SpellSlot.Q].IsReady())
                {
                    var predQ = Prediction.Position.PredictLinearMissile(kstarget, Spells[SpellSlot.Q].Range, 50, 250,
                        1600, 999);
                    Spells[SpellSlot.Q].Cast(predQ.CastPosition);
                }

                if (ComboMenu["useWKS"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.W].IsReady() &&
                    kstarget.Distance(_Player) > Spells[SpellSlot.W].Range &&
                    kstarget.Health < (_Player.GetSpellDamage(kstarget, SpellSlot.W)) || Spells[SpellSlot.W].IsReady())
                {
                    Spells[SpellSlot.W].Cast();
                }

                if (ComboMenu["useEKS"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.E].IsReady() &&
                    kstarget.Distance(_Player) > Spells[SpellSlot.E].Range &&
                    kstarget.Health < (_Player.GetSpellDamage(kstarget, SpellSlot.E)) || Spells[SpellSlot.E].IsReady())
                {
                    var e = SpellE.GetPrediction(kstarget);
                    if (e.HitChance >= HitChance.High)
                    {
                        var predE = Prediction.Position.PredictLinearMissile(kstarget, Spells[SpellSlot.E].Range, 60,
                            250,
                            1550, 0);
                        Spells[SpellSlot.E].Cast(predE.CastPosition);
                    }
                }
            }
        }

        public static void Harass()
        {
            var target = TargetSelector.GetTarget(1550, DamageType.Physical);

            if (target == null) return;

            if (Orbwalker.IsAutoAttacking) return;

            if (HarassMenu["useQHarass"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.Q].IsReady())
            {
                if (target.Distance(_Player) <= Spells[SpellSlot.Q].Range ||
                    (_Player.ManaPercent > 40 && SmartMode.CurrentValue))
                {
                    var predQ = Prediction.Position.PredictLinearMissile(target, Spells[SpellSlot.Q].Range, 50, 250,
                        1600, 999);
                    Spells[SpellSlot.Q].Cast(predQ.CastPosition);
                    return;
                }
            }

            if (HarassMenu["useWHarass"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.W].IsReady())
            {
                if (target.Distance(_Player) <= Spells[SpellSlot.W].Range ||
                    (_Player.ManaPercent > 40 && SmartMode.CurrentValue))
                {
                    Spells[SpellSlot.W].Cast();
                    return;
                }
            }

            if (HarassMenu["useEHarass"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.E].IsReady())
            {
                if (target.Distance(_Player) <= Spells[SpellSlot.E].Range ||
                    (_Player.ManaPercent > 40 && SmartMode.CurrentValue))
                {
                    var e = SpellE.GetPrediction(target);
                    if (e.HitChance >= HitChance.High)
                    {
                        var predE = Prediction.Position.PredictLinearMissile(target, Spells[SpellSlot.E].Range, 60,
                            250,
                            1550, 0);
                        Spells[SpellSlot.E].Cast(predE.CastPosition);
                    }
                }
            }
        }

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(1550, DamageType.Mixed);


            if (target == null) return;

            if (Orbwalker.IsAutoAttacking) return;


            if (ComboMenu["useECombo"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.E].IsReady())
            {
                var e = SpellE.GetPrediction(target);
                if (e.HitChance >= HitChance.High)
                {
                    var predE = Prediction.Position.PredictLinearMissile(target, Spells[SpellSlot.E].Range, 60,
                        250,
                        1550, 0);
                    Spells[SpellSlot.E].Cast(predE.CastPosition);
                }
            }

            if (ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.Q].IsReady())
            {
                var predQ = Prediction.Position.PredictLinearMissile(target, Spells[SpellSlot.Q].Range, 50, 250, 1600,
                    999);
                Spells[SpellSlot.Q].Cast(predQ.CastPosition);
                return;
            }

            if (ComboMenu["useWCombo"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.W].IsReady())
            {
                Spells[SpellSlot.W].Cast();
            }
        }

        private static void JungleClear()
        {
            if (_Player.ManaPercent >= JungleMenu["Mana"].Cast<Slider>().CurrentValue)
            {
                foreach (Obj_AI_Base minion in EntityManager.GetJungleMonsters(_Player.Position.To2D(), 1000f))
                {
                    if (minion.IsValidTarget() && _Player.ManaPercent >= JungleMenu["Mana"].Cast<Slider>().CurrentValue)
                    {
                        if (JungleMenu["E"].Cast<CheckBox>().CurrentValue)
                        {
                            Spells[SpellSlot.E].Cast(minion);
                        }
                        if (JungleMenu["Q"].Cast<CheckBox>().CurrentValue)
                        {
                            Spells[SpellSlot.Q].Cast(minion);
                        }
                        if (JungleMenu["W"].Cast<CheckBox>().CurrentValue)
                        {
                            Spells[SpellSlot.W].Cast(minion);
                        }
                    }
                }
            }
        }

        static void Flee()
        {
          
            if (FleeMenu["R"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.R].IsReady())
            {
                Spells[SpellSlot.R].Cast(mousePos);
            }
        }
    }
}