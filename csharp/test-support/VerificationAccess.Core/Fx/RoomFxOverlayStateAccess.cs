namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomFxOverlayState"/> members production does not use.</summary>
internal static class RoomFxOverlayStateAccess
{
    /// <summary>Builds room FX from an explicit room-FX record through the same core path used by room entry.</summary>
    extension(RoomFxOverlayState)
    {
        /// <summary>
        /// Builds room FX from an explicit immutable FX definition through the normal room-load path, its sounds joining
        /// requests of their own, as a runtime hands the effect its update's (#2057, #2544).
        /// </summary>
        internal static RoomFxOverlayState ForDefinition(RoomEffectDefinition definition, ushort randomNumber) =>
            ForDefinition(definition, randomNumber, SuperMetroid.Core.Audio.GameplayAudioRequests.Standalone());

        /// <summary>
        /// Builds room FX from an explicit immutable FX definition through the normal room-load path, its sounds joining the
        /// check's <paramref name="sounds"/>, which it reads (#2544).
        /// </summary>
        internal static RoomFxOverlayState ForDefinition(RoomEffectDefinition definition, ushort randomNumber,
            SuperMetroid.Core.Audio.GameplaySoundRequests sounds)
        {
            Ensure.NotNull(definition);
            return RoomFxOverlayState.ForSource(new RoomFxLoadSource
            {
                Fx = null,
                Door = null,
                RandomNumber = randomNumber,
                Room = null,
                SuppliedDefinition = definition,
            }, sounds);
        }
    }

    /// <summary>The liquid lines a check requires the room's effect to name (#1546), and one update of the effect as the runtime runs it.</summary>
    extension(RoomFxOverlayState self)
    {
        /// <summary>
        /// Runs one update of the effect as the runtime does (#2053): the liquid's motion pass before the update's random
        /// draw, then the effect's step, which places the liquid where that pass left it.
        /// </summary>
        /// <param name="system">The shared game state the liquid's motion pass reads and, for lava and acid, swaps the random halves of.</param>
        /// <param name="frames">The room's animation frame selections.</param>
        /// <param name="frame">What the step reads of the update; its freeze also holds the motion pass.</param>
        internal void RunUpdate(SuperMetroid.Core.Progression.SharedGameState system, SuperMetroid.Core.Assets.ArtworkFrameSelections frames,
            RoomFxOverlayFrame frame)
        {
            Ensure.NotNull(frame);
            self.AdvanceLiquidMotion(system, frame.TimeIsFrozen);
            self.Step(frames, frame);
        }

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
