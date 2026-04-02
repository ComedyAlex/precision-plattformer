using Godot;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

public partial class PlayerController : CharacterBody2D
{
	[Export]
	public float Speed { get; set; } = 10.0f;
	int speed_multiplier = 30;
	[Export]
	public float JumpVelocity  { get; set; } = 10.0f;
    int jump_multiplier = -30; 
	[Export]
	public float WallJumpVelocity { get; set; } = 10.0f;
	[Export]
	public float DashVelocity { get; set; } = 10.0f;
	int dash_multiplier = 30;
	bool IsDashAvailable = true;
	[Export]
	public float Friction { get; set; } = 20;
	[Export]
	public float Acceleration { get; set; } = 10;
	[Export]
	public float AirFriction { get; set; } = 5f;
	public override void _PhysicsProcess(double delta)
	{
		Godot.Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity * jump_multiplier;
		}

		// Movement
		Godot.Vector2 direction = Godot.Vector2.Zero;
		if (Input.IsActionPressed("move_left"))
		{
			direction = Godot.Vector2.Left;
		}
		if (Input.IsActionPressed("move_right"))
		{
			direction = Godot.Vector2.Right;
		}

		if (direction != Godot.Vector2.Zero)
		{
			velocity.X = Mathf.MoveToward(Velocity.X, direction.X * Speed * speed_multiplier, Acceleration);
		}
		else if(!IsOnFloor())
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, AirFriction);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Friction);
		}
		//Dash
		if(Input.IsActionJustPressed("dash") && IsDashAvailable)
		{
			velocity = Godot.Vector2.Zero;
			Godot.Vector2 dashDirection = Godot.Vector2.Zero;
			if(Input.IsActionPressed("move_up"))
			{
				dashDirection += Godot.Vector2.Up;
			}
			if(Input.IsActionPressed("move_down"))
			{
				dashDirection += Godot.Vector2.Down;
			}
			if(Input.IsActionPressed("move_right"))
			{
				dashDirection += Godot.Vector2.Right;
			}
			if(Input.IsActionPressed("move_left"))
			{
				dashDirection += Godot.Vector2.Left;
			}
			if(Input.IsActionPressed("move_right") && Input.IsActionPressed("move_up"))
			{
				dashDirection += Godot.Vector2.Right + Godot.Vector2.Up;
			}
			if(Input.IsActionPressed("move_left") && Input.IsActionPressed("move_up"))
			{
				dashDirection += Godot.Vector2.Left + Godot.Vector2.Up;
			}
			if(Input.IsActionPressed("move_right") && Input.IsActionPressed("move_down"))
			{
				dashDirection += Godot.Vector2.Right + Godot.Vector2.Down;
			}
			if(Input.IsActionPressed("move_left") && Input.IsActionPressed("move_down"))
			{
				dashDirection += Godot.Vector2.Left + Godot.Vector2.Down;
			}
			velocity += dashDirection.Normalized() * DashVelocity * dash_multiplier;
			IsDashAvailable = false;
		}
		if(IsOnFloor())
		{
			IsDashAvailable = true;
		}
		if(Input.IsActionJustPressed("jump") && GetNode<RayCast2D>("RayCastLeft").IsColliding() && !IsOnFloor())
		{
			velocity.Y = WallJumpVelocity * jump_multiplier * 2;
			velocity.X = WallJumpVelocity * -jump_multiplier;
		}
		if(Input.IsActionJustPressed("jump") && GetNode<RayCast2D>("RayCastRight").IsColliding() && !IsOnFloor())
		{
			velocity.Y = WallJumpVelocity * jump_multiplier * 2;
			velocity.X = WallJumpVelocity * jump_multiplier;
		}

		Velocity = velocity;
		MoveAndSlide();

		//Animation
		AnimatedSprite2D animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		if(direction != Godot.Vector2.Zero)
		{
			animatedSprite2D.Animation = "move";
			if(direction == Godot.Vector2.Left)
			{
				animatedSprite2D.FlipH = true;
			}
			else
			{
				animatedSprite2D.FlipH = false;
			}
		}
		if(velocity.X == 0 & velocity.Y == 0)
		{
			animatedSprite2D.Animation = "idle";
		}
	}
}
