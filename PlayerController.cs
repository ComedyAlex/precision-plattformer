using Godot;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

public partial class PlayerController : CharacterBody2D
{
	[Export] public float Speed { get; set; } = 10.0f;
	int speed_multiplier = 30;
	[Export] public float JumpVelocity  { get; set; } = 10.0f;
    int jump_multiplier = -30; 
	bool IsJumping = false;
	[Export] public float CoyoteFrames { get; set; } = 6;
	[Export] public float WallJumpVelocity { get; set; } = 10.0f;
	[Export] public float DashVelocity { get; set; } = 10.0f;
	int dash_multiplier = 30;
	bool isDashAvailable = true;
	bool isDashing = false;
	public double dashTimer = .2f;
	public double dashTimerReset = .2f;

	Image particleImage = Image.LoadFromFile("res://brackeys_platformer_assets/brackeys_platformer_assets/sprites/knight(solo_sprite).png");
	Image particleImageFlipped = Image.LoadFromFile("res://brackeys_platformer_assets/brackeys_platformer_assets/sprites/knight(solo_sprite).png");

	[Export] public float Friction { get; set; } = 20;
	[Export] public float Acceleration { get; set; } = 10;
	[Export] public float AirFriction { get; set; } = 5;
	[Export] public float WallGravity { get; set; } = 10;
	public int wall_gravity_mulitplier = 100;
	bool InCoyoteTime = false;
	bool lastFloorFrame;
	bool playerDead = false;

	public void OnHitBoxBodyEntered(Node body)
	{
		GD.Print("bruh man I'm dead :|");
		if(body is TileMapLayer || body is PhysicsBody2D)
		{
			die();
		}
	}
    public override void _Ready()
    {
		//retry screen
        GetNode<ColorRect>("Camera2D/Retry").Hide();
		//time to frames
		GetNode<Timer>("CoyoteTimer").WaitTime = CoyoteFrames / 60;
		Connect("body_entered", new Callable(this, nameof(OnHitBoxBodyEntered)));
		particleImageFlipped.FlipX();
    }
	
		public override void _PhysicsProcess(double delta)
	{
		Godot.Vector2 velocity = Velocity;

		
		// jump
		if (Input.IsActionJustPressed("jump") && (IsOnFloor() || InCoyoteTime))
		{
			velocity.Y = JumpVelocity * jump_multiplier;
			IsJumping = true;
		}
		if(IsOnFloor())
		{
			IsJumping = false;
		}

		// direction vector for movement
		Godot.Vector2 direction = Godot.Vector2.Zero;
		if (Input.IsActionPressed("move_left"))
		{
			direction = Godot.Vector2.Left;
		}
		if (Input.IsActionPressed("move_right"))
		{
			direction = Godot.Vector2.Right;
		}

		// adding velocity
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
		
		// dash
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

		
		//dashtimer & dashParticle
		GpuParticles2D dashParticleEffect = GetNode<GpuParticles2D>("DashParticleEffect");
		if(isDashing)
		{
			
			if(direction == Godot.Vector2.Left)
			{
				dashParticleEffect.Texture = ImageTexture.CreateFromImage(particleImageFlipped);
			}
			else if(direction == Godot.Vector2.Right)
			{
				dashParticleEffect.Texture = ImageTexture.CreateFromImage(particleImage);
			}
			dashParticleEffect.Emitting = true;
			dashTimer -= delta;
			if(dashTimer <= 0)
			{
				isDashing = false;
				dashParticleEffect.Emitting = false;
			}
		}
		//wall detection
		RayCast2D onWallLeft = GetNode<RayCast2D>("RayCastLeft");
		RayCast2D onWallRight = GetNode<RayCast2D>("RayCastRight");

		//add gravity
		if (!IsOnFloor() && !isDashing && (!onWallLeft.IsColliding() || !onWallRight.IsColliding()))
		{
			velocity += GetGravity() * (float)delta;
		}
		if(IsOnFloor() && !isDashing)
		{
			isDashAvailable = true;
		}
		
		//walljump
		if(Input.IsActionJustPressed("jump") && onWallLeft.IsColliding() && !IsOnFloor())
		{
			velocity.Y = WallJumpVelocity * jump_multiplier * 2;
			velocity.X = WallJumpVelocity * -jump_multiplier;
		}
		if(Input.IsActionJustPressed("jump") && onWallRight.IsColliding() && !IsOnFloor())
		{
			velocity.Y = WallJumpVelocity * jump_multiplier * 2;
			velocity.X = WallJumpVelocity * jump_multiplier;
		}

		//wallslide
		
		if(!IsOnFloor() && onWallLeft.IsColliding() && direction.X < 0 && velocity.Y > 0)
		{
			velocity.Y =  WallGravity * wall_gravity_mulitplier * (float)delta;
			GetNode<GpuParticles2D>("WallSlideParticleEffectL").Emitting = true;
		}
		else{GetNode<GpuParticles2D>("WallSlideParticleEffectL").Emitting = false;}
		if(!IsOnFloor() && onWallRight.IsColliding() && direction.X > 0 && velocity.Y > 0)
		{
			velocity.Y =  WallGravity * wall_gravity_mulitplier * (float)delta;
			GetNode<GpuParticles2D>("WallSlideParticleEffectR").Emitting = true;
		}
		else{GetNode<GpuParticles2D>("WallSlideParticleEffectR").Emitting = false;}

		//player death
		if(playerDead)
		{
			velocity = Godot.Vector2.Zero;

		}

		Velocity = velocity;
		lastFloorFrame = IsOnFloor();
		MoveAndSlide();

		
		//coyote time
		if(!IsOnFloor() && !IsJumping && lastFloorFrame)
		{
			InCoyoteTime = true;
			GetNode<Timer>("CoyoteTimer").Start();
		}

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
	public  void OnCoyoteTimerTimeout()
	{
		InCoyoteTime = false;
	}
	public void OnDeathAreaBodyEntered(PhysicsBody2D Player)
	{
		die();
	}

	public void die() {
		playerDead = true;
		GetNode<Control>("Camera2D/Retry").Show();
		Velocity = Godot.Vector2.Zero;
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Stop();
	}
	
}
