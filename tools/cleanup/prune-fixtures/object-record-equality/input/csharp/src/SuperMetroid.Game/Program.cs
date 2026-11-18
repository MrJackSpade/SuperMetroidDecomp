using SuperMetroid.Core.Menus;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>
/// The fixture's player host: it compares rows through <see cref="object"/>'s own equality, the static comparison of
/// two rows and the instance comparison of a row held by an interface, both of which dispatch to the records'
/// synthesized equality, so each row's value, which nothing reads by name, decides the result.
/// </summary>
internal static class Program
{
    /// <summary>Prints what the comparisons find.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            Console.WriteLine(object.Equals(new StaticComparedRow(1), new StaticComparedRow(2)));
            IComparedRow[] held = [new InterfaceComparedRow(1), new InterfaceComparedRow(2)];
            Console.WriteLine(held[0].Equals(held[1]) || held[0].Page != held[1].Page);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
