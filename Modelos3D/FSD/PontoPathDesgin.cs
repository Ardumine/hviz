using Godot;
using System;

namespace Ardumine.SistemaFSD.GUI;
public partial class PontoPathDesgin : Node3D
{
	public int ID { get; set; }
	public Guid GuidCaminho { get; set; }
	public bool Inicial { get; set; }
}
