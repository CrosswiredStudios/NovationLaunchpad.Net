using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Midi;
using Launchpad.NET.Effects;
using Launchpad.NET.Models;

// https://global.novationmusic.com/sites/default/files/novation/downloads/10529/launchpad-mk2-programmers-reference-guide_0.pdf

namespace Launchpad.NET
{
    public enum LaunchpadMK2Color : byte
    {
        Off = 0,
        Black = 1,
        White = 3,
        Salmon = 4,
        Red = 5,
        Maroon = 6,
        Black2 = 7,
        Peach = 8,
        Orange = 9,
        Brown = 10,
        DarkBrown = 11,
        Yellow = 12,
        Pink = 53
    }

    public class LaunchpadMk2 : Launchpad
    {
        public LaunchpadMk2(string name, MidiInPort inPort, IMidiOutPort outPort)
        {
            Name = name;
            this.inPort = inPort;
            this.outPort = outPort;            
            Effects = new ObservableCollection<ILaunchpadEffect>();
            gridButtons = new List<LaunchpadButton>();
            topButtons = new List<LaunchpadTopButton>();

            // Create all the grid buttons            
            for (var y = 1; y <= 8; y++)
            for (var x = 1; x <= 9; x++)
            {
                    gridButtons.Add(new LaunchpadButton(0, (byte)(int.Parse(y.ToString() + x.ToString())), (byte)LaunchpadMK2Color.Off, outPort));
            }

            // Create all the top buttons            
            for (var x = 104; x < 111; x++)
                topButtons.Add(new LaunchpadTopButton((byte)x, (byte)LaunchpadColor.Off, outPort));

            for (var y = 0; y < 8; y++)
                for (var x = 0; x < 8; x++)
                {
                    SetButtonColor(x, y, LaunchpadColor.Off);
                }

            // Process messages from device
            inPort.MessageReceived += InPort_MessageReceived;
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

                    Debug.WriteLine($"Grid Button Ch:{onMessage.Channel}, Note:{onMessage.Note}, Vel:{onMessage.Velocity} (x:{onMessage.Note % 10}, y:{(int)(onMessage.Note / 10)}) " + (onMessage.Velocity == 0 ? "Released" : "Pressed"));

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

        public override void SendMessage(IMidiMessage message)
        {
            outPort.SendMessage(message);
        }

        public override void SetButtonColor(int x, int y, LaunchpadColor color)
        {
            
        }

        public override void UnregisterEffect(ILaunchpadEffect effect)
        {
            
        }
    }
}
