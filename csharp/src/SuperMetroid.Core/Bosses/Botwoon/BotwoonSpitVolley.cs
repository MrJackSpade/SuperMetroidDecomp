namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>One of Botwoon's spit volleys: how many spits it fires and where its fan starts.</summary>
/// <param name="Count">The spits the volley fires.</param>
/// <param name="FirstOffsetSteps">The turn, in steps, from his aim to the volley's first spit.</param>
internal readonly record struct BotwoonSpitVolley(int Count, int FirstOffsetSteps);
