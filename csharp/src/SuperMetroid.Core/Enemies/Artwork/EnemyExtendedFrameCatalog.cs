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
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-extended-oam-v1", content =>
        {
            foreach ((int frame, EnemyExtendedDrawComponent[] components) in frames.OrderBy(pair => pair.Key))
            {
                content.Append("frame", frame);
                content.Append("components", components.Length);
                foreach (EnemyExtendedDrawComponent component in components)
                {
                    content.Append("offset-x", component.Offset.X);
                    content.Append("offset-y", component.Offset.Y);
                    content.AppendEnemyParts(component.Parts);
                }
            }
            foreach ((int native, int selected) in displayFrames.OrderBy(pair => pair.Key))
            {
                content.Append("native-binding", native);
                content.Append("selected-binding", selected);
            }
        });

    /// <summary>Physical bank-and-pointer frame identities mapped to compiled OAM components.</summary>
    internal readonly Dictionary<int, EnemyExtendedDrawComponent[]> frames;
    /// <summary>Editable display choices keyed by the immutable native frame identity.</summary>
    internal readonly Dictionary<int, int> displayFrames;

    /// <summary>Retains validated visual components and same-family display bindings.</summary>
    internal EnemyExtendedFrameCatalog(
        Dictionary<int, EnemyExtendedDrawComponent[]> frames,
        Dictionary<int, int> displayFrames)
    {
        this.frames = frames;
        this.displayFrames = displayFrames;
    }

    /// <summary>Finds physical frame components, including the shared empty frame at bank-local $804F.</summary>
    internal bool TryGet(byte bank, ushort pointer,
        out ReadOnlyMemory<EnemyExtendedDrawComponent> components)
    {
        // The bank-local $804F extended frame is shared by ordinary enemy
        // banks. Its sole component points to the empty $804D OAM frame; no
        // editable drawing or cartridge lookup is needed before initialization.
        if (CommonEnemyEmptyExtendedFrameDefinitions.HasFrame(bank, pointer))
        {
            components = ReadOnlyMemory<EnemyExtendedDrawComponent>.Empty;
            return true;
        }
        if (frames.TryGetValue(((int)new SnesAddress(bank, pointer)),
                out EnemyExtendedDrawComponent[]? found))
        {
            components = found;
            return true;
        }
        components = default;
        return false;
    }

    /// <summary>Maps an immutable physical extended-frame identity to editable draw components.</summary>
    internal bool TryGetDisplay(byte bank, ushort nativePointer,
        out ReadOnlyMemory<EnemyExtendedDrawComponent> components)
    {
        int identity = ((int)new SnesAddress(bank, nativePointer));
        if (displayFrames.TryGetValue(identity, out int selected) &&
            frames.TryGetValue(selected, out EnemyExtendedDrawComponent[]? found))
        {
            components = found;
            return true;
        }
        return TryGet(bank, nativePointer, out components);
    }

    /// <summary>Resolves the matching BG2 half of a display frame without mutating its physical selector.</summary>
    internal ushort GetDisplayPointer(byte bank, ushort nativePointer) =>
        displayFrames.TryGetValue(((int)new SnesAddress(bank, nativePointer)), out int selected)
            ? unchecked((ushort)selected) : nativePointer;
}
