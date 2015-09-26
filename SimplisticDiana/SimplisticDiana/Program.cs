using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;

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
            // Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            //  Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            //  Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += OnDraw;
        }

        public static void Game_OnTick(EventArgs args)
        {
            Spells.Start();
        }

        public static void OnDraw(EventArgs args)
        {
        }
    }
}