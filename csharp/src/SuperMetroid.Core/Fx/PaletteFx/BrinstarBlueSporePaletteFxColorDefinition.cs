namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One room-specific entry into the fourteen-frame blue-spore palette loop.</summary>
public sealed class BrinstarBlueSporePaletteFxColorDefinition
{
    /// <summary>Restricts entries to the two room families that share the blue-spore colors.</summary>
    internal BrinstarBlueSporePaletteFxColorDefinition(BrinstarBlueSporePaletteOwner owner)
    {
        if (owner is not (BrinstarBlueSporePaletteOwner.StandardRooms or BrinstarBlueSporePaletteOwner.SporeSpawnRoom))
            throw new ArgumentOutOfRangeException(nameof(owner));
        Owner = owner;
    }

    /// <summary>The room family represented by this program.</summary>
    public BrinstarBlueSporePaletteOwner Owner { get; }

    /// <summary>The Spore Spawn room installs the area-mini-boss death callback at $8D:EE2D.</summary>
    public bool DeletesWithAreaMiniBoss => Owner == BrinstarBlueSporePaletteOwner.SporeSpawnRoom;

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorRows Rows => new(DeletesWithAreaMiniBoss ? 396 : 382, BrinstarBlueSporePaletteFxColorDefinitions.FrameCount, BrinstarBlueSporePaletteFxColorDefinitions.ColorsPerFrame);

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => Rows.Color(frame, color);
}
