using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomLayer3FxState"/> members production does not use.</summary>
internal static class RoomLayer3FxStateAccess
{
    /// <summary>Builds room FX from an explicit room-FX record through the same core path used by room entry.</summary>
    extension(RoomLayer3FxState)
    {
        /// <summary>Builds room FX from an explicit immutable FX definition through the normal room-load path.</summary>
        internal static RoomLayer3FxState ForDefinition(RoomFxSession session, GameplayColours cgram,
            RoomFxRecordDefinition definition, ushort randomNumber)
        {
            Ensure.NotNull(definition);
            return RoomLayer3FxState.ForRecord(session, cgram.BackgroundSets, new RoomFxLoadSource
            {
                Fx = null,
                Door = definition.Door,
                RandomNumber = randomNumber,
                Room = null,
                SuppliedDefinition = definition,
            });
        }
    }
}
