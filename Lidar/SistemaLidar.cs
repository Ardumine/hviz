using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;
using BaseSLAM;
using Newtonsoft.Json;
using Websocket.Client;

public class SistemaLidar
{

    public static int TamMapa = 1000;

    /// <summary>
    /// Pos currente do Lidar. Pos em tipo de jogo.
    /// </summary>
    public Vector2 PosCurr = new Vector2(0, 0);

    /// <summary>
    /// Angulo current em radianos
    /// </summary>
    public float AngCurr = 0f;


    private static WebsocketClient wsLidar;
    private static WebsocketClient wsPos;


    public delegate void OnDadosRecebidosLidarEvent(List<Ray> dados);
    public event OnDadosRecebidosLidarEvent OnDadosRecebidos;

    public SistemaLidar(int tamMapa)
    {
        TamMapa = tamMapa;
    }

    public void Iniciar(string IP)
    {
        wsLidar = new WebsocketClient(new Uri("ws://" + IP + ":8000/wslidar"));
        wsPos = new WebsocketClient(new Uri("ws://" + IP + ":8000/wspos"));

        wsLidar.MessageReceived.Subscribe((msg) =>
        {
            var dados_recs = JsonConvert.DeserializeObject<List<Ray>>(msg.Text);
            for (int i = 0; i < dados_recs.Count; i++)
            {
                dados_recs[i] = new(dados_recs[i].Angle, dados_recs[i].Radius / 100.0f);
            }

            OnDadosRecebidos?.Invoke(dados_recs);
            Thread.Sleep(1500);
            wsLidar.Send("lu");

        });

        wsPos.MessageReceived.Subscribe((msg) =>
        {
            var dados_recs = JsonConvert.DeserializeObject<Vector3>(msg.Text);

            PosCurr = PosSLAMParaJogo(dados_recs.X, dados_recs.Y);
            AngCurr = dados_recs.Z;

            Thread.Sleep(100);
            wsPos.Send("lu");

        });
        wsLidar.Start();
        wsPos.Start();

        Thread.Sleep(800);
        wsLidar.Send("lu");
        wsPos.Send("lu");



    }

    public static void MandarDadosStrWs(string dados)
    {
        if (wsPos.IsRunning)
        {
            wsPos.Send(dados);
        }
    }



    public static Vector2 PosSLAMParaJogo(float Xl, float Yl)
    {
        return new Vector2((Xl - TamMapa / 2.0f) / 100.0f, (Yl - TamMapa / 2.0f) / 100.0f);// de jogo para mapa
    }
    public static Vector2 PosSLAMParaJogo(Vector2 pos)
    {
        return new Vector2((pos.X - TamMapa / 2.0f) / 100.0f, (pos.Y - TamMapa / 2.0f) / 100.0f);// de jogo para mapa
    }

    public static Vector2 PosSLAMParaJogo(Point p)
    {
        return new Vector2((p.X - TamMapa / 2.0f) / 100.0f, (p.Y - TamMapa / 2.0f) / 100.0f);// de jogo para mapa
    }

    public static Vector2 PosJogoParaSLAM(Vector3 pos)
    {
        return new Vector2((pos.X * 100.0f) + TamMapa / 2.0f, (pos.Z * 100.0f) + TamMapa / 2.0f);// de jogo para mapa
    }
    public static Vector2 PosJogoParaSLAM(Vector2 pos)
    {
        return new Vector2((pos.X * 100.0f) + TamMapa / 2.0f, (pos.Y * 100.0f) + TamMapa / 2.0f);// de jogo para mapa
    }
}