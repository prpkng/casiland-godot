using System.Collections.Generic;
using Godot;
using Serilog;

namespace Casiland.Entities.Weapons.WeaponSystem;

[GlobalClass]
public partial class WeaponManager : Node2D
{
    [Export] public NodePath WeaponSocketPath;
    [Export] public PackedScene WeaponScene;

    private IAimProvider _aimProvider;
    private Node2D _weaponSocket;
    private Weapon _currentWeapon;

    private readonly List<WeaponData> _inventory = [];

    private Vector2 GetAimDirection() => _aimProvider?.GetAimDirection() ?? Vector2.Right;


    public override void _Ready()
    {
        _weaponSocket = GetNode<Node2D>(WeaponSocketPath);
        _aimProvider = GetParent() as IAimProvider;

        if (_aimProvider == null)
            Log.Error("WeaponManager parent doesn't implement IAimProvider!\nWeapons will not be able to aim properly!");
    }

    public override void _Process(double delta)
    {
        if (_currentWeapon == null) return;
        
        // Input Forwarding
        if (Input.IsActionJustPressed("fire"))
            _currentWeapon.PrimaryPressed();
        
        if (Input.IsActionJustReleased("fire"))
            _currentWeapon.PrimaryReleased(); 
    }

    public async void EquipWeapon(WeaponData data)
    {
        UnequipCurrent();

        _currentWeapon = WeaponScene.Instantiate<Weapon>();
        _currentWeapon.GetAimDirection = GetAimDirection;
        _currentWeapon.Data = data;
        _weaponSocket.AddChild(_currentWeapon);
        
        _currentWeapon.OnEquip(GetParent());
    }

    public void UnequipCurrent()
    {
        if (_currentWeapon == null) return;

        _currentWeapon.OnUnequip(); 
        _currentWeapon.QueueFree(); 
        _currentWeapon = null; 
    }

    /* =============
     * = INVENTORY =
     * =============*/
    public void AddWeapon(WeaponData data)
    {
        _inventory.Add(data);

        if (_currentWeapon == null)
            EquipWeapon(data);
    }

    public void EquipNext()
    {
        if (_inventory.Count == 0)
            return;

        int index = _inventory.IndexOf(_currentWeapon.Data);
        int next = (index + 1) % _inventory.Count;
        
        EquipWeapon(_inventory[next]);
    }
}