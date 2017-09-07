# Launchpad.NET
A suite for interacting with the Novation Launchpad in .NET/C# on the Universal Windows Platform (UWP)

I was using a launchpad as a controller for a series of puzzles in an escape room set up and needed a way to use the launchpad with raspberry pi's that are running windows iot. This is specifically for use with UWP/UAP projects and will not work with .NET Core (although that version will likely come soon)

## Usage
To create an instance, simply create a launchpad object with the name of the device. Typically this will be "Launchpad (1)" but may be different on different devices. The underlying code uses ToLower().Contains(*name*.ToLower()), so you can just use "launchpad" and it will work. For **connecting to multiple launchpads** you will need to use exact names.
    
	var launchpad = new Launchpad("launchpad");
	launchpad.Initialize();

or

	var launchpadOne = new Launchpad("Launchpad (1)");
	var launchpadTwo = new Launchpad("Launchpad (2)");
	launchpadOne.Initialize();
	launchpadTwo.Initialize();

## Effects
The effects engine allows you to create pre-programmed effects that will run on the Launchpad. You can register as many effects to run as you'd like. Effects can be registered as **foreground** or **background** - which determines which effect gets last say on the state of a button. This makes the order you add effects important to the final result.

	var launchpad = new Launchpad("launchpad");
	launchpad.BackgroundEffects.Add(new SpiralChaseEffect());
	launchpad.Initialize();

### Included Effects

* SpiralChaseEffect
* SquareChaseEffect

