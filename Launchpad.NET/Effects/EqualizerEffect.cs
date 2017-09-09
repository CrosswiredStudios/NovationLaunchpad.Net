using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Launchpad.NET.Models;

namespace Launchpad.NET.Effects
{
    public class EqualizerEffect : ILaunchpadEffect
    {


        List<LaunchpadButton> gridButtons;
        List<LaunchpadButton> sideButtons;
        List<LaunchpadTopButton> topButtons;
        List<LaunchpadColor> horizontalColorKey;
        LaunchpadColor[] verticalColorkey;
        readonly Subject<ILaunchpadEffect> whenComplete = new Subject<ILaunchpadEffect>();

        public LaunchPadEffectLayer Layer { get; set; }
        public IObservable<ILaunchpadEffect> WhenComplete => whenComplete;

        public void Initiate(List<LaunchpadButton> gridButtons, List<LaunchpadButton> sideButtons, List<LaunchpadTopButton> topButtons, IObservable<ILaunchpadButton> whenButtonStateChanged)
        {
            this.gridButtons = gridButtons;
            this.sideButtons = sideButtons;
            this.topButtons = topButtons;

            horizontalColorKey = new List<LaunchpadColor>()
            {
                LaunchpadColor.AmberFull,
                LaunchpadColor.GreenFull,
                LaunchpadColor.RedFull,
                LaunchpadColor.Yellow,
                LaunchpadColor.RedFull,
                LaunchpadColor.AmberFull,
                LaunchpadColor.GreenFull,
                LaunchpadColor.RedFull
            };

            whenButtonStateChanged
                .Subscribe(ProcessInput);
        }

        public void ProcessInput(ILaunchpadButton button)
        {
            switch (button)
            {
                case LaunchpadButton gridButton:
                    // Clear above
                    var clearIndex = button.Id - 16;
                    while (clearIndex >= 0)
                    {
                        gridButtons.FirstOrDefault(b => b.Id == clearIndex).Color = LaunchpadColor.Off;
                        clearIndex -= 16;
                    }

                    // Light at pressed button and below
                    var lightIndex = button.Id;
                    while (lightIndex < 127)
                    {
                        gridButtons.FirstOrDefault(b => b.Id == lightIndex).Color = horizontalColorKey[button.Id % 16];
                        lightIndex += 16;
                    }
                    break;
                case LaunchpadTopButton topButton:
                    LaunchpadColor nextColor = LaunchpadColor.Off;
                    switch (horizontalColorKey[button.Id % 104])
                    {
                        case LaunchpadColor.Off:
                            nextColor = LaunchpadColor.AmberFull;
                            break;
                        case LaunchpadColor.AmberFull:
                            nextColor = LaunchpadColor.GreenFull;
                            break;
                        case LaunchpadColor.GreenFull:
                            nextColor = LaunchpadColor.RedFull;
                            break;
                        case LaunchpadColor.RedFull:
                            nextColor = LaunchpadColor.Yellow;
                            break;
                        case LaunchpadColor.Yellow:
                            nextColor = LaunchpadColor.AmberFull;
                            break;
                    }
                    horizontalColorKey[button.Id % 104] = nextColor;
                    UpdateColumnColor(button.Id % 104);
                    break;
            }
            
        }
        
        void ILaunchpadEffect.Update()
        {

        }

        void UpdateColumnColor(int column)
        {
            for (var x = column; x < 64; x += 8)
            {
                if (gridButtons[x].Color == LaunchpadColor.Off) continue;

                gridButtons[x].Color = horizontalColorKey[column];
            }
        }
    }
}
