namespace Casiland.Systems.ProceduralGen.Steps;

public abstract class GenerationStep(GenerationState state, ProceduralGenerationSettings settings)
{
    protected GenerationState State { get; private set; } = state;
    protected ProceduralGenerationSettings Settings { get; private set; } = settings;


    public abstract void Perform();
}

