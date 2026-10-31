namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Launches the eight fragments of Mother Brain's escape door.</summary>
internal static class MotherBrainDoorFragments
{
    /// <summary>
    /// Returns the fragment selected by enemy-projectile parameter zero through seven. The
    /// interleaved offsets at <c>$86:C992-$86:C9B1</c> are a zero X and a Y that steps eight
    /// pixels from 32 above the door.
    /// </summary>
    internal static MotherBrainDoorFragmentDefinition ForParameter(int parameter)
    {
        if (parameter >= MotherBrainDoorFragmentDefinitions.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter, "Mother Brain door fragment parameter must be zero through seven.");
        }

        return new(new Offset16<RoomPixels>(0,
                WordWrap.Signed(MotherBrainDoorFragmentDefinitions.FirstY + MotherBrainDoorFragmentDefinitions.YStep * parameter)),
            new EightEightVector<RoomPixels>(MotherBrainDoorFragmentDefinitions.XVelocity, MotherBrainDoorFragmentDefinitions.YVelocities[parameter]));
    }
}
