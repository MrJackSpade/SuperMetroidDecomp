namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// What a running colour effect shows its frames on (#2544), handed to it as it starts: the room's pictures, a frontend
/// screen's pictures, or an owner that lays each frame's shade over its own draws (<see cref="OnOwner"/>: Samus's suit, the
/// escape's shutters, a glowing enemy, or a lit Torizo whose defeat the effect watches). Each target accepts only an
/// effect whose colours it shows, so a frame is shown on its target with no choice left to make.
/// </summary>
internal abstract partial record ColourEffectTarget
{
    /// <summary>Only the targets below derive.</summary>
    private ColourEffectTarget()
    {
    }

    /// <summary>The room's pictures, which take each frame's shade (<see cref="ColourEffectRoomColourDefinitions.RoomColours"/>).</summary>
    internal static ColourEffectTarget RoomPictures { get; } = new OnRoomPictures();

    /// <summary>A frontend screen's pictures, which take each frame's shade (<see cref="ColourEffectScreenColourDefinitions.FrontendScreenGroups"/>).</summary>
    internal static ColourEffectTarget ScreenPictures { get; } = new OnScreenPictures();

    /// <summary>Samus's suit, which lays each frame's shade over her draws (<see cref="ColourEffectSamusColourDefinitions.SuitColours"/>).</summary>
    internal static ColourEffectTarget SamusSuit { get; } = new OnSamusSuit();

    /// <summary>
    /// Whichever destroyable shutters the room holds, which lay each frame of the escape's flash over their draws
    /// (<see cref="ColourEffectEnemyColourDefinitions.EscapeShutterColours"/>).
    /// </summary>
    internal static ColourEffectTarget EscapeShutters { get; } = new OnEscapeShutters();

    /// <summary>
    /// The target a gameplay effect whose start hands it no enemy shows <paramref name="effect"/>'s colours on: the room's
    /// pictures, Samus's suit or the escape's shutters, as the effect's colours are theirs.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// An enemy shows the effect's colours, which its start must hand it, or no gameplay picture shows them.
    /// </exception>
    internal static ColourEffectTarget OfGameplay(ColourEffectId effect)
    {
        ColourEffectColours colours = ColourEffectAnimations.Definition(effect).Colours;
        if (ColourEffectRoomColourDefinitions.RoomColours.Contains(colours))
            return RoomPictures;
        if (ColourEffectSamusColourDefinitions.SuitColours.Contains(colours))
            return SamusSuit;
        if (colours == ColourEffectEnemyColourDefinitions.EscapeShutterColours)
            return EscapeShutters;
        throw new InvalidOperationException(ColourEffectEnemyColourDefinitions.EnemyColours.Contains(colours)
            ? $"Colour effect {effect} shows on an enemy, which its start must hand it."
            : $"Colour effect {effect} shows {colours}, which no gameplay picture shows.");
    }

    /// <summary>Checks that <paramref name="effect"/> can show on this target: its colours are the target's.</summary>
    /// <exception cref="InvalidOperationException">The effect shows colours, or watches a condition, the target has none of.</exception>
    internal abstract void Accept(ColourEffectId effect, ColourEffectDefinition definition);

    /// <summary>Checks that <paramref name="definition"/>'s colours lie in <paramref name="shown"/>, the colours this target shows.</summary>
    /// <exception cref="InvalidOperationException">They do not.</exception>
    private void AcceptColours(ColourEffectId effect, ColourEffectDefinition definition, IReadOnlySet<ColourEffectColours> shown)
    {
        if (!Ensure.NotNull(shown).Contains(Ensure.NotNull(definition).Colours))
            throw new InvalidOperationException($"Colour effect {effect} shows {definition.Colours}, which {this} does not show.");
    }

    /// <summary>Checks that <paramref name="definition"/> shows on an enemy.</summary>
    /// <exception cref="InvalidOperationException">It shows on no enemy.</exception>
    private static void AcceptOnEnemy(ColourEffectId effect, ColourEffectDefinition definition)
    {
        if (!ColourEffectEnemyColourDefinitions.EnemyColours.Contains(Ensure.NotNull(definition).Colours))
            throw new InvalidOperationException($"Colour effect {effect} shows on no enemy, but its start handed it one.");
    }

    /// <summary>The room's pictures, which take the shade of the frame the effect holds.</summary>
    private sealed partial record OnRoomPictures : ColourEffectTarget
    {
        /// <inheritdoc/>
        internal override void Accept(ColourEffectId effect, ColourEffectDefinition definition) =>
            AcceptColours(effect, definition, ColourEffectRoomColourDefinitions.RoomColours);
    }

    /// <summary>A frontend screen's pictures, which take the shade of the frame the effect holds.</summary>
    private sealed partial record OnScreenPictures : ColourEffectTarget
    {
        /// <inheritdoc/>
        internal override void Accept(ColourEffectId effect, ColourEffectDefinition definition) =>
            AcceptColours(effect, definition, ColourEffectScreenColourDefinitions.FrontendScreenGroups);
    }

    /// <summary>
    /// An owner that shows the shade of each frame the effect shows over its own draws: Samus's suit, the escape's
    /// shutters, or an enemy.
    /// </summary>
    internal abstract partial record OnOwner : ColourEffectTarget
    {
        /// <summary>Only the owners below derive.</summary>
        private protected OnOwner()
        {
        }

        /// <summary>Keeps <paramref name="shown"/> in <paramref name="owners"/> for this owner, which lays its shade over its draws.</summary>
        internal abstract void Show(ShownColourEffectFrame shown, ColourEffectFramesOnOwners owners);
    }

    /// <summary>Samus's suit, which lays the shade of each frame over her draws.</summary>
    private sealed partial record OnSamusSuit : OnOwner
    {
        /// <inheritdoc/>
        internal override void Accept(ColourEffectId effect, ColourEffectDefinition definition) =>
            AcceptColours(effect, definition, ColourEffectSamusColourDefinitions.SuitColours);

        /// <inheritdoc/>
        internal override void Show(ShownColourEffectFrame shown, ColourEffectFramesOnOwners owners) => Ensure.NotNull(owners).Suit.Add(shown);
    }

    /// <summary>The escape's shutters, which lay the shade of each frame of the flash over their draws.</summary>
    private sealed partial record OnEscapeShutters : OnOwner
    {
        /// <inheritdoc/>
        internal override void Accept(ColourEffectId effect, ColourEffectDefinition definition)
        {
            if (Ensure.NotNull(definition).Colours != ColourEffectEnemyColourDefinitions.EscapeShutterColours)
                throw new InvalidOperationException($"Colour effect {effect} shows {definition.Colours}, not the escape's shutters' colours.");
        }

        /// <inheritdoc/>
        internal override void Show(ShownColourEffectFrame shown, ColourEffectFramesOnOwners owners) =>
            Ensure.NotNull(owners).ShutterFlash.Add(shown.Frame);
    }

    /// <summary>
    /// The enemy whose placement started the effect (#1733, #2288), which shows each of its frames in enemy colours on its own
    /// draws for as long as the effect runs (#1783).
    /// </summary>
    /// <param name="Enemy">The enemy that shows the effect's frames.</param>
    internal sealed partial record OnGlowingEnemy(IGlowingEnemy Enemy) : OnOwner
    {
        /// <summary>The enemy that shows the effect's frames.</summary>
        public IGlowingEnemy Enemy { get; } = Ensure.NotNull(Enemy);

        /// <inheritdoc/>
        internal override void Accept(ColourEffectId effect, ColourEffectDefinition definition)
        {
            AcceptOnEnemy(effect, definition);
            if (definition.Condition is ColourEffectCondition.EndsWhenTorizoDies)
                throw new InvalidOperationException($"Colour effect {effect} watches for a Torizo's defeat, but its start handed it no Torizo.");
        }

        /// <inheritdoc/>
        internal override void Show(ShownColourEffectFrame shown, ColourEffectFramesOnOwners owners) =>
            Ensure.NotNull(owners).Glows.Add(new ShownEnemyGlow(Enemy, shown.Frame));
    }

    /// <summary>
    /// The Torizo whose belly the effect lights (#1700, #2240): it shows each of the effect's frames on its draws, and the
    /// effect watches it, ending on the first update it reports its defeat
    /// (<see cref="ColourEffectCondition.EndsWhenTorizoDies"/>).
    /// </summary>
    /// <param name="Torizo">The Torizo the effect lights and watches.</param>
    internal sealed partial record OnLitTorizo(ILitTorizo Torizo) : OnOwner
    {
        /// <summary>The Torizo the effect lights and watches.</summary>
        public ILitTorizo Torizo { get; } = Ensure.NotNull(Torizo);

        /// <inheritdoc/>
        /// <remarks>Each belly glow watches for its Torizo's defeat, as its table is checked once when built (<see cref="ColourEffectSystem.BellyGlowEffects"/>).</remarks>
        internal override void Accept(ColourEffectId effect, ColourEffectDefinition definition) => AcceptOnEnemy(effect, definition);

        /// <inheritdoc/>
        internal override void Show(ShownColourEffectFrame shown, ColourEffectFramesOnOwners owners) =>
            Ensure.NotNull(owners).Glows.Add(new ShownEnemyGlow(Torizo, shown.Frame));
    }
}
