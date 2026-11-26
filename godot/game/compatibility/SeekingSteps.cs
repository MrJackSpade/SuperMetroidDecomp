using Engine.Shared;

namespace Game.Compatibility;

/// <summary>
/// The steps the cartridge's seeking fliers change their speed by, one at a time: Ridley on Ceres and in Norfair, and
/// the giant Metroid. A step carries a unit into the next: one that takes the speed across nothing owes the next step a
/// 256th of a pixel more, and a seek's first step is owed one when counting places within 65536 reverses the order of
/// the flier and its target. Summing a seek's steps plainly gives other speeds: from 16/256 with steps of 10/256 the
/// cartridge reaches -13/256 on the third update where a plain sum reaches -12/256.
/// </summary>
public static class SeekingSteps
{
    /// <summary>The finest step of a speed the cartridge holds: a 256th of a pixel an update.</summary>
    public static readonly Pixels FinestStep = Pixels.FromParts(0, Pixels.FractionsPerPixel / 256);

    /// <summary>The step a seek takes for a distance already divided by how gently it seeks: a 256th of a pixel for each pixel.</summary>
    public static Pixels StepFor(int pixels)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pixels);
        return Pixels.FromParts(pixels >> 8, (pixels & 255) * (Pixels.FractionsPerPixel / 256));
    }

    /// <summary>
    /// Raises a seek's speed by one step, and by a 256th of a pixel more when one is owed. A raise that takes the speed
    /// from below nothing to nothing or above owes the next step one.
    /// </summary>
    public static void Raise(ref Pixels speed, ref bool owed, Pixels amount)
    {
        Pixels raised = speed + amount + (owed ? FinestStep : Pixels.Zero);
        owed = speed.IsNegative && !raised.IsNegative;
        speed = raised;
    }

    /// <summary>Lowers a seek's speed as <see cref="Raise"/> raises it: a lowering from nothing or above to below nothing owes a unit.</summary>
    public static void Lower(ref Pixels speed, ref bool owed, Pixels amount)
    {
        Pixels lowered = speed - amount - (owed ? FinestStep : Pixels.Zero);
        owed = !speed.IsNegative && lowered.IsNegative;
        speed = lowered;
    }

    /// <summary>
    /// Whether a seek's first step is owed a unit: the cartridge counts places within 65536, and does so when the way
    /// to the target across that count is the other way round from the places' order, as for a target above the room's
    /// top.
    /// </summary>
    public static bool WrapReversesOrder(int at, int target)
    {
        int from = at & 0xFFFF, to = target & 0xFFFF;
        return unchecked((short)(from - to)) < 0 != from < to;
    }

    /// <summary>How far a place lies from a target the short way across the cartridge's count of places: negative when the target lies ahead.</summary>
    public static int ShortWay(int at, int target) => unchecked((short)(at - target));
}
