using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Installed visual compositions for extended enemy frames. Hitbox records and
/// callback pointers remain application-owned and never come from this JSON.
/// </summary>
public sealed class EnemyExtendedFrameCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-extended-oam-v2", content =>
        {
            foreach ((EnemyFrameId frame, EnemyExtendedDrawComponent[] components) in frames.OrderBy(pair => pair.Key.ToString(), StringComparer.Ordinal))
            {
                content.AppendName("frame", frame.ToString());
                content.Append("components", components.Length);
                foreach (EnemyExtendedDrawComponent component in components)
                {
                    content.Append("offset-x", component.Offset.X);
                    content.Append("offset-y", component.Offset.Y);
                    content.AppendEnemyParts(component.Parts);
                }
            }
            foreach ((EnemyFrameId native, EnemyFrameId selected) in displayFrames.OrderBy(pair => pair.Key.ToString(), StringComparer.Ordinal))
            {
                content.AppendName("native-binding", native.ToString());
                content.AppendName("selected-binding", selected.ToString());
            }
        });

    /// <summary>Each authored frame's compiled OAM components.</summary>
    internal readonly Dictionary<EnemyFrameId, EnemyExtendedDrawComponent[]> frames;
    /// <summary>Editable display choices keyed by the frame gameplay shows.</summary>
    internal readonly Dictionary<EnemyFrameId, EnemyFrameId> displayFrames;

    /// <summary>Retains validated visual components and same-family display bindings.</summary>
    internal EnemyExtendedFrameCatalog(
        Dictionary<EnemyFrameId, EnemyExtendedDrawComponent[]> frames,
        Dictionary<EnemyFrameId, EnemyFrameId> displayFrames)
    {
        this.frames = frames;
        this.displayFrames = displayFrames;
    }

    /// <summary>Finds a frame's components, including the shared empty extended frame's.</summary>
    internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components)
    {
        // The shared empty extended frame's sole component draws the empty spritemap; no editable drawing is needed
        // before initialization.
        if (CommonEnemyEmptyExtendedFrameDefinitions.IsFrame(frame))
        {
            components = ReadOnlyMemory<EnemyExtendedDrawComponent>.Empty;
            return true;
        }
        if (frames.TryGetValue(frame, out EnemyExtendedDrawComponent[]? found))
        {
            components = found;
            return true;
        }
        components = default;
        return false;
    }

    /// <summary>Maps the frame gameplay shows to the editable draw components displayed for it.</summary>
    internal bool TryGetDisplay(EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components)
    {
        if (displayFrames.TryGetValue(frame, out EnemyFrameId selected) &&
            frames.TryGetValue(selected, out EnemyExtendedDrawComponent[]? found))
        {
            components = found;
            return true;
        }
        return TryGet(frame, out components);
    }

    /// <summary>The frame displayed for <paramref name="frame"/>, whose BG2 half accompanies the displayed components.</summary>
    internal EnemyFrameId GetDisplayFrame(EnemyFrameId frame) =>
        displayFrames.TryGetValue(frame, out EnemyFrameId selected) ? selected : frame;
}
