using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomLayer3FxState"/> members production does not use.</summary>
internal static class RoomLayer3FxStateAccess
{
    /// <summary>Loads an explicit room-FX record through the same core path used by room entry.</summary>
    extension(RoomLayer3FxState self)
    {
        /// <summary>Loads an explicit immutable FX definition through the normal room-load path.</summary>
        internal void LoadDefinition(SnesVram vram, SnesCgram cgram,
            RoomFxRecordDefinition definition, ushort randomNumber)
        {
            Ensure.NotNull(definition);
            self.LoadCore((SnesVram)(vram), (SnesCgram)(cgram), new RoomFxLoadSource
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
