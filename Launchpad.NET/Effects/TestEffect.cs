using Launchpad.NET.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Launchpad.NET.Models;
using Windows.Foundation;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Reactive;

namespace Launchpad.NET.Effects
{
    public class TestEffect : ILaunchpadEffect
    {
        Point stackOrigin;
        List<LaunchpadButton> gridButtons;
        List<LaunchpadButton> sideButtons;
        List<LaunchpadTopButton> topButtons;
        
        readonly Subject<ILaunchpadEffect> whenComplete = new Subject<ILaunchpadEffect>();

        int pass = 1;

        public string Name => "Stacker Game Effect";

        public IObservable<ILaunchpadEffect> WhenComplete => whenComplete;

        public void Initiate(List<LaunchpadButton> gridButtons, List<LaunchpadButton> sideButtons, List<LaunchpadTopButton> topButtons, IObservable<ILaunchpadButton> whenButtonStateChanged)
        {
            stackOrigin = new Point(1, 1);
            //stackButton1.Color = (byte)LaunchpadMk2Color.Off;
            this.gridButtons = gridButtons;
            // Clear all the buttons
            gridButtons.ForEach(button => button.Color = (byte)LaunchpadMk2Color.Off);
            //gridButtons.ForEach(button => button.Color = (byte)LaunchpadMk2Color.DarkArmyGreen);


        }

        

        public void Terminate()
        {

        }

        int ToId(Point point)
        {
            return int.Parse(point.X.ToString() + point.Y.ToString());
        }


        public void Update()
        {

            //gridButtons.ForEach(button => button.Color = (byte)LaunchpadMk2Color.Blue);
            //gridButtons.ForEach(button => button.Color = (byte)LaunchpadMk2Color.Blue);

            //var stackButton = gridButtons.First(button => button.Id == (byte)(int.Parse("1" + "1")));
            //var stackButton = Grid[onMessage.Note % 10 - 1, onMessage.Note / 10 - 1];

            var stackButton = gridButtons.First(b => b.Id == ToId(stackOrigin));
            stackButton.Color = (byte)LaunchpadMk2Color.Blue;

            System.Diagnostics.Debug.WriteLine("STUFF" + (byte)(int.Parse("1" + "1")));
            //stackButton.Color = (byte)LaunchpadMk2Color.Blue;


            System.Diagnostics.Debug.WriteLine("Pass #: " + pass);

            pass++;




        }
    }
}
