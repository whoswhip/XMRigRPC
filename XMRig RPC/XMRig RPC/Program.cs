using DiscordRPC;
using System;
using System.Diagnostics;
using System.Text.Json;



class Program
{
    public static Timestamps currentTimestamp = new Timestamps(DateTime.UtcNow);
    public static string address = "addressnotset";
    private DiscordRpcClient client;
    static async Task Main(string[] args)
    {
        Console.Title = "XMRig RPC v1.0";
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(" __  __     __    __     ______     __     ______        ______     ______   ______    \r\n/\\_\\_\\_\\   /\\ \"-./  \\   /\\  == \\   /\\ \\   /\\  ___\\      /\\  == \\   /\\  == \\ /\\  ___\\   \r\n\\/_/\\_\\/_  \\ \\ \\-./\\ \\  \\ \\  __<   \\ \\ \\  \\ \\ \\__ \\     \\ \\  __<   \\ \\  _-/ \\ \\ \\____  \r\n  /\\_\\/\\_\\  \\ \\_\\ \\ \\_\\  \\ \\_\\ \\_\\  \\ \\_\\  \\ \\_____\\     \\ \\_\\ \\_\\  \\ \\_\\    \\ \\_____\\ \r\n  \\/_/\\/_/   \\/_/  \\/_/   \\/_/ /_/   \\/_/   \\/_____/      \\/_/ /_/   \\/_/     \\/_____/ \r\n                                                                                       ");
        Program program = new Program();

        

        if (File.Exists("address.txt"))
        {
            address = File.ReadAllText("address.txt");
            string path = Path.GetFullPath("address.txt");
            if (address == "addressnotset")
            {
                Console.Write("Please enter your Monero address: ");
                address = Console.ReadLine().ToString();
                File.WriteAllText("address.txt", address);
                address = File.ReadAllText("address.txt");
            }
        }
        else
        {
            File.Create("address.txt").Close();
            string path = Path.GetFullPath("address.txt");
            if (address == "addressnotset")
            {
                Console.Write("Please enter your Monero address: ");
                address = Console.ReadLine().ToString();
                File.WriteAllText("address.txt", address);
                address = File.ReadAllText("address.txt");
            }
        }
        program.InitializeDiscordRpcClient();
        program.Updater();
        Console.WriteLine("Discord RPC is running. Press any key to stop...");
        Console.ReadKey();

        program.client.ClearPresence();
        program.client.Dispose();
    }
    private void InitializeDiscordRpcClient()
    {
        client = new DiscordRpcClient("1227193008768815134");
        client.Initialize();
    }
    private async void Updater()
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
    public static async Task<string> Hash()
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
}
