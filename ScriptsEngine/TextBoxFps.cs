using Godot;
using System;

public partial class TextBoxFps : RichTextLabel
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	double DeltaAdd = 0;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(DeltaAdd > 1){
			Text = $"FPS: {Engine.GetFramesPerSecond()} {delta}"; 
			DeltaAdd = 0;
		}
		DeltaAdd += delta;

	}
}
