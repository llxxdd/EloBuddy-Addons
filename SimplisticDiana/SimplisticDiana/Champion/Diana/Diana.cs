#region

using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;
using SimplisticTemplate.Champion.Diana.Modes;
using SimplisticTemplate.Champion.Diana.Utils;

#endregion

namespace SimplisticTemplate.Champion.Diana
{
    internal static class Diana
    {
        /*
        Spell Init
        */
        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Targeted R;

        public static void Initialize()
        {
            Bootstrap.Init(null);

            InitSpells();
            InitMisc();
        }

        private static void InitSpells()
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 830, SkillShotType.Cone, 500, 1600, 195);
            W = new Spell.Active(SpellSlot.E, 350);
            E = new Spell.Active(SpellSlot.W, 200);
            R = new Spell.Targeted(SpellSlot.R, 825);
            Q.AllowedCollisionCount = int.MaxValue;

            //end of spell init
        }

        private static void InitMisc()
        {
            GameMenu.Initialize();
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawings.OnDraw;
            Drawing.OnEndScene += Drawings.OnDamageDraw;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    Combo.Execute();
                    break;
                case Orbwalker.ActiveModes.Harass:
                    Harass.Execute();
                    break;
                case Orbwalker.ActiveModes.LaneClear:
                    Farming.ExecuteLaneClear();
                    break;
                case Orbwalker.ActiveModes.JungleClear:
                    Farming.ExecuteJungleClear();
                    break;
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) &&
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                Farming.ExecuteLaneClear();
                Farming.ExecuteJungleClear();
            }
            if (GameMenu.ComboMenu["misayacombo"].Cast<KeyBind>().CurrentValue) Combo.MisayaCombo();
        }
    }
}