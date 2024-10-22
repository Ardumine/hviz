using System;
using Godot;

public partial class Camera : Camera3D
{
    Node3D Pivot { get; set; }
    Vector3 Velocity = Vector3.Zero;
    public float ACCELERATION = 0.7f;
    public float MOUSE_SENSITIVITY = 0.03f;

    public float TargetSpeed = 0.1f;

    public void Setup()
    {
        Pivot = new();
        CallDeferred(MethodName.AddSibling, Pivot);

        //AddSibling(Pivot);

        Pivot.Position = Position;
        Pivot.Rotation = Rotation;
        Pivot.Name = "FreecamPivot";
        //Reparent(Pivot);
        CallDeferred(MethodName.Reparent, Pivot);

        Position = new Vector3(0, 0, 0);
        Rotation = new Vector3(0, 0, 0);


    }
    public override void _Ready()
    {
        Setup();
        //Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Process(double delta)
    {

        var dir = Vector3.Zero;

        dir.X += Input.GetActionStrength("BaixoMover") - Input.GetActionStrength("CimaMover");
        dir.Z += Input.GetActionStrength("EsquerdaMover") - Input.GetActionStrength("DireitaMover");

        dir = dir.Normalized();
        dir = dir.Rotated(Vector3.Up, Pivot.Rotation.Y);

        Velocity = Meth.Lerp(Vector3.Zero, dir * TargetSpeed, ACCELERATION);
        Pivot.Position += Velocity;




        //Parte rodar
        var XJoyStick = Input.GetActionStrength("DireitaCamera") - Input.GetActionStrength("EsquerdaCamera");
        var YJoyStick = Input.GetActionStrength("BaixoCamera") - Input.GetActionStrength("CimaCamera");

        Pivot.RotateY(-XJoyStick * MOUSE_SENSITIVITY);
        RotateY(-YJoyStick * MOUSE_SENSITIVITY);
        Rotation = new Vector3((float)Math.Clamp(Rotation.X, -Math.PI/2, Math.PI/2), Rotation.Y, Rotation.Z);


    }
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
       
    }

}