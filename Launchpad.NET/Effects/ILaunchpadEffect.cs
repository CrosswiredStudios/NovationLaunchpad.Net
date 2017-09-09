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
        IObservable<ILaunchpadEffect> WhenComplete { get; }
        void Initiate(List<LaunchpadButton> gridButtons, List<LaunchpadButton> sideButtons, List<LaunchpadTopButton> topButtons, IObservable<ILaunchpadButton> whenButtonStateChanged);
        void Update();
        
    }
}
