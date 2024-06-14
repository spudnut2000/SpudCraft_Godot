using Godot;
using System;
using System.Linq;
using InGameConsole;
using SpudCraftGodot.assets.scripts;
using SpudCraftGodot.assets.scripts.resources;

public partial class Player : CharacterBody3D
{
	[Export] public Node3D Head;
	[Export] public Camera3D Camera;
	[Export] public RayCast3D RayCast;
	[Export] public MeshInstance3D BlockHighlight;

	[Export] public Label FpsLabel;
	[Export] public Label CoordsLabel;
	[Export] public Label LookingAtLabel;
	[Export] public Label SelectedBlockLabel;
	[Export] public GameMode GameMode { get; set; } = GameMode.Survival;
	
	[Export] private float _mouseSensitivity = 0.001f;
	[Export] private float _movementSpeed = 15f;
	[Export] private float _jumpVelocity = 15f;

	public Tool CurrentTool = new Tool() { Name = "Pickaxe", ToolType = ToolType.Shovel, Efficiency = 10 };

	private float _cameraXRotation;
	private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	
	private string _selectedBlock = "dirt";
	private int _selectedBlockIndex = 0;
	
	private bool _isBreakingBlock = false;
	private Timer _breakTimer = new Timer();
	
	public override void _Ready()
	{
		World.Instance.Player = this;
		//Input.MouseMode = Input.MouseModeEnum.Captured;
		AddChild(_breakTimer);
	}

	public override void _Process(double delta)
	{
		HandleRaycast();
		FpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
		CoordsLabel.Text = $"Coords: {(Vector3I)GlobalPosition}";
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleMovement(delta);
		CycleBlocks();
		
		SelectedBlockLabel.Text = $"Selected block: {_selectedBlock}";
	}
	

	public override void _Input(InputEvent @event)
	{
		HandleCameraRotation(@event);
	}

	private void CycleBlocks()
	{
		var blocksArray = BlockRegistry.Blocks.Keys.ToArray();
		if (Input.IsActionJustPressed("cycle_block"))
		{
			_selectedBlockIndex += 1;
			if (_selectedBlockIndex >= blocksArray.Length)
			{
				_selectedBlockIndex = 0;
			}
			_selectedBlock = blocksArray[_selectedBlockIndex];
		}
	}

	private void HandleCameraRotation(InputEvent @event)
	{
		if (@event is InputEventMouseMotion)
		{
			var mouseMotion = @event as InputEventMouseMotion;
			var deltaX = mouseMotion.Relative.Y * _mouseSensitivity;
			var deltaY = -mouseMotion.Relative.X * _mouseSensitivity;
			
			Head.RotateY(Mathf.DegToRad(deltaY));
			if (_cameraXRotation + deltaX > -90 && _cameraXRotation + deltaX < 90)
			{
				Camera.RotateX(Mathf.DegToRad(-deltaX));
				_cameraXRotation += deltaX;
			}
		}
	}
	
	private void HandleMovement(double delta)
	{
		var velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity.Y -= _gravity * (float)delta;
		}

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = _jumpVelocity;
		}

		var inputDir = Input.GetVector("move_left", "move_right", "move_back", "move_forward").Normalized();
		var dir = Vector3.Zero;

		dir += inputDir.X * Head.GlobalBasis.X;
		dir += inputDir.Y * -Head.GlobalBasis.Z;

		velocity.X = dir.X * _movementSpeed;
		velocity.Z = dir.Z * _movementSpeed;

		Velocity = velocity;
		MoveAndSlide();
	}

	private void HandleRaycast()
	{
		if (RayCast.IsColliding() && RayCast.GetCollider() is Chunk chunk)
		{
			BlockHighlight.Visible = RayCast.IsColliding();
		
			var blockPos = RayCast.GetCollisionPoint() - 0.5f * RayCast.GetCollisionNormal();
			var intBlockPos = new Vector3I(Mathf.FloorToInt(blockPos.X), Mathf.FloorToInt(blockPos.Y), Mathf.FloorToInt(blockPos.Z));
			BlockHighlight.GlobalPosition = intBlockPos + new Vector3(0.5f, 0.5f, 0.5f);
			BlockHighlight.GlobalRotation = new(0,0,0);

			Vector3I actualBlockPos = (Vector3I)(intBlockPos - chunk.GlobalPosition);
			Block block = chunk.GetBlock(actualBlockPos);
			
			LookingAtLabel.Text = $"Looking at: {intBlockPos}:{block.Name}";
		
			
			if (Input.IsActionPressed("left_click") && actualBlockPos.Y < ChunkData.ChunkHeight - 1)
			{
				if (GameMode is GameMode.Creative)
				{
					chunk.SetBlock(actualBlockPos, BlockRegistry.GetBlockByID("air"));
				}
				else if (GameMode is GameMode.Survival)
				{
					if (!block.IsUnbreakable && block.RequiredToolType == CurrentTool.ToolType || block.RequiredToolType == ToolType.None && block.Name != "air")
					{
						if (!_isBreakingBlock)
						{
							_isBreakingBlock = true;
							float timeToBreak = block.Hardness;
							if (CurrentTool.ToolType == block.PreferredToolType)
							{
								timeToBreak = block.Hardness / CurrentTool.Efficiency;
							}
							
							_breakTimer.Start(timeToBreak);
							
							_breakTimer.Timeout += () =>
							{
								chunk.SetBlock(actualBlockPos, BlockRegistry.GetBlockByID("air"));
								_isBreakingBlock = false;
							};
						}
					}
				}
				else
				{
					GameConsole.PrintError("Player: Unknown game mode.");
				}
			}
			else if (Input.IsActionJustReleased("left_click"))
			{
				_breakTimer.Stop();
				_isBreakingBlock = false;
			}

			
			if (Input.IsActionJustPressed("right_click"))
			{
				World.Instance.ChunkManager.SetBlock((Vector3I)(intBlockPos + RayCast.GetCollisionNormal()), BlockRegistry.GetBlockByID(_selectedBlock));
			}
		}
		else
		{
			LookingAtLabel.Text = $"Looking at: Nothing";
			BlockHighlight.Visible = false;
		}
	}
}

public enum GameMode
{
	Creative,
	Survival
}
