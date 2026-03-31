using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
	[Export]
	public float Speed { get; set; } = 10.0f;
	int speed_multiplier = 30;
	[Export]
	public float JumpVelocity  { get; set; }= 10.0f;
	int jump_multiplier = -30;
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity * jump_multiplier;
		}


		Vector2 direction = Vector2.Zero;
		if (Input.IsActionPressed("move_left")){
			direction = Vector2.Left;
		}
		if (Input.IsActionPressed("move_right")){
			direction = Vector2.Right;
		}

		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed * speed_multiplier;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed * speed_multiplier);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
