using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
public class Program
{



    static Random rnd = new Random();
    static PontoLidar[] GerarPontos(uint IDScan)
    {
        PontoLidar[] structArray = new PontoLidar[rnd.Next(400, 500)];
        for (int i = 0; i < structArray.Length; i++)
        {
            structArray[i].Ang = rnd.Next(-314, 314) / 100.0f;
            structArray[i].Range = rnd.Next(10, 10000);
            structArray[i].ID = IDScan;
        }
        return structArray;
    }


    static void ModoEscrever()
    {
        var sisScans = new SistemaSessaoScans();
        sisScans.CriarSessao("sessTeste");

        var sw = Stopwatch.StartNew();
        var swTot = Stopwatch.StartNew();

        List<long> Tempos = new();

        int numScans = (10 * 60) * 30;//Tempo mapeamento

        for (int i = 0; i < numScans; i++)
        {
            var pontos = GerarPontos(sisScans.UltIDX);
            //Console.WriteLine($"Tempo gerar pontos: {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            sisScans.AdicionarPontos(pontos, Vector3.One);
            Tempos.Add(sw.ElapsedMilliseconds);
            //Console.WriteLine($"Tempo adicionar scan: {sw.ElapsedMilliseconds}ms");
            //sw.Restart();

        }
        Console.WriteLine($"Tempo escrever scan: {swTot.ElapsedMilliseconds}ms");
        Console.WriteLine($"Tempo por cada scan: {swTot.ElapsedMilliseconds / (float)numScans}");
        Console.WriteLine($"Tempo por cada scan mean: {Tempos.Average()}");
        
        swTot.Restart();
        sisScans.Close();
        Console.WriteLine($"Tempo close: {swTot.ElapsedMilliseconds}");

    }
    static void ModoLer()
    {
        var sisScans = new SistemaSessaoScans();

        var sw = Stopwatch.StartNew();
        uint numScans = sisScans.LerSessao("sessTeste");

        Console.WriteLine($"Tempo ler: {sw.ElapsedMilliseconds} Num scans: {numScans}");

        sw.Restart();
        PontoLidar[][] pontosSessao = new PontoLidar[numScans][];
        for (int i = 0; i < numScans; i++)
        {
            var scan = sisScans.LerScan();
            pontosSessao[i] = sisScans.LerPontosScan(scan.Len);
        }
        Console.WriteLine($"Tempo ler scan: {sw.ElapsedMilliseconds}");

        /*
        for (int i = 0; i < numScans; i++)
        {
            var scan = sisScans.LerScan();
            Console.WriteLine($"ID: {scan.ID}, Tempo: {scan.Tempo}, lenPontos: {scan.Len}");

            var pontos = sisScans.LerPontosScan(scan.Len);
            foreach (var ponto in pontos)
            {
                Console.WriteLine($"Ponto: Id: {ponto.ID}, Range: {ponto.Range}, Ang: {ponto.Ang}");
            }
        }
*/


    }

    public static void Main()
    {
        ModoEscrever();

        Console.WriteLine();
        ModoLer();


        /*
                Scan scan = GerarScan(pontos);
                Console.WriteLine($"Tempo criar scan: {sw.ElapsedMilliseconds}ms");


                Escrever(scan, "dadosPontos");

                var scanLido = Ler("dadosPontos");
                Console.WriteLine($"É igual: {Enumerable.SequenceEqual(scanLido.DadosScan, scan.DadosScan)}");

        */



        // sw.Restart();
        // Console.WriteLine($"Tempo encod Rap: {sw.ElapsedMilliseconds}ms len: {byteArray.Length}");

        //sw.Restart();
        //var dec = DecodeByteArrayToStructArray<PontoLidar>(byteArray, byteArray.Length);
        // Console.WriteLine($"Tempo decod fast: {sw.ElapsedMilliseconds}");

        //Console.WriteLine($"Disco preciso: em 1 minuto: {(byteArray.Length * 10.0f) * 60.0f / 1000.0f / 1000.0f}Mb");

    }
}