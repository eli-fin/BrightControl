# Written in June 2017

# General information
    This is a small app that adds an system tray icon.
    (you can see the icon in the Resources folder)
    When clicked, it will popup a small slider which
    can be used to adjust the screen brightness.
    You can right-click to exit.

    There's a screenshot in the wiki (from the images folder)

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

# How to run this interactively
    It is possible to run this application under the following interactive environments:
    (not that I can see any use for it -:) )

    ## CSI (C Sharp Interactive): (Tested on Win10x64 + Visual Studion 2017 + CSI 2.6.0.62329)
    1. Start csi (from Visual Studio command line)
    2. Run the following code:
        string exePath = @"Drive:\path\to\exe\BrightControl.exe";          // Define executable path
        var assembly=System.Reflection.Assembly.LoadFrom(exePath);         // Load the assembly
        var type=assembly.GetType("BrightControl.App");                    // Get the type
        // Create a new thread with STA apartment thread (WinForms can only run in STA threads)
        System.Threading.Thread thread = new System.Threading.Thread(() =>
        {
            dynamic app=type.GetConstructors()[0].Invoke(new object[]{});  // Construct an application
            app.InitializeComponent();
            app.Run();
        });
        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.Start()

    ## PowerShell: (Tested on Win10x64 PS 5.1.16299.98)
    1. Start PowerShell in STA mode (powershell -sta)
        This is the default from V3. You can check current mode with by running [System.Threading.Thread]::CurrentThread.GetApartmentState()
        You can check your version by typing $PSVersionTable
    2. Run the following code:
        $exePath = "Drive:\path\to\exe\BrightControl.exe"          # Define executable path
        # Becuse Add-Type requires a dll extention, create a temporary copy of the executable
        $dllPath = [System.IO.Path]::GetTempPath() + [Guid]::NewGuid() + ".dll"
        copy $exePath $dllPath                                     # Copy content
        Add-Type -path $dllPath                                    # Load assembly
        $app=[BrightControl.App]::new()                            # Create instance
        $app.InitializeComponent()
        $app.Run()