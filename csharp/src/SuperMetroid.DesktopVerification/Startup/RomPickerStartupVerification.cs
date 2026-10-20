using SuperMetroid.Core;
using System.Reflection;
using System.Runtime.InteropServices;
namespace SuperMetroid.DesktopVerification.Startup;

/// <summary>Opens and cancels the real setup button's native dialog on the player's entry apartment.</summary>
internal static partial class RomPickerStartupVerification
{
    /// <summary>Invokes the actual Choose ROM button on the game's apartment and cancels its native dialog.</summary>
    [ReflectionAudit("published-assembly dialog test: the form type comes from the loaded package, and Button.PerformClick is inert on an unshown form, so WinForms OnClick is raised directly")]
    public static void Run(string gameAssemblyPath)
    {
        Assembly game = Assembly.LoadFrom(Path.GetFullPath(gameAssemblyPath));
        MethodInfo entry = game.EntryPoint ?? throw new InvalidDataException("Game has no entry point.");
        ApartmentState apartment = entry.IsDefined(typeof(STAThreadAttribute)) ? ApartmentState.STA : ApartmentState.MTA;
        Exception? failure = null;
        bool opened = false, returned = false;
        var thread = new Thread(() =>
        {
            try
            {
                // Construct without showing setup: OnShown must not inspect or install into
                // the player's app-data directory during this ROM-free dialog regression.
                using var setup = (Form)(Activator.CreateInstance(
                    (game.GetType("SuperMetroid.Game.RomSetupForm", throwOnError: true) ?? throw new InvalidOperationException("Verification requires game.GetType(\"SuperMetroid.Game.RomSetupForm\", throwOnError: true).")),
                    [Array.Empty<string>()]) ?? throw new InvalidOperationException("Verification requires Activator.CreateInstance( (game.GetType(\"SuperMetroid.Game.RomSetupForm\", throwOnError:...."));
                _ = (setup ?? throw new InvalidOperationException("Verification requires setup.")).Handle;
                var choose = (Button)(setup.AcceptButton ?? throw new InvalidOperationException("Verification requires setup.AcceptButton."));
                using var context = new ApplicationContext();
                using var timer = new System.Windows.Forms.Timer { Interval = 50 };
                uint ownerThread = GetCurrentThreadId();
                long deadline = Environment.TickCount64 + 5000;
                timer.Tick += (_, _) =>
                {
                    EnumThreadWindows(ownerThread, (window, _) =>
                    {
                        // Close only the native common dialog on this test-owned thread.
                        var className = new System.Text.StringBuilder(64);
                        _ = GetClassName(window, className, className.Capacity);
                        if (className.ToString() == "#32770")
                        {
                            opened = true;
                            PostMessage(window, 0x0010 /* WM_CLOSE */, 0, 0);
                        }
                        return true;
                    }, 0);
                    if (Environment.TickCount64 >= deadline)
                    {
                        failure ??= new TimeoutException("ROM picker did not open and cancel within five seconds.");
                        context.ExitThread();
                    }
                };
                ThreadExceptionEventHandler onError = (_, error) =>
                {
                    failure = error.Exception;
                    context.ExitThread();
                };
                Application.ThreadException += onError;
                bool started = false;
                EventHandler onIdle = (_, _) =>
                {
                    if (started) return;
                    started = true;
                    timer.Start();
                    (typeof(Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Verification requires typeof(Button).GetMethod(\"OnClick\", BindingFlags.Instance | BindingFlags.NonPublic)."))
                        .Invoke(choose, [EventArgs.Empty]);
                    returned = true;
                    // An async-void click failure is posted to this message loop. Allow it
                    // to dispatch before ending the test, instead of treating it as success.
                    setup.BeginInvoke(() => context.ExitThread());
                };
                Application.Idle += onIdle;
                try { Application.Run(context); }
                finally
                {
                    Application.Idle -= onIdle;
                    Application.ThreadException -= onError;
                }
            }
            catch (Exception error) { failure = error; }
        }) { IsBackground = true };
        thread.SetApartmentState(apartment);
        thread.Start();
        if (!thread.Join(TimeSpan.FromSeconds(10)))
            throw new TimeoutException("ROM-picker test thread stopped pumping messages.");
        if (failure is not null) throw new InvalidOperationException($"Choose ROM failed on the game's {apartment} apartment.", failure);
        if (!opened || !returned) throw new InvalidOperationException("Choose ROM did not open and return from its native dialog.");
        Console.WriteLine("PASS actual Choose ROM handler: native dialog opened and cancellation returned on the player's STA apartment; no installation or saves touched.");
    }

    /// <summary>Receives each top-level window on the test-owned dialog thread.</summary>
    private delegate bool WindowCallback(nint window, nint parameter);
    /// <summary>Gets the native ID used to enumerate only the test thread's windows.</summary>
    [LibraryImport("kernel32.dll")] private static partial uint GetCurrentThreadId();
    /// <summary>Enumerates windows owned by the ROM-picker dialog thread.</summary>
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnumThreadWindows(uint thread, WindowCallback callback, nint parameter);
    /// <summary>Reads each window class so the native common dialog can be identified.</summary>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(nint window, System.Text.StringBuilder name, int capacity);
    /// <summary>Posts WM_CLOSE to the identified common dialog to exercise cancellation.</summary>
    [LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PostMessage(nint window, uint message, nint wParam, nint lParam);
}
