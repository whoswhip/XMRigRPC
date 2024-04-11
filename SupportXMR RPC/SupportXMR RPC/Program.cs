using DiscordRPC;
using SupportXMR_RPC;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SupportXMR_RPC
{

    public partial class Form1 : Form
    {
        private NotifyIcon notifyIcon;
        private DiscordRpcClient client;
        public static string address = "addressnotset";
        public static Timestamps currentTimestamp = new Timestamps(DateTime.UtcNow);
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }


        public Form1()
        {
            if (File.Exists("address.txt"))
            {
                address = File.ReadAllText("address.txt");
                string path = Path.GetFullPath("address.txt");
                if (address == "addressnotset")
                {
                    MessageBox.Show($"Please enter your SupportXMR address in the text file at {path}", "SupportXMR RPC", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                File.Create("address.txt").Close();
                string path = Path.GetFullPath("address.txt");
                if (address == "addressnotset")
                {
                    MessageBox.Show($"Please enter your SupportXMR address in the text file at {path}", "SupportXMR RPC", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            InitializeNotifyIcon();
            InitializeDiscordRpcClient();
            HideForm();
            StartHashing();
            //File.Create("address.txt").Close();
            File.WriteAllText("address.txt", address);
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SupportXMR_RPC.icon.ico"));
            notifyIcon.Text = "Discord RPC";
            notifyIcon.Visible = true;

            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            ToolStripMenuItem showMenuItem = new ToolStripMenuItem("Show");
            showMenuItem.Click += (sender, e) =>
            {
                ShowForm();
            };
            contextMenuStrip.Items.Add(showMenuItem);

            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += (sender, e) =>
            {
                ExitApplication();
            };
            contextMenuStrip.Items.Add(exitMenuItem);

            notifyIcon.ContextMenuStrip = contextMenuStrip;
        }

        private void InitializeDiscordRpcClient()
        {
            client = new DiscordRpcClient("1227193008768815134");
            client.Initialize();
        }

        private async void StartHashing()
        {
            Thread.Sleep(2500); // Wait for the user to read and put their address in the text file, if not display a message box
            if (address == "addressnotset")
            {
                MessageBox.Show("Please enter your SupportXMR address in the text file at " + Path.GetFullPath("address.txt") +", continuing anyway!", "SupportXMR RPC", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                while (true)
                {
                    //Debug.WriteLine("Updated!");
                    address = File.ReadAllText("address.txt");
                    await Hash();
                    await UpdatePresence();
                    await Task.Delay(1750);
                }
            }
            else
            {
                while (true)
                {
                    //Debug.WriteLine("Updated!");
                    address = File.ReadAllText("address.txt");
                    await Hash();
                    await UpdatePresence();
                    await Task.Delay(1750);
                }
            }

        }

        public async Task<string> Hash()
        {
            string formattedHashes = "";

            using (HttpClient httpClient = new HttpClient())
            {
                string url = "https://supportxmr.com/api/miner/" + address + "/stats";
                string jsonResponse = await httpClient.GetStringAsync(url);

                JsonDocument document = JsonDocument.Parse(jsonResponse);
                JsonElement root = document.RootElement;
                long totalHashes = root.GetProperty("totalHashes").GetInt64();
                formattedHashes = totalHashes.ToString("N0");
                //Debug.WriteLine(formattedHashes);
                //Debug.WriteLine(totalHashes);
                //Debug.WriteLine(url);
            }

            return formattedHashes;
        }

        private async Task UpdatePresence()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string formattedHashes = await Hash();
                //Debug.WriteLine(formattedHashes);
                bool isXmrigRunning = Process.GetProcessesByName("xmrig").Length > 0;
                if (isXmrigRunning)
                {
                    client.SetPresence(new RichPresence()
                    {
                        Details = "Mining XMR",
                        State = "Mined Hashes: " + formattedHashes,
                        Timestamps = currentTimestamp,
                        Assets = new Assets()
                        {
                            LargeImageKey = "monero-logo-png-transparent-993169300",
                            LargeImageText = "Monero",
                            SmallImageKey = "xmrrpclogo",
                            SmallImageText = "SupportXMR RPC v1.0",
                        }
                    });
                }
                else
                {
                    client.SetPresence(new RichPresence()
                    {
                        Details = "Idling...",
                        Timestamps = currentTimestamp,
                        Assets = new Assets()
                        {
                            LargeImageKey = "monero-logo-png-transparent-993169300",
                            LargeImageText = "Monero",
                            SmallImageKey = "xmrrpclogo",
                            SmallImageText = "SupportXMR RPC v1.0",
                        }
                    });
                }
            }
        }

        private void ShowForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void HideForm()
        {
            this.Hide();
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
        }

        private void ExitApplication()
        {
            notifyIcon.Visible = false;
            Application.Exit();
            client.Dispose();
        }

        private TextBox textBox1;
        //private void textBox1_TextChanged(object sender, EventArgs e)
        //{
        //    address = textBox1.Text;
        //    File.WriteAllText("address.txt", address);
        //}

    }
}
