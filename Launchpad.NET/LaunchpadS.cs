using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Windows.Devices.Midi;
using Launchpad.NET.Effects;
using Launchpad.NET.Models;

/*  
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
    public class LaunchpadS : Launchpad
    {
              
        
        bool initiated;
        readonly string name;
        List<LaunchpadButton> sideColumn;
        List<LaunchpadButton> topRow;
        Timer updateTimer;                             

        /// <summary>
        /// Create a launchpad instance
        /// </summary>
        /// <param name="name">The name of the launchpad. Often 'Launchpad (1)'.</param>
        public LaunchpadS(string name, MidiInPort inPort, IMidiOutPort outPort)
        {
            Name = name;
            this.inPort = inPort;
            this.outPort = outPort;           
            effectsDisposables = new Dictionary<ILaunchpadEffect, IDisposable>();
            effectsTimers = new Dictionary<ILaunchpadEffect, Timer>();
            gridButtons = new List<LaunchpadButton>();
            sideButtons = new List<LaunchpadButton>();
            topButtons = new List<LaunchpadTopButton>();
            Effects = new ObservableCollection<ILaunchpadEffect>();            

            // Create all the grid buttons            
            for (var y = 0; y < 8; y++)
            for (var x = 0; x < 8; x++)
            {
                gridButtons.Add(new LaunchpadButton((byte)0, (byte)(y * 16 + x), LaunchpadColor.Off, outPort));
            }

            // Create all the side buttons
            for (var x = 8; x < 120; x += 16)
            {
                sideButtons.Add(new LaunchpadButton((byte)0, (byte)x, LaunchpadColor.Off, outPort));
            }

            // Create all the top buttons            
            for (var x = 104; x < 111; x++)
                topButtons.Add(new LaunchpadTopButton((byte)x, LaunchpadColor.Off, outPort));

            // Process messages from device
            inPort.MessageReceived += InPort_MessageReceived;
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

                    Debug.WriteLine($"Grid ({onMessage.Note % 16}, {(int)(onMessage.Note / 16)}) " + (onMessage.Velocity == 0 ? "Released" : "Pressed"));

                    // Get a reference to the button
                    var gridButton = gridButtons.FirstOrDefault(button => button.Id == onMessage.Note);

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
        /// Sends a MIDI message to the Launchpad
        /// </summary>
        /// <param name="message">The MIDI message to send.</param>
        public override void SendMessage(IMidiMessage message)
        {
            outPort.SendMessage(message);
        }

        /// <summary>
        /// Sets the color of the specified grid button
        /// </summary>
        /// <param name="x">X coordinate, should be 0-7.</param>
        /// <param name="y">Y coordinate, should be 0-7.</param>
        /// <param name="color">The color to set the button to.</param>
        public override void SetButtonColor(int x, int y, LaunchpadColor color)
        {
            SendMessage(new MidiNoteOnMessage(0, (byte)(16 * y + x), (byte)color));
        }

        /// <summary>
        /// Removes an effect from the launchpad
        /// </summary>
        /// <param name="effect">The effect to remove.</param>
        public override void UnregisterEffect(ILaunchpadEffect effect)
        {
            try
            {
                // Dispose of the update timer for the effect
                effectsTimers[effect]?.Dispose();

                // Dispose of the OnComplete subscription
                effectsDisposables[effect]?.Dispose();

                // Remove the effect from the launchpad
                Effects.Remove(effect);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
