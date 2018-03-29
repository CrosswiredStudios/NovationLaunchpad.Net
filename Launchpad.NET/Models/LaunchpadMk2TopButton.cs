using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Midi;
using Windows.UI;

namespace Launchpad.NET.Models
{
    public class LaunchpadMk2TopButton : ViewModelBase, ILaunchpadButton
    {
        Color color;
        readonly IMidiOutPort outPort;

        public byte Channel { get; set; }
        public Color Color
        {
            get => color;
            set
            {
                color = value;
                RaisePropertyChanged();
            }
        }
        public byte Id { get; set; }
        public LaunchpadButtonState State { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsPulsing { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public LaunchpadMk2TopButton(byte id, Color color)
        {
            Id = id;
            Color = color;
        }

        public LaunchpadMk2TopButton(byte id, Color color, IMidiOutPort outPort)
        {
            this.outPort = outPort;
            Id = id;
            Color = color;
        }
    }
}
