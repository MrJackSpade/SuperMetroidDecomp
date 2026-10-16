using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// One frame's sprite description: the OAM sprites that actors not yet converted still append, plus the
/// whole-frame draws of converted actors, kept in one emission order. Beginning and finalizing a frame always
/// act on both, so a draw can never outlive the OAM image it was built with.
/// </summary>
public sealed class SpriteFrameBuffer
{
    /// <summary>Draws appended since <see cref="BeginFrame"/>.</summary>
    private readonly List<OrderedSpriteDraw> pending = [];

    /// <summary>The OAM image built alongside the draws.</summary>
    public OamBuffer Oam { get; } = new();

    /// <summary>The finalized draws; never mutated once published, so <see cref="Draws"/> can share it.</summary>
    private OrderedSpriteDraw[] finalized = [];

    /// <summary>The draws of the most recently finalized (or latched) frame, in emission order.</summary>
    public ImmutableArray<OrderedSpriteDraw> Draws => ImmutableCollectionsMarshal.AsImmutableArray(finalized);

    /// <summary>Starts building a frame: clears the OAM construction state and the pending draws.</summary>
    public void BeginFrame()
    {
        Oam.BeginFrame();
        pending.Clear();
    }

    /// <summary>Appends a whole-frame draw after every sprite emitted so far this frame.</summary>
    public void Add(SpriteDraw draw)
    {
        Ensure.NotNull(draw);
        pending.Add(new(Oam.AppendedSpriteCount, draw));
    }

    /// <summary>Finishes the frame: finalizes OAM and publishes the pending draws as <see cref="Draws"/>.</summary>
    public void FinalizeFrame()
    {
        Oam.FinalizeFrame();
        finalized = [.. pending];
        pending.Clear();
    }

    /// <summary>Replaces the displayed draws with a published frame's, for a render consumer.</summary>
    public void LoadDraws(ImmutableArray<OrderedSpriteDraw> draws)
    {
        if (draws.IsDefault) throw new ArgumentException("A published frame needs a draw list.", nameof(draws));
        pending.Clear();
        finalized = ImmutableCollectionsMarshal.AsArray(draws) ?? throw new ArgumentException("A published frame needs a draw list.", nameof(draws));
    }

    /// <summary>
    /// Points every finalized draw over artwork of <paramref name="kind"/> at <paramref name="artwork"/>. The NMI uses it
    /// after its uploads, so a frame's draws show the characters the same NMI bound, as the displayed OAM reads the VRAM
    /// that NMI wrote.
    /// </summary>
    /// <param name="kind">The artwork kind to rebind.</param>
    /// <param name="artwork">The artwork those draws now show.</param>
    public void RebindArtwork(SpriteArtworkKind kind, SpriteArtwork artwork)
    {
        Ensure.NotNull(artwork);
        if (!Array.Exists(finalized, ordered => ordered.Draw.Artwork.Id.Kind == kind && !ReferenceEquals(ordered.Draw.Artwork, artwork)))
            return;
        // A published draw list is shared with the staging buffer it was latched from, so rebinding copies it.
        finalized = Array.ConvertAll(finalized, ordered => ordered.Draw.Artwork.Id.Kind != kind ? ordered
            : ordered with { Draw = ordered.Draw.WithArtwork(artwork) });
    }

    /// <summary>Whether any finalized draw shows artwork of <paramref name="kind"/>.</summary>
    public bool Shows(SpriteArtworkKind kind) => Array.Exists(finalized, ordered => ordered.Draw.Artwork.Id.Kind == kind);

    /// <summary>Makes a finalized frame the displayed one, as the NMI's OAM DMA does.</summary>
    /// <param name="staging">The buffer the main loop built and finalized.</param>
    public void LatchFrom(SpriteFrameBuffer staging)
    {
        Ensure.NotNull(staging);
        Oam.CopyFinalizedFrom(staging.Oam);
        finalized = staging.finalized;
    }
}
