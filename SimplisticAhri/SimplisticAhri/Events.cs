using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace SimplisticAhri
{
    class Events
    {
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        
        public static void Init()
        {
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
        }



        private static long _lastChange = Environment.TickCount;

        public static void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapCloserEventArgs e)
        {
            if (sender.IsEnemy && e.End.Distance(_Player) < 200)
            {
                Program.Spells[SpellSlot.E].Cast(e.End);
            }
        }

        private static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
           
        }
    }
}
