using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Vector2 = System.Numerics.Vector2;

namespace Ardumine.SistemaFSD.GUI;
public partial class Fsd3d : Node3D
{
	PackedScene RecursoPontoPathDesgin;
	BaseMaterial3D shaderCorPontoIncial;
	BaseMaterial3D shaderCorPontoFinal;
	Path3D PathSpline;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RecursoPontoPathDesgin = GD.Load<PackedScene>("res://Modelos3D/FSD/PontoPathDesgin.tscn");


		shaderCorPontoIncial = GD.Load<BaseMaterial3D>("res://Modelos3D/FSD/shaderMaterialPontoInicial.tres");
		shaderCorPontoFinal = GD.Load<BaseMaterial3D>("res://Modelos3D/FSD/shaderMaterialPontoFinal.tres");
		PathSpline = GetNode<Path3D>("PathSpline");
		Camera.OnClick3D += (e) =>
		{
			if (AtivarMover)
			{
				var sw = Stopwatch.StartNew();
				//ReDrawPontos(ObterPontosTotais());
				UpdateDesginerPontos();
				//GD.Print($"Tempo update loop: {sw.ElapsedMilliseconds}");
			}
		};
		/*OnBtnAdicionarCurva(new Vector2(0.5f, -0.022f), 2.8f, -0.1f);//0
		OnBtnAdicionarCurva(new Vector2(4.1f, 0.8f));
		OnBtnAdicionarCurva(new Vector2(4.3f, 3.4f));
		OnBtnAdicionarCurva(new Vector2(3.07f, 4.28f));
		OnBtnAdicionarCurva(new Vector2(0.45f, 4.38f));
		OnBtnAdicionarCurva(new Vector2(-0.5f, 3.006f));
		OnBtnAdicionarCurva(new Vector2(-2.28f, 1.65f));
		AtivarMover = false;
*/

	}

	PontoPathDesgin AdicionarPontoPathDesgin(Vector2 pos, int id, Guid GuidCaminho, bool Inicial)
	{
		var node = (PontoPathDesgin)RecursoPontoPathDesgin.Instantiate();

		node.ID = id;
		node.Position = OpVec.Vector2pVector3(pos);
		node.GuidCaminho = GuidCaminho;
		node.Inicial = Inicial;
		if (Inicial)
		{
			node.GetChild<CsgPrimitive3D>(0).MaterialOverlay = shaderCorPontoIncial;
			node.GetChild<CsgPrimitive3D>(1).MaterialOverlay = shaderCorPontoIncial;
			node.GetChild<Label3D>(2).Position = new Vector3(0, 0.2f, 0);

		}
		else
		{
			node.GetChild<CsgPrimitive3D>(0).MaterialOverlay = shaderCorPontoFinal;
			node.GetChild<CsgPrimitive3D>(1).MaterialOverlay = shaderCorPontoFinal;
			node.GetChild<Label3D>(2).Position = new Vector3(0, 0.56f, 0);

		}
		node.GetChild(0).GetChild(0).GetChild<CollisionShape3D>(0).Disabled = true;
		node.GetChild(1).GetChild(0).GetChild<CollisionShape3D>(0).Disabled = true;

		node.GetChild<Label3D>(2).Text = id.ToString();

		AddChild(node);
		return node;
	}
	PontoPathDesgin ObterPontoPathDesgin(int id)
	{
		Godot.Collections.Array<Node> list = GetChildren();
		for (int i = 0; i < list.Count; i++)
		{
			Node item = list[i];
			if (@item is PontoPathDesgin ponto)
			{
				if (ponto.ID == id)
				{
					return ponto;
				}
			}
		}


		return null;
	}

	public List<Vector2> ObterPontos(Vector2 ini, Vector2 fim, double angOffSetDeg = 0)
	{
		List<Vector2> saida = new();

		var dx = fim.X - ini.X;
		var dy = fim.Y - ini.Y;

		//var angel = Meth.Rad4Deg(Math.Atan2(dy, dx));

		double angIni = 270 + (angOffSetDeg * 2);//Meth.NormAng(270 + angOffSetDeg);
		double lim = 360 + (angOffSetDeg * 2);// Meth.NormAng

		//GD.Print($"Ang: {angel}  angOffSetDeg {angOffSetDeg}  angIni {angIni}    lim {lim}");


		for (double i = angIni; i < lim; i += 1)
		{
			double ang = Meth.Deg4Rad(i);
			float x = (float)Math.Cos(ang) + (float)Math.Sin(Meth.Deg4Rad(angOffSetDeg));//- 0.5f;
			float y = (float)Math.Sin(ang) + (float)Math.Cos(Meth.Deg4Rad(angOffSetDeg));//+ 0.5F;//+ 1 - 0.5

			x *= dx;
			y *= dy;

			x += ini.X;
			y += ini.Y;

			//saida.Add(Meth.RotatePoint(new Vector2(
			//	x, y
			//), 0));//angOffSetDeg * 2.
			saida.Add(new Vector2(
							x, y
						));//angOffSetDeg * 2.

		}

		if (angOffSetDeg >= 45)
		{
			saida.Reverse();
		}
		return saida;
	}


	void ReDrawPontos(List<Vector2> Pontos)
	{
		PathSpline.Curve.ClearPoints();
		for (int i = 0; i < Pontos.Count; i++)
		{
			PathSpline.Curve.AddPoint(OpVec.Vector2pVector3(Pontos[i], i / 2500.0f));

		}
	}

	List<IntrucaoPath> Instrucoes = new();
	public int UltIDIntruc = 0;

	public void OnBtnAdicionarCurva(Vector2 PosCursor, float opX = 1, float opY = 1)
	{
		IntrucaoPath novaIntru = new();
		novaIntru.Linear = false;
		novaIntru.guid = Guid.NewGuid();
		if (Instrucoes.Count == 0)
		{
			novaIntru.PInicial = PosCursor;
			novaIntru.PFinal = new Vector2(opX, opY);

			novaIntru.AngP = Meth.Rad4Deg(Meth.ObterDifAngRad(Vector2.Zero, novaIntru.PInicial));
			novaIntru.AngP = 0;

		}
		else
		{
			novaIntru.PInicial = Instrucoes.Last().PFinal;
			novaIntru.PFinal = PosCursor;

			novaIntru.AngP = Meth.Rad4Deg(Meth.ObterDifAngRad(Instrucoes.Last().PInicial, Instrucoes.Last().PFinal));
		}

		novaIntru.IDPInicial = UltIDIntruc;
		AdicionarPontoPathDesgin(novaIntru.PInicial, UltIDIntruc, novaIntru.guid, true);
		UltIDIntruc++;

		novaIntru.IDPFinal = UltIDIntruc;
		PontoMoverSelecionado = AdicionarPontoPathDesgin(novaIntru.PFinal, UltIDIntruc, novaIntru.guid, false);
		UltIDIntruc++;

		Instrucoes.Add(novaIntru);
		UpdateDesginerPontos();

		AtivarMover = true;

	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (AtivarMover)
		{
			PontoMoverSelecionado.Position = OpVec.Vector2pVector3(Camera.PosCursor);
		}
	}

	PontoPathDesgin PontoMoverSelecionado;
	bool AtivarMover = false;

	List<Vector2> FiltrarPontos(List<Vector2> pontos)
	{
		List<Vector2> saida = new(){
			 pontos[0]
		};
		Vector2 UltPonto = pontos[0];

		for (int i = 0; i < pontos.Count; i++)
		{
			if (Meth.ObterDist(pontos[i], UltPonto) > 0.02)
			{//adicionar pontos com dist maior de 2 cm
				saida.Add(pontos[i]);
				UltPonto = pontos[i];
			}
		}

		return saida;
	}

	public List<Vector2> ObterPontosTotais()
	{
		List<Vector2> Pontos = new();
		double ultAng = 0;
		double ultAng2 = 0;
		var PontoInicial = Instrucoes[0].PInicial;
		Vector2 ultPonto = PontoInicial;
		for (int i = 0; i < Instrucoes.Count; i++)//Hell
		{
			Vector2 ini = Instrucoes[i].PInicial;
			Vector2 fim = Instrucoes[i].PFinal;


			var dx = fim.X - ini.X;
			var dy = fim.Y - ini.Y;

			var angel = Meth.Rad4Deg(Math.Atan2(dy, dx));

			//Instrucoes[i].AngP = double.Parse(File.ReadAllText("aa" + i));
			
			ultAng += angel;
			Instrucoes[i].AngP = angel;// angel - 
			//ultAng2 += ultAng;

			var cc = Instrucoes[i].AngP;//  90.0 -
			cc = Math.Round(Math.Abs(cc % 90) / 90.0);//% 90.0
			//cc = Math.Min(cc, 90);
			GD.Print(cc);
			Instrucoes[i].Pontos = ObterPontos(ini, fim, cc * 90);
			//Instrucoes[i].AngP = double.Parse(File.ReadAllText("aa" + i));
			//Instrucoes[i].Pontos = ObterPontos(ini, fim, Instrucoes[i].AngP);

			Pontos.AddRange(Instrucoes[i].Pontos);
			ultPonto = Instrucoes[i].PInicial;
		}
		return FiltrarPontos(Pontos);
	}


	void UpdateDesginerPontos()
	{
		if (AtivarMover)
		{
			if (PontoMoverSelecionado.Inicial)
			{
				var curr = Instrucoes.Where(w => w.guid == PontoMoverSelecionado.GuidCaminho).ToList().FirstOrDefault();
				if (curr != null)
				{
					curr.PInicial = OpVec.Vector3pVector2(PontoMoverSelecionado.Position);

					var ant = LinqEx.GetPrevious(Instrucoes, curr);
					if (ant != null)
					{
						var nodeAnt = ObterPontoPathDesgin(ant.IDPFinal);

						if (nodeAnt != null)
						{
							nodeAnt.Position = PontoMoverSelecionado.Position;
						}
						ant.PFinal = OpVec.Vector3pVector2(PontoMoverSelecionado.Position);
					}

				}
			}
			else
			{

				var curr = Instrucoes.Where(w => w.guid == PontoMoverSelecionado.GuidCaminho).ToList().FirstOrDefault();
				if (curr != null)
				{
					curr.PFinal = OpVec.Vector3pVector2(PontoMoverSelecionado.Position);

					var prox = LinqEx.GetNext(Instrucoes, curr);
					if (prox != null)
					{
						var nodeProx = ObterPontoPathDesgin(prox.IDPInicial);

						if (nodeProx != null)
						{
							nodeProx.Position = PontoMoverSelecionado.Position;
						}
						prox.PInicial = OpVec.Vector3pVector2(PontoMoverSelecionado.Position);
					}
				}

			}
		}
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.M)
			{
				var sw = Stopwatch.StartNew();
				UpdateDesginerPontos();

				ReDrawPontos(ObterPontosTotais());
				GD.Print($"Tempo: {sw.ElapsedMilliseconds}");
			}
			else if (keyEvent.Keycode == Key.X && !AtivarMover)
			{
				OnBtnAdicionarCurva(Camera.PosCursor);
			}
			else if (keyEvent.Keycode == Key.B)
			{
				if (!AtivarMover)
				{
					if (Camera.UltObj.Name == "StaticBodyPontoPathDesgin")
					{
						PontoMoverSelecionado = Camera.UltObj.GetParent().GetParent<PontoPathDesgin>();
						PontoMoverSelecionado.GetChild(0).GetChild(0).GetChild<CollisionShape3D>(0).Disabled = true;
						PontoMoverSelecionado.GetChild(1).GetChild(0).GetChild<CollisionShape3D>(0).Disabled = true;
						AtivarMover = true;
					}
				}
				else
				{
					UpdateDesginerPontos();

					PontoMoverSelecionado.GetChild(0).GetChild(0).GetChild<CollisionShape3D>(0).Disabled = false;
					PontoMoverSelecionado.GetChild(1).GetChild(0).GetChild<CollisionShape3D>(0).Disabled = false;

					AtivarMover = false;
				}
			}
		}
	}
}


class IntrucaoPath
{
	public Guid guidAent;

	public Guid guid;
	public Vector2 PInicial { get; set; }
	public Vector2 PFinal { get; set; }

	public int IDPInicial { get; set; }
	public int IDPFinal { get; set; }

	public List<Vector2> Pontos { get; set; }
	public bool Linear { get; set; }
	public double AngP { get; set; }
}
