using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Launchpad.NET.Models;
using System.Reactive;

namespace Launchpad.NET.Effects
{
    public enum LaunchPadEffectLayer
    {
        Background,
        Foreground
    }

    public interface ILaunchpadEffect
    {
        string Name { get; }
        IObservable<int> WhenChangeUpdateFrequency { get; }
        IObservable<Unit> WhenComplete { get; }
        void Initiate(Launchpad launchpad);
        void Terminate();
        void Update();
    }
}
