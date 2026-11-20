namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification's view of a coloured door cap: the colour it was placed with, read from the rules it follows.</summary>
internal static class ColoredDoorObjectAccess
{
    /// <summary>Reads the colour a cap stands for.</summary>
    extension(ColoredDoorObject cap)
    {
        /// <summary>The colour the cap was placed with: the one whose rules the cap counts its shots and hits by.</summary>
        /// <exception cref="InvalidOperationException">No colour, or more than one, has the cap's rules.</exception>
        internal ColoredDoorColor PlacedColor
        {
            get
            {
                ColoredDoorColorDefinition rules = PrivateState.Field<ColoredDoorObject, ColoredDoorColorDefinition>(cap, "_rules");
                return ColoredDoorDefinitions.Colors.Single(pair => ReferenceEquals(pair.Value, rules)).Key;
            }
        }
    }
}
