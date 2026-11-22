namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's three health colourings (#2242), each lower one showing him more hurt.</summary>
public static partial class RidleyHealthShadeDefinitions
{
    /// <summary>Ridley wounded: Norfair Ridley and Ceres Ridley in their wounded health bands.</summary>
    public static RidleyHealthShade Wounded { get; } = new("wounded");

    /// <summary>Ridley badly wounded: Norfair Ridley in his badly wounded health band; Ceres Ridley never shows it.</summary>
    public static RidleyHealthShade BadlyWounded { get; } = new("badly-wounded");

    /// <summary>Ridley nearly dead: Norfair Ridley and Ceres Ridley in their nearly-dead health bands.</summary>
    public static RidleyHealthShade NearlyDead { get; } = new("nearly-dead");

    /// <summary>The colourings, least hurt first.</summary>
    public static IReadOnlyList<RidleyHealthShade> LeastHurtFirst { get; } = [Wounded, BadlyWounded, NearlyDead];
}
