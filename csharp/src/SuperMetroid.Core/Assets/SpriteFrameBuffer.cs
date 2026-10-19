using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// One frame's sprite description: the whole-frame draws of every actor, in emission order, frontmost first. Gameplay
/// builds one per frame and the NMI latches it; backends draw it without OAM or VRAM.
/// </summary>
public sealed class SpriteFrameBuffer
{
    /// <summary>Draws appended since <see cref="BeginFrame"/>.</summary>
    private readonly List<SpriteDraw> pending = [];

    /// <summary>The finalized draws; never mutated once published, so <see cref="Draws"/> can share it.</summary>
    private SpriteDraw[] finalized = [];

    /// <summary>The draws of the most recently finalized (or latched) frame, in emission order.</summary>
    public ImmutableArray<SpriteDraw> Draws => ImmutableCollectionsMarshal.AsImmutableArray(finalized);

    /// <summary>Starts building a frame: clears the pending draws.</summary>
    public void BeginFrame() => pending.Clear();

    /// <summary>Appends a whole-frame draw after every draw emitted so far this frame.</summary>
    public void Add(SpriteDraw draw) => pending.Add(Ensure.NotNull(draw));

    /// <summary>Finishes the frame: publishes the pending draws as <see cref="Draws"/>.</summary>
    public void FinalizeFrame()
    {
        finalized = [.. pending];
        pending.Clear();
    }

    /// <summary>Replaces the displayed draws with a published frame's, for a render consumer.</summary>
    public void LoadDraws(ImmutableArray<SpriteDraw> draws)
    {
        if (draws.IsDefault) throw new ArgumentException("A published frame needs a draw list.", nameof(draws));
        pending.Clear();
        finalized = ImmutableCollectionsMarshal.AsArray(draws) ?? throw new ArgumentException("A published frame needs a draw list.", nameof(draws));
    }

    /// <summary>
    /// Points every finalized draw over artwork of <paramref name="kind"/> at <paramref name="artwork"/>. The NMI uses it
    /// after its uploads, so a frame's draws show the characters the same NMI bound.
    /// </summary>
    /// <param name="kind">The artwork kind to rebind.</param>
    /// <param name="artwork">The artwork those draws now show.</param>
    public void RebindArtwork(SpriteArtworkKind kind, SpriteArtwork artwork)
    {
        Ensure.NotNull(artwork);
        if (!Array.Exists(finalized, draw => draw.Artwork.Id.Kind == kind && !ReferenceEquals(draw.Artwork, artwork)))
            return;
        // A published draw list is shared with the staging buffer it was latched from, so rebinding copies it.
        finalized = Array.ConvertAll(finalized, draw => draw.Artwork.Id.Kind != kind ? draw : draw.WithArtwork(artwork));
    }

    /// <summary>Whether any finalized draw shows artwork of <paramref name="kind"/>.</summary>
    public bool Shows(SpriteArtworkKind kind) => Array.Exists(finalized, draw => draw.Artwork.Id.Kind == kind);

    /// <summary>Makes a finalized frame the displayed one, as the NMI does.</summary>
    /// <param name="staging">The buffer the main loop built and finalized.</param>
    public void LatchFrom(SpriteFrameBuffer staging) => finalized = Ensure.NotNull(staging).finalized;
}
