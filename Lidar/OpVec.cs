using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
class OpVec
{

    /// <summary>
    /// Não tranforma em slam e jogo
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Godot.Vector3 Vector2pVector3(Vector2 vector)
    {
        return new Godot.Vector3(vector.X, 0, vector.Y);
    }

    /// <summary>
    /// Não tranforma em slam e jogo
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Godot.Vector3 Vector2pVector3(Vector2 vector, float Z)
    {
        return new Godot.Vector3(vector.X, Z, vector.Y);
    }

    /// <summary>
    /// Não tranforma em slam e jogo
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector2 Vector3pVector2(Godot.Vector3 vector)
    {
        return new Vector2(vector.X, vector.Z);
    }

    /// <summary>
    /// Não tranforma em slam e jogo
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Point Vector2pPoint(Vector2 vector)
    {
        return new Point()
        {
            X = Convert.ToInt32(vector.X),
            Y = Convert.ToInt32(vector.Y)
        };
    }

    /// <summary>
    /// Não tranforma em slam e jogo
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Godot.Vector3 PointpVector3(Point vector)
    {
        return new Godot.Vector3(vector.X, 0, vector.Y);
    }


    /// <summary>
    /// Não tranforma em slam e jogo
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector2 PointpVector2(Point vector)
    {
        return new Vector2(vector.X, vector.Y);
    }



    public static List<Vector2> PointSLAMParaVector2(List<Point> pontos)
    {
        var vecs = new List<Vector2>();
        for (int i = 0; i < pontos.Count; i++)
        {
            vecs.Add(SistemaLidar.PosSLAMParaJogo(pontos[i]));
        }
        return vecs;
    }


}