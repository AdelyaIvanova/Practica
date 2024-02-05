using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace PraktikaFurniture
{
    public class Updater
    {
        public GitHubClient? GitClient { get; private set; }
        public Task<IReadOnlyList<Release>>? Releases { get; private set; }
        private string? ExeName { get; set; }
        private string? ExePath { get; set; }

        public Updater()
        {
            if (IsConnectionOk())
            {
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);
                ExeName = AppDomain.CurrentDomain.FriendlyName + ".exe";
                ExePath = Process.GetCurrentProcess().MainModule.FileName;

                GitClient = new GitHubClient(new ProductHeaderValue("praktika-jewelry"))
                { Credentials = new Credentials("gh?p_LB?kRkrTc?uzdYRLdYCEZ?wP?phZ59?XmAk1M?24c4".Replace("?", "")) };
                Releases = GitClient.Repository.Release.GetAll("AdelyaIvanova", "Practica");
            }
        }

        public static bool IsConnectionOk()
        {
            try
            {
                Dns.GetHostEntry("github.com");
                return true;
            }
            catch { return false; }
        }

        public void ApplyNewUpdateCmd()
        { Cmd($"taskkill /f /im \"{ExeName}\" && timeout /t 1 && del \"{ExePath}\" && ren newUpdate.exe \"{ExeName}\" && \"{ExePath}\""); }

        private void Cmd(string line)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {line}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                });
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
