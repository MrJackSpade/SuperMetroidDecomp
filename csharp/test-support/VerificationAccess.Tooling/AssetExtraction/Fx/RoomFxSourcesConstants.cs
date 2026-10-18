using SuperMetroid.Core;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Cartridge values and helpers of <see cref="RoomFxSources"/> that only verification reads.</summary>
internal static class RoomFxSourcesConstants
{
    /// <summary>The header pointer of <paramref name="fx"/>.</summary>
    public static ushort PointerOf(RoomFxDefinition fx)
    {
        Ensure.NotNull(fx);
        return RoomFxSourceDefinitions.ByPointer.Single(pair => Object.ReferenceEquals(pair.Value, fx)).Key;
    }
}
