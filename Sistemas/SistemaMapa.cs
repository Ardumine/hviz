using System;
using System.Net.Sockets;
using System.Threading;
using AFCP;
using Emgu.CV;
using Godot;

class SistemaMapa
{


	public delegate void OnDadosRecebidosEvent(byte[] dados);
	public event OnDadosRecebidosEvent? OnDadosRecebidos;



	static int TamMapa = 1000;
	/// <summary>
	/// Mapa. Cada pixel = 1 cm
	/// </summary>
	public byte[] Dados_mapa = new byte[TamMapa * TamMapa];

	public void LoadMapaBMP(string NomeFich)
	{
		Mat mat = CvInvoke.Imread(NomeFich, Emgu.CV.CvEnum.ImreadModes.Grayscale);
		mat.CopyTo(Dados_mapa);
		mat.Dispose();

		new Thread(() =>
		{
			Thread.Sleep(300);
			OnDadosRecebidos?.Invoke(Dados_mapa);
		}).Start();

	}
	public void Iniciar(ChannelManager _channelManager, int tamMapa)
	{
		TamMapa = tamMapa;
		var map = _channelManager.GetInterfaceForChannel<byte[]>("/SLAMmap");
		map.AddEvent((s) =>
		{
			Dados_mapa = s!;
			OnDadosRecebidos?.Invoke(s!);
		});

	}
}