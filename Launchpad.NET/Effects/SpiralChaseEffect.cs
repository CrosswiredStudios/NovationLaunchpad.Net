using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Launchpad.NET.Models;

namespace Launchpad.NET.Effects
{
    public struct SpiralChaseEffectConfig
    {
        public bool HasTrail;
        public int Height;
        public Point StartLocation;
        public int TrailLength;
        public int Width;
    }

    public class SpiralChaseEffect : ILaunchpadEffect
    {
        public enum SpiralDirection
        {
            Clockwise,
            CounterClockwise
        };

        public event EventHandler OnCompleted;

        LaunchpadButton actor;
        Point actorLocation;
        SpiralChaseEffectConfig config;
        List<LaunchpadButton> trail;

        public SpiralChaseEffect(SpiralChaseEffectConfig config)
        {
            this.config = config;
        }

        public TimeSpan UpdateFrequency { get; }

        public List<LaunchpadButton> Initiate()
        {
            x = 0;
            y = 0;

            lastLastButton = new LaunchpadButton() { Color = Launchpad.LaunchpadColor.Off, Id = 0 };
            lastButton = new LaunchpadButton() { Color = Launchpad.LaunchpadColor.GreenFull, Id = 0 };

            return new List<LaunchpadButton>()
            {
                lastButton
            };
        }

        public List<LaunchpadButton> ProcessInput(byte pressedButtonId)
        {
            return new List<LaunchpadButton>();
        }

        public List<LaunchpadButton> Update()
        {
            x++;
            if (x > 7)
            {
                x = 0;
                y++;
                if (y > 7)
                {
                    y = 0;
                }
            }

            var updatedLastLastButton = new LaunchpadButton()
            {
                Color = Launchpad.LaunchpadColor.Off,
                Id = lastLastButton.Id
            };

            var updatedLastButton = new LaunchpadButton()
            {
                Color = Launchpad.LaunchpadColor.GreenLow,
                Id = lastButton.Id
            };

            var newButton = new LaunchpadButton()
            {
                Color = Launchpad.LaunchpadColor.GreenFull,
                Id = (byte)((y * 16) + x)
            };

            lastLastButton = lastButton;
            lastButton = newButton;

            return new List<LaunchpadButton>()
            {
                updatedLastLastButton,
                updatedLastButton,
                newButton
            };
        }
    }    
}
