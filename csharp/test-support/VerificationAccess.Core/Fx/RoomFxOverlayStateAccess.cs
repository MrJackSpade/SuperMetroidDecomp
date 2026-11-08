namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomFxOverlayState"/> members production does not use.</summary>
internal static class RoomFxOverlayStateAccess
{
    /// <summary>Builds room FX from an explicit room-FX record through the same core path used by room entry.</summary>
    extension(RoomFxOverlayState)
    {
        /// <summary>Builds room FX from an explicit immutable FX definition through the normal room-load path.</summary>
        internal static RoomFxOverlayState ForDefinition(RoomFxSession session, RoomFxRecordDefinition definition, ushort randomNumber)
        {
            Ensure.NotNull(definition);
            return RoomFxOverlayState.ForRecord(session, new RoomFxLoadSource
            {
                Fx = null,
                Door = null,
                RandomNumber = randomNumber,
                Room = null,
                SuppliedDefinition = definition,
            });
        }
    }
}
