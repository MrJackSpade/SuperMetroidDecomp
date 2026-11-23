using SuperMetroid.Core;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Cartridge values and helpers of <see cref="RoomFxSources"/> that only verification reads.</summary>
internal static class RoomFxSourcesConstants
{
    /// <summary>The header pointer of <paramref name="fx"/>.</summary>
    public static ushort PointerOf(RoomFxSelection fx)
    {
        Ensure.NotNull(fx);
        // The cartridge stores the quicksand shafts' one selection twice; its first header serves.
        return RoomFxSourceDefinitions.ByPointer.Where(pair => Object.ReferenceEquals(pair.Value, fx)).Min(pair => pair.Key);
    }
}
