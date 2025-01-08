using Kernel.AFCP;
using Ardumine.Modules.YDLidar;
using Ardumine.SistemaFSD.GUI;
using Godot;
using Kernel.Modules;
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
	Vector2 PosCursor = Vector2.Zero;

	string IP = "127.0.0.1";//192.168.137.5 127.0.0.1
	ScanCloud? scanCloud;

	CsgBox3D? BoxApontador;
	CsgBox3D? lidar;
	CsgBox3D? posObjCaixa;
	Camera? camera;
	RichTextLabel? LogTextBox;
	Slider? sliderImportPesoFSD;
	OptionButton? optTipoAlgo;
	Sprite3D? SpriteMapa;
	ImageTexture? image;
	Fsd3d? fsd3D;


	static int TamMapa = 1000;
	SistemaLidar? sisLidar;
	SistemaMotores? sisMotores;
	SistemaMapa? sisMapa;
	Ardumine.SistemaFSD.PlaneadorAuto? sisFSD;
	Ardumine.SistemaFSD.Condutor.CondutorCaminhoAuto? condutorAuto;

	LidarPoint[]? dadosLidar;


	List<CsgBox3D> boxParaPoses = new();
	List<Vector2> posesParaObj = new();


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		lidar = GetNode<CsgBox3D>("lidar");
		scanCloud = GetNode<ScanCloud>("lidar/scanCloud");
		SpriteMapa = GetNode<Sprite3D>("Mapa");
		posObjCaixa = GetNode<CsgBox3D>("pos_obj");
		camera = GetNode<Camera>("../Camera");
		BoxApontador = GetNode<CsgBox3D>("../BoxApontador");
		LogTextBox = GetNode<RichTextLabel>("../LogTextBox");
		fsd3D = GetNode<Fsd3d>("../fsd3D");

		sliderImportPesoFSD = GetNode<Slider>("../UI_FSD/gd/peso/slider");
		optTipoAlgo = GetNode<OptionButton>("../UI_FSD/gd/heuristic/optTipoAlgo");


		Camera.OnClick3D += (pos) =>
		{
			PosCursor = OpVec.Vector3pVector2(pos);
			BoxApontador.Position = OpVec.Vector2pVector3(PosCursor);
		};


		optTipoAlgo.AddItem("Manhattan");
		optTipoAlgo.AddItem("MaxDXDY");
		optTipoAlgo.AddItem("DiagonalShortCut");
		optTipoAlgo.AddItem("Euclidean");
		optTipoAlgo.AddItem("EuclideanNoSQR");
		optTipoAlgo.AddItem("Custom1");
		optTipoAlgo.Selected = 3;

		Iniciar();


		sisFSD!.IDXSelect = 3 + 1;

		optTipoAlgo.ItemSelected += (e) =>
		{
			sisFSD.IDXSelect = optTipoAlgo.Selected + 1;
		};

	}

	public void Iniciar()
	{
		GD.Print("Load f1!");


		var ConnectedKernels = new List<KernelDescriptor>();

		var channelManager = new ChannelManager() { ConnectedKernels = ConnectedKernels };

		var moduleManager = new ModuleManager(channelManager);
		GD.Print("Join!");

		channelManager.Join("127.0.0.1", 8000);



		GD.Print("Load f2!");
		sisLidar = new SistemaLidar(TamMapa);
		sisLidar.Iniciar(channelManager);
		sisLidar!.OnDadosRecebidos += (dados) =>
		{
			Log("Dados Lidar REC!" + dados.Length);
			scanCloud!.UpdatePontos(dados);
			dadosLidar = dados;
		};

		sisMapa = new SistemaMapa();
		sisMapa.Iniciar(channelManager, TamMapa);
		//sisMapa.LoadMapaBMP("testing/testingMap.bmp");

		sisMapa.OnDadosRecebidos += (dados) =>
		{
			image = ImageTexture.CreateFromImage(Image.CreateFromData(TamMapa, TamMapa, false, Image.Format.L8, dados));
		};

		sisMotores = new SistemaMotores(channelManager);

		sisFSD = new();
		condutorAuto = new();
		Log(System.Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")!);
		GD.Print("Load OK!");
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
		if (Contador_update > 10)
		{
			SpriteMapa!.Texture = image;

			if (boxParaPoses.Count > 0)
			{
				try
				{
					for (int i = 1; i < condutorAuto!.IdxCurr; i++)
					{
						boxParaPoses[i - 1].Material = matPosConc;

					}
					boxParaPoses[condutorAuto.IdxCurr - 1].Material = matPosFazer;

				}
				catch
				{
				}

			}
			Contador_update = 0;
		}
		if (Contador_update % 4 == 0)
		{
			lidar!.Position = OpVec.Vector2pVector3(sisLidar!.PosCurr);
			lidar.Rotation = new Vector3(lidar.Rotation.X, sisLidar.AngCurr, lidar.Rotation.Z);

			scanCloud!.Rotation = new Vector3(scanCloud.Rotation.X, -sisLidar.AngCurr * 2.0f, scanCloud.Rotation.Z);//tem de ser *2 pois ao virar o child "lidar" tmb movemos a cloud. a outra é para que n se mexa e fique no seu lugar

		}
		if (!Camera.ModoCamera)
		{
			sisMotores!.MandarSteer((int)((Input.GetActionStrength("DireitaMover") - Input.GetActionStrength("EsquerdaMover")) * 100), (int)((Input.GetActionStrength("CimaMover") - Input.GetActionStrength("BaixoMover")) * 100));
		}
		LogTextBox!.Text = texto_log;

		HandleActions();
		Contador_update++;
		sisFSD!.Mult = (float)sliderImportPesoFSD!.Value;

	}



	void AdicionarPosObj(Vector2 pos, float tamApontador, bool Prior, float IDX)
	{
		if (Prior) posesParaObj.Add(pos);
		var matPos = new StandardMaterial3D() { AlbedoColor = new Color(0.5f, 0.2f, tamApontador) };

		boxParaPoses.Add(new CsgBox3D()
		{
			Position = OpVec.Vector2pVector3(pos, 0),//IDX	
			Size = new Vector3(0.01f, tamApontador, 0.01f),
			Material = matPos,
		});
		Call(MethodName.AddChild, boxParaPoses[boxParaPoses.Count - 1]);
	}


	void FazerLimpeza()
	{

		Log("A fazer limpeza...");

		foreach (var child in boxParaPoses)
		{
			//RemoveChild(child);
			child.Call(MethodName.QueueFree);
			Call(MethodName.RemoveChild, child);
		}

		posesParaObj.Clear();
		boxParaPoses.Clear();

		Log("Limpesa ok!");
	}

	void FazerTrace()
	{
		Log("T was pressed");
		FazerLimpeza();

		//(posmapa - TamMapa / 2) / (float)DivMapa de mapa para jogo
		//new System.Numerics.Vector2(pos_obj.Position.X * (float)DivMapa + (TamMapa / 2), pos_obj.Position.Z * (float)DivMapa + (TamMapa / 2));

		posObj = OpVec.Vector3pVector2(posObjCaixa!.Position);

		Log("A fazer...");
		var sw = Stopwatch.StartNew();

		var saida = sisFSD!.Fazer(TamMapa, sisMapa!.Dados_mapa, sisLidar!.PosCurr, posObj, sisLidar.AngCurr);
		Log("Concluido: " + sw.ElapsedMilliseconds + "ms");

		image = ImageTexture.CreateFromImage(Image.CreateFromData(TamMapa, TamMapa, false, Image.Format.L8, saida.ImgTransformada));
		//SpriteMapa.Texture = image;

		for (int i = 0; i < saida.PontosCaminho!.Count; i++)
		{
			Vector2 ponto = saida.PontosCaminho[i];
			AdicionarPosObj(ponto, 1.1f, true, i / 100.0f);
		}


		for (int i = 0; i < saida.PontosCaminhoRaw!.Count; i++)
		{
			Vector2 ponto = saida.PontosCaminhoRaw[i];
			AdicionarPosObj(ponto, 0.09f, false, i / 1000.0f);
		}

		Log("Tempo total: " + sw.ElapsedMilliseconds + "ms");

		sw.Restart();
		Log($"Obj jogo: {posObj} start lidar: {sisLidar.PosCurr} Pontos:: {posesParaObj.Count}");

		condutorAuto!.Preparar(sisLidar, sisMotores!, Log);
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
			//			posObj = new Vector2((float)Math.Round(camera.Pivot.Position.X, 2), (float)Math.Round(camera.Pivot.Position.Z, 2));
			posObj = PosCursor;

			posObjCaixa!.Position = OpVec.Vector2pVector3(posObj);
		}
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			//Definir img
			if (keyEvent.Keycode == Key.J)
			{
				image = ImageTexture.CreateFromImage(Image.CreateFromData(TamMapa, TamMapa, false, Image.Format.L8, sisMapa!.Dados_mapa));
				SpriteMapa!.Texture = image;
			}

			else if (keyEvent.Keycode == Key.H)
			{
				sisLidar!.PosCurr = PosCursor;
			}

			else if (keyEvent.Keycode == Key.Z)
			{
				camera!.ModoSet2D(!camera.Modo2D);
				Log($"Modo camera 2D: {camera.Modo2D}");
			}

			//T é o novo. Y(def obj) -> T(fazer percurso) -> G(go, ir para o lugar, mexer o robo)
			else if (keyEvent.Keycode == Key.T)
			{
				FazerTrace();
			}

			//Go, Seguir caminho
			else if (keyEvent.Keycode == Key.G)
			{
				Emerg = false;

				new Thread(() =>
				{
					condutorAuto!.SeguirPontos(posesParaObj);
					//SeguirPontos(posesParaObj);
				}).Start();
			}

			//Fazer limpeza(limpar todos os pontos obj)
			else if (keyEvent.Keycode == Key.Backspace)
			{
				FazerLimpeza();
				/*Log("A fazer limpeza...");
				posesParaObj.Clear();
				foreach (var child in boxParaPoses)
				{
					RemoveChild(child);
				}
				boxParaPoses.Clear();
				idx_seg = 0;
				posesParaObj = new();
				Log("Ok!");
*/
			}

			//Para emergencia
			else if (keyEvent.Keycode == Key.P)
			{
				Log("A parar!!");
				condutorAuto!.PararEmerg();
				for (int i = 0; i < 20; i++)
				{
					sisMotores!.MandarSteer(0, 0);
					Emerg = true;
					Thread.Sleep(20);
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
						sisMotores!.MandarSteer(0, 90);
					}
					sisMotores!.MandarSteer(0, 0);
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
				//var ang = 0;
				//var ang0 = dadosLidar!.OrderBy(item => Math.Abs(ang - item.AngleRad)).First();
				//Log($"Ang 0 dist: {ang0.Distance} {ang0.AngleRad}");
			}

		}
	}


}
