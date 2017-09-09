using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Launchpad.NET.Models;

namespace Launchpad.NET.Effects
{
    public enum LaunchPadEffectLayer
    {
        Background,
        Foreground
    }

    public interface ILaunchpadEffect
    {
        LaunchPadEffectLayer Layer { get; set; }
        IObservable<ILaunchpadEffect> WhenComplete { get; set; }
        void Initiate(List<LaunchpadButton> gridButtons, List<LaunchpadButton> sideButtons, List<LaunchpadTopButton> topButtons, IObservable<ILaunchpadButton> whenButtonStateChanged);
        void ProcessInput(ILaunchpadButton button);
        void ProcessInput(byte buttonPressedId);
        void Update();
        
    }
}
