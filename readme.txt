# Written in June 2017

# General information
	This is a small app that adds an system tray icon.
	(you can see the icon in the Resources folder)
	When clicked, it will popup a small slider which
	can be used to adjust the screen brightness.
	You can right-click to exit.

# Support
	This app uses the WmiMonitorBrightness class.
	It seems to be supported only with laptop/tablet screens.

	If your computer isn't supported, you'll get an error at startup.

	I've tested it on several laptops with windows 10 and it seems
	to work fine. It will probably work on some earlier OS version too.

	It uses .NET 4.5. It worked fine on .NET 4 too, but some
	of the WPF visuals weren't looking so good.
	Havn't tested it on earlier .NET versions.

# Why I wrote this
	I bought a new laptop and it had no intuitive way to adjust
	the the screen brightness (like a keyboard shortcut)
