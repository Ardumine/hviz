using System;
using System.Net.Sockets;
using System.Threading;
using Godot;

class SistemaMapa{

    
    public delegate void OnDadosRecebidosEvent(byte[] dados);
    public event OnDadosRecebidosEvent OnDadosRecebidos;



	static int TamMapa = 1000;
	public byte[] Dados_mapa = new byte[TamMapa * TamMapa];
    public void Iniciar(string IP){
        new Thread(() =>
		{
			while (true)
			{
				try
				{
					TcpClient client = new TcpClient(IP, 8090);
					NetworkStream stream = client.GetStream();
					while (client.Connected)
					{
						//int bytesRead = stream.Read(Dados_mapa, 0, Dados_mapa.Length); // read the data from the server

						int messageSize = Dados_mapa.Length;
						int readSoFar = 0;
						byte[] dados = new byte[messageSize];

						while (readSoFar < messageSize)
						{
							var read = stream.Read(dados, readSoFar, dados.Length - readSoFar);
							readSoFar += read;
							if (read == 0)
								break;   // connection was broken
										 //Log(read);
						}
						Dados_mapa = dados;
                        OnDadosRecebidos?.Invoke(dados);

					}
				}
				catch(Exception e)
				{
					GD.PrintErr("erro TCP!" + e.Message);
				}
			}
		}).Start();
    }
}