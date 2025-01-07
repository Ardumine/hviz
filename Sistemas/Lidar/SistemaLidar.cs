using System.Drawing;
using System.Numerics;
using System.Threading;
using AFCP;
using Ardumine.Modules.YDLidar;

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



	public delegate void OnDadosRecebidosLidarEvent(LidarPoint[] dados);
	public event OnDadosRecebidosLidarEvent? OnDadosRecebidos;

	public SistemaLidar(int tamMapa)
	{
		TamMapa = tamMapa;
	}

	public void Iniciar(ChannelManager _channelManager)
	{
		var posChannel = _channelManager.GetInterfaceForChannel<Vector3>("/slamPos");
		var lidarDataChannel = _channelManager.GetInterfaceForChannel<LidarPoint[]>("/lidarData");

		posChannel.AddEvent((pos) =>
		{
			PosCurr = PosSLAMParaJogo(pos.X, pos.Y);
			AngCurr = pos.Z;
		});
/*
		lidarDataChannel.AddEvent((dados) =>
		{
			for (int i = 0; i < dados!.Length; i++)
			{
				dados[i] = new()
				{
					AngleRad = dados[i].AngleRad,
					Distance = dados[i].Distance / 100.0f

				};
			}
			new Thread(() =>
			{
				OnDadosRecebidos?.Invoke(dados);
			}).Start();

		});
*/

	}

	public static void MandarDadosStrWs(string dados)
	{
		//if (wsPos != null && wsPos.IsRunning)
		{
			//   wsPos.Send(dados);
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
