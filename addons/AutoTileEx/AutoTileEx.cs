#if TOOLS
using Godot;
using System;
using System.IO;
using Casiland.AutoTileEx.Api.Data;
using Casiland.AutoTileEx.Editor;
using Serilog;

namespace Casiland.AutoTileEx;

[Tool]
public partial class AutoTileEx : EditorPlugin
{
	private AutoTileEditor _dockControl;
	private const string DockPath = "res://addons/AutoTileEx/Editor/Dock.tscn";
	private EditorDock _dock;
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.

		_dockControl = ResourceLoader.Load<PackedScene>(DockPath).Instantiate<AutoTileEditor>();
		_dockControl.UndoRedoManager = GetUndoRedo();
		_dock = new EditorDock
		{
			Title = "Auto Tile Rules",
			DockIcon = _dockControl.GetThemeIcon("TileMap", "EditorIcons"),
			DefaultSlot = EditorDock.DockSlot.Bottom,
			AvailableLayouts = EditorDock.DockLayout.Horizontal
		};
		_dock.AddChild(_dockControl);
		AddDock(_dock);
		_dock.Close();

	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveDock(_dock);
		_dock = null;
		_dockControl = null;
	}

	public override bool _Handles(GodotObject @object)
	{
		return @object is AutoTileRuleSet;
	}

	public override void _Edit(GodotObject @object)
	{
		// if (@object == null) 
		// 	_dock.Close();
		if (@object is not AutoTileRuleSet rs) return;

		_dockControl.CurrentEditingRuleSet = rs;
		_dockControl.UpdateTileset(rs.tileSet);
		_dockControl.ShowElements();
	}

	public override void _MakeVisible(bool visible)
	{
		if (visible) _dock.MakeVisible();
		else _dock.Close();
	}
}
#endif
