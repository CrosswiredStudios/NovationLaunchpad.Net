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
            actorLocation = config.StartLocation;
            

            return new List<LaunchpadButton>()
            {

            };
        }

        public void ProcessInput(byte pressedButtonId, List<LaunchpadButton> grid)
        {
            
        }

        public void ProcessInput(byte buttonPressedId, List<LaunchpadTopButton> top)
        {
            
        }

        public List<LaunchpadButton> Update()
        {
            return new List<LaunchpadButton>();
        }
    }    
}
