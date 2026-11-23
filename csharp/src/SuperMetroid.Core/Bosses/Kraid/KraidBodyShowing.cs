using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// How Kraid's body shows on his body layer (#1352, #1515, #2560): a closed choice of a body readied at a depth that has yet
/// to show, a body shown at its depth with his head over it, and a hidden body, as a defeated room and his sunk body show.
/// </summary>
internal abstract partial record KraidBodyShowing
{
    /// <summary>A body that shows nothing, before his body layer is readied and once his body is hidden.</summary>
    internal static KraidBodyShowing Hidden { get; } = new HiddenBody();

    /// <summary>What his body layer shows of his body now.</summary>
    internal abstract KraidBodyAppearance Appearance { get; }

    /// <summary>The body as it shows from now on: a readied body shows at its depth; a shown or hidden body stays as it is.</summary>
    internal abstract KraidBodyShowing Shown();

    /// <summary>The body brought in front of the sprites, as his growth brings it (#1634).</summary>
    /// <exception cref="InvalidOperationException">The body is not behind the sprites.</exception>
    internal abstract KraidBodyShowing InFront();

    /// <summary>
    /// The body shown behind the sprites from now on, grown or not, as his death redraws it (#2646): the cartridge shows his
    /// sinking arm, which the room's front tiles cover, in front of his grown body. A hidden body stays hidden.
    /// </summary>
    internal abstract KraidBodyShowing ShownBehindSprites();

    /// <summary>His appearance at <paramref name="depth"/>.</summary>
    private static KraidBodyAppearance AppearanceAt(BossBodyDepth depth) => depth switch
    {
        BossBodyDepth.BehindSprites => KraidBodyAppearance.BehindSprites,
        BossBodyDepth.InFrontOfSprites => KraidBodyAppearance.InFrontOfSprites,
        _ => throw new InvalidOperationException($"Undefined body depth {depth}."),
    };

    /// <summary>A body readied at <paramref name="Depth"/> that has yet to show: his layer shows none of it.</summary>
    /// <param name="Depth">The depth it shows at once it shows.</param>
    internal sealed partial record Readied(BossBodyDepth Depth) : KraidBodyShowing
    {
        /// <inheritdoc/>
        internal override KraidBodyAppearance Appearance => KraidBodyAppearance.Hidden;

        /// <inheritdoc/>
        internal override KraidBodyShowing Shown() => new ShownBody(Depth);

        /// <inheritdoc/>
        internal override KraidBodyShowing InFront() => Depth == BossBodyDepth.BehindSprites
            ? new Readied(BossBodyDepth.InFrontOfSprites)
            : throw new InvalidOperationException($"Kraid's growth reached a body readied {Depth}.");

        /// <inheritdoc/>
        internal override KraidBodyShowing ShownBehindSprites() => new ShownBody(BossBodyDepth.BehindSprites);
    }

    /// <summary>A body shown at <paramref name="Depth"/>, his whole body at once with his head at its current pose over it.</summary>
    /// <param name="Depth">The depth it shows at.</param>
    internal sealed partial record ShownBody(BossBodyDepth Depth) : KraidBodyShowing
    {
        /// <inheritdoc/>
        internal override KraidBodyAppearance Appearance => AppearanceAt(Depth);

        /// <inheritdoc/>
        internal override KraidBodyShowing Shown() => this;

        /// <inheritdoc/>
        internal override KraidBodyShowing InFront() => Depth == BossBodyDepth.BehindSprites
            ? new ShownBody(BossBodyDepth.InFrontOfSprites)
            : throw new InvalidOperationException($"Kraid's growth reached a body shown {Depth}.");

        /// <inheritdoc/>
        internal override KraidBodyShowing ShownBehindSprites() => new ShownBody(BossBodyDepth.BehindSprites);
    }

    /// <summary>A hidden body, which shows nothing whatever shows it.</summary>
    private sealed partial record HiddenBody : KraidBodyShowing
    {
        /// <inheritdoc/>
        internal override KraidBodyAppearance Appearance => KraidBodyAppearance.Hidden;

        /// <inheritdoc/>
        internal override KraidBodyShowing Shown() => this;

        /// <inheritdoc/>
        internal override KraidBodyShowing InFront() =>
            throw new InvalidOperationException("Kraid's growth reached a hidden body.");

        /// <inheritdoc/>
        internal override KraidBodyShowing ShownBehindSprites() => this;
    }
}
