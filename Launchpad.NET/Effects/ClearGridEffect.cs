using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Launchpad.NET.Models;

namespace Launchpad.NET.Effects
{
    public class ClearGridEffect : ILaunchpadEffect
    {
        public string Name
        {
            get { return "Clear"; }
            set { }
        }

        readonly Subject<ILaunchpadEffect> whenComplete = new Subject<ILaunchpadEffect>();

        public IObservable<ILaunchpadEffect> WhenComplete => whenComplete;
        public void Initiate(List<LaunchpadButton> gridButtons, List<LaunchpadButton> sideButtons, List<LaunchpadTopButton> topButtons, IObservable<ILaunchpadButton> whenButtonStateChanged)
        {        
            // Clear all the buttons
            gridButtons.ForEach(button=>button.Color = LaunchpadColor.Off);
            whenComplete.OnNext(this);
        }

        public void Terminate()
        {
            whenComplete.OnNext(this);
        }

        public void Update()
        {
            
        }
    }
}
