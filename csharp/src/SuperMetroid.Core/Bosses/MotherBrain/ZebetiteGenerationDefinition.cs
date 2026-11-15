namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One Zebetite barrier generation (#2012): its form and where it stands.</summary>
/// <param name="Form">Whether the generation is one large barrier or a split pair.</param>
/// <param name="X">The room pixel column every half of the generation stands on.</param>
/// <param name="CentreY">The room pixel row of the generation's centre line, which a single barrier stands on.</param>
/// <param name="HalfHeight">Each half's vertical reach above and below its own centre, in pixels.</param>
internal readonly record struct ZebetiteGenerationDefinition(ZebetiteBarrierForm Form, int X, int CentreY, int HalfHeight);
