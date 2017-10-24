using System.Runtime.InteropServices.WindowsRuntime;
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
        int X { get; set; }
        int Y { get; set; }
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

        public int X { get; set; }

        public int Y { get; set; }

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

        public void StartPulse(LaunchpadMk2Color color)
        {
            outPort?.SendMessage(new MidiNoteOnMessage(2, Id, (byte)color));
        }

        public void StopPulse()
        {
            outPort?.SendMessage(new MidiNoteOnMessage(1, Id, 0));
        }

        public void SetColor(byte r, byte g, byte b)
        {
            var command = new byte[] { 240, 0, 32, 41, 2, 24, 10, Id, r, g, b, 247 };
            outPort?.SendMessage(new MidiSystemExclusiveMessage(command.AsBuffer()));
        }

        public LaunchpadButtonState State { get; set; }
    }
}
