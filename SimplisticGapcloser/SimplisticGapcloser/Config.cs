using System.Linq;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace SimplisticGapcloser
{
    internal class Config
    {
        public static Menu menu,
            GMenu,
            IMenu,
            TMenu;

        public static void Load()
        {
            menu = MainMenu.AddMenu("Simplistic Gapcloser", "SimplisticGapcloser");
            menu.AddGroupLabel("Simplistic Gapcloser");
            menu.AddSeparator();

            GMenu = menu.AddSubMenu("Gapcloser", "gapcloser");
            GMenu.AddSeparator();

            IMenu = menu.AddSubMenu("Interrupter", "interrupter");
            IMenu.AddSeparator();

            TMenu = menu.AddSubMenu("Spells", "spells");
            var enemyChampions = HeroManager.Enemies.Select(obj => obj.ChampionName).ToArray();
            var ex = Gapcloser.GapCloserList.Where(s => enemyChampions.Contains(s.ChampName));
            foreach (var gap in ex)
            {
                var sname = gap.SpellName;
                TMenu.Add(sname.ToLower(), new CheckBox(gap.ChampName + " Spell " + gap.SpellSlot + "/ Gapclose it? "));
                TMenu.AddSeparator();
            }
        }
    }
}