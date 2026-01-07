using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Casiland.AutoTileEx.Api;
using Fractural.Tasks;
using HCoroutines;

namespace Casiland.AutoTileEx.Editor;




[Tool]
[GlobalClass]
public partial class TilesetView : Control
{
	private TileSet _tileset;
	[Export] public TileSet TileSet
	{
		get => _tileset;
		set => RefreshTileset(value);
	}
	private Texture2D _checkerboard;
	private Rid _backgroundCanvasItem;
	private Dictionary<Material, Rid> _canvasItemPerMaterial = [];

	private Vector2 _tilesSize;
	private Vector2 _gridSize;
	private List<int> _disabledSources = [];
	private TileInfo _currentlySelectedTile;

	private TileInfo? _clickedTile;
	private TileInfo? _lastClickedTile;
	private bool _isPerformingSelection;

	private float _currentZoomLevel = 1f;
	private float ZoomLevel
	{
		get => _currentZoomLevel;
		set => OnZoomValueChanged(value);
	}

	public override void _EnterTree()
	{
		_currentZoomLevel = 1f;
		_backgroundCanvasItem = RenderingServer.CanvasItemCreate();
		RenderingServer.CanvasItemSetParent(_backgroundCanvasItem, GetCanvasItem());
		RenderingServer.CanvasItemSetDrawBehindParent(_backgroundCanvasItem, true);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		RenderingServer.FreeRid(_backgroundCanvasItem);
		foreach (var ci in _canvasItemPerMaterial.Values) 
			RenderingServer.FreeRid(ci);
		_canvasItemPerMaterial.Clear();
	}

	public override void _Ready()
	{
		base._Ready();
		_checkerboard = GetThemeIcon("Checkerboard", "EditorIcons");

		FocusExited += OnFocusExited;
	}

	private void RefreshTileset(TileSet ts)
	{
		GD.Print("Refreshing tileset");
		if (ts == null) return;
		GD.Print("Tileset valid");
	
		
		_tileset = ts;

		_tilesSize = Vector2.Zero;
		_disabledSources = _disabledSources.Where(ts.HasSource).ToList();

		for (int i = 0; i < ts.GetSourceCount(); i++)
		{
			var sourceId = ts.GetSourceId(i);
			var source = ts.GetSource(sourceId) as TileSetAtlasSource;
			if (source?.Texture == null) continue;

			_tilesSize.X = Mathf.Max(_tilesSize.X, source.Texture.GetWidth());
			_tilesSize.Y += source.Texture.GetHeight();

			_gridSize = source.TextureRegionSize;

		}

		OnZoomValueChanged(_currentZoomLevel);
	}

	private bool IsTileInSource(TileSetAtlasSource source, Vector2I coord)
	{
		var origin = source.GetTileAtCoords(coord);
		if (origin == -Vector2I.One) return false;

		var size = source.GetTileSizeInAtlas(origin);
		return coord.X < origin.X + size.X && coord.Y < origin.Y + size.Y;
	}


	private float timestamp;
	private void OnZoomValueChanged(float value)
	{
		_currentZoomLevel = value;
		var minSize = new Vector2(_currentZoomLevel * _tilesSize.X, _currentZoomLevel * _tilesSize.Y);
		CustomMinimumSize = minSize;

		var now = Time.GetTicksMsec();
		CallDeferred("queue_redraw");
		
		timestamp = now;
		
	}

	private Rid GetTileCanvasItem(TileData td)
	{
		return GetCanvasItem();
		if (td.Material == null)
			return GetCanvasItem();
		if (_canvasItemPerMaterial.TryGetValue(td.Material, out var mat))
			return mat;

		var rid = RenderingServer.CanvasItemCreate();
		RenderingServer.CanvasItemSetMaterial(rid, td.Material.GetRid());
		RenderingServer.CanvasItemSetParent(rid, GetCanvasItem());
		RenderingServer.CanvasItemSetDrawBehindParent(rid, true);
		RenderingServer.CanvasItemSetDefaultTextureFilter(rid, RenderingServer.CanvasItemTextureFilter.Nearest);
		_canvasItemPerMaterial[td.Material] = rid;
		return rid;
	}
	
	private void DrawTileData(Texture2D texture,
		Rect2 rect,
		Rect2 srcRect,
		TileData td,
		bool drawSides = true)
	{
		var flippedRect = rect;
		var size = flippedRect.Size;
		if (td.FlipH)
			size.X = -rect.Size.X;
		if (td.FlipV)
			size.Y = -rect.Size.Y;
		flippedRect.Size = size;
		
		
		RenderingServer.CanvasItemAddTextureRectRegion(
			GetTileCanvasItem(td),
			flippedRect,
			texture.GetRid(),
			srcRect,
			td.Modulate,
			td.Transpose
			);
		
		
	}

	public override void _Draw()
	{
		if (_tileset == null) return;
		
		
		RenderingServer.CanvasItemClear(_backgroundCanvasItem);
		foreach (var ci in _canvasItemPerMaterial.Values)
			RenderingServer.CanvasItemClear(ci);
			

		var offset = Vector2.Zero;

		for (int i = 0; i < _tileset.GetSourceCount(); i++)
		{
			var sourceId = _tileset.GetSourceId(i);
			if (_disabledSources.Contains(sourceId)) continue;
			var source = _tileset.GetSource(sourceId) as TileSetAtlasSource;
			if (source?.Texture == null) continue;
			
			RenderingServer.CanvasItemAddTextureRect(
				_backgroundCanvasItem,
				new Rect2(offset, _currentZoomLevel * source.Texture.GetSize()),
				_checkerboard.GetRid(),
				true);

			for (int t = 0; t < source.GetTilesCount(); t++)
			{
				var coord = source.GetTileId(t);
				var rect = source.GetTileTextureRegion(coord, 0);
				var targetRect = new Rect2(offset + _currentZoomLevel * (Vector2)rect.Position,
					_currentZoomLevel * (Vector2)rect.Size);

				var td = source.GetTileData(coord, 0);

				DrawTileData(source.Texture, targetRect, rect, td);

				if (_isPerformingSelection && _currentlySelectedTile.Pos == coord 
				    && _currentlySelectedTile.Source == sourceId)
				{
					DrawRect(targetRect, Colors.White, false);
					DrawRect(targetRect, new Color(1f, 1f, 1f, 0.2f));
				}
				
				if (_isPerformingSelection && _lastClickedTile?.Pos == coord && _lastClickedTile?.Source == sourceId)
				{
					DrawRect(targetRect, Colors.White, false);
					DrawRect(targetRect, new Color(1f, 1f, 1f, 0.5f));
				}

			}
			offset.Y += _currentZoomLevel * source.Texture.GetHeight();
		}
		
		if (!_isPerformingSelection)
			DrawRect(new Rect2(Vector2.Zero, Size), new Color(0, 0, 0, 0.8f));
		
	}

	private TileInfo GetTileAtPosition(Vector2 mousePos)
	{
		if (_tileset == null) return new TileInfo(-Vector2I.One, -1);

		var offset = Vector2.Zero;

		for (int i = 0; i < _tileset.GetSourceCount(); i++)
		{
			var sourceId = _tileset.GetSourceId(i);
			if (_disabledSources.Contains(sourceId)) continue;
			var source = _tileset.GetSource(sourceId) as TileSetAtlasSource;
			if (source?.Texture == null) continue;
			for (int t = 0; t < source.GetTilesCount(); t++)
			{
				var coord = source.GetTileId(t);
				var rect = source.GetTileTextureRegion(coord, 0);
				var targetRect = new Rect2(offset + _currentZoomLevel * (Vector2)rect.Position,
					_currentZoomLevel * (Vector2)rect.Size);

				if (!targetRect.HasPoint(mousePos))
					continue;

				return new TileInfo(coord, sourceId);
			}
			offset.Y += _currentZoomLevel * source.Texture.GetHeight();
		}

		return new TileInfo(-Vector2I.One, -1);
	}


	private Vector2 _currentMousePosition;
	public override void _GuiInput(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventMouseButton { ButtonIndex: MouseButton.WheelUp } mb when (mb.CtrlPressed || mb.MetaPressed):
				AcceptEvent();
				ZoomLevel *= 1.1f;
				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.WheelDown } mb
				when (mb.CtrlPressed || mb.MetaPressed):
				AcceptEvent();
				ZoomLevel /= 1.1f;
				break;
			case InputEventMouseMotion ev:
				var prevPosition = _currentMousePosition;
				_currentMousePosition = ev.Position;
				var tile = GetTileAtPosition(_currentMousePosition);
				if (tile != _currentlySelectedTile)
				{
					_currentlySelectedTile = tile;
					QueueRedraw();
				}

				break;
			case InputEventMouseButton { ButtonIndex: MouseButton.Left } when _isPerformingSelection:
				if (_currentlySelectedTile.IsValid())
					_clickedTile = _currentlySelectedTile;
				QueueRedraw();
				break;
				
				
		}
	}

	private void OnFocusExited()
	{
		_isPerformingSelection = false;
	}
	
	
	
	#region PUBLIC API

	public async void StartTilePicking(TileInfo lastSelected, Action<TileInfo?> onFinished)
	{
		GrabFocus();
		_clickedTile = null;
		_lastClickedTile = lastSelected;
		_isPerformingSelection = true;
		GD.Print("Starting tile picking");
		QueueRedraw();
		while (_clickedTile == null && _isPerformingSelection)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		
		GD.Print("Finished tile picking");

		_isPerformingSelection = false;
		QueueRedraw();
		
		onFinished(_clickedTile);
	} 
	
	
	#endregion
	
}
