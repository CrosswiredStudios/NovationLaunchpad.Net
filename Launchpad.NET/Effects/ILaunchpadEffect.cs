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
        List<LaunchpadButton> Initiate();
        List<LaunchpadButton> ProcessInput(byte buttonPressedId);
        List<LaunchpadButton> Update();
        TimeSpan UpdateFrequency { get; }
    }
}
