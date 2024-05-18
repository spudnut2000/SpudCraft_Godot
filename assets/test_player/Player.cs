using Godot;
using System;
using SpudCraftGodot.assets.scripts;

public partial class Player : CharacterBody3D
{
	[Export] public Node3D Head;
	[Export] public Camera3D Camera;
	[Export] public RayCast3D RayCast;
	[Export] public MeshInstance3D BlockHighlight;

	[Export] public Label FpsLabel;
	[Export] public Label CoordsLabel;
	
	[Export] private float _mouseSensitivity = 0.001f;
	[Export] private float _movementSpeed = 5f;
	[Export] private float _jumpVelocity = 5f;

	private float _cameraXRotation;
	private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	
	public override void _Ready()
	{
		World.Instance.Player = this;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Process(double delta)
	{
		HandleRaycast();
		FpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleMovement(delta);
	}
	

	public override void _Input(InputEvent @event)
	{
		HandleCameraRotation(@event);
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
		
			if (Input.IsActionJustPressed("left_click"))
			{
				chunk.SetBlock((Vector3I)(intBlockPos - chunk.GlobalPosition), BlockRegistry.GetBlockByID("air"));
			}
			
			if (Input.IsActionJustPressed("right_click"))
			{
				World.Instance.ChunkManager.SetBlock((Vector3I)(intBlockPos + RayCast.GetCollisionNormal()), BlockRegistry.GetBlockByID("wood"));
			}
		}
		else
		{
			BlockHighlight.Visible = false;
		}
	}
}
