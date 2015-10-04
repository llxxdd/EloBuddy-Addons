using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using SimplisticGapcloser.Champions;

namespace SimplisticGapcloser
{
    internal class Program
    {
        public static bool Hero;

        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Checks;
        }

        public static void Checks(EventArgs args)
        {
            CheckHero();
            if (!Hero) return;
            Bootstrap.Init(null);
            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += OnDraw;
        }

        public static void Game_OnTick(EventArgs args)
        {
            CheckHero();
            if (!Hero)
            {
                Chat.Print("Simplistic Gapcloser: This Champion is not supported.");
                return;
            }
            Bootstrap.Init(null);
            Chat.Print("<b>Simplistic Gapcloser (", ObjectManager.Player.Hero, ")</b> - Loaded!");
        }

        public static void OnDraw(EventArgs args)
        {
        }

        public static bool CheckHero()
        {
            switch (ObjectManager.Player.Hero)
            {
                case Champion.Vayne:
                    Vayne.Start();
                    return true;
                case Champion.Thresh:
                    Thresh.Start();
                    return true;
                case Champion.Jinx:
                    Jinx.Start();
                    return true;
                case Champion.LeeSin:
                    Lee.Start();
                    return true;
                case Champion.Draven:
                    Draven.Start();
                    return true;
                case Champion.Tristana:
                    Tristana.Start();
                    return true;
                case Champion.Riven:
                    Riven.Start();
                    return true;
                case Champion.Caitlyn:
                    Caitlyn.Start();
                    return true;
            }
            return false;
        }
    }
}