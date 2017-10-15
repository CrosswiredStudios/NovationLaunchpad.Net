using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Midi;
using Launchpad.NET.Effects;
using Launchpad.NET.Models;
using Windows.UI;

// https://global.novationmusic.com/sites/default/files/novation/downloads/10529/launchpad-mk2-programmers-reference-guide_0.pdf

namespace Launchpad.NET
{
    public enum LaunchpadMk2Color : byte
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
        DarkYellow = 14,
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

    public class LaunchpadMk2 : Launchpad, ILaunchpad
    {
        public LaunchpadButton[,] Grid { get; }

        public Color[,] GridBuffer { get; }

        public List<LaunchpadButton> SideButtons => sideButtons;

        public LaunchpadMk2(string name, MidiInPort inPort, IMidiOutPort outPort)
        {
            Name = name;
            this.inPort = inPort;
            this.outPort = outPort;            
            Effects = new ObservableCollection<ILaunchpadEffect>();
            gridButtons = new List<LaunchpadButton>();
            sideButtons = new List<LaunchpadButton>();
            topButtons = new List<LaunchpadTopButton>();
            Grid = new LaunchpadButton[8, 8];
            GridBuffer = new Color[8, 8];

            // Create all the grid buttons            
            for (var y = 10; y <= 80; y=y+10)
            for (var x = 1; x <= 8; x++)
            {
                    var button = new LaunchpadButton(0, (byte)(y+x), (byte)LaunchpadMk2Color.Off, outPort);
                    //gridButtons.Add(button);

                    Grid[x-1,y/10-1] = button;
                    GridBuffer[x - 1, y / 10 - 1] = Colors.Black;
                    button.Color = (byte)LaunchpadMk2Color.Off;
            }

            for(var x = 1; x < 9; x++)
            {
                sideButtons.Add(new LaunchpadButton(0, (byte)(10*x+9), (byte)LaunchpadMk2Color.Off, outPort));
            }

            //// Create all the top buttons            
            //for (var x = 104; x < 111; x++)
            //    topButtons.Add(new LaunchpadTopButton((byte)x, (byte)LaunchpadColor.Off, outPort));

            ClearAll();

            // Process messages from device
            inPort.MessageReceived += InPort_MessageReceived;
        }

        public void ClearAll()
        {
            SetAllButtonsColor(LaunchpadMk2Color.Off);
        }

        public void ClearGrid()
        {
            // Create all the grid buttons            
            for (var y = 0; y < 8; y++)
                for (var x = 0; x < 8; x++)
                {
                    Grid[x,y].Color = (byte)LaunchpadMk2Color.Off;
                }

        }

        public void ClearGridRow(int row)
        {
            var commandBytes = new List<byte>();
            for (var x = 1; x < 9; x++)
            {
                var buttonId = row * 10 + x;
                commandBytes.AddRange(new byte[] { 240, 0, 32, 41, 2, 24, 10, (byte)buttonId, (byte)LaunchpadMk2Color.Off, 247 });
            }
            outPort?.SendMessage(new MidiSystemExclusiveMessage(commandBytes.ToArray().AsBuffer()));
        }

        public void FlushGridBuffer(bool clearBufferAfter = true)
        {
            var commandBytes = new List<byte>();
            for(var y=0; y<8; y++)
            {
                for(var x=0; x<8; x++)
                {
                    //Test
                    var buttonId = (y+1) * 10 + (x+1);
                    commandBytes.AddRange(new byte[] { 240, 0, 32, 41, 2, 24, 11, (byte)buttonId, (byte)(GridBuffer[x, y].R / 4), (byte)(GridBuffer[x, y].G / 4), (byte)(GridBuffer[x, y].B / 4), 247 });
                }
            }
            outPort?.SendMessage(new MidiSystemExclusiveMessage(commandBytes.ToArray().AsBuffer()));
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

                    if (onMessage.Note % 10 == 9)
                    {
                        var sideButton = sideButtons.FirstOrDefault(button => button.Id == onMessage.Note);
                        sideButton.State = onMessage.Velocity == 0
                            ? LaunchpadButtonState.Released
                            : LaunchpadButtonState.Pressed;
                        whenButtonStateChanged.OnNext(sideButton);
                    }
                    else
                    {
                        // Get a reference to the button
                        var gridButton = Grid[onMessage.Note % 10 - 1, onMessage.Note / 10 - 1];

                        // If the grid button could not be found (should never happen), return
                        if (gridButton == null) return;

                        // Update the state (Launchpad sends midi on message for press and release - Velocity 0 is released, 127 is pressed)
                        gridButton.State = onMessage.Velocity == 0
                            ? LaunchpadButtonState.Released
                            : LaunchpadButtonState.Pressed;

                        // Notify any observable subscribers of the event
                        whenButtonStateChanged.OnNext(gridButton);
                    }
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

        public void PulseButton(int x, int y, LaunchpadMk2Color color)
        {
            //var command = new byte[] { 240, 0, 32, 41, 2, 24, 40, Grid[x, y].Id, (byte)color, 247 };
            //outPort?.SendMessage(new MidiSystemExclusiveMessage(command.AsBuffer()));

            outPort?.SendMessage(new MidiNoteOnMessage(2, Grid[x, y].Id, (byte)color));
        }

        public override void SendMessage(IMidiMessage message)
        {
            outPort.SendMessage(message);
        }

        public void SetAllButtonsColor(LaunchpadMk2Color color)
        {
            var command = new byte[] { 240, 0, 32, 41, 2, 24, 14, (byte)color, 247 };
            outPort?.SendMessage(new MidiSystemExclusiveMessage(command.AsBuffer()));
        }

        public void SetButtonColor(int x, int y, Color color)
        {
            var command = new byte[] { 240, 0, 32, 41, 2, 24, 11, Grid[x-1,y-1].Id, (byte)(color.R/4), (byte)(color.G / 4), (byte)(color.B / 4), 247};
            outPort?.SendMessage(new MidiSystemExclusiveMessage(command.AsBuffer()));

        }

        public void SetButtonColor(int x, int y, LaunchpadMk2Color color)
        {
            var command = new byte[] { 240, 0, 32, 41, 2, 24, 10, Grid[x,y].Id, (byte)color, 247 };
            outPort?.SendMessage(new MidiSystemExclusiveMessage(command.AsBuffer()));
        }

        public void SetButtonColor(int x, int y, byte r, byte g, byte b)
        {
            var command = new byte[] { 240, 0, 32, 41, 2, 24, 11, Grid[x, y].Id, r, g, b, 247 };
            outPort?.SendMessage(new MidiSystemExclusiveMessage(command.AsBuffer()));

        }

        public void SetColumnColor(int column, LaunchpadMk2Color color)
        {
            var command = new byte[] { 240, 0, 32, 41, 2, 24, 12, (byte)column, (byte)color, 247 };
            outPort?.SendMessage(new MidiSystemExclusiveMessage(command.AsBuffer()));
        }

        public void SetRowColor(int row, LaunchpadMk2Color color)
        {
            var commandBytes = new List<byte>();
            for (var x = 1; x < 9; x++)
            {
                var buttonId = row * 10 + x;
                commandBytes.AddRange(new byte[] { 240, 0, 32, 41, 2, 24, 10, (byte)buttonId, (byte)color, 247 });
            }
            outPort?.SendMessage(new MidiSystemExclusiveMessage(commandBytes.ToArray().AsBuffer()));
        }

        public override void UnregisterEffect(ILaunchpadEffect effect)
        {
            EffectsDisposables[effect].Dispose();
            effect.Dispose();
        }
    }
}
