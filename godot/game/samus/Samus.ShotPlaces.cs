using Engine.Actors;
using Engine.Shared;
using Engine.World;
using Game.Data;

namespace Game;

/// <content>
/// Where a beam of hers fired straight to the left starts, which on the cartridge depends on the shot that last held
/// its place. A new beam is tried against the terrain at its muzzle before it has a speed; one fired straight left
/// is given the smallest speed leftward for that, a 256th of a pixel, and moved by it, and its part of a pixel is then
/// dropped as it takes its real speed. Starting on a whole pixel, that leaves it a pixel further left for its whole
/// flight. A place keeps the part of a pixel across of what held it as her shots were all cleared at once, when a
/// room is left, and the next shot to take the place starts with it: where it is a 256th or more, the beam's small
/// move stays within the pixel and it starts at the muzzle. Whatever takes a place uses up what it kept.
/// </content>
public partial class Samus
{
    /// <summary>The move a beam fired straight left makes at its muzzle before it has its speed: a 256th of a pixel.</summary>
    private const int MuzzleStepFractions = Pixels.FractionsPerPixel / 256;

    /// <summary>The part of a pixel across each of her shot places keeps for the next shot to take it, in <see cref="Pixels.FractionsPerPixel"/>ths.</summary>
    private int[] _shotPlaceKeeps = [];

    /// <summary>Each of her shot places keeps the part of a pixel across of what holds it as the room is left.</summary>
    protected internal override void OnLeavingRoom(Room room)
    {
        string pool = ShotsPool;
        int places = Weapons.PlacesIn(pool);
        if (_shotPlaceKeeps.Length != places)
            Array.Resize(ref _shotPlaceKeeps, places);
        for (int place = 0; place < places; place++)
        {
            // A place nothing holds keeps what it was keeping.
            if (Weapons.StandsIn(pool, place) is { } stands)
                _shotPlaceKeeps[place] = stands.X.Fraction;
        }
    }

    /// <summary>
    /// Has each shot just fired use up what its place kept, and starts a beam fired straight left a pixel further
    /// left unless its place kept enough of a pixel for the move at the muzzle to stay within it.
    /// </summary>
    private void StartFromShotPlaces(WeaponStats weapon, IReadOnlyList<Projectile> shots)
    {
        if (weapon.Pool != ShotsPool)
            return;
        bool beamStraightLeft = weapon.Name == Arsenal.Beam && AimDirection is (-1, 0);
        // Projectiles of one firing that hold a place together all start from what that place kept.
        var keptIn = new Dictionary<int, int>();
        foreach (Projectile shot in shots)
        {
            int kept = 0;
            if (Weapons.PlaceOf(shot) is { } place && place < _shotPlaceKeeps.Length && !keptIn.TryGetValue(place, out kept))
            {
                keptIn[place] = kept = _shotPlaceKeeps[place];
                _shotPlaceKeeps[place] = 0;
            }

            if (beamStraightLeft && kept < MuzzleStepFractions)
                shot.Position += PixelOffset.FromWhole(-1, 0);
        }
    }
}
