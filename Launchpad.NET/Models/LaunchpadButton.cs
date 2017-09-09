using Windows.Devices.Midi;

namespace Launchpad.NET.Models
{
    public enum LaunchpadButtonState
    {
        Pressed,
        Released
    }

    public interface ILaunchpadButton
    {
        LaunchpadColor Color { get; set; }
        byte Id { get; set; }
        LaunchpadButtonState State { get; set; }
    }

    public class LaunchpadButton : ILaunchpadButton
    {
        LaunchpadColor color;
        readonly IMidiOutPort outPort;

        public LaunchpadColor Color
        {
            get => color;
            set
            {
                outPort?.SendMessage(new MidiNoteOnMessage(0, Id, (byte)Color));
                color = value;
            }
        }

        public byte Id { get; set; }

        public LaunchpadButton(byte id, LaunchpadColor color)
        {
            Id = id;
            Color = color;
            State = LaunchpadButtonState.Released;
        }

        public LaunchpadButton(byte id, LaunchpadColor color, IMidiOutPort outPort)
        {
            this.outPort = outPort;
            Id = id;
            Color = color;
            State = LaunchpadButtonState.Released;
        }

        public LaunchpadButtonState State { get; set; }
    }
}
