#region

using System;
using System.Linq;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

#endregion

namespace SimplisticDiana
{
    internal class Config
    {
        public static Menu menu,
            ComboMenu,
            HarassMenu,
            FarmMenu,
            JungleMenu,
            GapMenu,
            GapSMenu,
            PredMenu,
            SkinMenu;

        public static void Load(EventArgs args)
        {
            menu = MainMenu.AddMenu("Simplistic Diana", "simplisticDiana");
            menu.AddGroupLabel("Simplistic Diana");
            menu.AddSeparator();

            ComboMenu = menu.AddSubMenu("Combo", "combo");
            ComboMenu.Add("misaya", new KeyBind("Misaya Combo (R->Q)", false, KeyBind.BindTypes.HoldActive, 'A'));
            ComboMenu.AddSeparator();
            ComboMenu.Add("GapKS", new CheckBox("Gapclose Minions to KS"));
            ComboMenu.Add("useRBuff", new CheckBox("Wait for R Buff even when Target is killable", false));
            ComboMenu.Add("useQ", new CheckBox("Use Q"));
            ComboMenu.Add("useW", new CheckBox("Use W"));
            ComboMenu.Add("useE", new CheckBox("Use E"));
            ComboMenu.Add("useI", new CheckBox("Use Ignite"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("useR", new CheckBox("Use R "));
            ComboMenu.Add("useR2", new CheckBox("Use R to KS"));
            ComboMenu.Add("useR2Count", new Slider("Max close enemies for secure kill with R", 5, 1, 5));

            HarassMenu = menu.AddSubMenu("Harass", "harass");
            HarassMenu.Add("useQHarass", new CheckBox("Use Q"));
            HarassMenu.Add("useWHarass", new CheckBox("Use W"));
            HarassMenu.Add("useEHarass", new CheckBox("Use E"));
            ComboMenu.AddSeparator();
            HarassMenu.Add("Mana", new Slider("Min. Mana Percent:", 20, 0, 100));

            FarmMenu = menu.AddSubMenu("Farm", "farm");
            FarmMenu.Add("qlc", new CheckBox("Use Q LaneClear"));
            FarmMenu.Add("wlc", new CheckBox("Use W LaneClear"));
            FarmMenu.Add("elc", new CheckBox("Use E LaneClear"));
            FarmMenu.AddSeparator();
            FarmMenu.Add("qct", new Slider("Minions in range for Q", 2, 1, 5));
            FarmMenu.AddSeparator();
            FarmMenu.Add("Mana", new Slider("Min. Mana Percent:", 20, 0, 100));

            JungleMenu = menu.AddSubMenu("JungleClear", "jungleclear");
            JungleMenu.Add("useQ", new CheckBox("Use Q", true));
            JungleMenu.Add("useW", new CheckBox("Use W", true));
            JungleMenu.Add("useE", new CheckBox("Use E", true));
            JungleMenu.AddSeparator();
            JungleMenu.Add("Mana", new Slider("Min. Mana Percent:", 20, 0, 100));

            GapMenu = menu.AddSubMenu("Automatic Events", "automaticevents");
            GapMenu.Add("GapW", new CheckBox("Gapclose using W"));
            GapMenu.Add("GapE", new CheckBox("Gapclose using E"));
            GapMenu.Add("IntE", new CheckBox("Interrupt using E"));
            GapMenu.Add("IntR", new CheckBox("Interrupt using R", false));

            GapSMenu = menu.AddSubMenu("Gapcloser Spells", "spells");
            var enemyChampions = EntityManager.Heroes.Enemies.Select(obj => obj.ChampionName).ToArray();
            var ex = Gapcloser.GapCloserList.Where(s => enemyChampions.Contains(s.ChampName));
            foreach (var gap in ex)
            {
                var sname = gap.SpellName;
                GapMenu.Add(sname.ToLower(), new CheckBox(gap.ChampName + " Spell " + gap.SpellSlot + "/ Gapclose it? "));
                GapMenu.AddSeparator();
            }

            PredMenu = menu.AddSubMenu("Prediction", "pred");
            PredMenu.AddGroupLabel("Q Hitchance");
            var qslider = PredMenu.Add("hQ", new Slider("Q HitChance", 2, 0, 2));
            var qMode = new[] {"Low (Fast Casting)", "Medium", "High (Slow Casting)"};
            qslider.DisplayName = qMode[qslider.CurrentValue];

            qslider.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = qMode[changeArgs.NewValue];
                };


            SkinMenu = menu.AddSubMenu("Skin Chooser", "skinchooser");
            SkinMenu.AddGroupLabel("Choose your Skin and puff!");

            var skin = SkinMenu.Add("sID", new Slider("Skin", 0, 0, 2));
            var sID = new[] {"Classic", "Dark Valkyrie", "Lunar Goddess"};
            skin.DisplayName = sID[skin.CurrentValue];

            skin.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = sID[changeArgs.NewValue];
                };

            Program.Checks();
        }
    }
}