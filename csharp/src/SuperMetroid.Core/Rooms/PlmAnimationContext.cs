using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>What a typed PLM program reaches during one handler pass: the PLM owners, the room and the frame's inputs.</summary>
/// <param name="Plms">The room's PLM owners.</param>
/// <param name="Bus">The address space the draw dispatcher reads installed or fixture draws through.</param>
/// <param name="Level">The room's level data, which draws and terrain commands change.</param>
/// <param name="Streamer">The background tilemap streamer that publishes changed blocks.</param>
/// <param name="View">The BG1 view the tile writes reflect.</param>
/// <param name="Frame">What the handler reads from the rest of the frame.</param>
internal readonly record struct PlmAnimationContext(
    RoomPlmSystem Plms, ISnesAddressSpace Bus, RoomLevelData Level, BackgroundTilemapStreamer Streamer, PlmView View, PlmFrameInputs Frame)
{
    /// <summary>Draws the bank-$84 draw list <paramref name="drawList"/> at the slot's block, as a timed draw does.</summary>
    internal void Draw(PlmSlot slot, ushort drawList) =>
        Plms.Drawing.DrawPlmInstruction(Bus, Level, Streamer, slot.HeaderPointer, slot.BlockIndex, drawList, View);

    /// <summary>Queues <paramref name="sound"/> under the native queue cap <paramref name="maximumQueued"/>.</summary>
    internal void QueueSound(SoundEffectId sound, byte maximumQueued) =>
        Plms.Sounds._soundRequests.Add(Plms.Sounds.CreateSoundRequest(sound, MaximumQueued: maximumQueued));

    /// <summary>Moves the slot's origin by whole blocks, as the cartridge's move-PLM instructions do.</summary>
    internal void MoveOrigin(PlmSlot slot, int columns, int rows) =>
        slot.BlockIndex = checked(slot.BlockIndex + columns + rows * Level.Size.X);
}
