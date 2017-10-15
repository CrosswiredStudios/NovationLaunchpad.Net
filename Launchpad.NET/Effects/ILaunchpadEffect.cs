using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Launchpad.NET.Models;
using System.Reactive;
using System.Reactive.Disposables;

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
        IObservable<ILaunchpadEffect> WhenComplete { get; }
        void Dispose();
        void Initiate(Launchpad launchpad);
        void Terminate();
        void Update();
    }

    public abstract class LaunchpadEffect : ILaunchpadEffect, IDisposable
    {
        protected CompositeDisposable disposables;

        public virtual string Name => "Launchpad Effect";

        public virtual IObservable<int> WhenChangeUpdateFrequency => null;

        public virtual IObservable<ILaunchpadEffect> WhenComplete => null;

        public LaunchpadEffect()
        {
            disposables = new CompositeDisposable();
        }

        public void Dispose()
        {
            disposables.Dispose();
        }

        public virtual void Initiate(Launchpad launchpad)
        {
            throw new NotImplementedException();
        }

        public virtual void Terminate()
        {
            throw new NotImplementedException();
        }

        public virtual void Update()
        {
            throw new NotImplementedException();
        }
    }
}
