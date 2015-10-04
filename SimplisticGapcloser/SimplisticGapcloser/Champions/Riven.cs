using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;

namespace SimplisticGapcloser.Champions
{
    internal class Riven
    {
        public static readonly Spell.Active sp = new Spell.Active(SpellSlot.W,
            (uint) _Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange);

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static void Start()
        {
            Gapcloser.OnGapcloser += g;
            Interrupter.OnInterruptableSpell += i;
            Config.Load();
            Config.GMenu.Add("useG", new CheckBox("Riven: Use W to Gapclose"));
            Config.IMenu.Add("useI", new CheckBox("Riven: Use W to Interrupt"));
        }

        private static void g(AIHeroClient s, Gapcloser.GapcloserEventArgs g)
        {
            if (Config.GMenu["useG"].Cast<CheckBox>().CurrentValue && _Player.Distance(g.Sender, true) <= sp.Range &&
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