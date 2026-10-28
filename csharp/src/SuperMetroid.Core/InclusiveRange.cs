namespace SuperMetroid.Core;

/// <summary>A closed range of whole numbers, both ends included: prepared draw layers, a row's occupied columns.</summary>
/// <param name="First">The range's first member.</param>
/// <param name="Last">The range's last member.</param>
internal readonly record struct InclusiveRange(int First, int Last);
