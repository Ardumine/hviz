using System;
using Godot;

public partial class CameraAnt
 : Camera3D
{
	private float _velocity;

	[Export(PropertyHint.Range, "0f,100f,0.01f")]
	public float BoostSpeedMultiplier = 3.0f;

	[Export(PropertyHint.Range, "0f,1000f,0.01f")]
	public float DefaultVelocity = 2f;

	[Export(PropertyHint.TypeString, "Maximum speed of camera movement.")]
	public float MaxSpeed = 1000f;

	[Export(PropertyHint.TypeString, "Minimum speed of camera movement.")]
	public float MinSpeed = 0.2f;

	[Export(PropertyHint.Range, "0f,10f,0.01f")]
	public float Sensitivity = 3f;

	[Export(PropertyHint.Range, "0f,10f,0.001f")]
	public float SpeedScale = 1.17f;


	public static bool ModoCamera = true;
	public override void _Ready()
	{
		base._Ready();
		_velocity = DefaultVelocity;

		GD.Print("INI sis cam");
	}
	public override void _Input(InputEvent @event)
	{
		if (!Current)
			return;
		if (!ModoCamera) return;
		//base._Input(@event);
		Vector3 tempRot = Rotation;

		if (Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			if (@event is InputEventMouseMotion mouseMotionEvent)
			{

				tempRot.Y -= mouseMotionEvent.Relative.X / 1000 * Sensitivity;
				tempRot.X -= mouseMotionEvent.Relative.Y / 1000 * Sensitivity;
				tempRot.X = Mathf.Clamp(tempRot.X, Mathf.Pi / -2, Mathf.Pi / 2);
				Rotation = tempRot;
			}
		}

		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Right)
			{

				/*var from = ProjectRayOrigin(mouseButtonEvent.Position);
				var to = from + ProjectRayNormal(mouseButtonEvent.Position) * 10000;
				Position = to;
				GD.Print(to);*/
			}

			switch (mouseButtonEvent.ButtonIndex)
			{
				case MouseButton.Right:
					Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
					break;

				case MouseButton.WheelUp:
					_velocity = Mathf.Clamp(_velocity * SpeedScale, MinSpeed, MaxSpeed);
					break;
				case MouseButton.WheelDown:
					_velocity = Mathf.Clamp(_velocity / SpeedScale, MinSpeed, MaxSpeed);
					break;
			}
		}
			

	}

	public override void _Process(double delta)
	{
		if (!Current)
			return;
		if (!ModoCamera) return;

		var direction = new Vector3(
				Input.GetActionStrength("DireitaMover") - Input.GetActionStrength("EsquerdaMover"),
				Input.GetActionStrength("SubirMover") - Input.GetActionStrength("DescerMover"),
				Input.GetActionStrength("BaixoMover") - Input.GetActionStrength("CimaMover"))
		.Normalized();
		Vector3 tempRot = Rotation;
		
			tempRot.Y -= (Input.GetActionStrength("DireitaCamera") - Input.GetActionStrength("EsquerdaCamera"))  * 0.05f;
				tempRot.X -= (Input.GetActionStrength("BaixoCamera") - Input.GetActionStrength("CimaCamera")) * 0.05f;
				tempRot.X = Mathf.Clamp(tempRot.X, Mathf.Pi / -2, Mathf.Pi / 2);
				Rotation = tempRot;

		if (Input.IsKeyPressed(Key.Ctrl))
		{
			Translate(direction * (float)(_velocity * delta * BoostSpeedMultiplier));
			//Position = direction * (float)(_velocity * delta * BoostSpeedMultiplier)+ Position;
		}
		else
		{
			Translate(direction * (float)(_velocity * delta));
			//Position = direction * (float)(_velocity * delta);
			//Position = direction * (float)(_velocity * delta) + Position;
		}
	}
}
