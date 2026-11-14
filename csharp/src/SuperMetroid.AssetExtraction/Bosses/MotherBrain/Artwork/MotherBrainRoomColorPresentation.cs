using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>The installed Mother Brain fake-death room flash, steady room, phase-two and room-entry colors, kept as installed.</summary>
public sealed class MotherBrainRoomColorPresentation
{
    /// <summary>The installed timed room-color flash.</summary>
    internal readonly InstalledColourRows flash;
    /// <summary>The installed steady gray room colors used after the fake-death flash ends.</summary>
    internal readonly InstalledColourRows finalRoom;
    /// <summary>The installed phase-two attack sprite colors.</summary>
    internal readonly InstalledColourRows phaseTwoAttack;
    /// <summary>The installed phase-two rear-leg colors.</summary>
    internal readonly InstalledColourRows phaseTwoRearLeg;
    /// <summary>The installed room-entry glass-shard colors.</summary>
    internal readonly InstalledColourRows initialGlassShard;
    /// <summary>The installed room-entry tube-projectile colors.</summary>
    internal readonly InstalledColourRows initialTubeProjectile;
    /// <summary>The installed room lights' recovery frames.</summary>
    internal readonly InstalledColourRows recoveryLights;

    /// <summary>Keeps every complete installed colour set.</summary>
    /// <param name="colours">Every set's colours, as installed.</param>
    /// <exception cref="InvalidDataException">A set is missing a frame or a colour.</exception>
    internal MotherBrainRoomColorPresentation(MotherBrainRoomColours colours)
    {
        flash = new InstalledColourRows(colours.Flash, MotherBrainRoomFlashDefinitions.FlashFrameCount,
            MotherBrainRoomColorGeometry.SliceColors * 2, "Mother Brain room flash");
        finalRoom = new InstalledColourRows(colours.FinalRoom, MotherBrainRoomColorGeometry.SliceColors * 2, "Mother Brain final room");
        phaseTwoAttack = new InstalledColourRows(colours.PhaseTwoAttack, MotherBrainRoomColorGeometry.PhaseTwoColors,
            "Mother Brain phase-two attack");
        phaseTwoRearLeg = new InstalledColourRows(colours.PhaseTwoRearLeg, MotherBrainRoomColorGeometry.PhaseTwoColors,
            "Mother Brain phase-two rear leg");
        initialGlassShard = new InstalledColourRows(colours.InitialGlassShard, MotherBrainRoomColorGeometry.InitialColors,
            "Mother Brain room-entry glass shard");
        initialTubeProjectile = new InstalledColourRows(colours.InitialTubeProjectile, MotherBrainRoomColorGeometry.InitialColors,
            "Mother Brain room-entry tube projectile");
        recoveryLights = new InstalledColourRows(colours.RecoveryLights, MotherBrainRoomColorDefinitions.RecoveryLightsFrames,
            MotherBrainRoomColorGeometry.RecoveryLightsSliceColors * 2, "Mother Brain room-light recovery");
    }
}
