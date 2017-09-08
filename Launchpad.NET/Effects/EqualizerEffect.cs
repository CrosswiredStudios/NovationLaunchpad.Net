using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Launchpad.NET.Models;

namespace Launchpad.NET.Effects
{
    public class EqualizerEffect : ILaunchpadEffect
    {
        public event EventHandler OnCompleted;

        List<LaunchpadButton> gridButtons;
        List<LaunchpadButton> sideButtons;
        List<LaunchpadTopButton> topButtons;
        List<Launchpad.LaunchpadColor> horizontalColorKey;
        Launchpad.LaunchpadColor[] verticalColorkey;



        public void Initiate(List<LaunchpadButton> gridButtons, List<LaunchpadButton> sideButtons, List<LaunchpadTopButton> topButtons)
        {
            this.gridButtons = gridButtons;
            this.sideButtons = sideButtons;
            this.topButtons = topButtons;

            horizontalColorKey = new List<Launchpad.LaunchpadColor>()
            {
                Launchpad.LaunchpadColor.AmberFull,
                Launchpad.LaunchpadColor.GreenFull,
                Launchpad.LaunchpadColor.RedFull,
                Launchpad.LaunchpadColor.Yellow,
                Launchpad.LaunchpadColor.RedFull,
                Launchpad.LaunchpadColor.AmberFull,
                Launchpad.LaunchpadColor.GreenFull,
                Launchpad.LaunchpadColor.RedFull
            };
        }

        public void ProcessInput(ILaunchpadButton button, List<LaunchpadButton> grid)
        {
            
        }

        public void ProcessInput(byte buttonPressedId, List<LaunchpadButton> grid)
        {
            // Clear above
            var clearIndex = buttonPressedId - 16;
            while (clearIndex >= 0)
            {
                grid.FirstOrDefault(button => button.Id == clearIndex).Color = Launchpad.LaunchpadColor.Off;
                clearIndex -= 16;
            }

            // Light at pressed button and below
            var lightIndex = buttonPressedId;            
            while (lightIndex < 127)
            {
                grid.FirstOrDefault(button=>button.Id == lightIndex).Color = horizontalColorKey[buttonPressedId%16];
                lightIndex += 16;
            }

        }

        public void ProcessInput(byte buttonPressedId, List<LaunchpadTopButton> top)
        {
            Launchpad.LaunchpadColor nextColor = Launchpad.LaunchpadColor.Off;
            switch (horizontalColorKey[buttonPressedId % 104])
            {
                case Launchpad.LaunchpadColor.Off:
                    nextColor = Launchpad.LaunchpadColor.AmberFull;
                    break;
                case Launchpad.LaunchpadColor.AmberFull:
                    nextColor = Launchpad.LaunchpadColor.GreenFull;
                    break;
                case Launchpad.LaunchpadColor.GreenFull:
                    nextColor = Launchpad.LaunchpadColor.RedFull;
                    break;
                case Launchpad.LaunchpadColor.RedFull:
                    nextColor = Launchpad.LaunchpadColor.Yellow;
                    break;
                case Launchpad.LaunchpadColor.Yellow:
                    nextColor = Launchpad.LaunchpadColor.AmberFull;
                    break;
            }
            horizontalColorKey[buttonPressedId % 104] = nextColor;
            UpdateColumnColor(buttonPressedId % 104);
        }

        public List<LaunchpadButton> Update()
        {
            return new List<LaunchpadButton>();
        }

        void UpdateColumnColor(int column)
        {
            for (var x = column; x < 64; x += 8)
            {
                if (gridButtons[x].Color == Launchpad.LaunchpadColor.Off) continue;

                gridButtons[x].Color = horizontalColorKey[column];
            }
        }

        public TimeSpan UpdateFrequency { get; }
    }
}
