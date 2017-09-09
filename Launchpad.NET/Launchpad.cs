using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using Launchpad.NET.Effects;
using Launchpad.NET.Models;

/* 
 * Information for Uwp handling of MIDI devices
 * https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/midi
 * 
 * Information for launchpad programming
 * https://d2xhy469pqj8rc.cloudfront.net/sites/default/files/novation/downloads/4080/launchpad-programmers-reference.pdf
 */

namespace Launchpad.NET
{
    /// <summary>
    /// Colors supported by the Launchpad
    /// </summary>
    public enum LaunchpadColor
    {
        Off = 12,
        AmberLow = 29,
        AmberFull = 63,
        GreenLow = 28,
        GreenFull = 60,
        RedLow = 13,
        RedFull = 15,
        Yellow = 62,
    }

    /// <summary>
    /// Novation Launchpad .NET Interface
    /// </summary>
    public class Launchpad
    {
        List<LaunchpadButton> GridButtons;
        List<ILaunchpadEffect> effects;
        Dictionary<ILaunchpadEffect, IDisposable> effectsDisposables;
        readonly Dictionary<ILaunchpadEffect, Timer> effectsTimers;        
        List<LaunchpadButton> SideButtons;
        List<LaunchpadTopButton> TopButtons;
        bool initiated;
        MidiInPort inPort;
        readonly string name;
        IMidiOutPort outPort;
        List<LaunchpadButton> sideColumn;
        List<LaunchpadButton> topRow;
        Timer updateTimer;        
        readonly Subject<ILaunchpadButton> whenButtonStateChanged = new Subject<ILaunchpadButton>();       

        /// <summary>
        /// Observable event for when a button on the launchpad is pressed or released
        /// </summary>
        public IObservable<ILaunchpadButton> WhenButtonStateChanged => whenButtonStateChanged;

        /// <summary>
        /// Create a launchpad instance
        /// </summary>
        /// <param name="name">The name of the launchpad. Often 'Launchpad (1)'.</param>
        public Launchpad(string name)
        {
            initiated = false;

            this.name = name;
            effectsDisposables = new Dictionary<ILaunchpadEffect, IDisposable>();
            effectsTimers = new Dictionary<ILaunchpadEffect, Timer>();
            GridButtons = new List<LaunchpadButton>();
            SideButtons = new List<LaunchpadButton>();
            TopButtons = new List<LaunchpadTopButton>();
            effects = new List<ILaunchpadEffect>();            
        }

        /// <summary>
        /// Sets all grid buttons to Off
        /// </summary>
        public void ClearGridButtons()
        {
            for (var y = 0; y < 8; y++)
                for (var x = 0; x < 8; x++)
                {
                    SetButtonColor(x, y, LaunchpadColor.Off);
                }
        }

        /// <summary>
        /// Initiate the launchpad.
        /// </summary>
        public async void Initiate()
        {
            try
            {
                // Get all output MIDI devices
                string midiOutportQueryString = MidiOutPort.GetDeviceSelector();
                DeviceInformationCollection midiOutputDevices = await DeviceInformation.FindAllAsync(midiOutportQueryString);

                // Find the launchpad using the given name
                foreach (DeviceInformation deviceInfo in midiOutputDevices)
                {
                    if (deviceInfo.Name.ToLower().Contains(name.ToLower()))
                    {
                        outPort = await MidiOutPort.FromIdAsync(deviceInfo.Id);
                    }
                }

                // Get all input MIDI devices
                string midiInputQueryString = MidiInPort.GetDeviceSelector();
                DeviceInformationCollection midiInputDevices = await DeviceInformation.FindAllAsync(midiInputQueryString);

                // Find the launchpad
                foreach (DeviceInformation deviceInfo in midiInputDevices)
                {
                    if (deviceInfo.Name.ToLower().Contains(name.ToLower()))
                    {
                        inPort = await MidiInPort.FromIdAsync(deviceInfo.Id);

                        if (inPort == null)
                        {
                            System.Diagnostics.Debug.WriteLine("Unable to create MidiInPort from input device");
                            return;
                        }

                        // When we receive a message from the launchpad
                        inPort.MessageReceived += InPort_MessageReceived;
                    }
                }

                // Create all the grid buttons            
                for (var y = 0; y < 8; y++)
                for (var x = 0; x < 8; x++)
                {
                    GridButtons.Add(new LaunchpadButton((byte)(y * 16 + x), LaunchpadColor.Off, outPort));
                }

                // Create all the side buttons
                for (var x = 8; x < 120; x += 16)
                {
                    SideButtons.Add(new LaunchpadButton((byte)x, LaunchpadColor.Off, outPort));
                }

                // Create all the top buttons            
                for (var x = 104; x < 111; x++)
                    TopButtons.Add(new LaunchpadTopButton((byte)x, LaunchpadColor.Off, outPort));

                // We're done initiating!
                initiated = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }           
        }

        /// <summary>
        /// Process messages received from the Launchpad
        /// </summary>
        /// <param name="sender">Launchpad</param>
        /// <param name="args">Midi Message</param>
        void InPort_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            // Determine what kind of message it is
            switch (args.Message)
            {
                case MidiNoteOnMessage onMessage: // Grid and side buttons come as Midi Note On Message

                    // Get a reference to the button
                    var gridButton = GridButtons.FirstOrDefault(button => button.Id == onMessage.Note);

                    // If the grid button could not be found (should never happen), return
                    if (gridButton == null) return;

                    // Update the state (Launchpad sends midi on message for press and release - Velocity 0 is released, 127 is pressed)
                    gridButton.State = onMessage.Velocity == 0
                        ? LaunchpadButtonState.Released
                        : LaunchpadButtonState.Pressed;

                    // Notify any observable subscribers of the event
                    whenButtonStateChanged.OnNext(gridButton);               
                    break;
                case MidiControlChangeMessage changeMessage: // Top row comes as Control Change message
                                 
                    break;
            }
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
                // Add the effect to the launchpad
                effects.Add(effect);

                // When the effect is complete, unregister it (add the subscription to a dictionary so we can make sure to release it later)
                effectsDisposables.Add(effect,
                    effect
                        .WhenComplete
                        .Subscribe(UnregisterEffect));

                // Initiate the effect (provide all buttons and button changed event
                effect.Initiate(GridButtons, SideButtons, TopButtons, WhenButtonStateChanged);

                // Create an update timer at the specified frequency
                effectsTimers.Add(effect, new Timer(_=>effect.Update(), null, 0, (int)updateFrequency.TotalMilliseconds));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void SendControlChangeMessage(MidiControlChangeMessage message)
        {
            outPort.SendMessage(message);
        }

        public void SendMidiOffMessage(MidiNoteOffMessage message)
        {
            outPort.SendMessage(message);
        }

        public void SendMidiOnMessage(MidiNoteOnMessage message)
        {
            outPort.SendMessage(message);
        }

        public void SetButtonColor(int x, int y, LaunchpadColor color)
        {
            IMidiMessage midiMessageToSend = new MidiNoteOnMessage(0, (byte)(16 * y + x), (byte)color);

            outPort.SendMessage(midiMessageToSend);

            outPort.SendMessage(new MidiControlChangeMessage(0, (byte)(20 * y + x), 0));
        }

        /// <summary>
        /// Removes an effect from the launchpad
        /// </summary>
        /// <param name="effect">The effect to remove.</param>
        public void UnregisterEffect(ILaunchpadEffect effect)
        {
            try
            {
                // Dispose of the update timer for the effect
                effectsTimers[effect]?.Dispose();

                // Dispose of the OnComplete subscription
                effectsDisposables[effect]?.Dispose();

                // Remove the effect from the launchpad
                effects.Remove(effect);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
