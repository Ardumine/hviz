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

	string IP = "192.168.137.5";//192.168.137.5 127.0.0.1
	ScanCloud scanCloud;

	CsgBox3D lidar;
	CsgBox3D posObjCaixa;
	Camera camera;
	RichTextLabel LogTextBox;
	Slider sliderImportPesoFSD;
	OptionButton optTipoAlgo;
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

		sliderImportPesoFSD = GetNode<Slider>("../UI_FSD/gd/peso/slider");
		optTipoAlgo = GetNode<OptionButton>("../UI_FSD/gd/heuristic/optTipoAlgo");
		Iniciar();

		optTipoAlgo.AddItem("Manhattan");
		optTipoAlgo.AddItem("MaxDXDY");
		optTipoAlgo.AddItem("DiagonalShortCut");
		optTipoAlgo.AddItem("Euclidean");
		optTipoAlgo.AddItem("EuclideanNoSQR");
		optTipoAlgo.AddItem("Custom1");
		optTipoAlgo.Selected = 3;
		sisFSD.IDXSelect = 3 + 1;

		optTipoAlgo.ItemSelected += (e) =>
		{
			sisFSD.IDXSelect = optTipoAlgo.Selected + 1;
		};

	}

	public void Iniciar()
	{
		//pos_obj.Visible = false;

		sisLidar = new SistemaLidar(TamMapa);
		sisLidar.Iniciar(IP);
		sisLidar.OnDadosRecebidos += (dados) =>
		{
			Log("Dados Lidar REC!" + dados.Count);
			scanCloud.UpdatePontos(dados);
			dadosLidar = dados;
		};

		sisMapa = new SistemaMapa();
		sisMapa.Iniciar(IP, TamMapa);
		sisMapa.OnDadosRecebidos += (dados) =>
		{
			image = ImageTexture.CreateFromImage(Image.CreateFromData(TamMapa, TamMapa, false, Image.Format.L8, dados));
		};

		sisMotores = new SistemaMotores();
		sisMotores.IniMotores();

		sisFSD = new SistemaFSD();
		Log(System.Environment.GetEnvironmentVariable("LD_LIBRARY_PATH"));
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
				sisMotores.MandarSteer(0, 0);
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

		var currentLocation = sisLidar.PosCurr;
		double Dist_Prox = Math.Sqrt(Math.Pow(currentLocation.X - targetPoint.X, 2) + Math.Pow(currentLocation.Y - targetPoint.Y, 2));

		if (ModoContinuo)
		{
			if (Math.Abs(dif) > 15 && Dist_Prox > 0.15)
			{
				//Rodar
				RodarParaGrau(targetPoint, true, 30, token);
			}
			if (token.IsCancellationRequested || Emerg) return;
			NavegarParaPonto(targetPoint, true, 0.2, 1.1, token);

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
			Log($"RDif: {angleDifference} SLAC: {sisLidar.AngCurr} SC_S: {steer}");
			sisMotores.MandarSteer(steer, 60);
			if (token.IsCancellationRequested || Emerg) return;
			Thread.Sleep(30);

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

			int velRaw = (int)(Dist_Prox / Dist_final_inicio * 800.0 * multVel);
			int vel = Meth.SpeedControl(velRaw, 70);

			RotinaUpdateRodarParaPonto(targetPoint, vel);

			Log(Dist_Prox + "   aD" + Math.Round(angleDifference, 4) + "      vel: " + vel
			);
			Thread.Sleep(20);
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
		sisMotores.MandarSteer(Meth.SpeedControl((int)(angleDifference / 180 * 500), 45), vel);
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
	StandardMaterial3D matPosFazer = new StandardMaterial3D() { AlbedoColor = new Godot.Color(1, 0.5f, 0.8f) };

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//sisLidar.PosCurr = new System.Numerics.Vector2(camera.Position.X, camera.Position.Z);
		//Log(sisLidar.PosCurr.X.ToString());
		if (Contador_update > 10)
		{
			SpriteMapa.Texture = image;

			//GD.Print(Convert.ToHexString(SHA256.HashData(Dados_mapa)));

			//new Vector3(PosLidarParaJogo(dados_recs.Pose.X), 0, (dados_recs.Pose.Y - TamMapa / 2) / (float)DivMapa);

			if (boxParaPoses.Count > 0)
			{
				try
				{
					for (int i = 1; i < idx_seg; i++)
					{
						boxParaPoses[i - 1].Material = matPosConc;

					}
					boxParaPoses[idx_seg - 1].Material = matPosFazer;

				}
				catch
				{

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
		if (!Camera.ModoCamera)
		{
			sisMotores.MandarSteer((int)((Input.GetActionStrength("DireitaMover") - Input.GetActionStrength("EsquerdaMover")) * 70), (int)((Input.GetActionStrength("CimaMover") - Input.GetActionStrength("BaixoMover")) * 100));
		}
		HandleActions();
		Contador_update++;
		sisFSD.Mult = (float)sliderImportPesoFSD.Value;

	}



	void AdicionarPosObj(Vector2 pos, float tamApontador, bool Prior, float IDX)
	{
		if (Prior) posesParaObj.Add(pos);
		var matPos = new StandardMaterial3D() { AlbedoColor = new Color(0.5f, 0.2f, tamApontador) };

		boxParaPoses.Add(new CsgBox3D()
		{
			Position = OpVec.Vector2pVector3(pos, IDX),
			Size = new Vector3(0.01f, tamApontador, 0.01f),
			Material = matPos,
		});
		AddChild(boxParaPoses[boxParaPoses.Count - 1]);

	}


	void HandleActions()
	{
		if (Input.IsActionJustReleased("MudarModoMover"))
		{
			Camera.ModoCamera = !Camera.ModoCamera;

			Log("Modo usar camera: " + Camera.ModoCamera);
		}

		//Y definir objetivo
		else if (Input.IsActionJustReleased("BtnY"))
		{
			posObj = new Vector2((float)Math.Round(camera.Pivot.Position.X, 2), (float)Math.Round(camera.Pivot.Position.Z, 2));
			posObjCaixa.Position = OpVec.Vector2pVector3(posObj);
		}
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			//Definir img
			if (keyEvent.Keycode == Key.J)
			{
				SpriteMapa.Texture = image;
			}

			//Não usado
			else if (keyEvent.Keycode == Key.Y)
			{
				//AdicionarPosObj(new Vector2((float)Math.Round(camera.Position.X, 2), (float)Math.Round(camera.Position.Z, 2)));
			}

			else if (keyEvent.Keycode == Key.Z)
			{
				//sisFSD.Mult -= 1;
			}
			else if (keyEvent.Keycode == Key.X)
			{
				//sisFSD.Mult += 1;
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
				Log("Limpesa ok!");

				//(posmapa - TamMapa / 2) / (float)DivMapa de mapa para jogo
				//new System.Numerics.Vector2(pos_obj.Position.X * (float)DivMapa + (TamMapa / 2), pos_obj.Position.Z * (float)DivMapa + (TamMapa / 2));

				posObj = OpVec.Vector3pVector2(posObjCaixa.Position);

				Log("A fazer...");
				var sw = Stopwatch.StartNew();

				var saida = sisFSD.Fazer(TamMapa, sisMapa.Dados_mapa, sisLidar.PosCurr, posObj);
				Log("Concluido: " + sw.ElapsedMilliseconds + "ms");

				image = ImageTexture.CreateFromImage(Image.CreateFromData(TamMapa, TamMapa, false, Image.Format.L8, saida.ImgTransformada));
				SpriteMapa.Texture = image;

				for (int i = 0; i < saida.PontosCaminho.Count; i++)
				{
					Vector2 ponto = saida.PontosCaminho[i];
					AdicionarPosObj(ponto, 1.1f, true, i / 100.0f);
				}
				for (int i = 0; i < saida.PontosCaminhoRaw.Count; i++)
				{
					Vector2 ponto = saida.PontosCaminhoRaw[i];
					AdicionarPosObj(ponto, 0.09f, false, i / 1000.0f);
				}

				Log("Tempo total: " + sw.ElapsedMilliseconds + "ms");

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
