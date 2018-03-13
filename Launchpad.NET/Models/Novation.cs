using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;

namespace Launchpad.NET.Models
{
    public static class Novation
    {
        /// <summary>
        /// Returns Device Information for all connected Launchpads
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<DeviceInformation>> GetLaunchpadDeviceInformation()
        {
            // Get all output MIDI devices
            var outputs = await DeviceInformation.FindAllAsync(MidiOutPort.GetDeviceSelector());

            return outputs.Where(device => device.Name.ToLower().Contains("launchpad"));
        }

        /// <summary>
        /// Creates a dummy launchpad that can be used for simulating Launchpad input with a Launchpad UI control
        /// </summary>
        /// <returns></returns>
        public static async Task<Launchpad> Launchpad()
        {
            return new LaunchpadMk2("LaunchpadDummy");
        }

        /// <summary>
        /// Gets a launchpad object for a connected device
        /// </summary>
        /// <param name="id">The id of the launchpad</param>
        /// <returns></returns>
        public static async Task<Launchpad> Launchpad(string id)
        {
            List<DeviceInformation> inputs = (await DeviceInformation.FindAllAsync(MidiInPort.GetDeviceSelector())).ToList();
            List<DeviceInformation> outputs = (await DeviceInformation.FindAllAsync(MidiOutPort.GetDeviceSelector())).ToList();

            // Find the launchpad input
            foreach (var inputDeviceInfo in inputs)
            {
                try
                {
                    if (inputDeviceInfo.Id.Contains(id))
                    {
                        // Find the launchpad output 
                        foreach (var outputDeviceInfo in outputs)
                        {

                            // If not a match continue
                            if (!outputDeviceInfo.Id.Contains(id)) continue;

                            var inPort = await MidiInPort.FromIdAsync(inputDeviceInfo.Id);
                            var outPort = await MidiOutPort.FromIdAsync(outputDeviceInfo.Id);

                            // Return an MK2 if detected
                            if (outputDeviceInfo.Name.ToLower().Contains("mk2"))
                                return new LaunchpadMk2(outputDeviceInfo.Name, inPort, outPort);

                            return null;
                            // Otherwise return Standard
                            //return new LaunchpadS(outputDeviceInfo.Name, inPort, outPort);
                        }
                    }
                }
                catch (Exception ex)
                {

                    Debug.WriteLine(ex);
                }
            }

            // Return null if no devices matched the device name provided
            return null;
        }
    }
}
