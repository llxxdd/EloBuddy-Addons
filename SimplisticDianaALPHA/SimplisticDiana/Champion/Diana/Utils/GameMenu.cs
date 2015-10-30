#region

using EloBuddy;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

#endregion

namespace SimplisticTemplate.Champion.Diana.Utils
{
    internal static class GameMenu
    {
        private static Menu _menu;
        public static Menu ComboMenu;
        private static Menu _harassMenu;

        public static Menu FarmingMenu,
            DrawMenu;

        private static AIHeroClient Me
        {
            get { return ObjectManager.Player; }
        }

        public static void Initialize()
        {
            _menu = MainMenu.AddMenu("Simplistic " + Me.ChampionName, Me.ChampionName.ToLower());
            _menu.AddLabel("Simplistic Diana");
            _menu.AddLabel("by nonm");
            _menu.AddSeparator();
            _menu.AddLabel(
                "NOTE: This AddOn is really Complex, you should turn off R Gaploser and Drawings if you have a bad computer!");

            ComboMenu = _menu.AddSubMenu("Combo", "combo");
            ComboMenu.AddLabel("Combo Settings");
            ComboMenu.Add("misayacombo", new KeyBind("Misaya (R-Q) Burst", false, KeyBind.BindTypes.HoldActive, 'A'));
            ComboMenu.Add("useQ", new CheckBox("Use Q"));
            ComboMenu.Add("useW", new CheckBox("Use W"));
            ComboMenu.Add("useE", new CheckBox("Use E"));
            ComboMenu.Add("useR", new CheckBox("Use R"));
            ComboMenu.Add("useRnoBuff", new CheckBox("Use R even with no Q Buff if killable"));
            ComboMenu.Add("useRKS", new CheckBox("Use R to Steal a Kill"));
            ComboMenu.Add("useRMax", new Slider("Max close enemies for secure kill with R", 3, 1, 5));
            ComboMenu.Add("useRGap", new CheckBox("Use R on Minion to Gapclose and Kill Target"));

            _harassMenu = _menu.AddSubMenu("Harass", "harass");
            _harassMenu.AddLabel("Harass Settings");
            _harassMenu.Add("useQ", new CheckBox("Use Q"));
            _harassMenu.Add("useW", new CheckBox("Use W"));
            _harassMenu.Add("useE", new CheckBox("Use E"));

            FarmingMenu = _menu.AddSubMenu("Farming", "farming");
            FarmingMenu.AddLabel("LaneClear and JungleClear Settings");
            FarmingMenu.Add("useFarm",
                new KeyBind("Turn on LaneClear with Spells", true, KeyBind.BindTypes.PressToggle, 0x04));
            FarmingMenu.Add("useJungle", new CheckBox("LaneClear enabled"));
            FarmingMenu.Add("useQ", new CheckBox("Use Q"));
            FarmingMenu.Add("useW", new CheckBox("Use W"));
            FarmingMenu.Add("useE", new CheckBox("Use E"));

            DrawMenu = _menu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddLabel("Drawing Settings");
            DrawMenu.Add("disable", new CheckBox("Disable all Drawing", false));
            DrawMenu.Add("drawDamage", new CheckBox("Draw Damage"));
            DrawMenu.Add("drawQ", new CheckBox("Draw Q Range"));
            DrawMenu.Add("drawW", new CheckBox("Draw W Range"));
            DrawMenu.Add("drawE", new CheckBox("Draw E Range"));
            DrawMenu.Add("drawR", new CheckBox("Draw R Range"));
            DrawMenu.Add("drawRPred", new CheckBox("Draw Q Prediction"));
        }
    }
}