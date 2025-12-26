using System.Collections.Generic;
using System.Text.RegularExpressions;
using Casiland.Systems.ProceduralGen.Steps;
using Serilog;

namespace Casiland.Systems.ProceduralGen;

using Godot;

public partial class ProceduralRoomGenerator : Node
{
    [GeneratedRegex("[a-z][A-Z]")]
    private static partial Regex PascalCaseRegex();
    private static string ToSentenceCase(string str) => 
        PascalCaseRegex().Replace(str, m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");

    public GenerationState ResultingState;

    [Signal]
    public delegate void GenerationFinishedEventHandler();

    public void PerformGeneration(ProceduralGenerationSettings generationSettings, string seed = "default seed prpkng")
    {
        var state = new GenerationState
        {
            Rng = new RandomNumberGenerator
            {
                Seed = seed.Hash()
            }
        };

        List<GenerationStep> generationSteps =
        [
            new PlaceRoomsStep(state, generationSettings),
            new PickRoomsStep(state, generationSettings),
            new GenerateConnectionsStep(state, generationSettings),
            new PlaceCorridorsStep(state, generationSettings)
        ];
        
        Log.Information("Starting procedural generation with {StepCount} steps!", generationSteps.Count);
        var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();

        foreach (var step in generationSteps)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            step.Perform();
            
            Log.Debug("> Finished {StepName} in {Duration} ms", ToSentenceCase(step.GetType().Name), stopwatch.Elapsed.TotalMilliseconds);
        }
        
        
        Log.Information("Finished procedural generation in {Duration} ms!", totalStopwatch.Elapsed.TotalMilliseconds);

        ResultingState = state;
        
        EmitSignalGenerationFinished();
    }

}
