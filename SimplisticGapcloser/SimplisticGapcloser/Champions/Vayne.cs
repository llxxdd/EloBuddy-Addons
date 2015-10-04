using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;

namespace SimplisticGapcloser.Champions
{
    internal class Vayne
    {
        public static readonly Spell.Targeted sp = new Spell.Targeted(SpellSlot.E,
            (uint) _Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange);

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static void Start()
        {
            Gapcloser.OnGapcloser += g;
            Interrupter.OnInterruptableSpell += i;
            Config.Load();
            Config.GMenu.Add("useG", new CheckBox("Vayne: Use E to Gapclose"));
            Config.IMenu.Add("useI", new CheckBox("Vayne: Use E to Interrupt"));
        }

        private static void g(AIHeroClient s, Gapcloser.GapcloserEventArgs g)
        {
            if (Config.GMenu["useG"].Cast<CheckBox>().CurrentValue && _Player.Distance(g.Sender, true) < sp.Range &&
                s.IsValidTarget() && sp.IsReady() && Config.TMenu[g.SpellName].Cast<CheckBox>().CurrentValue)
            {
                sp.Cast(g.Sender);
            }
        }

        private static void i(Obj_AI_Base s,
            Interrupter.InterruptableSpellEventArgs g)
        {
            if (Config.GMenu["useI"].Cast<CheckBox>().CurrentValue && _Player.Distance(g.Sender, true) < sp.Range &&
                s.IsValidTarget() && sp.IsReady())
            {
                sp.Cast(g.Sender);
            }
        }
    }
}