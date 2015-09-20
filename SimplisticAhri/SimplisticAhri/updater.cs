using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EloBuddy;

namespace SimplisticAhri
{
    public static class updater
    {
        public static void UpdateCheck()
        {
            Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        using (var c = new WebClient())
                        {
                            var rawVersion =
                                c.DownloadString(
                                    "https://raw.githubusercontent.com/nonmCoding/EloBuddy-Addons/master/SimplisticAhri/SimplisticAhri/Properties/AssemblyInfo.cs");

                            var match =
                                new Regex(
                                    @"\[assembly\: AssemblyVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]")
                                    .Match(rawVersion);

                            if (match.Success)
                            {
                                var gitVersion =
                                    new System.Version(
                                        string.Format(
                                            "{0}.{1}.{2}.{3}",
                                            match.Groups[1],
                                            match.Groups[2],
                                            match.Groups[3],
                                            match.Groups[4]));

                                if (gitVersion != SimplisticAhri.Program.Version)
                                {
                                    Chat.Print("<b>SimplisticAhri</b> - <font color=\"#FF6666\">Outdated & newer version available!</font>");
                                }
                                else
                                {
                                    Chat.Print("<b>SimplisticAhri</b> - <font color=\"#FF6666\">You have got the newest Version!</font>");
                                }
                            }
                        }
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
        }
    }
}