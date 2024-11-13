using System;
using System.Numerics;
using System.Collections.Generic;
using System.Threading;
using Godot;
using Vector2 = System.Numerics.Vector2;

namespace Ardumine.SistemaFSD.Condutor;
class CaminhoManual
{
    List<Vector2> Caminho { get; set; }
    bool Emerg = false;
    int IdxCurr = 0;


    public void Preparar(List<Vector2> _caminho)
    {
        Caminho = _caminho;
        IdxCurr = 0;
    }

    public void PararEmerg()
    {
        new Thread(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                Emerg = true;
                Thread.Sleep(10);
            }
        }).Start();
    }

    //Para fazer conducao. Sem thread.
    public void Conduzir()
    {
        Vector2[] cam = new Vector2[Caminho.Count];
        Caminho.CopyTo(cam);
        for (IdxCurr = 0; IdxCurr < cam.Length; IdxCurr++)
        {
            Vector2 posC = cam[IdxCurr];
            Vector2 posProx = cam[Math.Min(IdxCurr + 5, cam.Length - 1)];
            GD.Print("A fazer");
            GD.Print("A fazer");

            if (Emerg) break;
        }
    }
}