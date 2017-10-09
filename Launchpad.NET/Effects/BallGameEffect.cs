using Launchpad.NET.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Launchpad.NET.Models;
using Windows.Foundation;
using System.Reactive.Subjects;

namespace AlienArtifactA.Models
{
    public class BallGameEffect : ILaunchpadEffect
    {

        Point ball;
        LaunchpadButton currentButton;
        List<LaunchpadButton> gridButtons;
        LaunchpadButton lastButton;
        Point velocity;
        Subject<ILaunchpadEffect> whenComplete = new Subject<ILaunchpadEffect>();

        public string Name => "Ball Game Effect";

        public IObservable<ILaunchpadEffect> WhenComplete => whenComplete;

        public void Initiate(List<LaunchpadButton> gridButtons, List<LaunchpadButton> sideButtons, List<LaunchpadTopButton> topButtons, IObservable<ILaunchpadButton> whenButtonStateChanged)
        {
            ball = new Point(3, 1);
            this.gridButtons = gridButtons;
            velocity = new Point(1, 1);
            currentButton = gridButtons.First(b => b.Id == ToId(ball));
            currentButton.Color = (byte)Launchpad.NET.LaunchpadMk2Color.DarkRed;

            whenButtonStateChanged
                .Subscribe(ProcessInput);
        }

        public void ProcessInput(ILaunchpadButton button)
        {
            switch (button)
            {
                case LaunchpadButton gridButton:
                    if(gridButton.Id.ToString().Last() == '9')
                    {
                        if(gridButton.State == LaunchpadButtonState.Pressed)
                            gridButtons.Where(b => b.Id.ToString().First() == gridButton.Id.ToString().First()).Select(rowButton => rowButton.Color = (byte)Launchpad.NET.LaunchpadMk2Color.Brown);
                        else
                            gridButtons.Where(b => b.Id.ToString().First() == gridButton.Id.ToString().First()).Select(rowButton => rowButton.Color = (byte)Launchpad.NET.LaunchpadMk2Color.Off);
                    }
                    break;

            }
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
            currentButton.Color = (byte)Launchpad.NET.LaunchpadMk2Color.DarkRed;

            ball.X += velocity.X;
            ball.Y += velocity.Y;

            if (ball.X == 1) velocity.X = 1;
            if (ball.X == 8) velocity.X = -1;
            if (ball.Y == 1) velocity.Y = 1;
            if (ball.Y == 8) velocity.Y = -1;
             var nextButton = gridButtons.First(b => b.Id == ToId(ball));

            nextButton.Color = (byte)Launchpad.NET.LaunchpadMk2Color.Off;
            currentButton = nextButton;
        }
    }
}
