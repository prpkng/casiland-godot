using System;
using System.Collections.Generic;
using System.Linq;
using Casiland.AutoTileEx.Api;
using Casiland.AutoTileEx.Api.Data;
using Godot;
using HCoroutines;

namespace Casiland.AutoTileEx.Editor;

[Tool]
[GlobalClass]
public partial class AutoTileEditor : Control
{
    public AutoTileRuleSet CurrentEditingRuleSet;
    private TilesetView _tilesetView;
    private Button _sourceTileBtn;
    private Button _targetTileBtn;
    private TileSet _tileset;

    private RightClickableButton[] _neighbourButtons;

    private Texture2D _nothingNeighbourIcon;
    private Texture2D _matchNeighbourIcon;
    private Texture2D _anyNeighbourIcon;
    
    public EditorUndoRedoManager UndoRedoManager;

    private VBoxContainer _rulesVboxContainer;
    private Button _addRuleButton;
    private Button _removeRuleButton;
    private Button _moveUpButton;
    private Button _moveDownButton;
    private List<Button> _ruleButtons;
    private List<Action> _ruleButtonActions;
    private List<SpinBox> _ruleModuloSettings; // x, xoffset, y, yoffset
    
    /// Nothing, Ignore, Match and Any
    private Button[] _toolbarSelectors;

    private NeighbourType _selectedNbType;
    private int _currentRuleIndex;
    
    
    public override void _Ready()
    {
        _tilesetView = GetNode("HBox/ScrollContainer/TilesetView") as TilesetView;
        _sourceTileBtn = GetNode<Button>("HBox/HSplitContainer/RulePanel/BG/VBox/TargetEditor/SourceTileBtn");
        _targetTileBtn = GetNode<Button>("HBox/HSplitContainer/RulePanel/BG/VBox/TargetEditor/TargetTileBtn");

        _nothingNeighbourIcon = GD.Load<Texture2D>("res://addons/AutoTileEx/Editor/Icons/ban.svg");
        _matchNeighbourIcon = GD.Load<Texture2D>("res://addons/AutoTileEx/Editor/Icons/circle-check.svg");
        _anyNeighbourIcon = GD.Load<Texture2D>("res://addons/AutoTileEx/Editor/Icons/circle-question-mark.svg");
        _sourceTileBtn.Pressed += OnSourceTilePressed;
        _targetTileBtn.Pressed += OnTargetTilePressed;

        _neighbourButtons = GetNode("HBox/HSplitContainer/RulePanel/BG/VBox/Control/GridContainer").GetChildren()
            .Select(c => c as RightClickableButton).ToArray();

        for (int i = 0; i < _neighbourButtons.Length; i++)
        {
            int idx = i;
            _neighbourButtons[i].Pressed += () => OnNeighbourButtonClicked(idx);
            _neighbourButtons[i].RightClicked += () => OnNeighbourButtonRightClicked(idx);
        }

        _toolbarSelectors =
        [
            GetNode<Button>("HBox/HSplitContainer/RulePanel/BG/VBox/Toolbar/HBoxContainer/NothingSelector"),
            GetNode<Button>("HBox/HSplitContainer/RulePanel/BG/VBox/Toolbar/HBoxContainer/IgnoreSelector"),
            GetNode<Button>("HBox/HSplitContainer/RulePanel/BG/VBox/Toolbar/HBoxContainer/MatchSelector"),
            GetNode<Button>("HBox/HSplitContainer/RulePanel/BG/VBox/Toolbar/HBoxContainer/AnySelector"),
        ];

        for (int i = 0; i < _toolbarSelectors.Length; i++)
        {
            int idx = i;
            _toolbarSelectors[i].Pressed += () => OnNbToolbarButtonClicked(idx);
        }

        _rulesVboxContainer = GetNode<VBoxContainer>("HBox/HSplitContainer/RulesPanel/BG/ScrollContainer/VBox");

        _addRuleButton = GetNode<Button>("HBox/HSplitContainer/RulesPanel/BG/HBoxContainer/AddRule");
        _addRuleButton.Icon = GetThemeIcon("Add", "EditorIcons");
        _removeRuleButton = GetNode<Button>("HBox/HSplitContainer/RulesPanel/BG/HBoxContainer/RemoveRule");
        _removeRuleButton.Icon = GetThemeIcon("GuiClose", "EditorIcons");
        _moveUpButton = GetNode<Button>("HBox/HSplitContainer/RulesPanel/BG/HBoxContainer/MoveUp");
        _moveUpButton.Icon = GetThemeIcon("ArrowUp", "EditorIcons");
        _moveDownButton = GetNode<Button>("HBox/HSplitContainer/RulesPanel/BG/HBoxContainer/MoveDown");
        _moveDownButton.Icon = GetThemeIcon("ArrowDown", "EditorIcons");

        _ruleModuloSettings =
        [
            GetNode<SpinBox>("HBox/HSplitContainer/RulePanel/BG/VBox/ModuloSettings/X"),
            GetNode<SpinBox>("HBox/HSplitContainer/RulePanel/BG/VBox/ModuloSettings/XOffset"),
            GetNode<SpinBox>("HBox/HSplitContainer/RulePanel/BG/VBox/ModuloSettings/Y"),
            GetNode<SpinBox>("HBox/HSplitContainer/RulePanel/BG/VBox/ModuloSettings/YOffset"),
        ];
        
        foreach (var spinBox in _ruleModuloSettings)
            spinBox.ValueChanged += OnModuloChanged;

        _addRuleButton.Pressed += OnAddRulePressed;
        _removeRuleButton.Pressed += OnRemoveRulePressed;
        _moveUpButton.Pressed += OnMoveUpPressed;
        _moveDownButton.Pressed += OnMoveDownPressed;
        
    }

    private void OnModuloChanged(double value)
    {
        var rule = CurrentEditingRuleSet.Rules[_currentRuleIndex];
        rule.ModuloX = (int)_ruleModuloSettings[0].Value;
        rule.ModuloXOffset = (int)_ruleModuloSettings[1].Value;
        rule.ModuloY = (int)_ruleModuloSettings[2].Value;
        rule.ModuloYOffset = (int)_ruleModuloSettings[3].Value;
    }

    private void RecomputeModuloSettings()
    {
        var rule = CurrentEditingRuleSet.Rules[_currentRuleIndex];
        _ruleModuloSettings[0].Value = rule.ModuloX;
        _ruleModuloSettings[1].Value = rule.ModuloXOffset;
        _ruleModuloSettings[2].Value = rule.ModuloY;
        _ruleModuloSettings[3].Value = rule.ModuloYOffset;
    }

    private void OnAddRulePressed()
    {
        var newRule = new AutoTileRule();
        newRule.Neighbours[24] = NeighbourType.Required;
        var currentRule = CurrentEditingRuleSet.Rules[_currentRuleIndex];
        CurrentEditingRuleSet.Rules.Add(newRule);
        RecomputeRuleButtons();
    }
    private void OnRemoveRulePressed() 
    {
        CurrentEditingRuleSet.Rules.RemoveAt(_currentRuleIndex);
        if (_currentRuleIndex >= CurrentEditingRuleSet.Rules.Count - 1)
            _currentRuleIndex--;
        RecomputeTileButtons();
        RecomputeNeighbourButtons();
        RecomputeRuleButtons();
        RecomputeModuloSettings();
    }
    private void OnMoveUpPressed()
    {
        if (_currentRuleIndex == 0) return;
        var rules = CurrentEditingRuleSet.Rules;
        var rule = rules[_currentRuleIndex];
        rules.RemoveAt(_currentRuleIndex);
        _currentRuleIndex--;
        rules.Insert(_currentRuleIndex, rule);
        RecomputeRuleButtons();
    }
    private void OnMoveDownPressed()
    {
        var rules = CurrentEditingRuleSet.Rules;
        if (_currentRuleIndex >= rules.Count-1) return;
        var rule = rules[_currentRuleIndex];
        rules.RemoveAt(_currentRuleIndex);
        _currentRuleIndex++;
        rules.Insert(_currentRuleIndex, rule);
        RecomputeRuleButtons();
    }

    ~AutoTileEditor()
    {
        _sourceTileBtn.Pressed -= OnSourceTilePressed;
        _targetTileBtn.Pressed -= OnTargetTilePressed;
    }

    public void ShowElements()
    {
        RecomputeRuleButtons();
        RecomputeNeighbourButtons();
        RecomputeTileButtons();
        RecomputeModuloSettings();
    }
    
    public void RecomputeRuleMasks() => CurrentEditingRuleSet.RecomputeAllRuleMasks();

    
    public void RecomputeRuleButtons()
    {
        foreach (var c in _rulesVboxContainer.GetChildren())
            c.Free();

        _ruleButtons = [];
        _ruleButtonActions = [];

        var packedScene = GD.Load<PackedScene>("res://addons/AutoTileEx/Editor/RuleSet.tscn");
        
        for (int i = 0; i < CurrentEditingRuleSet.Rules.Count; i++)
        {
            int idx = i;
            var action = () => RuleBtnPressed(idx);
            var btn = packedScene.Instantiate<Button>();
            var rule = CurrentEditingRuleSet.Rules[i];
            var sourceTileSource = (_tileset.GetSource(rule.TargetTileSourceId) as TileSetAtlasSource);
            var sourceTexture = new AtlasTexture
            {
                Atlas = sourceTileSource?.Texture,
                Region = sourceTileSource?.GetTileTextureRegion(rule.TargetTileCoords) ?? new Rect2()
            };
            btn.GetNode<TextureRect>("Icon").Texture = sourceTexture;
            btn.GetNode<Label>("Label").Text = $"Rule {idx+1}";
            btn.Pressed += action;
            btn.ButtonPressed = i == _currentRuleIndex;
            _ruleButtonActions.Add(action);
            _ruleButtons.Add(btn);
            _rulesVboxContainer.AddChild(btn);
        }
        RecomputeRuleMasks();
    }

    private void RuleBtnPressed(int index)
    {
        GD.Print($"Rule Btn pressed {index}");
        _currentRuleIndex = index;
        for (int i = 0; i < _ruleButtons.Count; i++)
            _ruleButtons[i].ButtonPressed = i == _currentRuleIndex;
        
        RecomputeNeighbourButtons();
        RecomputeTileButtons();
        RecomputeModuloSettings();
    }

    public void UpdateTileset(TileSet tileSet)
    {
        _tileset = tileSet;
        _tilesetView.TileSet = tileSet;
    }

    private void OnSourceTilePressed()
    {
        var last = new TileInfo(CurrentEditingRuleSet.SourceTileCoords, CurrentEditingRuleSet.SourceTileSourceId);
        _tilesetView.StartTilePicking(last, tileInfo =>
        {
            if (tileInfo == null || !tileInfo.Value.IsValid()) return;

            UndoRedoManager.CreateAction($"Set rule {_currentRuleIndex} source tile");
            UndoRedoManager.AddUndoProperty(CurrentEditingRuleSet, AutoTileRuleSet.PropertyName.SourceTileSourceId, CurrentEditingRuleSet.SourceTileSourceId);
            UndoRedoManager.AddDoProperty(CurrentEditingRuleSet, AutoTileRuleSet.PropertyName.SourceTileSourceId, tileInfo.Value.Source);
            UndoRedoManager.AddUndoProperty(CurrentEditingRuleSet, AutoTileRuleSet.PropertyName.SourceTileCoords, CurrentEditingRuleSet.SourceTileCoords);
            UndoRedoManager.AddDoProperty(CurrentEditingRuleSet, AutoTileRuleSet.PropertyName.SourceTileCoords, tileInfo.Value.Pos);
            UndoRedoManager.AddDoMethod(this, MethodName.RecomputeTileButtons);
            UndoRedoManager.AddUndoMethod(this, MethodName.RecomputeTileButtons);
            
            UndoRedoManager.CommitAction();
            
            RecomputeTileButtons();
        });
    }

    private void OnTargetTilePressed()
    {
        var rule = CurrentEditingRuleSet.Rules[_currentRuleIndex];
        var last = new TileInfo(rule.TargetTileCoords, rule.TargetTileSourceId);
        _tilesetView.StartTilePicking(last, tileInfo =>
        {
            if (tileInfo == null || !tileInfo.Value.IsValid()) return;

            UndoRedoManager.CreateAction($"Set rule {_currentRuleIndex} source tile");
            UndoRedoManager.AddUndoProperty(rule, AutoTileRule.PropertyName.TargetTileSourceId, rule.TargetTileSourceId);
            UndoRedoManager.AddDoProperty(rule, AutoTileRule.PropertyName.TargetTileSourceId, tileInfo.Value.Source);
            UndoRedoManager.AddUndoProperty(rule, AutoTileRule.PropertyName.TargetTileCoords, rule.TargetTileCoords);
            UndoRedoManager.AddDoProperty(rule, AutoTileRule.PropertyName.TargetTileCoords, tileInfo.Value.Pos);
            UndoRedoManager.AddDoMethod(this, MethodName.RecomputeTileButtons);
            UndoRedoManager.AddUndoMethod(this, MethodName.RecomputeTileButtons);
            
            UndoRedoManager.CommitAction();
            rule.TargetTileSourceId = tileInfo.Value.Source;
            rule.TargetTileCoords = tileInfo.Value.Pos;
            
            RecomputeTileButtons();
        });
    }


    private void RecomputeTileButtons()
    {
        var rule = CurrentEditingRuleSet.Rules[_currentRuleIndex];

        var sourceTileSource = _tileset.GetSource(CurrentEditingRuleSet.SourceTileSourceId) as TileSetAtlasSource;
        var sourceTexture = new AtlasTexture
        {
            Atlas = sourceTileSource?.Texture,
            Region = sourceTileSource?.GetTileTextureRegion(CurrentEditingRuleSet.SourceTileCoords) ?? new Rect2()
        };
        _sourceTileBtn.Icon = sourceTexture;
        // _middleNeighboursBtn.Icon = sourceTexture;
        
        var targetTileSource = _tileset.GetSource(rule.TargetTileSourceId) as TileSetAtlasSource;
        var targetTexture = new AtlasTexture
        {
            Atlas = targetTileSource?.Texture,
            Region = targetTileSource?.GetTileTextureRegion(rule.TargetTileCoords) ?? new Rect2()
        };
        _targetTileBtn.Icon = targetTexture;
        
        _ruleButtons[_currentRuleIndex].GetNode<TextureRect>("Icon").Texture = targetTexture;
        RecomputeRuleMasks();
    }

    private void RecomputeNeighbourButtons()
    {
        var rule = CurrentEditingRuleSet.Rules[_currentRuleIndex];

        while (rule.Neighbours.Count < 8)
            rule.Neighbours.Add(NeighbourType.Ignore);
        for (int i = 0; i < _neighbourButtons.Length; i++)
        {
            var texture = rule.Neighbours[i] switch
            {
                NeighbourType.Ignore => null,
                NeighbourType.Nothing => _nothingNeighbourIcon,
                NeighbourType.Required => _matchNeighbourIcon,
                NeighbourType.Other => _anyNeighbourIcon,
                _ => throw new ArgumentOutOfRangeException()
            };
            _neighbourButtons[i].Icon = texture;
        }
        RecomputeRuleMasks();
        
    }

    private void OnNeighbourButtonRightClicked(int index)
    {
        var rule = CurrentEditingRuleSet.Rules[_currentRuleIndex];
        var lastType = rule.Neighbours[index];
        var newType = NeighbourType.Ignore;
        UndoRedoManager.CreateAction($"Set neighbour {index} to {Enum.GetName(newType)}");
        
        UndoRedoManager.AddDoMethod(this, MethodName.SetNeighbour, rule, index, (int)newType);
        UndoRedoManager.AddUndoMethod(this, MethodName.SetNeighbour, rule, index, (int)lastType);
        
        UndoRedoManager.AddDoMethod(this, MethodName.RecomputeNeighbourButtons);
        UndoRedoManager.AddUndoMethod(this, MethodName.RecomputeNeighbourButtons);
        
        UndoRedoManager.CommitAction();
        

        RecomputeNeighbourButtons();
    }
    private void OnNeighbourButtonClicked(int index)
    {
        var rule = CurrentEditingRuleSet.Rules[_currentRuleIndex];
        var lastType = rule.Neighbours[index];
        var newType = _selectedNbType;
        UndoRedoManager.CreateAction($"Set neighbour {index} to {Enum.GetName(newType)}");
        
        UndoRedoManager.AddDoMethod(this, MethodName.SetNeighbour, rule, index, (int)newType);
        UndoRedoManager.AddUndoMethod(this, MethodName.SetNeighbour, rule, index, (int)lastType);
        
        UndoRedoManager.AddDoMethod(this, MethodName.RecomputeNeighbourButtons);
        UndoRedoManager.AddUndoMethod(this, MethodName.RecomputeNeighbourButtons);
        
        UndoRedoManager.CommitAction();
        

        RecomputeNeighbourButtons();
    }

    private void OnNbToolbarButtonClicked(int index)
    {
        _selectedNbType = index switch
        {
            0 => NeighbourType.Nothing,
            1 => NeighbourType.Ignore,
            2 => NeighbourType.Required,
            3 => NeighbourType.Other,
            _ => NeighbourType.Nothing
        };

        for (int i = 0; i < _toolbarSelectors.Length; i++)
        {
            _toolbarSelectors[i].ButtonPressed = i == index;
        }
    }

    private void SetNeighbour(AutoTileRule rule, int index, int value) => rule.Neighbours[index] = (NeighbourType)value;
}