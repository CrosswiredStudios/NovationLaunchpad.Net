# Launchpad.NET
A suite for interacting with the Novation Launchpad in .NET/C# on the Universal Windows Platform (UWP)

I was using a launchpad as a controller for a series of puzzles in an escape room set up and needed a way to use the launchpad with raspberry pi's that are running windows iot. This is specifically for use with UWP/UAP projects and will not work with .NET Core (although that version will likely come soon)

## Usage
var launchPad = new Launchpad("Launchpad");
