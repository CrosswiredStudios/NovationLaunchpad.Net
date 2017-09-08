using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Midi;

namespace Launchpad.NET.Models
{
    public class LaunchpadTopButton
    {
        Launchpad.LaunchpadColor color;
        readonly IMidiOutPort outPort;

        public Launchpad.LaunchpadColor Color
        {
            get => color;
            set
            {
                outPort?.SendMessage(new MidiControlChangeMessage(0, Id, (byte)Color));
                color = value;
            }
        }

        public byte Id { get; set; }

        public LaunchpadTopButton(byte id, Launchpad.LaunchpadColor color)
        {
            Id = id;
            Color = color;
        }

        public LaunchpadTopButton(byte id, Launchpad.LaunchpadColor color, IMidiOutPort outPort)
        {
            this.outPort = outPort;
            Id = id;
            Color = color;
        }
    }
}
