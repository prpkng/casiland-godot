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

    [ExportGroup("Movement", "movement")]
    [Export] public float MovementSpeed;
    [Export] public float MovementAcceleration;
    [Export] public float MovementDeceleration;

    public Vector2 MovementInput;

    public override string[] _GetConfigurationWarnings()
    {
        if (CharacterBody == null)
            return ["TopDownMovement needs a ChracterBody2D to work properly"];
        return [];
    }

    private StateMachine _fsm;
    
    public override void _Ready()
    {
        _fsm = new StateMachine();
        
        _fsm.AddState("idle", null, OnIdle);
        _fsm.AddState("move", null, OnMove);
        
        _fsm.Init();
    }

    private void OnIdle(State<string, string> state)
    {
        Log.Information("idle");
    }
    private void OnMove(State<string, string> state)
    {
        
    }

    public override void _PhysicsProcess(double delta)
    {
        _fsm.OnLogic();
    }
}