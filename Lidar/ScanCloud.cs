using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ardumine.Modules.YDLidar;
public partial class ScanCloud : Node3D
{

	List<CsgSphere3D> Points = new List<CsgSphere3D>();
	List<CsgCylinder3D> Cylinders = new List<CsgCylinder3D>();


	StandardMaterial3D matPonto = new StandardMaterial3D() { AlbedoColor = new Color(0, 254, 0) };
	StandardMaterial3D MatLig = new StandardMaterial3D() { AlbedoColor = new Color(0, 254, 254) };


	Sprite3D sprite = new Sprite3D();

	public override void _Ready()
	{
	}

	LidarPoint[]? raios;
	bool exec_update = false;
	public void UpdatePontos(LidarPoint[] _raios)
	{
		raios = _raios;
		exec_update = true;
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (exec_update)
		{
			var sw = Stopwatch.StartNew();
			exec_update = false;
			
			//Esconder se não forem precisos
			for (int i = raios!.Length; i < Points.Count; i++)
			{
				Points[i].Visible = false;
			}
			for (int i = raios.Length - 1; i < Cylinders.Count; i++)
			{
				Cylinders[i].Visible = false;
			}

			//Criar se não existir
			if (Points.Count < raios.Length)
			{
				// If there are not enough points, create new ones
				int additionalPointsNeeded = raios.Length - Points.Count;
				for (int i = 0; i < additionalPointsNeeded; i++)
				{
					var point = new CsgSphere3D
					{
						Radius = 0.025f,
						Material = matPonto
					};
					AddChild(point);
					Points.Add(point);	
				}
			}
			if (Cylinders.Count < raios.Length - 1)
			{
				// If there are not enough cylinders, create new ones
				int additionalCylindersNeeded = raios.Length - 1 - Cylinders.Count;
				for (int i = 0; i < additionalCylindersNeeded; i++)
				{
					var cylinder = new CsgCylinder3D
					{
						Radius = 0.01f, // Adjust as needed
						Material = MatLig
					};
					AddChild(cylinder);
					Cylinders.Add(cylinder);
				}
			}

			// Update the position of each point
			for (int i = 0; i < raios.Length; i++)
			{
				float r = raios[i].Distance;
				var angle = raios[i].Angle;
				double px = r * Math.Cos(angle);
				double py = r * Math.Sin(angle);

				Points[i].Position = new Vector3((float)px, 0, (float)py);
			}
			for (int i = 0; i < raios.Length - 1; i++)
			{
				var point1 = Points[i].Position;
				var point2 = Points[i + 1].Position;

				var midpoint = (point1 + point2) / 2;
				var direction = (point2 - point1).Normalized();
				var length = (point2 - point1).Length();
				var ang = (float)Math.Atan2(direction.X, direction.Z);

				Cylinders[i].Height = length;
				Cylinders[i].Rotation = new Vector3((float)Meth.Deg4Rad(90), ang, 0);
				Cylinders[i].Position = midpoint;
				Cylinders[i].Visible = true;
			}
			//GD.Print(sw.ElapsedMilliseconds);
		}
	}


}
