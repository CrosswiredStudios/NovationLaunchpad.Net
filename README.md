# Launchpad.NET
A suite for interacting with the Novation Launchpad in .NET/C# on the Universal Windows Platform (UWP)

I was using a launchpad as a controller for a series of puzzles in an escape room set up and needed a way to use the launchpad with raspberry pi's that are running windows iot. This is specifically for use with UWP/UAP projects and will not work with .NET Core (although that version will likely come soon)

## Usage
To create an instance, simply create a launchpad object with the name of the device. Typically this will be "Launchpad (1)" but may be different on different devices. The underlying code uses ToLower().Contains(*name*.ToLower()), so you can just use "launchpad" lowercase and it will work. For **connecting to multiple launchpads** you will need to use exact names.
    
	var launchpad = new Launchpad("launchpad");
	launchpad.Initialize();

or

	var launchpadOne = new Launchpad("Launchpad (1)");
	var launchpadTwo = new Launchpad("Launchpad (2)");
	launchpadOne.Initialize();
	launchpadTwo.Initialize();

## Effects
The effects engine allows you to create pre-programmed effects that will run on the Launchpad. You can register as many effects to run as you'd like. Effects can be registered as **foreground** or **background** - which determines which effect gets last say on the state of a button. This makes the order you add effects important to the final result. You can also pass in an update fequency for the effect to speed it up or slow it down (default is 50ms)

	var launchpad = new Launchpad("launchpad");
	launchpad.RegisterEffect(new ClearGridEffect(), 50);
	launchpad.Initialize();

### Included Effects

* ClearGridEffect
* SpiralChaseEffect
* SquareChaseEffect

### Creating Effects

Creating custom effects is easy! Here is a basic effect that turns off all the grid buttons:

1. Create a class and implement the **ILaunchpadEffect** interface.

    public class ClearGridEffect : ILaunchpadEffect
    {
	    readonly Subject<ILaunchpadEffect> whenComplete = new Subject<ILaunchpadEffect>();

        public IObservable<ILaunchpadEffect> WhenComplete => whenComplete;

        public void Initiate(List<LaunchpadButton> gridButtons, List<LaunchpadButton> sideButtons, List<LaunchpadTopButton> topButtons, IObservable<ILaunchpadButton> whenButtonStateChanged)
        {        
        }

        public void Update()
        {            
        }
	}

2. **Handle the initiation of the effect** - here we add code to go through each grid button and set it's color to off

    public void Initiate(List<LaunchpadButton> gridButtons, List<LaunchpadButton> sideButtons, List<LaunchpadTopButton> topButtons, IObservable<ILaunchpadButton> whenButtonStateChanged)
    {        
        // Clear all the buttons
        gridButtons.ForEach(button=>button.Color = LaunchpadColor.Off);
    }

3. **Handle updates** - for this example, we have no update logic, but this would be where you update animations or interactions

4. **Handle completion** - Once the buttons are turned off, this effect is no longer needed. We can fire off the completed observable to let the Lauchpad know we are done and to remove the effect and free up resources

	public void Initiate(List<LaunchpadButton> gridButtons, List<LaunchpadButton> sideButtons, List<LaunchpadTopButton> topButtons, IObservable<ILaunchpadButton> whenButtonStateChanged)
    {        
        // Clear all the buttons
        gridButtons.ForEach(button=>button.Color = LaunchpadColor.Off);
        whenComplete.OnNext(this);
    }

### Complete code sample

	public class ClearGridEffect : ILaunchpadEffect
    {
	    readonly Subject<ILaunchpadEffect> whenComplete = new Subject<ILaunchpadEffect>();

        public IObservable<ILaunchpadEffect> WhenComplete => whenComplete;

        public void Initiate(List<LaunchpadButton> gridButtons, List<LaunchpadButton> sideButtons, List<LaunchpadTopButton> topButtons, IObservable<ILaunchpadButton> whenButtonStateChanged)
        {        
			// Clear all the buttons
			gridButtons.ForEach(button=>button.Color = LaunchpadColor.Off);
			whenComplete.OnNext(this);
        }

        public void Update()
        {            
        }
	}