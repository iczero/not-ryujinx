﻿using Gtk;
using Ryujinx.Common.Utilities;
using Ryujinx.Ui.Helper;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Ryujinx.Ui.Windows
{
    public partial class AboutWindow : Window
    {
        public AboutWindow() : base($"Ryujinx {Program.Version} - About")
        {
            InitializeComponent();

            _ = DownloadPatronsJson();
        }

        private async Task DownloadPatronsJson()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                _patreonNamesText.Buffer.Text = "Error.";
            }

            HttpClient httpClient = new HttpClient();

            try
            {
                string patreonJsonString = await httpClient.GetStringAsync("https://patreon.ryujinx.org/");

                _patreonNamesText.Buffer.Text = string.Join(", ", JsonHelper.Deserialize<string[]>(patreonJsonString));
            }
            catch
            {
                _patreonNamesText.Buffer.Text = "Error.";
            }
        }

        //
        // Events
        //
        private void RyujinxButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://ryujinx.org");
        }

        private void PatreonButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://www.patreon.com/ryujinx");
        }

        private void GitHubButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://github.com/Ryujinx/Ryujinx");
        }

        private void DiscordButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://discordapp.com/invite/N2FmfVc");
        }

        private void TwitterButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://twitter.com/RyujinxEmu");
        }

        private void ContributorsButton_Pressed(object sender, ButtonPressEventArgs args)
        {
            OpenHelper.OpenUrl("https://github.com/Ryujinx/Ryujinx/graphs/contributors?type=a");
        }
    }
}