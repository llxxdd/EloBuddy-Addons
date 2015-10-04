using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;

namespace SimplisticGapcloser.Champions
{
    internal class Jinx
    {
        public static readonly Spell.Skillshot sp = new Spell.Skillshot(SpellSlot.R, 920, SkillShotType.Circular, 250,
            1750, 1200);

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static void Start()
        {
            Gapcloser.OnGapcloser += g;
            Interrupter.OnInterruptableSpell += i;
            Config.Load();
            Config.GMenu.Add("useG", new CheckBox("Jinx: Use E to Gapclose"));
            Config.IMenu.Add("useI", new CheckBox("Jinx: Use E to Interrupt"));
        }

        private static void g(AIHeroClient s, Gapcloser.GapcloserEventArgs g)
        {
            if (Config.GMenu["useG"].Cast<CheckBox>().CurrentValue && _Player.Distance(g.Sender, true) < sp.Range &&
                s.IsValidTarget() && sp.IsReady() && Config.TMenu[g.SpellName].Cast<CheckBox>().CurrentValue)
            {
                var pred = sp.GetPrediction(g.Sender);
                sp.Cast(pred.CastPosition);
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