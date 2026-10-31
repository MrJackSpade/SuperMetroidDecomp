using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Verification's native view of Draygon's identities (#1413): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class DraygonNativeWords
{
    /// <summary>The bank-$A5 routine word naming each <see cref="DraygonEyeFunction"/>.</summary>
    internal static readonly NativeWordIdentities<DraygonEyeFunction> EyeFunctions = new(new Dictionary<DraygonEyeFunction, ushort>
    {
        [DraygonEyeFunction.Inert] = 0x804b,
        [DraygonEyeFunction.FacingLeft] = 0xc48d,
        [DraygonEyeFunction.FacingRight] = 0xc513,
    });

    /// <summary>Pre-instruction <c>$84:DB64</c>, accepting missiles and Super Missiles.</summary>
    public const ushort MissileHitPreInstruction = 0xdb64;

    /// <summary>
    /// The bytes of one swoop path entry: the cartridge's record keeps the swoop entry and the path's length as byte
    /// distances into a table of four-byte entries (#1473).
    /// </summary>
    public const int SwoopEntryBytes = 4;

    /// <summary>What the record word the cartridge shares between three of Draygon's values holds.</summary>
    internal enum SharedValue
    {
        /// <summary>The current intro stage's frame count.</summary>
        IntroFrames,
        /// <summary>The dying body's burial wait.</summary>
        BurialWait,
        /// <summary>The swoop path entry.</summary>
        SwoopEntry,
    }

    /// <summary>
    /// What the record word the cartridge shares between the intro frame count, the burial wait and the swoop entry
    /// holds during <paramref name="function"/>: the intro count before the first swoop, the burial wait while dying,
    /// and the swoop entry otherwise.
    /// </summary>
    internal static SharedValue MeaningOfSharedValue(DraygonAiFunction function) =>
        function is DraygonAiFunction.None or DraygonAiFunction.IntroInitialDelay or DraygonAiFunction.IntroDance
            ? SharedValue.IntroFrames
            : function is DraygonAiFunction.Dying or DraygonAiFunction.DyingSink or DraygonAiFunction.DyingFinish
                ? SharedValue.BurialWait
                : SharedValue.SwoopEntry;
}
