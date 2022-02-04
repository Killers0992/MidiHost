using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TobiasErichsen.teVirtualMIDI;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace MidiHost
{
    public class Program
    {
        public static ConcurrentQueue<MidiInfo> SendMidi = new ConcurrentQueue<MidiInfo>();

        public static TeVirtualMIDI midi;

        public static Config MidiConfig;

        static void Main(string[] args)
        {
            if (!File.Exists("./config.json"))
                File.WriteAllText("./config.json", JsonConvert.SerializeObject(new Config(), Formatting.Indented));

            MidiConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText("./config.json"));

            Task.Factory.StartNew(async () =>
            {
                Console.WriteLine($"Start virtual midi device with name {MidiConfig.MidiDeviceName} !");
                midi = new TeVirtualMIDI(MidiConfig.MidiDeviceName);
                while (true)
                {
                    if (SendMidi.TryDequeue(out MidiInfo type))
                    {
                        Console.WriteLine($"Dequeue midi {type.Number} from queue...");
                        Program.midi.sendCommand(new byte[]
                        {
                            0x90,   //Channel
                            Convert.ToByte(type.Number),   //Number
                            Convert.ToByte(type.Velocity),   //Velocity
                            0x03,
                        });
                        Console.WriteLine($"Send midi {type.Number} to virtual device {Program.MidiConfig.MidiDeviceName} !");
                    }
                    await Task.Delay(10);
                }
            });
            Console.WriteLine($"Starting twitch bot!");
            Bot bot = new Bot();
            Console.ReadLine();
        }
    }

    class Bot
    {
        TwitchClient client;

        public static List<int> execute = new List<int>();

        public Bot()
        {
            ConnectionCredentials credentials = new ConnectionCredentials(Program.MidiConfig.TwitchUsername, Program.MidiConfig.TwitchOAuth);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, Program.MidiConfig.ChannelName);

            client.OnLog += Client_OnLog;
            client.OnMessageReceived += Client_OnMessageReceived;

            client.Connect();
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            Console.WriteLine($" [BOT] {e.Data}");
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (!e.ChatMessage.Message.StartsWith("!"))
                return;

            string cmdName = e.ChatMessage.Message.Remove(0, 1);

            if (Program.MidiConfig.Commands.TryGetValue(cmdName.ToLower(), out MidiInfo midi))
            {
                Program.SendMidi.Enqueue(midi);
                Console.WriteLine($"User {e.ChatMessage.Username} executed command {cmdName} and sended midi data with Number {midi.Number} and Velocity {midi.Velocity}");
            }
        }
    }
}