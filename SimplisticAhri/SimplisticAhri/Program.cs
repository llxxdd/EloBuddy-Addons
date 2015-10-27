using System;
using System.Collections.Generic;
using System.Linq;
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
        private static Dictionary<SpellSlot, Spell.SpellBase> spells = new Dictionary<SpellSlot, Spell.SpellBase>
        {
            {SpellSlot.Q, new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear, 250, 1400, 90)},
            {SpellSlot.W, new Spell.Skillshot(SpellSlot.W, 550, SkillShotType.Circular, 250, 1200, 300)},
            {SpellSlot.E, new Spell.Skillshot(SpellSlot.E, 970, SkillShotType.Linear, 300, 1500, 60)},
            {SpellSlot.R, new Spell.Skillshot(SpellSlot.R, 850, SkillShotType.Circular, 250, 1400, 250)}
        };


        private static readonly Dictionary<string, object> _Q = new Dictionary<string, object>
        {
            {"MinSpeed", 400},
            {"MaxSpeed", 2500},
            {"Acceleration", -3200},
            {"Speed1", 1400},
            {"Delay1", 250},
            {"Range1", 880},
            {"Delay2", 0},
            {"Range2", int.MaxValue},
            {"IsReturning", false},
            {"Target", null},
            {"Object", null},
            {"LastObjectVector", null},
            {"LastObjectVectorTime", null},
            {"CatchPosition", null}
        };

        private static readonly Dictionary<string, object> _E = new Dictionary<string, object>
        {
            {"LastCastTime", 0f},
            {"Object", null}
        };

        private static readonly Dictionary<string, object> _R = new Dictionary<string, object> {{"EndTime", 0f}};

        private static readonly Spell.Skillshot SpellE = new Spell.Skillshot(SpellSlot.E, 970, SkillShotType.Linear, 300,
            1500, 60);

        private static readonly Spell.Skillshot SpellQ = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear, 250,
            1400, 90);


        public static Menu menu,
            ComboMenu,
            HarassMenu,
            FarmMenu,
            KillStealMenu,
            JungleMenu,
            FleeMenu,
            GapMenu,
            PredMenu,
            DrawingMenu,
            SkinMenu;

        public static CheckBox SmartMode;

        private static Vector3 mousePos
        {
            get { return Game.CursorPos; }
        }

        public static AIHeroClient selectedHero { get; set; }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static Dictionary<SpellSlot, Spell.SpellBase> Spells
        {
            get { return spells; }
            set { spells = value; }
        }

        public static Dictionary<SpellSlot, Spell.SpellBase> Spells1
        {
            get { return spells; }
            set { spells = value; }
        }

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (_Player.Hero != Champion.Ahri)
            {
                Chat.Print("Champion not supported!");
                return;
            }
            Chat.Print("<b>Simplistic Ahri</b> - Loaded!");

            Bootstrap.Init(null);

            menu = MainMenu.AddMenu("Simplistic Ahri1", "simplisticahri");
            menu.AddGroupLabel("Simplistic Ahri");
            menu.AddLabel("This project is being updated daily.");
            menu.AddLabel("Expect Bugs and bad Prediction!");
            menu.AddSeparator();
            SmartMode = menu.Add("smartMode", new CheckBox("Smart Mana Management", true));
            menu.AddLabel("Harass Smart Mana Mode");

            ComboMenu = menu.AddSubMenu("Combo", "ComboAhri");
            ComboMenu.Add("burst", new KeyBind("Burst Target",false, KeyBind.BindTypes.HoldActive, 'N' ));
            ComboMenu.Add("useQCombo", new CheckBox("Use Q"));
            ComboMenu.Add("useWCombo", new CheckBox("Use W"));
            ComboMenu.Add("useECombo", new CheckBox("Use E"));
            ComboMenu.Add("SmartUlt", new CheckBox("Use Smart R"));
            ComboMenu.Add("useCharm", new CheckBox("Smart Target"));
            ComboMenu.Add("waitAA", new CheckBox("wait for AA to finish", false));

            KillStealMenu = menu.AddSubMenu("Killsteal", "ksAhri");
            KillStealMenu.Add("useKS", new CheckBox("Killsteal"));
            KillStealMenu.AddLabel("Check Killsteal to turn this Feature on.");
            KillStealMenu.Add("useQKS", new CheckBox("Use Q for KS"));
            KillStealMenu.Add("useWKS", new CheckBox("Use W for KS"));
            KillStealMenu.Add("useEKS", new CheckBox("Use E for KS"));
            KillStealMenu.Add("useRKS", new CheckBox("Use R for KS"));

            HarassMenu = menu.AddSubMenu("Harass", "HarassAhri");
            HarassMenu.Add("useQHarass", new CheckBox("Use Q"));
            HarassMenu.Add("useWHarass", new CheckBox("Use W"));
            HarassMenu.Add("useEHarass", new CheckBox("Use E"));
            HarassMenu.Add("waitAA", new CheckBox("wait for AA to finish", false));

            FarmMenu = menu.AddSubMenu("Farm", "FarmAhri");
            FarmMenu.Add("qlh", new CheckBox("Use Q LastHit"));
            FarmMenu.Add("Mana", new Slider("Min. Mana Percent:", 20, 0, 100));

            JungleMenu = menu.AddSubMenu("JungleClear", "JungleClear");
            JungleMenu.Add("Q", new CheckBox("Use Q", true));
            JungleMenu.Add("W", new CheckBox("Use W", true));
            JungleMenu.Add("E", new CheckBox("Use E", true));
            JungleMenu.Add("Mana", new Slider("Min. Mana Percent:", 20, 0, 100));

            FleeMenu = menu.AddSubMenu("Flee", "Flee");
            FleeMenu.Add("R", new CheckBox("Use R to mousePos", true));

            GapMenu = menu.AddSubMenu("Auto E ", "autoe");
            GapMenu.Add("GapE", new CheckBox("Use E on Gapclosers", true));
            GapMenu.Add("IntE", new CheckBox("Use E on Interruptable Spells", true));

            DrawingMenu = menu.AddSubMenu("Drawings ", "drawings");
            DrawingMenu.Add("drawQ", new CheckBox("Draw Q Range", true));
            DrawingMenu.Add("drawE", new CheckBox("Draw E Range", true));
     


            PredMenu = menu.AddSubMenu("Prediction", "pred");

            // Q Prediction
            PredMenu.AddGroupLabel("Q Hitchance");
            var qslider = PredMenu.Add("hQ", new Slider("Q HitChance", 2, 0, 2));
            var qMode = new[] {"Low (Fast Casting)", "Medium", "High (Slow Casting)"};
            qslider.DisplayName = qMode[qslider.CurrentValue];

            qslider.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = qMode[changeArgs.NewValue];
                };
            //--------------


            // E Prediction
            PredMenu.AddGroupLabel("E Hitchance");
            var eslider = PredMenu.Add("hE", new Slider("E HitChance", 2, 0, 2));
            var eMode = new[] {"Low (Fast Casting)", "Medium", "High (Slow Casting)"};
            eslider.DisplayName = eMode[qslider.CurrentValue];

            eslider.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = eMode[changeArgs.NewValue];
                };
            //--------------

            SkinMenu = menu.AddSubMenu("Skin Chooser", "skinchooser");
            SkinMenu.AddGroupLabel("Choose your Skin and puff!");

            // Skin Chooser
            var skin = SkinMenu.Add("sID", new Slider("Skin", 0, 0, 6));
            var sID = new[] {"Classic", "Dynasty", "Midnight", "Foxfire", "Popstar", "Challenger", "Academy"};
            skin.DisplayName = sID[skin.CurrentValue];

            skin.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = sID[changeArgs.NewValue];
                };
            //--------------


            SpellE.AllowedCollisionCount = 0;
            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += OnDraw;
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
            else if (ComboMenu["burst"].Cast<KeyBind>().CurrentValue) BurstCombo();
            KillSteal();
            sChoose();
        }

        private static void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloser)
        {
            if (!GapMenu["GapE"].Cast<CheckBox>().CurrentValue && !sender.IsValidTarget()) return;
            if (ObjectManager.Player.Distance(gapcloser.Sender, true) <
                Spells[SpellSlot.E].Range*Spells[SpellSlot.E].Range && sender.IsValidTarget())
            {
                Spells[SpellSlot.E].Cast(gapcloser.Sender);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (selectedHero.IsValidTarget())
            {
                Drawing.DrawCircle(selectedHero.Position, Spells[SpellSlot.W].Range, System.Drawing.Color.Red);
            }

            if (SpellQ.IsReady() && DrawingMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(_Player.Position, SpellQ.Range, System.Drawing.Color.BlanchedAlmond);
            }

            if (SpellE.IsReady() && DrawingMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(_Player.Position, SpellE.Range, System.Drawing.Color.Brown);
            }

        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            if (!GapMenu["IntE"].Cast<CheckBox>().CurrentValue && !sender.IsValidTarget()) return;

            if (ObjectManager.Player.Distance(sender, true) < Spells[SpellSlot.E].Range*Spells[SpellSlot.E].Range)
            {
                Spells[SpellSlot.E].Cast(sender);
            }
        }

        public static void WaveClear()
        {
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
            var minion = minions.OrderByDescending(a => minions2.Count(b => b.Distance(a) <= 200)).FirstOrDefault();
            Orbwalker.ForcedTarget = minion;
        }

        public static void KillSteal()
        {
            if (KillStealMenu["useKS"].Cast<CheckBox>().CurrentValue)
            {
                var kstarget = TargetSelector.GetTarget(2500, DamageType.Magical);
                if (!kstarget.IsValidTarget() || kstarget == null) return;

                if (kstarget.IsValidTarget(Spells[SpellSlot.E].Range) && kstarget.HealthPercent <= 40)
                {
                    if (KillStealMenu["useQKS"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.Q].IsReady() &&
                        kstarget.Distance(_Player) < Spells[SpellSlot.Q].Range &&
                        Damage(kstarget, SpellSlot.Q) >= kstarget.Health)
                    {
                        var predQ = Prediction.Position.PredictLinearMissile(kstarget, Spells[SpellSlot.Q].Range, 50,
                            250,
                            1600, 999);
                        Spells[SpellSlot.Q].Cast(predQ.CastPosition);
                    }

                    if (KillStealMenu["useEKS"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.E].IsReady() &&
                        kstarget.Distance(_Player) < Spells[SpellSlot.E].Range &&
                        Damage(kstarget, SpellSlot.E) >= kstarget.Health)
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

                    if (KillStealMenu["useRKS"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.R].IsReady() &&
                        kstarget.Distance(_Player) < 400 && Damage(kstarget, SpellSlot.R) >= kstarget.Health)
                    {
                        Spells[SpellSlot.R].Cast(kstarget);
                    }

                    if (KillStealMenu["useWKS"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.W].IsReady() &&
                        kstarget.Distance(_Player) < Spells[SpellSlot.W].Range &&
                        Damage(kstarget, SpellSlot.W) >= kstarget.Health)
                    {
                        Spells[SpellSlot.W].Cast();
                    }
                }
            }
        }

        public static void Harass()
        {
            var target = TargetSelector.GetTarget(1550, DamageType.Magical);
            var qval = SpellQ.GetPrediction(target).HitChance >= PredQ();
            var eval = SpellE.GetPrediction(target).HitChance >= PredE();

            if (target == null || !target.IsValid())
            {
                return;
            }

            if (Orbwalker.IsAutoAttacking && HarassMenu["waitAA"].Cast<CheckBox>().CurrentValue) return;
            if (HarassMenu["useQHarass"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.Q].IsReady() && qval)
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
                    Spells[SpellSlot.W].Cast(target);
                    return;
                }
            }

            if (HarassMenu["useEHarass"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.E].IsReady() && eval)
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

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != (uint)WindowMessages.LeftButtonDown)
            {
                return;
            }
            selectedHero =
                EntityManager.Heroes.Enemies
                    .FindAll(hero => hero.IsValidTarget() && hero.Distance(Game.CursorPos, true) < 40000) // 200 * 200
                    .OrderBy(h => h.Distance(Game.CursorPos, true)).FirstOrDefault();
        }

        public static void BurstCombo()
        {
            var target = TargetSelector.GetTarget(1000, DamageType.Magical);

            if (selectedHero != null)
            {
                target = selectedHero;  
            }

            if (target == null || !target.IsValidTarget() || target.IsAlly)
            {
                return;
            }

            Orbwalker.OrbwalkTo(mousePos);
            Spells[SpellSlot.R].Cast(mousePos);

            if (Spells[SpellSlot.E].IsReady() &&
                SpellE.GetPrediction(target).HitChance >= PredE())
            {
                var predE = Prediction.Position.PredictLinearMissile(target, Spells[SpellSlot.E].Range, 60,
                    250,
                    1550, 0);
                Spells[SpellSlot.E].Cast(predE.CastPosition);
            }

            if (Spells[SpellSlot.Q].IsReady() &&
                SpellQ.GetPrediction(target).HitChance >= PredQ())
            {
                var predQ = Prediction.Position.PredictLinearMissile(target, Spells[SpellSlot.Q].Range, 50, 250, 1600,
                    999);
                Spells[SpellSlot.Q].Cast(predQ.CastPosition);
                return;
            }

            if (Spells[SpellSlot.W].IsReady())
            {
                Spells[SpellSlot.W].Cast(target);
            }
        }

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(1550, DamageType.Magical);
            var charmed = EntityManager.Heroes.Enemies.Find(h => h.HasBuffOfType(BuffType.Charm));
            var cc = EntityManager.Heroes.Enemies.Find(h => h.HasBuffOfType(BuffType.Fear));

            if (target == null || !target.IsValidTarget() || target.IsAlly)
            {
                return;
            }

            if (Orbwalker.IsAutoAttacking && HarassMenu["waitAA"].Cast<CheckBox>().CurrentValue) return;

            if (ComboMenu["useCharm"].Cast<CheckBox>().CurrentValue && charmed != null && charmed.IsValidTarget())
            {
                target = charmed;
            }
            else if (ComboMenu["useCharm"].Cast<CheckBox>().CurrentValue && charmed == null && cc != null && cc.Health < ComboDamage(cc) && cc.IsValidTarget())
            {
                target = cc;
            }
            else
            {
                target = TargetSelector.GetTarget(1550, DamageType.Magical);
            }


            HandleRCombo(target);

            if (ComboMenu["useECombo"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.E].IsReady() &&
                SpellE.GetPrediction(target).HitChance >= PredE())
            {
                var predE = Prediction.Position.PredictLinearMissile(target, Spells[SpellSlot.E].Range, 60,
                    250,
                    1550, 0);
                Spells[SpellSlot.E].Cast(predE.CastPosition);
            }

            if (ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.Q].IsReady() &&
                SpellQ.GetPrediction(target).HitChance >= PredQ())
            {
                var predQ = Prediction.Position.PredictLinearMissile(target, Spells[SpellSlot.Q].Range, 50, 250, 1600,
                    999);
                Spells[SpellSlot.Q].Cast(predQ.CastPosition);
                return;
            }

            if (ComboMenu["useWCombo"].Cast<CheckBox>().CurrentValue && Spells[SpellSlot.W].IsReady())
            {
                Spells[SpellSlot.W].Cast(target);
            }
        }


        private static void JungleClear()
        {
            if (_Player.ManaPercent >= JungleMenu["Mana"].Cast<Slider>().CurrentValue)
            {
                foreach (Obj_AI_Base minion in EntityManager.MinionsAndMonsters.GetJungleMonsters(_Player.Position, 1000f))
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
            var tower =
                ObjectManager.Get<Obj_AI_Turret>()
                    .FirstOrDefault(x => x.IsValidTarget() && x.Distance(Player.Instance) < 500);

            if (target.IsValidTarget(Spells[SpellSlot.R].Range + SpellE.Range) && (SpellQ.IsReady() || SpellE.IsReady()) && ComboDamage(target) > target.Health && tower == null && target.CountEnemiesInRange(400) < 3)
            {
                Spells[SpellSlot.R].Cast(_Player.ServerPosition.Extend(Game.CursorPos, Spells[SpellSlot.R].Range).To3D());
            }

        }
        


        private static void LastHit()
        {
            if (ObjectManager.Player.ManaPercent < FarmMenu["Mana"].Cast<Slider>().CurrentValue)
            {
                return;
            }
            var minions = ObjectManager.Get<Obj_AI_Base>()
                .Where(
                    a =>
                        a.IsEnemy && a.Distance(_Player) <= Spells[SpellSlot.Q].Range &&
                        a.Health <= _Player.GetSpellDamage(a, SpellSlot.Q)*1.1);

            var minions2 = ObjectManager.Get<Obj_AI_Base>()
                .Where(
                    a =>
                        a.IsEnemy && a.Distance(_Player) <= Spells[SpellSlot.Q].Range &&
                        a.Health <= _Player.GetSpellDamage(a, SpellSlot.Q)*1.1);

            var minion = minions.OrderByDescending(a => minions2.Count(b => b.Distance(a) <= 200)).FirstOrDefault();
            Orbwalker.ForcedTarget = minion;
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

        public static float ComboDamage(Obj_AI_Base target)
        {
            var damage = 0d;

            if (SpellQ.IsReady(3))
            {
                damage += _Player.GetSpellDamage(target, SpellSlot.Q);
            }

            if (Spells[SpellSlot.W].IsReady(5))
            {
                damage += _Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (SpellE.IsReady(2))
            {
                damage += _Player.GetSpellDamage(target, SpellSlot.E);
            }

            if (Spells[SpellSlot.R].IsReady(5))
            {
                damage += _Player.GetSpellDamage(target, SpellSlot.R);
            }

            damage += _Player.GetAutoAttackDamage(target) * 2;
            return (float)damage;
        }

        private static HitChance PredQ()
        {
            var mode = PredMenu["hQ"].DisplayName;
            switch (mode)
            {
                case "Low (Fast Casting)":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High (Slow Casting)":
                    return HitChance.High;
            }
            return HitChance.Medium;
        }

        private static HitChance PredE()
        {
            var mode = PredMenu["hE"].DisplayName;
            switch (mode)
            {
                case "Low (Fast Casting)":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High (Slow Casting)":
                    return HitChance.High;
            }
            return HitChance.Medium;
        }

        private static void sChoose()
        {
            var style = SkinMenu["sID"].DisplayName;


            switch (style)
            {
                case "Classic":
                    Player.SetSkinId(0);
                    break;
                case "Dynasty":
                    Player.SetSkinId(1);
                    break;
                case "Midnight":
                    Player.SetSkinId(2);
                    break;
                case "Foxfire":
                    Player.SetSkinId(3);
                    break;
                case "Popstar":
                    Player.SetSkinId(4);
                    break;
                case "Challenger":
                    Player.SetSkinId(5);
                    break;
                case "Academy":
                    Player.SetSkinId(6);
                    break;
            }
        }
    }
}