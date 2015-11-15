#region

using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using SimplisticTemplate.Champion.Diana.Utils;

#endregion

namespace SimplisticTemplate.Champion.Diana.Modes
{
    internal static class Farming
    {
        // ReSharper disable once RedundantJumpStatement
        public static void ExecuteLaneClear()
        {
            if (!GameMenu.FarmingMenu["useFarm"].Cast<KeyBind>().CurrentValue) return;
            var useQ = GameMenu.FarmingMenu["useQ"].Cast<CheckBox>().CurrentValue;
            var useQCount = GameMenu.FarmingMenu["useQCount"].Cast<Slider>().CurrentValue;
            var useW = GameMenu.FarmingMenu["useW"].Cast<CheckBox>().CurrentValue;
            var useE = GameMenu.FarmingMenu["useE"].Cast<CheckBox>().CurrentValue;

            var minions =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                    ObjectManager.Player.ServerPosition, Diana.Q.Range, false).ToArray();

            if (minions.Length == 0)
            {
                return;
            }

            if (useQ && Diana.Q.IsReady())
            {
                if (
                    Diana.Q.Cast(
                        EntityManager.MinionsAndMonsters.GetLineFarmLocation(minions, Diana.Q.Width, (int) Diana.Q.Range)
                            .CastPosition) &&
                    EntityManager.MinionsAndMonsters.GetLineFarmLocation(minions, Diana.Q.Width, (int) Diana.Q.Range)
                        .HitNumber > useQCount)
                {
                    return;
                }
            }

            if (useW && Diana.W.IsReady() &&
                minions.FirstOrDefault().Distance(ObjectManager.Player.Position) < Diana.W.Range)
            {
                if (
                    Diana.W.Cast())
                {
                    return;
                }
            }

            if (useE && Diana.E.IsReady() &&
                minions.FirstOrDefault().Distance(ObjectManager.Player.Position) < Diana.E.Range)
            {
                if (
                    Diana.E.Cast())
                {
                    return;
                }
            }
        }

        public static void ExecuteJungleClear()
        {
            if (!GameMenu.FarmingMenu["useJungle"].Cast<CheckBox>().CurrentValue) return;
            var useQ = GameMenu.FarmingMenu["useQ"].Cast<CheckBox>().CurrentValue;
            var useW = GameMenu.FarmingMenu["useW"].Cast<CheckBox>().CurrentValue;
            var useE = GameMenu.FarmingMenu["useE"].Cast<CheckBox>().CurrentValue;

            var minions =
                EntityManager.MinionsAndMonsters.GetJungleMonsters(ObjectManager.Player.ServerPosition, Diana.Q.Range,
                    false).ToArray();
            if (minions.Length == 0)
            {
                return;
            }

            if (useQ && Diana.Q.IsReady())
            {
                if (
                    Diana.Q.Cast(
                        EntityManager.MinionsAndMonsters.GetLineFarmLocation(minions, Diana.Q.Width, (int) Diana.Q.Range)
                            .CastPosition))
                {
                    return;
                }
            }

            if (useW && Diana.W.IsReady() &&
                minions.FirstOrDefault().Distance(ObjectManager.Player.Position) < Diana.W.Range)
            {
                if (
                    Diana.W.Cast())
                {
                    return;
                }
            }

            if (useE && Diana.E.IsReady() &&
                minions.FirstOrDefault().Distance(ObjectManager.Player.Position) < Diana.E.Range)
            {
                if (
                    Diana.E.Cast())
                {
                }
            }
        }
    }
}