

// Author: FreeDOOM#4231 on Discord


using System;
using System.IO;

namespace ASD___5
{
    class Program
    {
        const string prefixIN = "Wyc_in_";
        const string prefixOUT = "Wyc_out_";

        readonly static string[] _fileName_ = new string[] {
            "1_Pietrzeniuk.txt",     // 0
            "ZeSpecyfikacji.txt",   // 1
            "ZeSpecyfikacji1.txt", // 2
            "Nowy.txt",           // 3
			};

        static void Main(string[] args)
        {
            string fileName = _fileName_[2];
            Graph graph = new Graph(prefixIN + fileName);
            graph.Debug_WriteToConsole();
            graph.Debug_WritePathSensation(graph.FindEulerCycle_Guided(graph.streetPriority.Peek.end0));
            Console.WriteLine($"\nHighest street with highest NetSensation: {graph.streetPriority.Peek}, with NetSensation= {graph.streetPriority.Peek.NetSensation}");

            Graph.Tour tour = graph.CalculateTour();
            File.WriteAllText(prefixOUT + fileName, tour.ToString());
        }
    }
}

/*
Warunki:
- Trasa musi przejść wszystkimi ulicami.
- Każda atrakcja dodaje tylko raz swoją ilość do zainteresowania.
- Każda ulica zmniejsza zainteresowanie o swoją długość.

Wskazówki:
- Podczas wczytywania ulic z pliku można sprawdzić czy suma atrakcji nie jest mniejsza od sumarycznej długości ulic,
    ponieważ inaczej dalsze poszukiwanie trasy nie ma sensu.

Co wiadomo na pewno to to że jest to graf eulera ("wszystkie skrzyżowania mają 4 wychodzące ulice").
Z racji, że to jedyne co wiem o grafach to można by zeksplorować dwa algorytmy: tworzenia cyklu eulera, oraz A*.
  Pierwsze z racji, że w tym zadaniu należy przejść po wszystkich krawędziach grafu,
  drugie, bo należy znaleźć najlepszą ścieżkę w grafie ważonym.
Najpierw można zacząć od zważenia wszystkich możliwych początków ścieżki i umieszczenia ich w kolejce priorytetowej
  (ulice są ważone pod względem ich sumy zainteresowania jakie generują). Następnie można przystąpić do eksplorowania
  grafu za pomocą zmodyfikowanego algorytmu A*.
*/
