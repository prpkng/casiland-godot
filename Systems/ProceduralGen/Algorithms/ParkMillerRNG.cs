using System;

namespace Casiland.Systems.ProceduralGen.Algorithms;

public class ParkMillerRng
{
    // Constants from Park–Miller minimal standard LCG
    private const int A = 16807;             // multiplier
    private const int M = 2147483647;        // modulus (2^31 - 1)
    private const int Q = 127773;            // M / A
    private const int R = 2836;              // M % A

    private int _seed;

    public ParkMillerRng(int seed)
    {
        if (seed is <= 0 or >= M)
            throw new ArgumentOutOfRangeException(nameof(seed),
                "Seed must be between 1 and 2147483646");

        _seed = seed;
    }

    // Returns (1 ... M-1). Never returns 0, per the original definition.
    public int NextInt()
    {
        int hi = _seed / Q;
        int lo = _seed % Q;

        int test = A * lo - R * hi;

        _seed = (test > 0) ? test : test + M;
        return _seed;
    }

    public int NextInt(int min, int max)
    {
        int diff = max - min;
        return min + NextInt() % (diff + 1) - 1;
    }

    // Returns a float in [0, 1)
    public float NextFloat()
    {
        return NextInt() * (1.0f / (M - 1));
    }

    public float NextFloat(float min, float max)
    {
        float diff = max - min;
        return min + diff * NextFloat();
    }
}
