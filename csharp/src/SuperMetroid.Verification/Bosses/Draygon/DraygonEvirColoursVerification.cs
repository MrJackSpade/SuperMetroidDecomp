using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>
/// Draygon's Evirs show his sprite colours (#2641): the game creates them in his sprite row, so his colours and their white
/// flash recolour their pictures. The import composed them in no row, so his colours found nothing to recolour and they
/// showed the pale colours of the room's first row.
/// </summary>
internal static class DraygonEvirColoursVerification
{
    /// <summary>Checks that the Evir pictures on his room's shared sheet are in his sprite colours, which his white flash recolours.</summary>
    internal static void VerifyDraygonEvirsShowHisSpriteColours()
    {
        SpawnedObjectSheet shared = InstalledFixtures.EnemySheets.Spawned(RoomEnemyGraphicsSetDefinitions.DraygonDefault, SpawnedObjectOwner.Shared);
        SpriteImage sheet = shared.Frame(EnemySheetDefinitions.EffectActorFrameName(EnemyFrameId.EffectActorEvirFacingLeftLegsStretched)).Sheet;
        AssertTrue(sheet.Shades.TryGetValue(DraygonShades.Sprite, out PictureRecolouring? sprite) && sprite is not null && !sprite.Substitutions.IsEmpty,
            "the Evirs' pictures are drawn in Draygon's sprite colours");
        AssertTrue(sheet.Shades.TryGetValue(DraygonShades.SpriteWhite, out PictureRecolouring? white) && white is not null
            && white.Substitutions.Any(substitution => substitution.Replaced.Colour != substitution.Shown),
            "Draygon's white flash recolours his Evirs' pictures");
    }

    /// <summary>Runs the Draygon Evir colours check.</summary>
    [VerificationCommand("--draygon-evir-colours")]
    internal static int DraygonEvirColoursCommand()
    {
        Suite(nameof(VerifyDraygonEvirsShowHisSpriteColours), VerifyDraygonEvirsShowHisSpriteColours);
        return 0;
    }
}
