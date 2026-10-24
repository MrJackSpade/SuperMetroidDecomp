using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The bosses' own copies of the enemy BG2 image (#1395): Crocomire's death working map and Mother Brain's staged body.
/// </summary>
/// <param name="crocomire">Crocomire, whose death sequence keeps a working BG2 map.</param>
/// <param name="motherBrain">Mother Brain's encounter, whose body is staged before it is presented.</param>
/// <param name="room">The room, whose enemy staging holds Mother Brain's BG2 body.</param>
internal sealed class BossBg2Copies(CrocomireBehavior crocomire, MotherBrainEncounter motherBrain, EnemyRoomBinding room) : IEnemyBg2Copies
{
    /// <inheritdoc/>
    public bool StagesBody => motherBrain.MotherBrain is not null;

    /// <inheritdoc/>
    public void Write(int destinationWord, ReadOnlySpan<ushort> words)
    {
        if (crocomire.Access.CrocomireDeath is { } death)
            words.CopyTo(death.MutableBg2WorkingTilemap.Slice(destinationWord, words.Length));
        if (StagesBody)
        {
            ushort[] bg2 = room.Staging.EnemyBg2Tilemap;
            for (int index = 0; index < words.Length; index++)
                bg2[destinationWord + index] = words[index];
        }
    }
}
