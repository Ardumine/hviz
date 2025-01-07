using System;
using System.Collections.Generic;
using System.Threading;
using Vector2 = System.Numerics.Vector2;

namespace Ardumine.SistemaFSD.Condutor;
class CondutorCaminhoAuto
{
    bool Emerg = false;
    public int IdxCurr = 0;
    SistemaLidar? sisLidar;
    SistemaMotores? sisMotores;
    Action<string>? Log;
    public void Preparar(SistemaLidar _sisLidar, SistemaMotores _sisMotores, Action<string> _log)
    {
        sisLidar = _sisLidar;
        sisMotores = _sisMotores;

        IdxCurr = 0;
        Log = _log;
    }

    public void PararEmerg()
    {
        Log!("A parar condutor auto!!");
        new Thread(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                Emerg = true;
                Thread.Sleep(10);
            }
        }).Start();
    }

    //Seguir pontos
    public void SeguirPontos(List<Vector2> PontosParaIr, CancellationToken token = default(CancellationToken))
    {
        Emerg = false;
        //Fase 1 virar para o primeiro ponto
        for (IdxCurr = 0; IdxCurr < PontosParaIr.Count; IdxCurr++)
        {
            //var angleDifference = Meth.ObterDifAng(sisLidar.PosCurr, sisLidar.AngCurr, PontosParaIr[idx_seg]);

            //double Dist_Prox = Meth.ObterDist(sisLidar.PosCurr, PontosParaIr[idx_seg]);


            /*if (Math.Abs(angleDifference) > 20 && Dist_Prox > 0.10)
			{
				//Rodar
				sisMotores.MandarSteer(0, 0);
				RodarParaGrau(PontosParaIr[idx_seg], true, 15, 0.2, token);
			}*/
            if (token.IsCancellationRequested || Emerg) return;

            IrParaPonto(PontosParaIr[IdxCurr], true);
            if (token.IsCancellationRequested || Emerg) return;

        }
        sisMotores!.MandarSteer(0, 0);

        if (!Emerg)
        {
            for (int i = 0; i < 5; i++)
            {
                Log!("---conc----x");
                sisMotores.MandarSteer(0, 0);
            }
        }
        else
        {
            Log!("Parar emerg!");
        }

    }

    void IrParaPonto(Vector2 targetPoint, bool ModoContinuo = false, CancellationToken token = default(CancellationToken))
    {
        var angleDifference = Meth.ObterDifAng(sisLidar!.PosCurr, sisLidar.AngCurr, targetPoint);

        double Dist_Prox = Meth.ObterDist(sisLidar.PosCurr, targetPoint);

        if (ModoContinuo)
        {
            if (Math.Abs(angleDifference) > 20 && Dist_Prox > 0.2)
            {
                //Rodar
                sisMotores!.MandarSteer(0, 0);
                sisMotores.MandarSteer(0, 0);
                RodarParaGrau(targetPoint, true, 30, 0.2, token);
            }
            if (token.IsCancellationRequested || Emerg) return;
            NavegarParaPonto(targetPoint, true, 0.2, 1.3, token);

        }
        else
        {
            if (Math.Abs(angleDifference) > 10)
            {
                //Rodar
                RodarParaGrau(targetPoint, token: token);
            }
            if (token.IsCancellationRequested || Emerg) return;
            NavegarParaPonto(targetPoint, token: token);
        }

    }

    void RodarParaGrau(Vector2 targetPoint, bool ModoContinuo = false, int AngMaxDif = 5, double ErroDistPox = 0.1, CancellationToken token = default(CancellationToken))
    {

        double angleDifference = Meth.ObterDifAng(sisLidar!.PosCurr, sisLidar.AngCurr, targetPoint);
        double Dist_Prox = Meth.ObterDist(sisLidar.PosCurr, targetPoint);

        while (Math.Abs(angleDifference) > AngMaxDif && !Emerg && Dist_Prox > ErroDistPox)
        {
            //Take a look to this later.
            angleDifference = Meth.ObterDifAng(sisLidar.PosCurr, sisLidar.AngCurr, targetPoint);

            int steer = Meth.SpeedControl((int)(angleDifference / 180.0 * 1200.0), 130);
            Log!($"RodParaProxPonto DifAng: {angleDifference:F2} LidAngCurr: {sisLidar.AngCurr:F2} MotSteer: {steer:F2}");
            sisMotores!.MandarSteer(steer, 0);
            if (token.IsCancellationRequested || Emerg) return;
            Dist_Prox = Meth.ObterDist(sisLidar.PosCurr, targetPoint);

            Thread.Sleep(30);
            if (token.IsCancellationRequested || Emerg) return;

        }
        if (!ModoContinuo)
        {
            sisMotores!.MandarSteer(0, 0);
            Log!("Rodar conc!");
            Thread.Sleep(200);
        }

    }

    void NavegarParaPonto(Vector2 posObj, bool ModoContinuo = false, double ErroDistPox = 0.1, double multVel = 1.0, CancellationToken token = default(CancellationToken))
    {

        double DistPFim_Inicial = Meth.ObterDist(sisLidar!.PosCurr, posObj);
        double DistPFim = Meth.ObterDist(sisLidar.PosCurr, posObj);


        while (DistPFim > ErroDistPox && !Emerg)
        {
            DistPFim = Meth.ObterDist(sisLidar.PosCurr, posObj);
            double angleDifference = Meth.ObterDifAng(sisLidar.PosCurr, sisLidar.AngCurr, posObj);

            double difPosNorm = DistPFim / DistPFim_Inicial;
            double difAngNorm = angleDifference / 180.0f;

            int velRaw = (int)(difPosNorm * 400.0 * multVel);
            int vel = Math.Min((int)(Meth.SpeedControl(velRaw, 75) * (1.0 - Math.Abs(difAngNorm / 1))), 65);

            int steer = Meth.SpeedControl((int)(difAngNorm * 400.0f ), 60);//* (2.0f - (DistPFim / DistPFim_Inicial))

            sisMotores!.MandarSteer(steer, vel);


            Log!($"DistPFim: {DistPFim:F4} VelMot: {vel} SteerMot: {steer}");
            Thread.Sleep(30);
            if (token.IsCancellationRequested || Emerg) return;
        }


        //Se for só para navegar para um ponto
        if (!ModoContinuo)
        {
            sisMotores!.MandarSteer(0, 0);
            for (int i = 0; i < 5; i++)
            {
                Log!("NotCont---conc----x");
            }
        }

    }


}