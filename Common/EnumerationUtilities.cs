using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Casiland.Common;

public static class EnumerationUtilities
{
    public static T PickRandom<T>(this IEnumerable<T> collection)
    {
        var arr = collection.ToArray();
        return arr[GD.RandRange(0, arr.Length-1)];
    }
    public static T PickRandom<T>(this IEnumerable<T> collection, RandomNumberGenerator rng)
    {
        var arr = collection.ToArray();
        return arr[rng.RandiRange(0, arr.Length-1)];
    }
}