using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>Native $8F:C1A9 sprite/sound publication through the shared object pool.</summary>
    public void SpawnEscapeExplosion(ushort x, ushort y, ushort random, ushort inheritedX)
    {
        int choice = random & 15;
        // The cartridge leaves X unchanged when the random nibble is 8..15. The
        // fourth-frame caller inherits zero; the nonblank caller leaves its block offset.
        // Preserve this quirk rather than importing the upstream C port's bugfix.
        if (choice < 8)
        {
            inheritedX = (ushort)choice;
            byte sound = _bus!.ReadByte(ZebesEscapeRomData.SoundTable + choice);
            if (sound != 0)
                _soundRequests.Add(new(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, sound), 6));
        }
        var kind = (RoomSpriteObjectKind)_bus!.ReadByte(ZebesEscapeRomData.SpriteTable + (inheritedX & 7));
        SpawnRoomSpriteObject(x, y, kind, 0);
    }
}
