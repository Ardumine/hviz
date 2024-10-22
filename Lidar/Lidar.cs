using BaseSLAM;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Vector2 = System.Numerics.Vector2;

public partial class Lidar : Node3D
{
	bool Emerg = false;
	Vector2 posObj;

	string IP = "192.168.137.1";
	ScanCloud scanCloud;

	CsgBox3D lidar;
	CsgBox3D posObjCaixa;
	Camera camera;
	RichTextLabel LogTextBox;

	Sprite3D SpriteMapa;
	ImageTexture image;


	static int TamMapa = 1000;
	SistemaLidar sisLidar;
	SistemaMotores sisMotores;
	SistemaMapa sisMapa;
	SistemaFSD sisFSD;

	List<Ray> dadosLidar = new();


	List<CsgBox3D> boxParaPoses = new();
	List<Vector2> posesParaObj = new();
	int idx_seg = 0;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		lidar = GetNode<CsgBox3D>("lidar");
		scanCloud = GetNode<ScanCloud>("lidar/scanCloud");
		SpriteMapa = GetNode<Sprite3D>("Mapa");
		posObjCaixa = GetNode<CsgBox3D>("pos_obj");
		camera = GetNode<Camera>("../Camera");
		LogTextBox = GetNode<RichTextLabel>("../LogTextBox");


		Iniciar();
	}

	public void Iniciar()
	{
		//pos_obj.Visible = false;

		sisLidar = new SistemaLidar(TamMapa);
		sisLidar.Iniciar(IP);
		sisLidar.OnDadosRecebidos += (dados) =>
		{
			scanCloud.UpdatePontos(dados);
			dadosLidar = dados;
		};

		sisMapa = new SistemaMapa();
		sisMapa.Iniciar(IP);
		sisMapa.OnDadosRecebidos += (dados) =>
		{
			image = ImageTexture.CreateFromImage(Image.CreateFromData(TamMapa, TamMapa, false, Image.Format.L8, dados));
		};

		sisMotores = new SistemaMotores();
		sisMotores.IniMotores();

		sisFSD = new SistemaFSD();
	}


	//Seguir pontos
	void SeguirPontos(List<Vector2> PontosParaIr, CancellationToken token = default(CancellationToken))
	{
		//Fase 1 virar para o primeiro ponto
		for (idx_seg = 0; idx_seg < PontosParaIr.Count; idx_seg++)
		{
			IrParaPonto(PontosParaIr[idx_seg], true);
			if (token.IsCancellationRequested || Emerg) return;

		}
		sisMotores.MandarSteer(0, 0);
		if (!Emerg)
		{
			for (int i = 0; i < 5; i++)
			{
				Log("---conc----x");
			}
		}
		else
		{
			Log("Parar emerg!");
		}

	}

	void IrParaPonto(Vector2 targetPoint, bool ModoContinuo = false, CancellationToken token = default(CancellationToken))
	{
		double angleToTarget = Math.Atan2(targetPoint.Y, targetPoint.X);

		var dif = Meth.calcDifBetweenAngles(Meth.Rad4Deg(sisLidar.AngCurr), Meth.Rad4Deg(angleToTarget));
		if (ModoContinuo)
		{
			if (Math.Abs(dif) > 15)
			{
				//Rodar
				RodarParaGrau(targetPoint, true, 10, token);
			}
			if (token.IsCancellationRequested || Emerg) return;
			NavegarParaPonto(targetPoint, true, 0.25, 1.1, token);

		}
		else
		{
			if (Math.Abs(dif) > 10)
			{
				//Rodar
				RodarParaGrau(targetPoint, token: token);
			}
			if (token.IsCancellationRequested || Emerg) return;
			NavegarParaPonto(targetPoint, token: token);

		}

	}

	void RodarParaGrau(Vector2 targetPoint, bool ModoContinuo = false, int AngMaxDif = 5, CancellationToken token = default(CancellationToken))
	{

		double angleDifference = Meth.ObterDifPos(sisLidar.PosCurr, Meth.Rad4Deg(sisLidar.AngCurr), targetPoint);//Meth.calcDifBetweenAngles(Meth.Rad4Deg(sisLidar.AngCurr), Meth.Rad4Deg(angleToTarget));

		while (!(angleDifference < AngMaxDif && angleDifference > -AngMaxDif) && !Emerg)
		{

			//angleDifference = Meth.ObterDifPos(sisLidar.PosCurr, Meth.Rad4Deg(sisLidar.AngCurr), targetPoint);//Meth.calcDifBetweenAngles(Meth.Rad4Deg(sisLidar.AngCurr), Meth.Rad4Deg(angleToTarget));

			//Take a look to this later.
			angleDifference = -Meth.ObterDifPos(targetPoint, Meth.Rad4Deg(sisLidar.AngCurr), sisLidar.PosCurr);//Meth.calcDifBetweenAngles(Meth.Rad4Deg(sisLidar.AngCurr), Meth.Rad4Deg(angleToTarget));

			int steer = Meth.SpeedControl((int)(angleDifference / 180.0 * 400.0), 50);
			Log($"RDif: {angleDifference} SLAC{sisLidar.AngCurr} SC_S: {steer}");
			sisMotores.MandarSteer(steer, 70);
			if (token.IsCancellationRequested || Emerg) return;
			Thread.Sleep(70);

		}
		if (!ModoContinuo)
		{
			sisMotores.MandarSteer(0, 0);
			Log("Rodar conc!");
			Thread.Sleep(200);
		}

	}

	void NavegarParaPonto(Vector2 targetPoint, bool ModoContinuo = false, double ErroDistPox = 0.1, double multVel = 1.0, CancellationToken token = default(CancellationToken))
	{

		var currentLocation = sisLidar.PosCurr;
		var Dist_final_inicio = Math.Sqrt(Math.Pow(currentLocation.X - targetPoint.X, 2) + Math.Pow(currentLocation.Y - targetPoint.Y, 2));


		double Dist_Prox = Math.Sqrt(Math.Pow(currentLocation.X - targetPoint.X, 2) + Math.Pow(currentLocation.Y - targetPoint.Y, 2));

		while (Dist_Prox > ErroDistPox && !Emerg)
		{
			currentLocation = sisLidar.PosCurr;

			double angleToTarget = Math.Atan2(targetPoint.Y - currentLocation.Y, targetPoint.X - currentLocation.X);
			double angleDifference = Meth.Rad4Deg(angleToTarget - sisLidar.AngCurr) / 360.0;

			Dist_Prox = Math.Sqrt(Math.Pow(currentLocation.X - targetPoint.X, 2) + Math.Pow(currentLocation.Y - targetPoint.Y, 2));

			int velRaw = (int)(Dist_Prox / Dist_final_inicio * 600.0 * multVel);
			int vel = Meth.SpeedControl(velRaw, 80);

			RotinaUpdateRodarParaPonto(targetPoint, vel);

			Log(Dist_Prox + "   aD" + Math.Round(angleDifference, 4) + "      vel: " + vel
			);
			Thread.Sleep(50);
			if (token.IsCancellationRequested || Emerg) return;

		}


		//Se for só para navegar para um ponto
		if (!ModoContinuo)
		{
			sisMotores.MandarSteer(0, 0);
			for (int i = 0; i < 5; i++)
			{
				Log("NotCont---conc----x");
			}
		}

	}
	void RotinaUpdateRodarParaPonto(Vector2 targetPoint, int vel)
	{
		//double angleDifference = -Meth.ObterDifPos(sisLidar.PosCurr, Meth.Rad4Deg(sisLidar.AngCurr), targetPoint);//Meth.calcDifBetweenAngles(Meth.Rad4Deg(sisLidar.AngCurr), Meth.Rad4Deg(angleToTarget));
		//Log($"Dif: {angleDifference} {sisLidar.AngCurr}");

		//Take a look to this later.
		double angleDifference = -Meth.ObterDifPos(targetPoint, Meth.Rad4Deg(sisLidar.AngCurr), sisLidar.PosCurr);//Meth.calcDifBetweenAngles(Meth.Rad4Deg(sisLidar.AngCurr), Meth.Rad4Deg(angleToTarget));
		sisMotores.MandarSteer(Meth.SpeedControl((int)(angleDifference / 180 * 800), 50), vel);
	}



	#region LOG
	string texto_log = "";
	List<string> logs = new();

	void Log(object txt)
	{
		if (logs.Count > 100)
		{
			logs.RemoveAt(0);
		}
		logs.Add($"{DateTime.Now.Second}: " + txt);
		texto_log = logs.ToArray().Join("\n");
		//GD.Print(txt);
	}


	#endregion



	int Contador_update = 0;
	StandardMaterial3D matPosConc = new StandardMaterial3D() { AlbedoColor = new Godot.Color(1, 1, 0.8f) };

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//sisLidar.PosCurr = new System.Numerics.Vector2(camera.Position.X, camera.Position.Z);
		//Log(sisLidar.PosCurr.X.ToString());
		if (Contador_update > 30)
		{
			SpriteMapa.Texture = image;

			//GD.Print(Convert.ToHexString(SHA256.HashData(Dados_mapa)));

			//new Vector3(PosLidarParaJogo(dados_recs.Pose.X), 0, (dados_recs.Pose.Y - TamMapa / 2) / (float)DivMapa);

			if (boxParaPoses.Count > idx_seg)
			{
				for (int i = 1; i < idx_seg + 1; i++)
				{
					boxParaPoses[i - 1].Material = matPosConc;
				}
			}
			LogTextBox.Text = texto_log;
			Contador_update = 0;
		}
		if (Contador_update % 4 == 0)
		{
			lidar.Position = OpVec.Vector2pVector3(sisLidar.PosCurr);
			lidar.Rotation = new Vector3(lidar.Rotation.X, sisLidar.AngCurr, lidar.Rotation.Z);

			scanCloud.Rotation = new Vector3(scanCloud.Rotation.X, -sisLidar.AngCurr * 2.0f, scanCloud.Rotation.Z);//tem de ser *2 pois ao virar o child "lidar" tmb movemos a cloud. a outra é para que n se mexa e fique no seu lugar

		}
		if(!Camera.ModoCamera){
			sisMotores.MandarSteer((int)((Input.GetActionStrength("DireitaMover") - Input.GetActionStrength("EsquerdaMover")) * 50), (int)((Input.GetActionStrength("CimaMover") - Input.GetActionStrength("BaixoMover")) * 100));
		}
		Contador_update++;
	}



	void AdicionarPosObj(Vector2 pos, float tamApontador)
	{
		posesParaObj.Add(pos);
		var matPos = new StandardMaterial3D() { AlbedoColor = new Color(0.5f, 0.2f, 1) };

		boxParaPoses.Add(new CsgBox3D()
		{
			Position = OpVec.Vector2pVector3(pos),
			Size = new Vector3(0.01f, 1, tamApontador),
			Material = matPos,
		});
		AddChild(boxParaPoses[boxParaPoses.Count - 1]);

	}

	public override void _Input(InputEvent @event)
	{

	 if (Input.GetActionStrength("MudarModoMover") ==1)
			{
				Camera.ModoCamera = !Camera.ModoCamera;
	
				Log("Modo usar camera: " + Camera.ModoCamera);
			}
	
			if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			//Definir img
			if (keyEvent.Keycode == Key.J)
			{
				SpriteMapa.Texture = image;
			}

			//Y definir objetivo
			else if (keyEvent.Keycode == Key.Y)
			{
				//AdicionarPosObj(new Vector2((float)Math.Round(camera.Position.X, 2), (float)Math.Round(camera.Position.Z, 2)));
				posObj = new Vector2((float)Math.Round(camera.Position.X, 2), (float)Math.Round(camera.Position.Z, 2));
				posObjCaixa.Position = OpVec.Vector2pVector3(posObj);

			}

			//T é o novo. Y(def obj) -> T(fazer percurso) -> G(go, ir para o lugar, mexer o robo)
			else if (keyEvent.Keycode == Key.T)
			{
				Log("T was pressed");

				Log("A fazer limpeza...");
				posesParaObj.Clear();
				foreach (var child in boxParaPoses)
				{
					RemoveChild(child);
				}
				boxParaPoses.Clear();
				idx_seg = 0;
				posesParaObj = new();
				Log("Ok!");

				//(posmapa - TamMapa / 2) / (float)DivMapa de mapa para jogo
				//new System.Numerics.Vector2(pos_obj.Position.X * (float)DivMapa + (TamMapa / 2), pos_obj.Position.Z * (float)DivMapa + (TamMapa / 2));

				posObj = OpVec.Vector3pVector2(posObjCaixa.Position);

				Log("A fazer distance tranform...");
				var sw = Stopwatch.StartNew();

				var saida = sisFSD.Fazer(TamMapa, sisMapa.Dados_mapa, sisLidar.PosCurr, posObj);

				image = ImageTexture.CreateFromImage(Image.CreateFromData(TamMapa, TamMapa, false, Image.Format.L8, saida.ImgTransformada));
				SpriteMapa.Texture = image;

				foreach (var ponto in saida.PontosCaminho)
				{
					AdicionarPosObj(ponto, 0.01f);
				}
				foreach (var ponto in saida.PontosCaminhoRaw)
				{
					//AdicionarPosObj(ponto, 0.05f);
				}

				Log("Tempo total: " + sw.ElapsedMilliseconds);

				sw.Restart();
				Log($"Obj jogo: {posObj} start lidar: {sisLidar.PosCurr} end lidar: {posObj}");


			}

			//Go, Seguir caminho
			else if (keyEvent.Keycode == Key.G)
			{
				Emerg = false;
				new Thread(() =>
				{
					SeguirPontos(posesParaObj);
				}).Start();
			}

			//Fazer limpeza(limpar todos os pontos obj)
			else if (keyEvent.Keycode == Key.Backspace)
			{

				Log("A fazer limpeza...");
				posesParaObj.Clear();
				foreach (var child in boxParaPoses)
				{
					RemoveChild(child);
				}
				boxParaPoses.Clear();
				idx_seg = 0;
				posesParaObj = new();
				Log("Ok!");

			}

			//Para emergencia
			else if (keyEvent.Keycode == Key.P)
			{
				Log("A parar!!");
				for (int i = 0; i < 20; i++)
				{
					sisMotores.MandarSteer(0, 0);
					Emerg = true;
					Thread.Sleep(50);
				}
				Log("Parar ok!");
			}

			//Testar mov
			else if (keyEvent.Keycode == Key.V)
			{
				new Thread(() =>
				{
					var sw = Stopwatch.StartNew();
					while (!Emerg && sw.ElapsedMilliseconds < 4000)
					{
						sisMotores.MandarSteer(0, 90);
					}
					sisMotores.MandarSteer(0, 0);
					Thread.Sleep(500);
					while (!Emerg && sw.ElapsedMilliseconds < 2500)
					{
						sisMotores.MandarSteer(100, 0);
					}
					sisMotores.MandarSteer(0, 0);


				}).Start();
			}

			//Obter a dif de distancia do ang 0
			else if (keyEvent.Keycode == Key.L)
			{
				var ang = 0;
				var ang0 = dadosLidar.OrderBy(item => Math.Abs(ang - item.Angle)).First();
				Log($"Ang 0 dist: {ang0.Radius} {ang0.Angle}");
			}

			//Trocar modo Comando/Controlar robo.
					/*if (keyEvent.Keycode == Key.C)
			{

				//new Thread(() =>
				//{

				Mat mat = new Mat(TamMapa, TamMapa, DepthType.Cv8U, 1);
				mat.SetTo(sisMapa.Dados_mapa);
				//Mat mate = new Mat(TamMapa, TamMapa, MatType.CV_8U, sisMapa.Dados_mapa);
				CvInvoke.Normalize(mat, mat, 0, 255.0, normType: NormType.MinMax, dType: DepthType.Cv8U);
				CvInvoke.Threshold(mat, mat, 200, 255, ThresholdType.Binary);

				//new Window("image1", mat);
				//CvInvoke.WaitKey();

				CvInvoke.DistanceTransform(mat, mat, null, DistType.L2, 3);
				CvInvoke.Normalize(mat, mat, 0, 255.0, NormType.MinMax, dType: DepthType.Cv8U);



				Log("Ok!");
				byte[] ExpandedMapData = new byte[TamMapa * TamMapa];
				Log("A copy...");
				mat.CopyTo(ExpandedMapData);

				image = ImageTexture.CreateFromImage(Image.CreateFromData(TamMapa, TamMapa, false, Image.Format.L8, ExpandedMapData));
				Log("Ok final!");
				SpriteMapa.Texture = image;

				//}).Start();
			}*/
		}
	}


}
