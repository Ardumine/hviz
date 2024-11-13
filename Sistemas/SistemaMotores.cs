using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Godot;
//using System.IO.Ports;
//<PackageReference Include="System.IO.Ports" Version="8.0.0" />

class SistemaMotores
{

	//SerialPort porta = new SerialPort();
#if modoMotoresWifi
	using IPMapper;
	
	TcpClient client = new();
	NetworkStream stream_motores;
	bool conectado = false;

	public void ConESPAA()
	{
		var IP_esp = IPMacMapper.FindIPFromMacAddress("c8-f0-9e-4e-0c-88"); //= "192.168.73.56"


		var ipEndPoint = new IPEndPoint(IPAddress.Parse(IP_esp), 7000);

		client = new();
		client.Connect(ipEndPoint);
		stream_motores = client.GetStream();
		//Mand_dados(0, 0);
		conectado = true;
	}
	public void IniMotores()
	{
		return;//Estou a usar linux
		/*porta.PortName = "com17";
		//porta.PortName = "/dev/ttyACM0";
		porta.RtsEnable = false;
		porta.BaudRate = 115200;
		porta.Open();
		MandarSteer(0, 0);*/

	}
	public void Mand_dadosAA(double velA, double velB)
	{
		velA = Math.Round(velA);
		velB = Math.Round(velB);
		if (conectado && client.Connected)
		{
			byte[] array = Encoding.UTF8.GetBytes($"{velA};{velB}|".Replace(",", "."));
			try
			{
				stream_motores.Write(array);
			}
			catch
			{
				GD.PrintErr("Erro mandar dados ESP!");
			}
		}
	}

#endif

	int ultSteer = -100000;
	int ultSpeed = -100000;

	public void MandarSteer(int steer, int vel)
	{
		if ((ultSpeed != vel || ultSteer != steer) || vel == 0)
		{
			SistemaLidar.MandarDadosStrWs($"{steer}, {vel}|");
			ultSteer = steer;
			ultSpeed = vel;
		}
		//porta.Write($"{steer}, {vel}|");
		//<   porta.Write($"{steer * maxSteer}, {vel * maxVel}|");

	}

	



}
