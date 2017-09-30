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
        Grey = 2,
        White = 3,
        Salmon = 4,
        Red = 5,
        DarkRed = 6,
        DarkerRed = 7,
        Beige = 8,
        Orange = 9,
        Brown = 10,
        DarkBrown = 11,
        Offwhite = 12,
        Yellow = 13,
        ArmyGreen = 14,
        DarkArmyGreen = 15,
        Teal = 16,
        ElectricGreen = 17,
        SkyBlue = 40,
        LightBlue = 41,
        DarkLightBlue = 42,
        DarkerLightBlue = 43,
        LightPurple = 44,
        Blue = 45,
        DarkerBlue = 46,
        DarkestBlue = 47,
        Pink = 53,
        Rose = 56,
        HotPink = 57,
        DarkHotPink = 58,
        DarkestHotPink = 59,
        DarkOrange = 60,
        Gold = 61,
        DarkBeige = 62,
        Green = 63
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
                    gridButtons.Add(new LaunchpadButton(0, (byte)(int.Parse(x.ToString() + y.ToString())), (byte)LaunchpadMK2Color.Off, outPort));
            }

            // Create all the top buttons            
            for (var x = 104; x < 111; x++)
                topButtons.Add(new LaunchpadTopButton((byte)x, (byte)LaunchpadColor.Off, outPort));

            for (var y = 1; y <= 8; y++)
                for (var x = 1; x <= 9; x++)
                {
                    SetButtonColor(x, y, (byte)LaunchpadMK2Color.Off);
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
                    Debug.WriteLine($"Top Button Ch:{changeMessage.Channel}, ID:{changeMessage.Controller}");

                    // Get a reference to the button
                    var topButton = topButtons.FirstOrDefault(button => button.Id == changeMessage.Controller);

                    // If the top button could not be found (should never happen), return
                    if (topButton == null) return;

                    whenButtonStateChanged.OnNext(topButton);
                    break;
            }
        }

        public override void SendMessage(IMidiMessage message)
        {
            outPort.SendMessage(message);
        }

        public override void SetButtonColor(int x, int y, byte color)
        {
            SendMessage(new MidiNoteOnMessage(0, (byte)(int.Parse(y.ToString() + x.ToString())), (byte)color));
        }

        public override void UnregisterEffect(ILaunchpadEffect effect)
        {
            
        }
    }
}
