using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using Launchpad.NET.Effects;
using Launchpad.NET.Models;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Reactive.Disposables;
using System.Reactive;

namespace Launchpad.NET
{
    public interface ILaunchpad
    {
        
    }

    public abstract class Launchpad
    {
        
        protected List<LaunchpadMk2Button> gridButtons;
        protected MidiInPort inPort;
        protected IMidiOutPort outPort;
        protected List<LaunchpadMk2Button> sideButtons;
        protected List<LaunchpadMk2TopButton> topButtons;        
        protected readonly Subject<Unit> whenButtonColorsChanged = new Subject<Unit>();
        protected readonly Subject<ILaunchpadButton> whenButtonStateChanged = new Subject<ILaunchpadButton>();
        protected readonly Subject<Unit> whenDisconnected = new Subject<Unit>();
        protected readonly Subject<Unit> whenReset = new Subject<Unit>();

        public Dictionary<ILaunchpadEffect, CompositeDisposable> EffectsDisposables { get; }
        public Dictionary<ILaunchpadEffect, Timer> EffectsTimers { get; }
        public string Name { get; set; }
        public IObservable<Unit> WhenButtonColorsChanged => whenButtonColorsChanged;
        /// <summary>
        /// Observable event for when a button on the launchpad is pressed or released
        /// </summary>
        public IObservable<ILaunchpadButton> WhenButtonStateChanged => whenButtonStateChanged;
        public IObservable<Unit> WhenDisconnected => whenDisconnected;
        public IObservable<Unit> WhenReset => whenReset;

        public Launchpad()
        {
            EffectsDisposables = new Dictionary<ILaunchpadEffect, CompositeDisposable>();
            EffectsTimers = new Dictionary<ILaunchpadEffect, Timer>();
        }

        void OnChangeEffectUpdateFrequency(ILaunchpadEffect effect, int newFrequency)
        {
            EffectsTimers[effect].Change(0, newFrequency);
        }

        public void RegisterEffect(ILaunchpadEffect effect, int updateFrequency)
        {
            RegisterEffect(effect, TimeSpan.FromMilliseconds(updateFrequency));
        }

        /// <summary>
        /// Add an effect to the launchpad
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="updateFrequency"></param>
        public void RegisterEffect(ILaunchpadEffect effect, TimeSpan updateFrequency)
        {
            try
            {

                // Register any observables being used
                CompositeDisposable effectDisposables = new CompositeDisposable();

                // If this effect needs the ability to change its frequency
                if(effect.WhenChangeUpdateFrequency != null)
                {
                    // Subscribe to the event to change the frequency and add it to this effects disposables
                    effectDisposables.Add(
                        effect
                        .WhenChangeUpdateFrequency
                        .Subscribe(newFrequency=>
                        {
                            // Change the frequency for this effect
                            OnChangeEffectUpdateFrequency(effect, newFrequency);
                        }));
                }

                // If this effect will notify us it needs to be unregistered
                if(effect.WhenComplete != null)
                {
                    effectDisposables.Add(
                        effect
                        .WhenComplete
                        .Subscribe(_ => 
                        {
                            // Unregister the effect and destroy its disposables
                            UnregisterEffect(effect);
                        }));
                }

                EffectsDisposables.Add(effect, effectDisposables);

                // Create an update timer at the specified frequency
                EffectsTimers.Add(effect, new Timer(state => effect.Update(), null, 0, (int)updateFrequency.TotalMilliseconds));

                // Initiate the effect (provide all buttons and button changed event
                effect.Initiate(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }
        public abstract void SendMessage(IMidiMessage message);

        public abstract void UnregisterEffect(ILaunchpadEffect effect);

        public abstract void UnregisterAllEffects();
    }

    
}
