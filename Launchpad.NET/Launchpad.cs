using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Novation Launchpad .NET Interface
    /// </summary>
    public class Launchpad
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

        List<LaunchpadButton> GridButtons;

        MidiInPort inPort;
        readonly string name;
        IMidiOutPort outPort;
        Timer updateTimer;

        List<LaunchpadButton> sideColumn;
        List<LaunchpadButton> topRow;

        /// <summary>
        /// Launchpad effects to run on the main grid as the background
        /// </summary>
        public List<ILaunchpadEffect> GridBackgroundEffects { get; set; }

        /// <summary>
        /// Launchpad effects to run on the main grid as the foreground
        /// </summary>
        public List<ILaunchpadEffect> GridForegroundEffects { get; set; }

        /// <summary>
        /// Create a launchpad instance
        /// </summary>
        /// <param name="name">The name of the launchpad. Often 'Launchpad (1)'.</param>
        public Launchpad(string name)
        {
            this.name = name;            

            GridBackgroundEffects = new List<ILaunchpadEffect>();
            GridForegroundEffects = new List<ILaunchpadEffect>();
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
        /// <param name="name"></param>
        public async void Initiate()
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
            GridButtons = new List<LaunchpadButton>();
            for (var y = 0; y < 8; y++)
            for (var x = 0; x < 8; x++)
            {
                GridButtons.Add(new LaunchpadButton((byte)(y * 16 + x), LaunchpadColor.Off, outPort));
            }

            // Initiate any effects
            GridBackgroundEffects.ForEach(effect => effect.Initiate());
            GridForegroundEffects.ForEach(effect => effect.Initiate());

            // Start the update timer (10ms resolution)
            updateTimer = new Timer(Update, null, 0, 10);
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
                case MidiNoteOnMessage onMessage:
                    GridBackgroundEffects.ForEach(effect => SetButtonColor(effect.ProcessInput(onMessage.Note)));
                    GridForegroundEffects.ForEach(effect => SetButtonColor(effect.ProcessInput(onMessage.Note)));
                    break;
                case MidiControlChangeMessage changeMessage:
                    // Top row
                    break;
            }

        }

        void SetButtonColor(IEnumerable<LaunchpadButton> launchpadButtons)
        {
            foreach (var launchpadButton in launchpadButtons)
            {
                SetButtonColor(launchpadButton.Id, launchpadButton.Color);
            }
        }

        public void SetButtonColor(byte id, LaunchpadColor color)
        {
            IMidiMessage midiMessageToSend = new MidiNoteOnMessage(0, id, (byte)color);

            outPort.SendMessage(midiMessageToSend);
        }

        public void SetButtonColor(int x, int y, LaunchpadColor color)
        {
            IMidiMessage midiMessageToSend = new MidiNoteOnMessage(0, (byte)(16 * y + x), (byte)color);

            outPort.SendMessage(midiMessageToSend);
        }

        void Update(object state)
        {
            GridBackgroundEffects.ForEach(effect => UpdateGrid(effect.Update()));
            GridForegroundEffects.ForEach(effect => UpdateGrid(effect.Update()));
        }

        void UpdateGrid(List<LaunchpadButton> launchpadButtons)
        {
            launchpadButtons.ForEach(button => GridButtons.)
        }
    }
}
