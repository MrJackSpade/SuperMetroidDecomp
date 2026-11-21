
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

        /// <summary>
        /// Shows Fireflea darkness in the loaded room as a Fireflea room's load starts it, from its first flash, for a check
        /// of how its shade reaches the screen; returns the darkness.
        /// </summary>
        internal FirefleaFxState StartFirefleaDarknessForCheck()
        {
            var darkness = new FirefleaFxState();
            self.Type = RoomFxType.Fireflea;
            self.Fireflea = darkness;
            return darkness;
        }

        /// <summary>
        /// Takes the liquid's motion from a native snapshot recorded mid-room (#2056): its motion has begun, whether an
        /// enemy has stopped it, where its tide stands, its base surface and its surface line.
        /// </summary>
        /// <param name="stopped">Whether an enemy has ended the liquid's motion.</param>
        /// <param name="tidePhase">How far round its rise and fall the tide is.</param>
        /// <param name="tideOffset">How far the tide displaces the surface from its base line.</param>
        /// <param name="baseY">The surface before the tide's displacement.</param>
        /// <param name="surface">The surface's room line, negative once a tide carries it above the room's top.</param>
        internal void ImportLiquidMotion(bool stopped, RotationAngle tidePhase, SignedSixteenSixteen tideOffset, FixedCoordinate baseY,
            int surface)
        {
            self.Motion = stopped ? SuperMetroid.Core.Compatibility.LiquidMotion.Stopped : SuperMetroid.Core.Compatibility.LiquidMotion.Moving;
            self.TidePhase = tidePhase;
            self.TideOffset = tideOffset;
            self.BaseY = baseY;
            self.Surface = surface;
        }

        /// <summary>Gives the liquid <paramref name="liquid"/>'s tide standing at <paramref name="phase"/> with <paramref name="offset"/>, for a check of one tide step.</summary>
        internal void SetTideForCheck(RoomLiquid liquid, RotationAngle phase, SignedSixteenSixteen offset)
        {
            self.Liquid = liquid;
            self.TidePhase = phase;
            self.TideOffset = offset;
        }

        /// <summary>Runs one step of the liquid's tide alone, the step its motion pass takes after the rise.</summary>
        internal void StepTideForCheck() => self.StepLiquidTide();

        /// <summary>Places the quake sounds at <paramref name="position"/> in their rhythm, for a check of the sound each position plays.</summary>
        internal void SetEarthquakeSoundRhythmPositionForCheck(int position) =>
            self.EarthquakeSoundRhythm = CyclingSequence<int>.At(RoomEarthquakeDefinitions.RisingLiquidSoundBaseDelays, position);

        /// <summary>Returns the loaded room to its own effect, <paramref name="type"/>, which has no Fireflea darkness.</summary>
        internal void EndFirefleaDarknessForCheck(RoomFxType type)
        {
            self.Type = type;
            self.Fireflea = null;
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
