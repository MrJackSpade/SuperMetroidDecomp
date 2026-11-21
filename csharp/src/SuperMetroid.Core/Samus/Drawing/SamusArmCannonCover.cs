using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// The arm cannon's cover (#2560): a closed choice of closed, opening at a picture, open, and closing at a picture. Only an
/// opening or a closing moves; a closed cover shows nothing and an open one its fully open picture.
/// </summary>
internal abstract record SamusArmCannonCover
{
    /// <summary>Only the four states below are covers.</summary>
    private SamusArmCannonCover()
    {
    }

    /// <summary>The closed cover.</summary>
    internal static SamusArmCannonCover Closed { get; } = new ClosedCover();

    /// <summary>The open cover.</summary>
    internal static SamusArmCannonCover Open { get; } = new OpenCover();

    /// <summary>The picture the cover shows, or null while it is closed.</summary>
    internal abstract SamusArmCannonCoverFrame? Shown { get; }

    /// <summary>Whether the cover is open or opening; false while closed or closing.</summary>
    internal abstract bool OpenOrOpening { get; }

    /// <summary>The cover one picture further on, for an opening or a closing.</summary>
    /// <exception cref="InvalidOperationException">The cover is not moving.</exception>
    internal virtual SamusArmCannonCover Advanced() =>
        throw new InvalidOperationException($"A {GetType().Name} arm cannon cover does not move.");

    /// <summary>The closed cover, which shows nothing.</summary>
    private sealed record ClosedCover : SamusArmCannonCover
    {
        /// <inheritdoc/>
        internal override SamusArmCannonCoverFrame? Shown => null;

        /// <inheritdoc/>
        internal override bool OpenOrOpening => false;
    }

    /// <summary>The open cover, which shows its fully open picture.</summary>
    private sealed record OpenCover : SamusArmCannonCover
    {
        /// <inheritdoc/>
        internal override SamusArmCannonCoverFrame? Shown => SamusArmCannonCoverFrame.FullyOpen;

        /// <inheritdoc/>
        internal override bool OpenOrOpening => true;
    }

    /// <summary>A cover opening, at <paramref name="Frame"/>: each update shows the next picture until it is fully open.</summary>
    /// <param name="Frame">The picture shown.</param>
    internal sealed record Opening(SamusArmCannonCoverFrame Frame) : SamusArmCannonCover
    {
        /// <inheritdoc/>
        internal override SamusArmCannonCoverFrame? Shown => Frame;

        /// <inheritdoc/>
        internal override bool OpenOrOpening => true;

        /// <inheritdoc/>
        internal override SamusArmCannonCover Advanced() => Frame switch
        {
            SamusArmCannonCoverFrame.SlightlyOpen => new Opening(SamusArmCannonCoverFrame.HalfOpen),
            SamusArmCannonCoverFrame.HalfOpen or SamusArmCannonCoverFrame.FullyOpen => Open,
            _ => throw new InvalidOperationException($"Undefined arm cannon cover frame {Frame}."),
        };
    }

    /// <summary>A cover closing, at <paramref name="Frame"/>: each update shows the previous picture until it is closed.</summary>
    /// <param name="Frame">The picture shown.</param>
    internal sealed record Closing(SamusArmCannonCoverFrame Frame) : SamusArmCannonCover
    {
        /// <inheritdoc/>
        internal override SamusArmCannonCoverFrame? Shown => Frame;

        /// <inheritdoc/>
        internal override bool OpenOrOpening => false;

        /// <inheritdoc/>
        internal override SamusArmCannonCover Advanced() => Frame switch
        {
            SamusArmCannonCoverFrame.FullyOpen => new Closing(SamusArmCannonCoverFrame.HalfOpen),
            SamusArmCannonCoverFrame.HalfOpen => new Closing(SamusArmCannonCoverFrame.SlightlyOpen),
            SamusArmCannonCoverFrame.SlightlyOpen => Closed,
            _ => throw new InvalidOperationException($"Undefined arm cannon cover frame {Frame}."),
        };
    }
}
