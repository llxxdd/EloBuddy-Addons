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
using Version = System.Version;

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

        public static Version Version;

        private static readonly Spell.Skillshot SpellE = new Spell.Skillshot(SpellSlot.E, 1000, SkillShotType.Linear,
            250, 1550, 60);

        public static Menu menu, ComboMenu, HarassMenu, FarmMenu, KillStealMenu, JungleMenu, FleeMenu;
        public static CheckBox SmartMode;

        private static Vector3 mousePos
        {
            get { return Game.CursorPos; }
        }

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
            ComboMenu.Add("SmartUlt", new CheckBox("Smart R "));
            ComboMenu.Add("UltInit", new CheckBox("Don't Initiate with R", false));
            ComboMenu.Add("useCharm", new CheckBox("Smart Charm Combo"));

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
            FarmMenu.Add("qlh", new CheckBox("Use Q LastHit"));
            FarmMenu.Add("qlc", new CheckBox("Use Q LaneClear"));
            FarmMenu.Add("wlc", new CheckBox("Use W LaneClear"));
            FarmMenu.Add("elc", new CheckBox("Use E LaneClear"));
            FarmMenu.Add("Mana", new Slider("Min. Mana Percent:", 20, 0, 100));

            JungleMenu = menu.AddSubMenu("JungleClear", "JungleClear");
            JungleMenu.Add("Q", new CheckBox("Use Q", true));
            JungleMenu.Add("W", new CheckBox("Use W", true));
            JungleMenu.Add("E", new CheckBox("Use E", true));
            JungleMenu.Add("Mana", new Slider("Min. Mana Percent:", 20, 0, 100));

            FleeMenu = menu.AddSubMenu("Flee", "Flee");
            FleeMenu.Add("R", new CheckBox("Use R to mousePos", true));

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
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)) LastHit();
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                WaveClear();
                JungleClear();
            }
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee)) Flee();
            KillSteal();
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
            if (FarmMenu["qlc"].Cast<CheckBox>().CurrentValue) Spells[SpellSlot.Q].Cast(minion);
            if (FarmMenu["wlc"].Cast<CheckBox>().CurrentValue) Spells[SpellSlot.W].Cast(minion);
            if (FarmMenu["elc"].Cast<CheckBox>().CurrentValue) Spells[SpellSlot.W].Cast(minion);
        }

        public static void KillSteal()
        {
            if (KillStealMenu["useKS"].Cast<CheckBox>().CurrentValue)
            {
                var kstarget = TargetSelector.GetTarget(2500, DamageType.Magical);
                if (kstarget.IsValidTarget(Spells[SpellSlot.E].Range) && kstarget.HealthPercent <= 40)
                {
                    if (ComboMenu["useQKS"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.Q].IsReady() &&
                        kstarget.Distance(_Player) < Spells[SpellSlot.Q].Range && Damage(kstarget,SpellSlot.Q) >= kstarget.Health)
                    {
                        Chat.Print("KS Q Try");
                        var predQ = Prediction.Position.PredictLinearMissile(kstarget, Spells[SpellSlot.Q].Range, 50,
                            250,
                            1600, 999);
                        Spells[SpellSlot.Q].Cast(predQ.CastPosition);
                    }

                    if (ComboMenu["useEKS"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.E].IsReady() &&
                        kstarget.Distance(_Player) < Spells[SpellSlot.E].Range && Damage(kstarget, SpellSlot.E) >= kstarget.Health)
                    {
                        Chat.Print("KS E Try");
                        var e = SpellE.GetPrediction(kstarget);
                        if (e.HitChance >= HitChance.High)
                        {
                            var predE = Prediction.Position.PredictLinearMissile(kstarget, Spells[SpellSlot.E].Range, 60,
                                250,
                                1550, 0);
                            Spells[SpellSlot.E].Cast(predE.CastPosition);
                        }
                    }

                    if (ComboMenu["useRKS"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.R].IsReady() &&
                        kstarget.Distance(_Player) < 400 &&
                         Damage(kstarget, SpellSlot.R) >= kstarget.Health)
                    {
                        Chat.Print("KS R Try");
                        Spells[SpellSlot.R].Cast(kstarget);
                    }

                    if (ComboMenu["useWKS"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.W].IsReady() &&
                        kstarget.Distance(_Player) < Spells[SpellSlot.W].Range &&
                        Damage(kstarget, SpellSlot.W) >= kstarget.Health)
                    {
                        Chat.Print("KS W Try");
                        Spells[SpellSlot.W].Cast();
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
            var charmed = HeroManager.Enemies.Find(h => h.HasBuffOfType(BuffType.Charm));

            if (target == null) return;

            if (Orbwalker.IsAutoAttacking) return;

            if (ComboMenu["useCharm"].Cast<CheckBox>().CurrentValue && charmed != null)
            {
                target = charmed;
            }

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
            HandleRCombo(target);
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

        private static void Flee()
        {
            if (FleeMenu["R"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.R].IsReady())
            {
                Spells[SpellSlot.R].Cast(mousePos);
            }
        }

        private static void HandleRCombo(AIHeroClient target)
        {
            if (Spells[SpellSlot.R].IsReady() && ComboMenu["SmartUlt"].Cast<CheckBox>().CurrentValue &&
                Spells[SpellSlot.R].IsReady())
            {
                //User chose not to initiate with R.
                if (ComboMenu["UltInit"].Cast<CheckBox>().CurrentValue)
                {
                    return;
                }
                //Neither Q or E are ready in <= 2 seconds and we can't kill the enemy with 1 R stack. Don't use R
                if ((!Spells[SpellSlot.Q].IsReady(2) && !Spells[SpellSlot.E].IsReady(2)) ||
                    !(GetComboDamage(target) >= target.Health + 20))
                {
                    return;
                }
                //Set the test position to the Cursor Position
                var testPosition = Game.CursorPos;
                //Safety checks
                if (IsSafe(testPosition))
                {
                    Spells[SpellSlot.R].Cast(testPosition);
                }
            }
        }


        public static float GetComboDamage(AIHeroClient enemy)
        {
            float totalDamage = 0;
            totalDamage += Spells[SpellSlot.Q].IsReady() ? (_Player.GetSpellDamage(enemy, SpellSlot.Q)) : 0;
            totalDamage += Spells[SpellSlot.W].IsReady() ? (_Player.GetSpellDamage(enemy, SpellSlot.W)) : 0;
            totalDamage += Spells[SpellSlot.E].IsReady() ? (_Player.GetSpellDamage(enemy, SpellSlot.E)) : 0;
            totalDamage += (Spells[SpellSlot.R].IsReady() || (RStacks() != 0))
                ? (_Player.GetSpellDamage(enemy, SpellSlot.R))
                : 0;
            return totalDamage;
        }

        public static int RStacks()
        {
            var rBuff = ObjectManager.Player.Buffs.Find(buff => buff.Name == "AhriTumble");
            return rBuff != null ? rBuff.Count : 0;
        }

        public static bool IsSafe(Vector3 myVector)
        {
            var killableEnemyPlayer = HeroManager.Enemies.Find(h => GetComboDamage(h) >= h.Health);
            var killableEnemyPlayerNumber = killableEnemyPlayer != null ? 1 : 0;

            if (killableEnemyPlayerNumber == 0)
            {
                return false;
            }
            if (myVector.CountEnemiesInRange(600f) == 1 || ObjectManager.Player.CountEnemiesInRange(600f) >= 1)
            {
                return true;
            }
            return myVector.CountEnemiesInRange(600f) - killableEnemyPlayerNumber >= 0;
        }

        private static void LastHit()
        {
            if (ObjectManager.Player.ManaPercent < FarmMenu["Mana"].Cast<Slider>().CurrentValue)
            {
                return;
            }
            if (FarmMenu["qlh"].Cast<CheckBox>().CurrentValue)
            {
                var minions =
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(
                            a =>
                                a.IsEnemy && a.Distance(_Player) <= Spells[SpellSlot.Q].Range &&
                                a.Health <= _Player.GetSpellDamage(a, SpellSlot.Q)*1.1);

                var minions2 =
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(
                            a =>
                                a.IsEnemy && a.Distance(_Player) <= Spells[SpellSlot.Q].Range &&
                                a.Health <= _Player.GetSpellDamage(a, SpellSlot.Q)*1.1);

                var minion = minions.OrderByDescending(a => minions2.Count(b => b.Distance(a) <= 200)).FirstOrDefault();
                Spells[SpellSlot.Q].Cast(minion);
                Orbwalker.ForcedTarget = minion;
            }
        }

        private static float Damage(Obj_AI_Base target, SpellSlot slot)
        {
            if (target.IsValidTarget())
            {
                if (slot == SpellSlot.Q)
                {
                    return
                        _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                            (float) 25*Spells[SpellSlot.Q].Level + 15 + 0.35f*_Player.FlatMagicDamageMod) +
                        _Player.CalculateDamageOnUnit(target, DamageType.True,
                            (float) 25*Spells[SpellSlot.Q].Level + 15 + 0.35f*_Player.FlatMagicDamageMod);
                }
                if (slot == SpellSlot.W)
                {
                    return 1.6f*
                           _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                               (float) 25*Spells[SpellSlot.W].Level + 15 + 0.4f*_Player.FlatMagicDamageMod);
                }
                if (slot == SpellSlot.E)
                {
                    return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                        (float) 35*Spells[SpellSlot.E].Level + 25 + 0.5f*_Player.FlatMagicDamageMod);
                }
                if (slot == SpellSlot.R)
                {
                    return 3*
                           _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                               (float) 40*Spells[SpellSlot.R].Level + 30 + 0.3f*_Player.FlatMagicDamageMod);
                }
            }
            return _Player.GetSpellDamage(target, slot);
        }
    }
}