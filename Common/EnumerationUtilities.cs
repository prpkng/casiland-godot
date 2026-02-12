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
    
    public static void Shuffle<T> (this RandomNumberGenerator rng, T[] array)
    {
        int n = array.Length;
        while (n > 1) 
        {
            int k = rng.RandiRange(0, -1 + n--);
            (array[n], array[k]) = (array[k], array[n]);
        }
    }
    
    public static IEnumerable<T> Shuffle<T> (this IEnumerable<T> enumerable, RandomNumberGenerator rng)
    {
        var array = enumerable.ToArray();
        int n = array.Length;
        while (n > 1) 
        {
            int k = rng.RandiRange(0, -1 + n--);
            (array[n], array[k]) = (array[k], array[n]);
        }

        return array;
    }
}