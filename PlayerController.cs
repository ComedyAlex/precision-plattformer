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
	bool isDashAvailable = true;
	bool isDashing = false;
	public double dashTimer = .2f;
	public double dashTimerReset = .2f;

	[Export]
	public float Friction { get; set; } = 20;
	[Export]
	public float Acceleration { get; set; } = 10;
	[Export]
	public float AirFriction { get; set; } = 5;
	bool playerDead = false;
	

    public override void _Ready()
    {
        GetNode<ColorRect>("Camera2D/Retry").Hide();
    }

		public override void _PhysicsProcess(double delta)
	{
		Godot.Vector2 velocity = Velocity;

		//Jump
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity * jump_multiplier;
		}

		// Direction vector for movement
		Godot.Vector2 direction = Godot.Vector2.Zero;
		if (Input.IsActionPressed("move_left"))
		{
			direction = Godot.Vector2.Left;
		}
		if (Input.IsActionPressed("move_right"))
		{
			direction = Godot.Vector2.Right;
		}
		// Adding velocity
		if (direction != Godot.Vector2.Zero)
		{
			velocity.X = Mathf.MoveToward(Velocity.X, direction.X * Speed * speed_multiplier, Acceleration);
		}
		else if(!IsOnFloor())
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, AirFriction); //airfriction
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, AirFriction);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Friction);	//friction
		}
		
		//Dash
		if(Input.IsActionJustPressed("dash") && isDashAvailable && !isDashing)
		{
			velocity = Godot.Vector2.Zero;
			Godot.Vector2 dashDirection = Godot.Vector2.Zero;
			if(Input.IsActionPressed("move_up"))
			{
				dashDirection += Godot.Vector2.Up;
				isDashing = true;
			}
			if(Input.IsActionPressed("move_down"))
			{
				dashDirection += Godot.Vector2.Down;
				isDashing = true;
			}
			if(Input.IsActionPressed("move_right"))
			{
				dashDirection += Godot.Vector2.Right;
				isDashing = true;
			}
			if(Input.IsActionPressed("move_left"))
			{
				dashDirection += Godot.Vector2.Left;
				isDashing = true;
			}
			if(Input.IsActionPressed("move_right") && Input.IsActionPressed("move_up"))
			{
				dashDirection += Godot.Vector2.Right + Godot.Vector2.Up;
				isDashing = true;
			}
			if(Input.IsActionPressed("move_left") && Input.IsActionPressed("move_up"))
			{
				dashDirection += Godot.Vector2.Left + Godot.Vector2.Up;
				isDashing = true;
			}
			if(Input.IsActionPressed("move_right") && Input.IsActionPressed("move_down"))
			{
				dashDirection += Godot.Vector2.Right + Godot.Vector2.Down;
				isDashing = true;
			}
			if(Input.IsActionPressed("move_left") && Input.IsActionPressed("move_down"))
			{
				dashDirection += Godot.Vector2.Left + Godot.Vector2.Down;
				isDashing = true;
			}
			velocity += dashDirection.Normalized() * DashVelocity * dash_multiplier;
			isDashAvailable = false;
			dashTimer = dashTimerReset;
		}
		//dashtimer
		if(isDashing)
		{
			dashTimer -= delta;
			if(dashTimer <= 0)
			{
				isDashing = false;
			}
		}
		//add gravity
		if (!IsOnFloor() && !isDashing)
		{
			velocity += GetGravity() * (float)delta;
		}
		if(IsOnFloor() && !isDashing)
		{
			isDashAvailable = true;
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
		if(playerDead)
		{
			velocity = Godot.Vector2.Zero;

		}
		Velocity = velocity;
		MoveAndSlide();

		

		if(playerDead && Input.IsActionJustPressed("accept"))
		{
			GetTree().ReloadCurrentScene();
		}
		
	

		//Animation
		AnimatedSprite2D animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		if(direction != Godot.Vector2.Zero)
		{
			animatedSprite2D.Animation = "move";
			if(direction == Godot.Vector2.Left && !playerDead)
			{
				animatedSprite2D.FlipH = true;
			}
			else
			{
				animatedSprite2D.FlipH = false;
			}
		}
		if(velocity.X == 0 && velocity.Y == 0 && !playerDead)
		{
			animatedSprite2D.Animation = "idle";
		}
	}
	public void OnDeathAreaBodyEntered( PhysicsBody2D Player)
	{
		playerDead = true;
		GetNode<Control>("Camera2D/Retry").Show();
		Velocity = Godot.Vector2.Zero;
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Stop();
	}
}
