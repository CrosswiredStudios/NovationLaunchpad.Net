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
        byte Channel { get; set; }
        byte Color { get; set; }
        byte Id { get; set; }
        LaunchpadButtonState State { get; set; }
    }

    public class LaunchpadButton : ILaunchpadButton
    {        
        byte color;
        readonly IMidiOutPort outPort;

        public byte Channel { get; set; }

        public byte Color
        {
            get => color;
            set
            {
                outPort?.SendMessage(new MidiNoteOnMessage(Channel, Id, (byte)Color));
                color = value;
            }
        }

        public byte Id { get; set; }

        public LaunchpadButton(byte channel, byte id, byte color)
        {
            this.color = color;
            Channel = channel;            
            Id = id;
            State = LaunchpadButtonState.Released;
        }

        public LaunchpadButton(byte channel, byte id, byte color, IMidiOutPort outPort)
        {
            this.color = color;
            this.outPort = outPort;
            Channel = channel;
            Id = id;            
            State = LaunchpadButtonState.Released;
        }

        public LaunchpadButtonState State { get; set; }
    }
}
