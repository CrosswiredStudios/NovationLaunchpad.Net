using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Launchpad.NET.Models;

namespace Launchpad.NET.Effects
{
    public interface ILaunchpadEffect
    {
        event EventHandler OnCompleted;
        void Initiate(List<LaunchpadButton> gridButtons, List<LaunchpadButton> sideButtons, List<LaunchpadTopButton> topButtons);
        void ProcessInput(ILaunchpadButton button, List<LaunchpadButton> grid);
        void ProcessInput(byte buttonPressedId, List<LaunchpadTopButton> top);
        List<LaunchpadButton> Update();
        TimeSpan UpdateFrequency { get; }
    }
}
