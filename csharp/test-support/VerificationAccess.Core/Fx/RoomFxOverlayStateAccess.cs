namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomFxOverlayState"/> members production does not use.</summary>
internal static class RoomFxOverlayStateAccess
{
    /// <summary>Builds room FX from an explicit room-FX record through the same core path used by room entry.</summary>
    extension(RoomFxOverlayState)
    {
        /// <summary>Builds room FX from an explicit immutable FX definition through the normal room-load path.</summary>
        internal static RoomFxOverlayState ForDefinition(RoomFxSession session, RoomEffectDefinition definition, ushort randomNumber)
        {
            Ensure.NotNull(definition);
            return RoomFxOverlayState.ForSource(session, new RoomFxLoadSource
            {
                Fx = null,
                Door = null,
                RandomNumber = randomNumber,
                Room = null,
                SuppliedDefinition = definition,
            });
        }
    }

    /// <summary>The liquid lines a check requires the room's effect to name (#1546).</summary>
    extension(RoomFxOverlayState self)
    {
        /// <summary>The liquid's base surface, which the check requires the effect to name.</summary>
        /// <exception cref="InvalidOperationException">The effect names no surface.</exception>
        internal FixedCoordinate NamedBaseY => self.BaseY
            ?? throw new InvalidOperationException("The room effect names no liquid surface.");

        /// <summary>The line the liquid moves toward, which the check requires the effect to name.</summary>
        /// <exception cref="InvalidOperationException">The effect names no target.</exception>
        internal int NamedTargetSurface => self.TargetSurface
            ?? throw new InvalidOperationException("The room effect names no liquid target.");

        /// <summary>The live surface line, which the check requires the effect to have.</summary>
        /// <exception cref="InvalidOperationException">The effect has no surface.</exception>
        internal int NamedSurface => self.Surface
            ?? throw new InvalidOperationException("The room effect has no liquid surface.");
    }

    /// <summary>The liquid lines a check requires a published effect to carry (#1546).</summary>
    extension(RoomFxOverlayRenderSnapshot self)
    {
        /// <summary>The published surface's room line, which the check requires.</summary>
        /// <exception cref="InvalidOperationException">The effect published no surface.</exception>
        internal int NamedSurface => self.Surface
            ?? throw new InvalidOperationException("The published room effect has no liquid surface.");

        /// <summary>The published surface's screen line, which the check requires.</summary>
        /// <exception cref="InvalidOperationException">The effect placed no surface on the screen.</exception>
        internal int NamedSurfaceScreenY => self.WaterSurfaceScreenY
            ?? throw new InvalidOperationException("The published room effect placed no liquid surface on the screen.");
    }
}
