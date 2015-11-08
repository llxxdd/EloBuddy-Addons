#region

using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace SimplisticAhri
{
    internal static class Program
    {
        private static readonly Spell.Skillshot SpellQ = new Spell.Skillshot(SpellSlot.E, 810, SkillShotType.Linear, 250,
            1550, 60);

        private static readonly Spell.Active SpellW = new Spell.Active(SpellSlot.W, 400);

        private static readonly Spell.Skillshot SpellE = new Spell.Skillshot(SpellSlot.Q, 780, SkillShotType.Linear, 250,
            1600, 50);

        private static readonly Spell.Skillshot SpellR = new Spell.Skillshot(SpellSlot.R, 800, SkillShotType.Circular,
            250, 1400, 250);


        private static Menu _menu,
            _comboMenu,
            _harassMenu,
            _farmMenu,
            _killStealMenu,
            _jungleMenu,
            _fleeMenu,
            _gapMenu,
            _predMenu,
            _drawingMenu,
            _skinMenu;

        private static CheckBox _smartMode;

        private static Vector3 MousePos
        {
            get { return Game.CursorPos; }
        }

        private static AIHeroClient SelectedHero { get; set; }

        private static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main()
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Hero != Champion.Ahri)
            {
                return;
            }

            Bootstrap.Init(null);

            _menu = MainMenu.AddMenu("Simplistic Ahri", "simplisticahri");
            _menu.AddGroupLabel("Simplistic Ahri");
            _menu.AddSeparator();
            _smartMode = _menu.Add("smartMode", new CheckBox("Smart Mana Management"));
            _menu.AddLabel("Harass Smart Mana Mode");

            _comboMenu = _menu.AddSubMenu("Combo", "ComboAhri");
            _comboMenu.Add("burst", new KeyBind("Burst Target", false, KeyBind.BindTypes.HoldActive, 'N'));
            _comboMenu.Add("useQCombo", new CheckBox("Use Q"));
            _comboMenu.Add("useWCombo", new CheckBox("Use W"));
            _comboMenu.Add("useECombo", new CheckBox("Use E"));
            _comboMenu.Add("SmartUlt", new CheckBox("Use Smart R"));
            _comboMenu.Add("useCharm", new CheckBox("Smart Target"));
            _comboMenu.Add("waitAA", new CheckBox("wait for AA to finish", false));

            _killStealMenu = _menu.AddSubMenu("Killsteal", "ksAhri");
            _killStealMenu.Add("useKS", new CheckBox("Killsteal"));
            _killStealMenu.AddLabel("Check Killsteal to turn this Feature on.");
            _killStealMenu.Add("useQKS", new CheckBox("Use Q for KS"));
            _killStealMenu.Add("useWKS", new CheckBox("Use W for KS"));
            _killStealMenu.Add("useEKS", new CheckBox("Use E for KS"));
            _killStealMenu.Add("useRKS", new CheckBox("Use R for KS"));

            _harassMenu = _menu.AddSubMenu("Harass", "HarassAhri");
            _harassMenu.Add("useQHarass", new CheckBox("Use Q"));
            _harassMenu.Add("useWHarass", new CheckBox("Use W"));
            _harassMenu.Add("useEHarass", new CheckBox("Use E"));
            _harassMenu.Add("waitAA", new CheckBox("wait for AA to finish", false));

            _farmMenu = _menu.AddSubMenu("Farm", "FarmAhri");
            _farmMenu.Add("qlh", new CheckBox("Use Q LastHit"));
            _farmMenu.Add("Mana", new Slider("Min. Mana Percent:", 20));

            _jungleMenu = _menu.AddSubMenu("JungleClear", "JungleClear");
            _jungleMenu.Add("Q", new CheckBox("Use Q"));
            _jungleMenu.Add("W", new CheckBox("Use W"));
            _jungleMenu.Add("E", new CheckBox("Use E"));
            _jungleMenu.Add("Mana", new Slider("Min. Mana Percent:", 20));

            _fleeMenu = _menu.AddSubMenu("Flee", "Flee");
            _fleeMenu.Add("R", new CheckBox("Use R to MousePos"));

            _gapMenu = _menu.AddSubMenu("Auto E ", "autoe");
            _gapMenu.Add("GapE", new CheckBox("Use E on Gapclosers"));
            _gapMenu.Add("IntE", new CheckBox("Use E on Interruptable Spells"));

            _drawingMenu = _menu.AddSubMenu("Drawings ", "drawings");
            _drawingMenu.Add("drawQ", new CheckBox("Draw Q Range"));
            _drawingMenu.Add("drawE", new CheckBox("Draw E Range"));


            _predMenu = _menu.AddSubMenu("Prediction", "pred");

            // Q Prediction
            _predMenu.AddGroupLabel("Q Hitchance");
            var qslider = _predMenu.Add("hQ", new Slider("Q HitChance", 2, 0, 2));
            var qMode = new[] {"Low (Fast Casting)", "Medium", "High (Slow Casting)"};
            qslider.DisplayName = qMode[qslider.CurrentValue];

            qslider.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = qMode[changeArgs.NewValue];
                };
            //--------------


            // E Prediction
            _predMenu.AddGroupLabel("E Hitchance");
            var eslider = _predMenu.Add("hE", new Slider("E HitChance", 2, 0, 2));
            var eMode = new[] {"Low (Fast Casting)", "Medium", "High (Slow Casting)"};
            eslider.DisplayName = eMode[qslider.CurrentValue];

            eslider.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = eMode[changeArgs.NewValue];
                };
            //--------------

            _skinMenu = _menu.AddSubMenu("Skin Chooser", "skinchooser");
            _skinMenu.AddGroupLabel("Choose your Skin and puff!");

            // Skin Chooser
            var skin = _skinMenu.Add("sID", new Slider("Skin", 0, 0, 6));
            var sId = new[] {"Classic", "Dynasty", "Midnight", "Foxfire", "Popstar", "Challenger", "Academy"};
            skin.DisplayName = sId[skin.CurrentValue];

            skin.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = sId[changeArgs.NewValue];
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
            else if (_comboMenu["burst"].Cast<KeyBind>().CurrentValue) BurstCombo();
            KillSteal();
            SChoose();
        }

        private static void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloser)
        {
            if (!_gapMenu["GapE"].Cast<CheckBox>().CurrentValue || !sender.IsValidTarget()) return;
            if (ObjectManager.Player.Distance(gapcloser.Sender, true) <
                SpellE.Range*SpellE.Range && sender.IsValidTarget())
            {
                SpellE.Cast(gapcloser.Sender);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (SelectedHero.IsValidTarget())
            {
                Drawing.DrawCircle(SelectedHero.Position, SpellW.Range, Color.Red);
            }

            if (SpellQ.IsReady() && _drawingMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(Player.Position, SpellQ.Range, Color.BlanchedAlmond);
            }

            if (SpellE.IsReady() && _drawingMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(Player.Position, SpellE.Range, Color.Brown);
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            if (!_gapMenu["IntE"].Cast<CheckBox>().CurrentValue || !sender.IsValidTarget()) return;

            if (ObjectManager.Player.Distance(sender, true) < SpellE.Range*SpellE.Range)
            {
                SpellE.Cast(sender);
            }
        }

        private static void WaveClear()
        {
            var minions = ObjectManager.Get<Obj_AI_Base>()
                .Where(
                    a =>
                        a.IsEnemy && a.Distance(Player) <= Player.AttackRange &&
                        a.Health <= Player.GetAutoAttackDamage(a)*1.1);
            var minions2 = ObjectManager.Get<Obj_AI_Base>()
                .Where(
                    a =>
                        a.IsEnemy && a.Distance(Player) <= Player.AttackRange &&
                        a.Health <= Player.GetAutoAttackDamage(a)*1.1);
            var minion = minions.OrderByDescending(a => minions2.Count(b => b.Distance(a) <= 200)).FirstOrDefault();
            Orbwalker.ForcedTarget = minion;
        }

        private static void KillSteal()
        {
            if (_killStealMenu["useKS"].Cast<CheckBox>().CurrentValue)
            {
                var kstarget = TargetSelector.GetTarget(2500, DamageType.Magical);
                if (!kstarget.IsValidTarget() || kstarget == null) return;

                if (kstarget.IsValidTarget(SpellE.Range) && kstarget.HealthPercent <= 40)
                {
                    if (_killStealMenu["useQKS"].Cast<CheckBox>().CurrentValue && SpellQ.IsReady() &&
                        kstarget.Distance(Player) < SpellQ.Range &&
                        Damage(kstarget, SpellSlot.Q) >= kstarget.Health)
                    {
                        var predQ = Prediction.Position.PredictLinearMissile(kstarget, SpellQ.Range, 50,
                            250,
                            1600, 999);
                        SpellQ.Cast(predQ.CastPosition);
                    }

                    if (_killStealMenu["useEKS"].Cast<CheckBox>().CurrentValue && SpellE.IsReady() &&
                        kstarget.Distance(Player) < SpellE.Range &&
                        Damage(kstarget, SpellSlot.E) >= kstarget.Health)
                    {
                        var e = SpellE.GetPrediction(kstarget);
                        if (e.HitChance >= HitChance.High)
                        {
                            var predE = Prediction.Position.PredictLinearMissile(kstarget, SpellE.Range, 60,
                                250,
                                1550, 0);
                            SpellE.Cast(predE.CastPosition);
                        }
                    }

                    if (_killStealMenu["useRKS"].Cast<CheckBox>().CurrentValue && SpellR.IsReady() &&
                        kstarget.Distance(Player) < 400 && Damage(kstarget, SpellSlot.R) >= kstarget.Health)
                    {
                        SpellR.Cast(kstarget);
                    }

                    if (_killStealMenu["useWKS"].Cast<CheckBox>().CurrentValue && SpellW.IsReady() &&
                        kstarget.Distance(Player) < SpellW.Range &&
                        Damage(kstarget, SpellSlot.W) >= kstarget.Health)
                    {
                        SpellW.Cast();
                    }
                }
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(1550, DamageType.Magical);
            var qval = SpellQ.GetPrediction(target).HitChance >= PredQ();
            var eval = SpellE.GetPrediction(target).HitChance >= PredE();

            if (target == null || !target.IsValidTarget())
            {
                return;
            }

            if (Orbwalker.IsAutoAttacking && _harassMenu["waitAA"].Cast<CheckBox>().CurrentValue) return;
            if (_harassMenu["useQHarass"].Cast<CheckBox>().CurrentValue && SpellQ.IsReady() && qval)
            {
                if (target.Distance(Player) <= SpellQ.Range ||
                    (Player.ManaPercent > 40 && _smartMode.CurrentValue))
                {
                    var predQ = Prediction.Position.PredictLinearMissile(target, SpellQ.Range, 50, 250,
                        1600, 999);
                    SpellQ.Cast(predQ.CastPosition);
                    return;
                }
            }

            if (_harassMenu["useWHarass"].Cast<CheckBox>().CurrentValue && SpellW.IsReady())
            {
                if (target.Distance(Player) <= SpellW.Range ||
                    (Player.ManaPercent > 40 && _smartMode.CurrentValue))
                {
                    SpellW.Cast(target);
                    return;
                }
            }

            if (_harassMenu["useEHarass"].Cast<CheckBox>().CurrentValue && SpellE.IsReady() && eval)
            {
                if (target.Distance(Player) <= SpellE.Range ||
                    (Player.ManaPercent > 40 && _smartMode.CurrentValue))
                {
                    var e = SpellE.GetPrediction(target);
                    if (e.HitChance >= HitChance.High)
                    {
                        var predE = Prediction.Position.PredictLinearMissile(target, SpellE.Range, 60,
                            250,
                            1550, 0);
                        SpellE.Cast(predE.CastPosition);
                    }
                }
            }
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != (uint) WindowMessages.LeftButtonDown)
            {
                return;
            }
            SelectedHero =
                EntityManager.Heroes.Enemies
                    .FindAll(hero => hero.IsValidTarget() && hero.Distance(Game.CursorPos, true) < 40000) // 200 * 200
                    .OrderBy(h => h.Distance(Game.CursorPos, true)).FirstOrDefault();
        }

        private static void BurstCombo()
        {
            var target = TargetSelector.GetTarget(1000, DamageType.Magical);

            if (SelectedHero != null)
            {
                target = SelectedHero;
            }

            if (target == null || !target.IsValidTarget() || target.IsAlly)
            {
                return;
            }

            Orbwalker.OrbwalkTo(MousePos);
            SpellR.Cast(MousePos);

            if (SpellE.IsReady() &&
                SpellE.GetPrediction(target).HitChance >= PredE())
            {
                var predE = Prediction.Position.PredictLinearMissile(target, SpellE.Range, 60,
                    250,
                    1550, 0);
                SpellE.Cast(predE.CastPosition);
            }

            if (SpellQ.IsReady() &&
                SpellQ.GetPrediction(target).HitChance >= PredQ())
            {
                var predQ = Prediction.Position.PredictLinearMissile(target, SpellQ.Range, 50, 250, 1600,
                    999);
                SpellQ.Cast(predQ.CastPosition);
                return;
            }

            if (SpellW.IsReady())
            {
                SpellW.Cast(target);
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1550, DamageType.Magical);
            var charmed = EntityManager.Heroes.Enemies.Find(h => h.HasBuffOfType(BuffType.Charm));
            var cc = EntityManager.Heroes.Enemies.Find(h => h.HasBuffOfType(BuffType.Fear));

            if (target == null || !target.IsValidTarget() || target.IsAlly)
            {
                return;
            }

            if (Orbwalker.IsAutoAttacking && _harassMenu["waitAA"].Cast<CheckBox>().CurrentValue) return;

            if (_comboMenu["useCharm"].Cast<CheckBox>().CurrentValue && charmed != null && charmed.IsValidTarget())
            {
                target = charmed;
            }
            else if (_comboMenu["useCharm"].Cast<CheckBox>().CurrentValue && charmed == null && cc != null &&
                     cc.Health < ComboDamage(cc) && cc.IsValidTarget())
            {
                target = cc;
            }
            else
            {
                target = TargetSelector.GetTarget(1550, DamageType.Magical);
            }


            HandleRCombo(target);

            if (_comboMenu["useECombo"].Cast<CheckBox>().CurrentValue && SpellE.IsReady() &&
                SpellE.GetPrediction(target).HitChance >= PredE())
            {
                var predE = Prediction.Position.PredictLinearMissile(target, SpellE.Range, 60,
                    250,
                    1550, 0);
                SpellE.Cast(predE.CastPosition);
            }

            if (_comboMenu["useQCombo"].Cast<CheckBox>().CurrentValue && SpellQ.IsReady() &&
                SpellQ.GetPrediction(target).HitChance >= PredQ())
            {
                var predQ = Prediction.Position.PredictLinearMissile(target, SpellQ.Range, 50, 250, 1600,
                    999);
                SpellQ.Cast(predQ.CastPosition);
                return;
            }

            if (_comboMenu["useWCombo"].Cast<CheckBox>().CurrentValue && SpellW.IsReady())
            {
                SpellW.Cast(target);
            }
        }


        private static void JungleClear()
        {
            if (Player.ManaPercent >= _jungleMenu["Mana"].Cast<Slider>().CurrentValue)
            {
                foreach (
                    var minion in EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, 1000f))
                {
                    if (minion.IsValidTarget() && Player.ManaPercent >= _jungleMenu["Mana"].Cast<Slider>().CurrentValue)
                    {
                        if (_jungleMenu["E"].Cast<CheckBox>().CurrentValue)
                        {
                            SpellE.Cast(minion);
                        }
                        if (_jungleMenu["Q"].Cast<CheckBox>().CurrentValue)
                        {
                            SpellQ.Cast(minion);
                        }
                        if (_jungleMenu["W"].Cast<CheckBox>().CurrentValue)
                        {
                            SpellW.Cast(minion);
                        }
                    }
                }
            }
        }


        private static void Flee()
        {
            if (_fleeMenu["R"].Cast<CheckBox>().CurrentValue && SpellR.IsReady())
            {
                SpellR.Cast(MousePos);
            }
        }


        private static void HandleRCombo(AIHeroClient target)
        {
            var tower =
                ObjectManager.Get<Obj_AI_Turret>()
                    .FirstOrDefault(x => x.IsValid && x.IsEnemy && x.Distance(EloBuddy.Player.Instance) < SpellR.Range);

            if (target.IsValidTarget(SpellR.Range + SpellE.Range) && (SpellQ.IsReady() || SpellE.IsReady()) &&
                ComboDamage(target) > target.Health && tower == null && target.CountEnemiesInRange(400) < 3)
            {
                SpellR.Cast(Player.ServerPosition.Extend(Game.CursorPos, SpellR.Range).To3D());
            }
        }


        private static void LastHit()
        {
            if (ObjectManager.Player.ManaPercent < _farmMenu["Mana"].Cast<Slider>().CurrentValue)
            {
                return;
            }
            var minions = ObjectManager.Get<Obj_AI_Base>()
                .Where(
                    a =>
                        a.IsEnemy && a.Distance(Player) <= SpellQ.Range &&
                        a.Health <= Player.GetSpellDamage(a, SpellSlot.Q)*1.1);

            var minions2 = ObjectManager.Get<Obj_AI_Base>()
                .Where(
                    a =>
                        a.IsEnemy && a.Distance(Player) <= SpellQ.Range &&
                        a.Health <= Player.GetSpellDamage(a, SpellSlot.Q)*1.1);

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
                        Player.CalculateDamageOnUnit(target, DamageType.Magical,
                            (float) 25*SpellQ.Level + 15 + 0.35f*Player.FlatMagicDamageMod) +
                        Player.CalculateDamageOnUnit(target, DamageType.True,
                            (float) 25*SpellQ.Level + 15 + 0.35f*Player.FlatMagicDamageMod);
                }
                if (slot == SpellSlot.W)
                {
                    return 1.6f*
                           Player.CalculateDamageOnUnit(target, DamageType.Magical,
                               (float) 25*SpellW.Level + 15 + 0.4f*Player.FlatMagicDamageMod);
                }
                if (slot == SpellSlot.E)
                {
                    return Player.CalculateDamageOnUnit(target, DamageType.Magical,
                        (float) 35*SpellE.Level + 25 + 0.5f*Player.FlatMagicDamageMod);
                }
                if (slot == SpellSlot.R)
                {
                    return 3*
                           Player.CalculateDamageOnUnit(target, DamageType.Magical,
                               (float) 40*SpellR.Level + 30 + 0.3f*Player.FlatMagicDamageMod);
                }
            }
            return Player.GetSpellDamage(target, slot);
        }

        private static float ComboDamage(Obj_AI_Base target)
        {
            var damage = 0d;

            if (SpellQ.IsReady(3))
            {
                damage += Player.GetSpellDamage(target, SpellSlot.Q);
            }

            if (SpellW.IsReady(5))
            {
                damage += Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (SpellE.IsReady(2))
            {
                damage += Player.GetSpellDamage(target, SpellSlot.E);
            }

            if (SpellR.IsReady(5))
            {
                damage += Player.GetSpellDamage(target, SpellSlot.R);
            }

            damage += Player.GetAutoAttackDamage(target)*2;
            return (float) damage;
        }

        private static HitChance PredQ()
        {
            var mode = _predMenu["hQ"].DisplayName;
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
            var mode = _predMenu["hE"].DisplayName;
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

        private static void SChoose()
        {
            var style = _skinMenu["sID"].DisplayName;


            switch (style)
            {
                case "Classic":
                    EloBuddy.Player.SetSkinId(0);
                    break;
                case "Dynasty":
                    EloBuddy.Player.SetSkinId(1);
                    break;
                case "Midnight":
                    EloBuddy.Player.SetSkinId(2);
                    break;
                case "Foxfire":
                    EloBuddy.Player.SetSkinId(3);
                    break;
                case "Popstar":
                    EloBuddy.Player.SetSkinId(4);
                    break;
                case "Challenger":
                    EloBuddy.Player.SetSkinId(5);
                    break;
                case "Academy":
                    EloBuddy.Player.SetSkinId(6);
                    break;
            }
        }
    }
}