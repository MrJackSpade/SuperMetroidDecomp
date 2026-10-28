using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>The opening cinematic's tilemaps, palette and Ceres artwork the game binds (#633, #1463).</summary>
public record IntroCinematicSceneParts
{
    /// <summary>Exactly four ordered 32x32 tilemap pages of the illustrated scenes.</summary>
    public required IReadOnlyList<RoomBackgroundTilemapAtlas> BackgroundPages { get; init; }

    /// <summary>One 32x32 Samus-head portrait page.</summary>
    public required RoomBackgroundTilemapAtlas PortraitTilemap { get; init; }

    /// <summary>One 32x32 pre-typewriter narration page.</summary>
    public required RoomBackgroundTilemapAtlas InitialNarrationTilemap { get; init; }

    /// <summary>Four rows referencing the Japanese subtitle glyphs.</summary>
    public required IntroFinalLineTilemap FinalLine { get; init; }

    /// <summary>Samus portrait eye rectangles, independent of blink timing.</summary>
    public required IntroEyeTilemapPresentation EyeFrames { get; init; }

    /// <summary>Native-precision colors loaded before the opening narration.</summary>
    public required IntroCinematicPalette Palette { get; init; }

    /// <summary>Mode-7 artwork and placements used after the narration fades to Ceres.</summary>
    public required CeresFlightArtwork CeresFlight { get; init; }

    /// <summary>Ceres destruction artwork and subsequent Zebes reveal artwork.</summary>
    public required CeresDestructionArtwork CeresDestruction { get; init; }
}
