using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Which spawned-object sheet a projectile or sprite object takes its pictures from, and how they are shaded (#1464,
/// #1512, #1513): what an enemy hands the objects it spawns so they draw its objects' pictures in its colours. The
/// default value is the emptied graphics: the room's shared sheet in the pictures' standard colours.
/// </summary>
public readonly record struct SpriteGraphics
{
    /// <summary>The shading given, or null for the emptied graphics' standard colours.</summary>
    private readonly PictureShading? shading;

    /// <summary>Graphics drawing from <paramref name="owner"/>'s sheet under <paramref name="shading"/>.</summary>
    /// <param name="owner">Whose spawned-object sheet the pictures come from.</param>
    /// <param name="shading">The shades and colour effects its pictures show.</param>
    public SpriteGraphics(SpawnedObjectOwner owner, PictureShading shading)
    {
        Owner = owner;
        this.shading = Ensure.NotNull(shading);
    }

    /// <summary>Whose spawned-object sheet the pictures come from.</summary>
    public SpawnedObjectOwner Owner { get; init; }

    /// <summary>The shades and colour effects its pictures show; the emptied graphics show their standard colours.</summary>
    public PictureShading Shading
    {
        get => shading ?? PictureShading.Standard;
        init => shading = Ensure.NotNull(value);
    }

    /// <summary>What an emptied slot holds: the room's shared sheet in its standard colours.</summary>
    internal static SpriteGraphics Emptied => default;

    /// <summary>The room's shared sheet under <paramref name="shading"/>.</summary>
    internal static SpriteGraphics SharedIn(PictureShading shading) => new(SpawnedObjectOwner.Shared, shading);

    /// <summary>
    /// What <paramref name="spawner"/> hands the objects it spawns: its own spawned-object sheet, in the shading it shows
    /// its pictures in now.
    /// </summary>
    internal static SpriteGraphics SpawnedBy(RoomEnemySlot spawner) =>
        new(SpawnedObjectOwner.Of(Ensure.NotNull(spawner).DefinitionId), spawner.Shading);

    /// <summary>The same sheet under <paramref name="shading"/>.</summary>
    internal SpriteGraphics In(PictureShading shading) => this with { Shading = shading };

    /// <summary>Whether both draw from the same sheet under the same shading.</summary>
    public bool Equals(SpriteGraphics other) => Owner == other.Owner && Shading == other.Shading;

    /// <summary>Hashes the sheet and shading <see cref="Equals(SpriteGraphics)"/> compares.</summary>
    public override int GetHashCode() => HashCode.Combine(Owner, Shading);
}
