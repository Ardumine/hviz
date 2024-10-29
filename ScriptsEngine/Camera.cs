using System;
using Godot;

public partial class Camera : Camera3D
{
    public Node3D Pivot { get; set; }
    Vector3 Velocity = Vector3.Zero;
    public float ACCELERATION = 0.6f;
    public float MOUSE_SENSITIVITY = 0.03f;

    public float TargetSpeed = 0.1f;


    public static bool ModoCamera = true;


    public void Setup()
    {
        Pivot = new();

        CallDeferred(MethodName.AddSibling, Pivot);


        Position = new Vector3(0, 0, 0);
        Rotation = new Vector3(0, 0, 0);

        Pivot.Position = Position;
        Pivot.Rotation = Rotation;
        Pivot.Name = "FreecamPivot";

        CallDeferred(MethodName.Reparent, Pivot);

        RotateX((float)(-Math.PI / 2.0f));
        //Pivot.Position += new Vector3(0, 3, 0);

    }
    public override void _Ready()
    {
        Setup();
        //Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Process(double delta)
    {
        if (ModoCamera)
        {

            var dir = Vector3.Zero;

            //valor negativo - valor positivo
            dir.Z += Input.GetActionStrength("BaixoMover") - Input.GetActionStrength("CimaMover");
            dir.X += Input.GetActionStrength("DireitaMover") - Input.GetActionStrength("EsquerdaMover");

            dir.Y += Input.GetActionStrength("SubirMover") - Input.GetActionStrength("DescerMover");


            dir = dir.Normalized();
            dir = dir.Rotated(Vector3.Up, Pivot.Rotation.Y);

            Velocity = Meth.Lerp(Vector3.Zero, dir * TargetSpeed, ACCELERATION);
            Pivot.Position += Velocity;




            //Parte rodar
            var XJoyStick = Input.GetActionStrength("DireitaCamera") - Input.GetActionStrength("EsquerdaCamera");
            var YJoyStick = Input.GetActionStrength("BaixoCamera") - Input.GetActionStrength("CimaCamera");

            Pivot.RotateY(-XJoyStick * MOUSE_SENSITIVITY);
            RotateX(-YJoyStick * MOUSE_SENSITIVITY);
            Rotation = new Vector3((float)Math.Clamp(Rotation.X, -Math.PI / 2, Math.PI / 2), Rotation.Y, Rotation.Z);

        }

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

                //Parte rodar
                var XJoyStick = mouseMotionEvent.Relative.X / 10;
                var YJoyStick = mouseMotionEvent.Relative.Y / 10;

                Pivot.RotateY(-XJoyStick * MOUSE_SENSITIVITY);
                RotateX(-YJoyStick * MOUSE_SENSITIVITY);
                Rotation = new Vector3((float)Math.Clamp(Rotation.X, -Math.PI / 2, Math.PI / 2), Rotation.Y, Rotation.Z);

            }
        }

        if (@event is InputEventMouseButton mouseButtonEvent)
        {


            switch (mouseButtonEvent.ButtonIndex)
            {
                case MouseButton.Right:
                    Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
                    break;
                case MouseButton.Left:
                    Input.MouseMode = Input.MouseModeEnum.Visible;
                    break;
            }
        }


    }


}