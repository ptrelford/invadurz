namespace WindowsPhoneApp

open System
open System.Net
open System.Windows
open System.Windows.Controls
open System.Windows.Documents
open System.Windows.Ink
open System.Windows.Input
open System.Windows.Media
open System.Windows.Media.Animation
open System.Windows.Shapes
open System.Windows.Navigation
open Microsoft.Phone.Controls
open Microsoft.Phone.Shell

type MainPage () as page=
    inherit PhoneApplicationPage()
    // Load the Xaml for the page.
    do Application.LoadComponent(page, new System.Uri("/Invadurz;component/MainPage.xaml", System.UriKind.Relative))
    do  page.Content <- new Invadurz.GameControl ()
    override page.OnOrientationChanged args =
        base.OnOrientationChanged args

/// One instance of this type is created in the application host project.
type App(app:Application) = 
    // Global handler for uncaught exceptions. 
    // Note that exceptions thrown by ApplicationBarItem.Click will not get caught here.
    do app.UnhandledException.Add(fun e -> 
            if (System.Diagnostics.Debugger.IsAttached) then
                // An unhandled exception has occurred, break into the debugger
                System.Diagnostics.Debugger.Break();
     )
    
    let rootFrame = new PhoneApplicationFrame()

    do app.RootVisual <- rootFrame;

    // Handle navigation failures
    do rootFrame.NavigationFailed.Add(fun _ -> 
        if (System.Diagnostics.Debugger.IsAttached) then
            // A navigation has failed; break into the debugger
            System.Diagnostics.Debugger.Break())

    //do rootFrame.Content <- MainPage(View())
    // Navigate to the main page 
    do rootFrame.Navigate(new Uri("/Invadurz;component/MainPage.xaml", UriKind.Relative)) |> ignore

    // Required object that handles lifetime events for the application
    let service = PhoneApplicationService()
    // Code to execute when the application is launching (eg, from Start)
    // This code will not execute when the application is reactivated
    do service.Launching.Add(fun _ -> ())
    // Code to execute when the application is closing (eg, user hit Back)
    // This code will not execute when the application is deactivated
    do service.Closing.Add(fun _ -> ())
    // Code to execute when the application is activated (brought to foreground)
    // This code will not execute when the application is first launched
    do service.Activated.Add(fun _ -> ())
    // Code to execute when the application is deactivated (sent to background)
    // This code will not execute when the application is closing
    do service.Deactivated.Add(fun _ -> ())

    do app.ApplicationLifetimeObjects.Add(service) |> ignore
