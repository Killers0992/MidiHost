namespace MidiHost
{
    public class Config
    {
        public string TwitchUsername { get; set; } = "twitch-username";
        public string TwitchOAuth { get; set; } = "twitch-oauth";
        public string ChannelName { get; set; } = "channel-name";

        public string MidiDeviceName { get; set; } = "TwitchMidiHandler";

        public Dictionary<string, MidiInfo> Commands { get; set; } = new Dictionary<string, MidiInfo>()
        {
            { "test", new MidiInfo() { Number = 0, Velocity = 1, } }
        };
    }
}
