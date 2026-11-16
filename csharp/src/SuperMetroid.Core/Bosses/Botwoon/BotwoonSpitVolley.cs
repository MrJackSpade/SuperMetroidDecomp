namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>One of Botwoon's spit volleys: how many spits it fires, where its fan starts and how long his head cools down after it.</summary>
/// <param name="Count">The spits the volley fires.</param>
/// <param name="FirstOffsetSteps">The turn, in steps, from his aim to the volley's first spit.</param>
/// <param name="CooldownUpdates">The updates after the volley before his head follows his movement again (#2267).</param>
internal readonly record struct BotwoonSpitVolley(int Count, int FirstOffsetSteps, int CooldownUpdates);
