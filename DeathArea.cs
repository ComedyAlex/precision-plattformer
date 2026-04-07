using Godot;
using System;

public partial class DeathArea : Area2D
{

		private static void OnDeathAreaBodyEntered()
		{
			GD.Print("Area entered");
		}

}
