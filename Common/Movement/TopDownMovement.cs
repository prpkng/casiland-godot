using Godot;
using Godot.Collections;
using TriangleNet;
using UnityHFSM;
using Log = Serilog.Log;

namespace Casiland.Common.Movement;

[GlobalClass]
public partial class TopDownMovement : Node
{
    [Export] public CharacterBody2D CharacterBody
    {
        get => _body;
        set
        {
            _body = value;
            UpdateConfigurationWarnings();
        }
    }
    private CharacterBody2D _body;

    [ExportGroup("Movement", "Movement")]
    [Export] public float MovementSpeed;
    [Export] public float MovementAcceleration = 0.6f;
    [Export] public float MovementDeceleration = 0.9f;

    public Vector2 MovementInput;

    public override string[] _GetConfigurationWarnings()
    {
        if (CharacterBody == null)
            return ["TopDownMovement needs a CharacterBody2D to work properly"];
        return [];
    }

    private StateMachine _fsm;
    
    public override void _Ready()
    {
        _fsm = new StateMachine();
        
        _fsm.AddState("idle", null, OnIdle);
        _fsm.AddState("move", null, OnMove);

        _fsm.AddTwoWayTransition("idle", "move", _ => MovementInput.Length() > Mathf.Epsilon);
        
        _fsm.Init();
    }

    private void MovementStep(Vector2 targetVector)
    {
        var speedDiff = targetVector - CharacterBody.Velocity;
        float accelRate = targetVector.Length() == 0 ? MovementDeceleration : MovementAcceleration;
        CharacterBody.Velocity += speedDiff * accelRate;
    }

    private void OnIdle(State<string, string> state)
    {
        MovementStep(Vector2.Zero);
    }
    private void OnMove(State<string, string> state)
    {
        MovementStep(MovementInput * MovementSpeed);
    }

    public override void _PhysicsProcess(double delta)
    { 
        _fsm.OnLogic();
        CharacterBody.MoveAndSlide();
    }
}