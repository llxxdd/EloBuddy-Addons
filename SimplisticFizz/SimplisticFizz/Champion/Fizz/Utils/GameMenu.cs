#region

using EloBuddy;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

#endregion

namespace SimplisticTemplate.Champion.Fizz.Utils
{
    internal static class GameMenu
    {
        public static Menu Menu,
            ComboMenu,
            HarassMenu,
            MiscMenu,
            DrawMenu;

        public static AIHeroClient Me
        {
            get { return ObjectManager.Player; }
        }

        public static void Initialize()
        {
            Menu = MainMenu.AddMenu("Simplistic " + Me.ChampionName, Me.ChampionName.ToLower());
            Menu.AddLabel("Simplistic Fizz");
            Menu.AddLabel("by nonm");

            ComboMenu = Menu.AddSubMenu("Combo", "combo");
            ComboMenu.AddLabel("Combo Settings");
            ComboMenu.Add("qrcombo", new KeyBind("Q - R Combo", false, KeyBind.BindTypes.HoldActive, 'A'));
            ComboMenu.Add("useQ", new CheckBox("Use Q"));
            ComboMenu.Add("useW", new CheckBox("Use W"));
            ComboMenu.Add("useE", new CheckBox("Use E"));
            ComboMenu.Add("useR", new CheckBox("Use R"));
            ComboMenu.Add("useEGap", new CheckBox("Use E to Gapclose and then Q if killable?"));
            ComboMenu.Add("useRGap", new CheckBox("Use R and then E for Gapclose if killable?"));

            HarassMenu = Menu.AddSubMenu("Harass", "harass");
            HarassMenu.AddLabel("Harass Settings");
            HarassMenu.Add("useQ", new CheckBox("Use Q"));
            HarassMenu.Add("useW", new CheckBox("Use W"));
            HarassMenu.Add("useE", new CheckBox("Use E"));
            HarassMenu.AddSeparator();
            HarassMenu.AddLabel("E Modes: (1) Back to Position (2) On Enemy");
            HarassMenu.Add("useEMode", new Slider("E Mode", 0, 0, 1));

            MiscMenu = Menu.AddSubMenu("Misc", "misc");
            MiscMenu.AddLabel("Misc Settings");
            MiscMenu.AddLabel("Use W : (1) Before Q (2) On Enemy");
            MiscMenu.Add("useWMode", new Slider("Use W", 0, 0, 1));
            MiscMenu.AddSeparator();
            MiscMenu.Add("useETower", new CheckBox("Use E to dodge Tower Shots"));

            DrawMenu = Menu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddLabel("Drawing Settings");
            DrawMenu.Add("disable", new CheckBox("Disable all Drawing", false));
            DrawMenu.Add("drawDamage", new CheckBox("Draw Damage"));
            DrawMenu.Add("drawQ", new CheckBox("Draw Q Range"));
            DrawMenu.Add("drawW", new CheckBox("Draw W Range"));
            DrawMenu.Add("drawE", new CheckBox("Draw E Range"));
            DrawMenu.Add("drawR", new CheckBox("Draw R Range"));
            DrawMenu.Add("drawRPred", new CheckBox("Draw R Range"));
        }
    }
}