#region

using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;

#endregion

namespace SimplisticDiana
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Config.Load;
        }

        public static void Checks()
        {
            if (ObjectManager.Player.Hero != Champion.Diana)
            {
                Chat.Print("Champion not supported!");
                return;
            }
            Chat.Print("<b>Simplistic Diana</b> - Loaded!");

            Bootstrap.Init(null);
            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += Spells.g;
            Interrupter.OnInterruptableSpell += Spells.i;
        }

        private static void Game_OnTick(EventArgs args)
        {
            Spells.Start();
        }
    }
}