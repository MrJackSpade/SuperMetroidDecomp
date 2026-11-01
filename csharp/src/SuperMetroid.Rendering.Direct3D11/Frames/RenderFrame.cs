using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// One frame handed to presentation: the game's immutable display packet under the identity the host gave it. The
/// game knows only its frame number; the sequence and generation that order and invalidate frames are the host's.
/// </summary>
public sealed class RenderFrame
{
    /// <summary>Pairs a display packet with the host's identity for it.</summary>
    /// <param name="identity">The host's sequence and generation for this frame.</param>
    /// <param name="display">The immutable display the game published.</param>
    /// <exception cref="ArgumentNullException"><paramref name="display"/> is null.</exception>
    public RenderFrame(RenderFrameIdentity identity, DisplayPacket display)
    {
        Identity = identity;
        Display = Ensure.NotNull(display);
    }

    /// <summary>The host's sequence and generation for this frame.</summary>
    public RenderFrameIdentity Identity { get; }

    /// <summary>The immutable display the game published.</summary>
    public DisplayPacket Display { get; }

    /// <summary>The identity and the game frame number, for diagnostics.</summary>
    public override string ToString() => $"{Identity} (game frame {Display.FrameNumber})";
}
