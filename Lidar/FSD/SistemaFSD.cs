using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Vector2 = System.Numerics.Vector2;

class SistemaFSD
{
	public float Mult = 10.0f;
	public int IDXSelect = 1;
	public bool UsarTransforms = true;
	public ResultadoPrepFSD Fazer(int TamMapa, byte[] DadosMapa, Vector2 posCurr, Vector2 posObj)
	{
		ResultadoPrepFSD saida = new();

		//var sw = Stopwatch.StartNew();
		Godot.GD.Print("A fazer tranform...");

		//Fazemos o mapa de distancias
		var DadosMapaTransformados = FazerDistanceTransformMapa(DadosMapa, TamMapa);
		saida.ImgTransformada = DadosMapaTransformados;
		Godot.GD.Print("Tranform OK!");

		//Resolvemos o mapa de distancias(fazer Path Finding)
		var SaidaPathFinder = ResolverMapa(DadosMapaTransformados, TamMapa, posCurr, posObj);
		saida.PontosCaminhoRaw = OpVec.PointSLAMParaVector2(SaidaPathFinder);//Usamos pontos em forma de SLAM em vez de vector2 de jogo, ou seja precisamos de converter
		Godot.GD.Print("Caminho OK!" + SaidaPathFinder.Count);

		//Fazer o tratamento
		var tratamento = FazerTratamentoPontos(SaidaPathFinder);
		saida.PontosCaminho = OpVec.PointSLAMParaVector2(tratamento);
		Godot.GD.Print("Tratamento OK!");

		saida.PontosCaminho.Add(posObj);
		return saida;
	}


	/// <summary>
	/// Faz o transform do mapa de 0 e 1 para um mapa de distancias
	/// </summary>
	/// <param name="DadosMapa">mapa. 1 pixel = 1 cm</param>
	/// <param name="TamMapa"></param>
	/// <returns></returns>
	private byte[] FazerDistanceTransformMapa(byte[] DadosMapa, int TamMapa)
	{
		//Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", $"{Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")}:/home/test123/testeEMGU/bin/Debug/net8.0/runtimes/linux-arm64/native");

		if(!UsarTransforms) return DadosMapa;
		// 1 é livre 0 é fechado
		var sw = Stopwatch.StartNew();
		Mat mat = new Mat(TamMapa, TamMapa, DepthType.Cv8U, 1);
		Godot.GD.Print("Mat criado!!... " + sw.ElapsedMilliseconds);

		mat.SetTo(DadosMapa);

		CvInvoke.Normalize(mat, mat, 0, 255.0, normType: NormType.MinMax, dType: DepthType.Cv8U);
		CvInvoke.Threshold(mat, mat, 200, 255, ThresholdType.Binary);
		mat.Save("mapa.png");

		//Tamanho robo
		Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(30, 30), new Point(-1, -1));
		CvInvoke.Erode(mat, mat, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
		CvInvoke.DistanceTransform(mat, mat, null, DistType.L2, 3);
		CvInvoke.Normalize(mat, mat, 0, 255.0, NormType.MinMax, dType: DepthType.Cv8U);

		byte[] DadosMapaTransformados = new byte[TamMapa * TamMapa];
		mat.CopyTo(DadosMapaTransformados);
		mat.Dispose();
		return DadosMapaTransformados;//DadosMapaTransformados
	}


	/// <summary>
	/// Resolve o mapa
	/// </summary>
	/// <param name="DadosMapa">Têm de ser após o distance transform!!</param>
	/// <returns>A lista de todos os ponto em pos de SLAM</returns>
	private List<Point> ResolverMapa(byte[] DadosMapa, int TamMapa, Vector2 posCurr, Vector2 posObj)
	{
		var worldGrid = new AStar.WorldGrid(TamMapa, TamMapa);
		Parallel.For(0, DadosMapa.Length, (i) =>
		{
			//Quanto maior o valor, o melhor.
			//worldGrid._grid[i] = (short)(((-DadosMapa[i]) / 255.0f) * Mult);//Preciso de div por 3 para ou snx n faz caminho  / 3

			//255.0f -
			//worldGrid._grid[i] = (short)((255 - DadosMapa[i]) / 10.0f);//Preciso de div por 3 para ou sn n faz caminho  / 3


			//worldGrid._grid[i] = (short)( DadosMapa[i] / 3);//Preciso de div por 3 para ou sn n faz caminho  / 3

			//worldGrid._grid[i] = (DadosMapa[i] == 255) ? (short)0 : (short)1;
			worldGrid._grid[i] = (DadosMapa[i] == 255) ? (short)0 : (short)(((DadosMapa[i]) / 255.0f) * Mult);

		});

		var pathfinderOptions = new AStar.Options.PathFinderOptions
		{
			SearchLimit = TamMapa * TamMapa,
			UseDiagonals = true, //tut diz false
			PunishChangeDirection = false, // tut diz true
			HeuristicFormula = (AStar.Heuristics.HeuristicFormula)IDXSelect, //sqr
			Weighting = AStar.Options.Weighting.Positive //Positive
		};
		var pathfinder = new AStar.PathFinder(worldGrid, pathfinderOptions);

		var ResultadoPathFinder = pathfinder.FindPath(
			OpVec.Vector2pPoint(SistemaLidar.PosJogoParaSLAM(posCurr)),
			OpVec.Vector2pPoint(SistemaLidar.PosJogoParaSLAM(posObj))
		).ToList();


		return ResultadoPathFinder;
	}

	/// <summary>
	/// Faz o tratamento dos pontos para se obter uma linha reta. Ver docs.
	/// </summary>
	/// <param name="pontos"></param>
	/// <returns></returns>
	private List<Point> FazerTratamentoPontos(List<Point> pontos)
	{
		double aX = 0;
		double bX = 0;

		double aY = 0;
		double bY = 0;


		var pontosSaida = new List<Point>();
		for (int i = 0; i < pontos.Count - 1; i += 1)
		{
			var pontoCurr = pontos[i];
			var pontoProx = pontos[i + 1];

			double x = i;
			double y = pontoProx.X;
			if ((aX * x + bX) != y)
			{
				pontosSaida.Add(pontoCurr);
				aX = (pontoCurr.X - pontoProx.X) / (x - (x + 1.0));//y por x
				bX = y - aX * x;
			}

			x = i;
			y = pontoProx.Y;
			if ((aY * x + bY) != y)
			{
				pontosSaida.Add(pontoCurr);
				aY = (pontoCurr.Y - pontoProx.Y) / (x - (x + 1.0));//#y por x
				bY = y - aY * x;
			}
		}
		//pontosSaida.RemoveAt(0);//Remover o primeiro ponto
		var pontoInicial = pontosSaida[0];
		var li = new List<Point>(pontosSaida);
		foreach(var ponto in li){
			if(Meth.ObterDist(pontoInicial, ponto) < 20){
				pontosSaida.Remove(ponto);
			}
		}
		return pontosSaida;
	}

}
class ResultadoPrepFSD
{
	/// <summary>
	/// Imagem tranformada do distance transform
	/// </summary>
	public byte[] ImgTransformada { get; set; }

	/// <summary>
	/// A lista de pontos da saida antes de ser tratada para que seja linear
	/// </summary>
	public List<Vector2> PontosCaminhoRaw { get; set; }

	/// <summary>
	/// Deve-se usar quando é FSD. è o resultado do tratamento de PontosCaminhoRaw
	/// </summary>
	public List<Vector2> PontosCaminho { get; set; }

}
