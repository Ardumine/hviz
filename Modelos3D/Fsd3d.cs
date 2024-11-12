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

		}
		else
		{
			node.GetChild<CsgPrimitive3D>(0).MaterialOverlay = shaderCorPontoFinal;
			node.GetChild<CsgPrimitive3D>(1).MaterialOverlay = shaderCorPontoFinal;

		}

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

		if (angOffSetDeg >= 50)
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

	public void OnBtnAdicionarCurva(Vector2 PosCursor)
	{
		IntrucaoPath novaIntru = new();
		novaIntru.Linear = false;
		novaIntru.guid = Guid.NewGuid();
		if (Instrucoes.Count == 0)
		{
			novaIntru.PInicial = PosCursor;
			novaIntru.PFinal = novaIntru.PInicial + new Vector2(0.5f);

			novaIntru.AngP = Meth.Rad4Deg(Meth.ObterDifAngRad(Vector2.Zero, novaIntru.PInicial));
			novaIntru.AngP = 0;

		}
		else
		{
			novaIntru.guidAnt = Instrucoes.Last().guid;

			novaIntru.PInicial = Instrucoes.Last().PFinal;
			novaIntru.PFinal = PosCursor;

			novaIntru.AngP = Meth.Rad4Deg(Meth.ObterDifAngRad(Instrucoes.Last().PInicial, Instrucoes.Last().PFinal));

		}

		novaIntru.IDPInicial = UltIDIntruc;
		AdicionarPontoPathDesgin(novaIntru.PInicial, UltIDIntruc++, novaIntru.guid, true);

		novaIntru.IDPFinal = UltIDIntruc;
		PontoMoverSelecionado = AdicionarPontoPathDesgin(novaIntru.PFinal, UltIDIntruc++, novaIntru.guid, false);

		Instrucoes.Add(novaIntru);

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

	public List<Vector2> ObterPontosTotais()
	{
		List<Vector2> Pontos = new();
		double ultAng = 0;
		for (int i = 0; i < Instrucoes.Count; i++)
		{
			Vector2 ini = Instrucoes[i].PInicial;
			Vector2 fim = Instrucoes[i].PFinal;


			var dx = fim.X - ini.X;
			var dy = fim.Y - ini.Y;

			var angel = Meth.Rad4Deg(Math.Atan2(dy, dx));

			//Instrucoes[i].AngP = double.Parse(File.ReadAllText("aa" + i));
			Instrucoes[i].AngP = angel - ultAng;
			ultAng = angel;

			var cc = Instrucoes[i].AngP;//  90.0 -
			cc = Math.Round(cc / 90.0);

			Instrucoes[i].Pontos = ObterPontos(ini, fim, cc * 90);
			Pontos.AddRange(Instrucoes[i].Pontos);
		}
		return Pontos;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.M)
			{
				var sw = Stopwatch.StartNew();

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
					if (PontoMoverSelecionado.Inicial)
					{
						Instrucoes.Where(w => w.guid == PontoMoverSelecionado.GuidCaminho).ToList().ForEach(s => s.PInicial = OpVec.Vector3pVector2(PontoMoverSelecionado.Position));
						Instrucoes.Where(w => w.guid == PontoMoverSelecionado.GuidCaminho).Skip(1).ToList().ForEach(s =>
							{
								var node = ObterPontoPathDesgin(s.IDPFinal);
								if (node != null)
								{
									node.Position = PontoMoverSelecionado.Position;
								}
								s.PFinal = OpVec.Vector3pVector2(PontoMoverSelecionado.Position);
							});
					}
					else
					{
						Instrucoes.Where(w => w.guid == PontoMoverSelecionado.GuidCaminho).ToList().ForEach(s => s.PFinal = OpVec.Vector3pVector2(PontoMoverSelecionado.Position));
						Instrucoes.Where(w => w.guid == PontoMoverSelecionado.GuidCaminho).Skip(-1).ToList().ForEach(s =>
						{
							var node = ObterPontoPathDesgin(s.IDPInicial);
							if (node != null)
							{
								//	node.Position = PontoMoverSelecionado.Position;
							}
							//s.PInicial = OpVec.Vector3pVector2(PontoMoverSelecionado.Position);
						});
					}

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
	public Guid guidAnt;

	public Guid guid;
	public Vector2 PInicial { get; set; }
	public Vector2 PFinal { get; set; }

	public int IDPInicial { get; set; }
	public int IDPFinal { get; set; }

	public List<Vector2> Pontos { get; set; }
	public bool Linear { get; set; }
	public double AngP { get; set; }
}
