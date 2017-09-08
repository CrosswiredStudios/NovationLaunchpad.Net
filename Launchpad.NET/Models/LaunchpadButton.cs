﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Midi;

namespace Launchpad.NET.Models
{
    public interface ILaunchpadButton
    {
        Launchpad.LaunchpadColor Color { get; set; }
        byte Id { get; set; }
        bool IsPressed { get; set; }
    }

    public class LaunchpadButton : ILaunchpadButton
    {
        Launchpad.LaunchpadColor color;
        readonly IMidiOutPort outPort;

        public Launchpad.LaunchpadColor Color
        {
            get => color;
            set
            {
                outPort?.SendMessage(new MidiNoteOnMessage(0, Id, (byte)Color));
                color = value;
            }
        }

        public byte Id { get; set; }

        public LaunchpadButton(byte id, Launchpad.LaunchpadColor color)
        {
            Id = id;
            Color = color;
            IsPressed = false;
        }

        public LaunchpadButton(byte id, Launchpad.LaunchpadColor color, IMidiOutPort outPort)
        {
            this.outPort = outPort;
            Id = id;
            Color = color;
            IsPressed = false;
        }

        public bool IsPressed { get; set; }
    }
}
