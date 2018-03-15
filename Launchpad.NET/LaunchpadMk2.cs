using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Midi;
using Launchpad.NET.Effects;
using Launchpad.NET.Models;
using Windows.UI;
using System;
using System.Reactive;
using System.Threading;

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
        public LaunchpadMk2Button[,] Grid { get; }
        public Color[,] GridBuffer { get; private set; }
        public bool IsSimulated { get; }
        public List<LaunchpadMk2Button> SideButtons => sideButtons;
        public List<LaunchpadMk2TopButton> TopButtons => topButtons;
        int resetCount;
        Timer resetTimer;

        public LaunchpadMk2(string name, MidiInPort inPort = null, IMidiOutPort outPort = null)
        {
            Name = name;
            this.inPort = inPort;
            this.outPort = outPort;
            gridButtons = new List<LaunchpadMk2Button>();
            resetCount = 0;
            sideButtons = new List<LaunchpadMk2Button>();
            topButtons = new List<LaunchpadMk2TopButton>();
            Grid = new LaunchpadMk2Button[8, 8];
            GridBuffer = new Color[8, 8];

            BuildButtons();            

            // Process resets triggered from launchpad
            resetTimer = new Timer((state) => {

                // If the user is pressing the 4 corner grid buttons
                if (Grid[0, 0].State == LaunchpadButtonState.Pressed &&
                    Grid[0, 7].State == LaunchpadButtonState.Pressed &&
                    Grid[7, 7].State == LaunchpadButtonState.Pressed &&
                    Grid[7, 0].State == LaunchpadButtonState.Pressed)
                {
                    // Increase the reset count
                    resetCount++;

                    // If the user has held the buttons for 10 seconds
                    if (resetCount > 10)
                    {
                        // Notify anyone interested in the event
                        whenReset.OnNext(Unit.Default);
                    }
                }
                else // If the user is not pressing the 4 corner buttons
                {
                    // Reset the count
                    resetCount = 0;
                }
            }, null, 0, 1000);

            // Flag as simulated mode if no connection provided
            IsSimulated = inPort == null || outPort == null;

            if(!IsSimulated)
            {
                // Process messages from device
                inPort.MessageReceived += InPort_MessageReceived;
            }
        }

        /// <summary>
        /// Creates refrences to all buttons on the LaunchpadMk2
        /// </summary>
        void BuildButtons()
        {
            CreateGridButtons();
            CreateSideButtons();
            CreateTopButtons();
        }

        public void ClearAll()
        {
            SetAllButtonsColor(LaunchpadMk2Color.Off);
        }

        /// <summary>
        /// Clears all square grod buttons on the Mk2
        /// </summary>
        public void ClearGrid()
        {
            var commandBytes = new List<byte>();
            for(var row = 1; row < 9; row++)
            for (var x = 1; x < 9; x++)
            {
                var buttonId = row * 10 + x;
                commandBytes.AddRange(new byte[] { 240, 0, 32, 41, 2, 24, 10, (byte)buttonId, (byte)0, 247 });
            }
            outPort?.SendMessage(new MidiSystemExclusiveMessage(commandBytes.ToArray().AsBuffer()));
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

        /// <summary>
        /// Creates the objects that represent the grid buttons
        /// </summary>
        void CreateGridButtons()
        {         
            for(var y = 0; y < 8; y++)
            for (var x = 0; x < 8; x++)
            {
                Grid[x, y] = new LaunchpadMk2Button(0, x, y, Colors.Black, outPort);
            }
        }

        /// <summary>
        /// Creates the objects that represent the side buttons
        /// </summary>
        void CreateSideButtons()
        {
            for (var y = 0; y < 8; y++)
            {
                sideButtons.Add(new LaunchpadMk2Button(0, 8, y, Colors.Black, outPort));
            }
        }

        /// <summary>
        /// Creates the objects that represent the top buttons
        /// </summary>
        void CreateTopButtons()
        {            
            for (var x = 0; x < 8; x++)
                topButtons.Add(new LaunchpadMk2TopButton((byte)(x + 104), Colors.Black, outPort));
        }

        public void FlushGridBuffer(bool clearBufferAfter = true)
        {
            try
            {
                var commandBytes = new List<byte>();
                for (var y = 0; y < 8; y++)
                {
                    for (var x = 0; x < 8; x++)
                    {
                        //Test
                        var buttonId = (y + 1) * 10 + (x + 1);
                        commandBytes.AddRange(new byte[] { 240, 0, 32, 41, 2, 24, 11, (byte)buttonId, (byte)(GridBuffer[x, y].R / 4), (byte)(GridBuffer[x, y].G / 4), (byte)(GridBuffer[x, y].B / 4), 247 });
                    }
                }
                outPort?.SendMessage(new MidiSystemExclusiveMessage(commandBytes.ToArray().AsBuffer()));

                if (clearBufferAfter)
                    GridBuffer = new Color[8, 8];
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
                // Grid and side buttons come as Midi Note On Message
                case MidiNoteOnMessage onMessage: 
                    // If it's a side button
                    if (onMessage.Note % 10 == 9)
                    {
                        UpdateSideButton(onMessage);
                    }
                    else // If it's a grid button
                    {
                        UpdateGridButton(onMessage);                        
                    }
                    break;
                // Top row comes as Control Change message
                case MidiControlChangeMessage changeMessage: 
                    UpdateTopButton(changeMessage);
                    break;
            }
        }

        public void PulseButton(int id, LaunchpadMk2Color color)
        {
            var command = new byte[] { 240, 0, 32, 41, 2, 24, 40, 0, (byte)id, (byte)color, 247 };
            outPort?.SendMessage(new MidiSystemExclusiveMessage(command.AsBuffer()));
        }

        public void PulseButton(int x, int y, LaunchpadMk2Color color)
        {
            //var command = new byte[] { 240, 0, 32, 41, 2, 24, 40, Grid[x, y].Id, (byte)color, 247 };
            //outPort?.SendMessage(new MidiSystemExclusiveMessage(command.AsBuffer()));
            try
            {
                outPort?.SendMessage(new MidiNoteOnMessage(2, Grid[x, y].Id, (byte)color));
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Could not pulse button. " + ex.Message);
            }
        }

        public void PulseSide(LaunchpadMk2Color color)
        {
            var commandBytes = new List<byte>();
            for (var x = 1; x < 9; x++)
            {
                var buttonId = x * 10 + 9;
                commandBytes.AddRange(new byte[] { 240, 0, 32, 41, 2, 24, 40, 0, (byte)buttonId, (byte)color, 247 });
            }
            outPort?.SendMessage(new MidiSystemExclusiveMessage(commandBytes.ToArray().AsBuffer()));
        }

        public override void SendMessage(IMidiMessage message)
        {
            outPort.SendMessage(message);
        }

        public void SetAllButtonsColor(LaunchpadMk2Color color)
        {
            try
            {
                var command = new byte[] { 240, 0, 32, 41, 2, 24, 14, (byte)color, 247 };
                outPort?.SendMessage(new MidiSystemExclusiveMessage(command.AsBuffer()));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// Sets the color of a grid button
        /// </summary>
        /// <param name="id"></param>
        /// <param name="color"></param>
        public void SetGridButtonColor(int id, Color color)
        {
            try
            {
                // Logisticaly update the button
                var button = Grid[id % 10 - 1, id / 10 - 1];
                button.Color = color;

                // If the launchpad is not simulated
                if (!IsSimulated)
                {
                    // Create and send a command to set the light color
                    var command = new byte[]
                    {
                        240, 0, 32, 41, 2, 24, 11,
                        (byte) id,
                        (byte) (color.R / 4),
                        (byte) (color.G / 4),
                        (byte) (color.B / 4),
                        247
                    };
                    outPort?.SendMessage(new MidiSystemExclusiveMessage(command.AsBuffer()));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to set the grid button color. {ex}");
            }
        }

        /// <summary>
        /// Sets the color of a grid button
        /// </summary>
        /// <param name="x">Grid x coordinate</param>
        /// <param name="y">Grid y coordinate</param>
        /// <param name="color">The color to set the button to</param>
        public void SetGridButtonColor(int x, int y, Color color)
        {
            // Convert x-y to id
            SetGridButtonColor((y+1) * 10 + x+1, color);
        }

        /// <summary>
        /// Sets the color of an entire column
        /// </summary>
        /// <param name="column">The column to color 0 - 7</param>
        /// <param name="color">The color to apply to the buttons</param>
        public void SetGridColumnColor(int column, Color color)
        {
            var command = new byte[] { 240, 0, 32, 41, 2, 24, 12, (byte)column, (byte)(color.R / 4), (byte)(color.G / 4), (byte)(color.B / 4), 247 };
            outPort?.SendMessage(new MidiSystemExclusiveMessage(command.AsBuffer()));
        }

        public void SetGridColor(Color color)
        {
            var commandBytes = new List<byte>();
            for(var y = 0; y < 8; y++)
            for (var x = 0; x < 8; x++)
            {
                commandBytes.AddRange(new byte[] { 240, 0, 32, 41, 2, 24, 11, (byte)Grid[x, y].Id, (byte)(color.R / 4), (byte)(color.G / 4), (byte)(color.B / 4), 247 });
            }
            outPort?.SendMessage(new MidiSystemExclusiveMessage(commandBytes.ToArray().AsBuffer()));
        }

        public void SetGridRowColor(int row, Color color)
        {
            var commandBytes = new List<byte>();
            for (var x = 0; x < 8; x++)
            {
                commandBytes.AddRange(new byte[] { 240, 0, 32, 41, 2, 24, 11, (byte)Grid[x, row].Id, (byte)(color.R / 4), (byte)(color.G / 4), (byte)(color.B / 4), 247 });
            }
            outPort?.SendMessage(new MidiSystemExclusiveMessage(commandBytes.ToArray().AsBuffer()));
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

        public void SetTopRowButtonColor(int id, Color color)
        {

            var command = new byte[] { 240, 0, 32, 41, 2, 24, 11, (byte)(id + 104), (byte)(color.R / 4), (byte)(color.G / 4), (byte)(color.B / 4), 247 };

            outPort?.SendMessage(new MidiSystemExclusiveMessage(command.AsBuffer()));
        }

        public void SetTopRowColor(Color color)
        {
            var commandBytes = new List<byte>();
            for (var buttonId = 104; buttonId < 112; buttonId++)
            {
                commandBytes.AddRange(new byte[] { 240, 0, 32, 41, 2, 24, 11, (byte)buttonId, (byte)(color.R / 4), (byte)(color.G / 4), (byte)(color.B / 4), 247 });
            }
            outPort?.SendMessage(new MidiSystemExclusiveMessage(commandBytes.ToArray().AsBuffer()));
        }

        /// <summary>
        /// Simulates pressing a button on the launchpad mk2
        /// </summary>
        /// <param name="id">The id of the button to simulate</param>
        public void SimulateGridPress(int id)
        {            
            UpdateGridButton(new MidiNoteOnMessage(0, (byte)id, 127));
        }

        /// <summary>
        /// Simulates pressing a button on the launchpad mk2
        /// </summary>
        /// <param name="x">The X coordinate of the button, 0 - 7</param>
        /// <param name="y">The Y coordinate of the button, 0 - 7</param>
        public void SimulateGridPress(int x, int y)
        {
            SimulateGridPress((y + 1) * 10 + x + 1);
        }

        /// <summary>
        /// Simulates releasing a button on the launchpad mk2
        /// </summary>
        /// <param name="id">The id of the button to simulate</param>
        public void SimulateGridRelease(int id)
        {
            UpdateGridButton(new MidiNoteOnMessage(0, (byte)id, 0));
        }

        /// <summary>
        /// Simulates releasing a button on the launchpad mk2
        /// </summary>
        /// <param name="x">The X coordinate of the button, 0 - 7</param>
        /// <param name="y">The Y coordinate of the button, 0 - 7</param>
        public void SimulateGridRelease(int x, int y)
        {
            SimulateGridRelease((y + 1) * 10 + x + 1);
        }

        public void SimulateTopPress(int id)
        {
            UpdateTopButton(new MidiControlChangeMessage(0, (byte)id, 127));
        }

        public void SimulateTopRelease(int id)
        {
            UpdateTopButton(new MidiControlChangeMessage(0, (byte)id, 0));
        }

        public override void UnregisterEffect(ILaunchpadEffect effect)
        {
            EffectsDisposables[effect].Dispose();
            EffectsDisposables.Remove(effect);
            EffectsTimers[effect].Dispose();
            EffectsTimers.Remove(effect);
            effect.Dispose();
        }

        public override void UnregisterAllEffects()
        {
            foreach(var effect in EffectsDisposables)
            {
                effect.Key.Dispose();
                effect.Value.Dispose();
            }
            EffectsDisposables.Clear();
            foreach (var effect in EffectsTimers)
            {
                effect.Value.Dispose();
            }
            EffectsTimers.Clear();
        }

        /// <summary>
        /// Updates the state of a grid button
        /// </summary>
        /// <param name="onMessage"></param>
        void UpdateGridButton(MidiNoteOnMessage onMessage)
        {
            Debug.WriteLine($"Side Button Ch:{onMessage.Channel}, Note:{onMessage.Note}, Vel:{onMessage.Velocity} (x:{onMessage.Note % 10}, y:{(int)(onMessage.Note / 10)}) " + (onMessage.Velocity == 0 ? "Released" : "Pressed"));

            try
            {
                // Get a reference to the grid button
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not update side button. {ex}");
            }
        }

        /// <summary>
        /// Updates the state of a side button
        /// </summary>
        /// <param name="onMessage"></param>
        void UpdateSideButton(MidiNoteOnMessage onMessage)
        {
            Debug.WriteLine($"Side Button Ch:{onMessage.Channel}, Note:{onMessage.Note}, Vel:{onMessage.Velocity} (x:{onMessage.Note % 10}, y:{(int)(onMessage.Note / 10)}) " + (onMessage.Velocity == 0 ? "Released" : "Pressed"));

            try
            {
                // Get a reference to the button
                var sideButton = sideButtons.FirstOrDefault(button => button.Id == onMessage.Note);

                // If the side button could not be found (should never happen), return
                if (sideButton == null) return;

                // Update its state
                sideButton.State = onMessage.Velocity == 0
                    ? LaunchpadButtonState.Released
                    : LaunchpadButtonState.Pressed;

                // Notify people interested in this event
                whenButtonStateChanged.OnNext(sideButton);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not update side button. {ex}");
            }
        }

        /// <summary>
        /// Updates the state of a top button
        /// </summary>
        /// <param name="changeMessage"></param>
        void UpdateTopButton(MidiControlChangeMessage changeMessage)
        {
            Debug.WriteLine($"Top Button Ch:{changeMessage.Channel}, ID:{changeMessage.Controller}, Velocity:{changeMessage.ControlValue}");

            try
            {
                // Get a reference to the button
                var topButton = topButtons.FirstOrDefault(button => button.Id == changeMessage.Controller);

                // If the top button could not be found (should never happen), return
                if (topButton == null) return;

                // Update its state
                topButton.State = changeMessage.ControlValue > 0
                    ? LaunchpadButtonState.Pressed
                    : LaunchpadButtonState.Released;

                // Notify people interested in this event
                whenButtonStateChanged.OnNext(topButton);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not update top button. {ex}");
            }
        }
    }
}
