using SuperMetroid.Core.Doors;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>
/// The fixture's player host: a new array of facings, a lock set back to its default and a speed a call leaves out each
/// hold an enum's zero member, which no code names (#2608).
/// </summary>
internal static class Program
{
    /// <summary>Prints what production reads.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            DoorFacing[] sides = new DoorFacing[2];
            Console.WriteLine(sides[1] == DoorFacing.Right);
            var door = new Door();
            Console.WriteLine(door.IsLocked);
            door.Unlock();
            Console.WriteLine(door.IsLocked);
            Console.WriteLine(Door.Moves());
            Console.WriteLine(Door.Moves(DoorSpeed.Fast));
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
