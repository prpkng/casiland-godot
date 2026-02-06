using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Casiland.Systems.ProceduralGen.Steps;
using Serilog;

namespace Casiland.Systems.ProceduralGen;

using Fractural.Tasks;
using Godot;

public partial class ProceduralRoomGenerator : Node
{
    [GeneratedRegex("[a-z][A-Z]")]
    private static partial Regex PascalCaseRegex();

    private static string ToSentenceCase(string str) =>
        PascalCaseRegex().Replace(str, m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");

    public GenerationState ResultingState;


    public GenerationState _currentState;
    public ulong _seed;

    [Signal]
    public delegate void GenerationFinishedEventHandler();

    public async Task PerformGeneration(
        ProceduralGenerationSettings generationSettings,
        GenerationState state)
    {
        _currentState = state;
        _seed = state.Rng.Seed;
        List<GenerationStep> generationSteps =
        [
            new PlaceRoomsStep(state, generationSettings),
            new PickRoomsStep(state, generationSettings),
            new GenerateConnectionsStep(state, generationSettings),
            new PlaceCorridorsStep(state, generationSettings),
            new PlaceRoomTilesStep(state, generationSettings),
            // new PerformAutoTileStep(state, generationSettings),
        ];

        Log.Information("Starting procedural generation with {StepCount} steps!", generationSteps.Count);
        var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();

        foreach (var step in generationSteps)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            await step.Perform();

            Log.Debug("> Finished {Description} in {ElapsedTotalMilliseconds} ms", step.StateDescription.ToLower(), stopwatch.Elapsed.TotalMilliseconds);
            await GDTask.DelayFrame(1);
        }


        Log.Information("Finished procedural generation in {Duration} ms!",
            totalStopwatch.Elapsed.TotalMilliseconds);


        ResultingState = state;

        EmitSignalGenerationFinished();
    }
}