using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Launchpad.NET.Models;

namespace Launchpad.NET.Effects
{
    public class SquareChaseEffect : ILaunchpadEffect
    {
        public enum Direction { Clockwise, CounterClockwise }

        int x, y, width, moveCount;
        Launchpad.LaunchpadColor color;
        Direction direction;
        bool movingDown, movingLeft, movingRight, movingUp;
        LaunchpadButton previousButton;


        public SquareChaseEffect(int x, int y, int width, Direction direction, Launchpad.LaunchpadColor color)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.color = color;
            this.direction = direction;

            moveCount = 0;
            movingDown = movingLeft = movingRight = movingUp = false;
        }

        public event EventHandler OnCompleted;

        public List<LaunchpadButton> Initiate()
        {
            if (direction == Direction.Clockwise)
            {
                movingRight = true;
            }
            else
            {
                movingDown = true;
            }
            return new List<LaunchpadButton>()
            {
                new LaunchpadButton((byte)(y*16+x), color)
            };
        }

        public void ProcessInput(byte buttonPressedId, List<LaunchpadButton> grid)
        {
            
        }

        public void ProcessInput(byte buttonPressedId, List<LaunchpadTopButton> top)
        {
            
        }

        public List<LaunchpadButton> Update()
        {
            switch (direction)
            {
                case Direction.Clockwise:
                    if (movingRight)
                    {
                        x++;
                        moveCount++;

                        if (moveCount == width)
                        {
                            moveCount = 0;
                            movingRight = false;
                            movingDown = true;
                        }
                    }
                    else if (movingDown)
                    {
                        y++;
                        moveCount++;

                        if (moveCount == width)
                        {
                            moveCount = 0;
                            movingDown = false;
                            movingLeft = true;
                        }
                    }
                    else if (movingLeft)
                    {
                        x--;
                        moveCount++;

                        if (moveCount == width)
                        {
                            moveCount = 0;
                            movingLeft = false;
                            movingUp = true;
                        }
                    }
                    else if (movingUp)
                    {
                        y--;
                        moveCount++;

                        if (moveCount == width)
                        {
                            moveCount = 0;
                            movingUp = false;
                            movingRight = true;
                        }
                    }
                    break;
                case Direction.CounterClockwise:
                    if (movingDown)
                    {
                        y++;
                        moveCount++;

                        if (moveCount == width)
                        {
                            moveCount = 0;
                            movingDown = false;
                            movingRight = true;
                        }
                    }
                    else if (movingRight)
                    {
                        x++;
                        moveCount++;

                        if (moveCount == width)
                        {
                            moveCount = 0;
                            movingRight = false;
                            movingUp = true;
                        }
                    }
                    else if (movingUp)
                    {
                        y--;
                        moveCount++;

                        if (moveCount == width)
                        {
                            moveCount = 0;
                            movingUp = false;
                            movingLeft = true;
                        }
                    }
                    else if (movingLeft)
                    {
                        x--;
                        moveCount++;

                        if (moveCount == width)
                        {
                            moveCount = 0;
                            movingLeft = false;
                            movingDown = true;
                        }
                    }
                    break;
            }

            var results = new List<LaunchpadButton>();

            if (previousButton != null)
            {
                var updatedPreviousButton = new LaunchpadButton(previousButton.Id, Launchpad.LaunchpadColor.Off);

                results.Add(updatedPreviousButton);
            }

            var newLocation = new LaunchpadButton((byte) (y * 16 + x), color);

            previousButton = newLocation;

            if (0 <= x && x <= 7 &&
                0 <= y && x <= 7)
            {
                results.Add(newLocation);
            }

            return results;
        }

        public TimeSpan UpdateFrequency { get; }
    }
}
