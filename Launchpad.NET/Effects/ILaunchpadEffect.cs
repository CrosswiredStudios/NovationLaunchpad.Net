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
        void ProcessInput(ILaunchpadButton button);
        void ProcessInput(byte buttonPressedId);
        void Update();
        TimeSpan UpdateFrequency { get; }
    }
}
