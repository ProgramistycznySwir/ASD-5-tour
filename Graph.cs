using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

// Algorytm ma pesymistyczną złożoność czasową O(n^2), jednak w większości przypadków jedynie O(n), ponieważ dzięki zastosowaniu
//  zachłannego wyboru kolejności krawędzi w cyklu eulera drastycznie zwiększa się szanse, że jeśli trasa wycieczki istnieje
//  to algorytm natrafi na nią w pierwszej iteracji.
// O(n) nie jest w żaden sposób czasem optymistycznym, jest to czas średni, O(n^2) aktywuje się dopiero kiedy algorytm chce się
//  upewnić, że na pewno nie popełnił błędu.

namespace ASD___5
{
    class Graph
    {
        // Stała która służy do określenia czy wytyczenie trasy wycieczki jest wgle możliwe.
        public readonly int sumSensationOfGraph;
        // Stała przechowująca ilość skrzyżowań w mieście.
        public readonly int crossoverCount;
        public int StreetCount => crossoverCount * 2;
        // Może to nie jest najlepszy pomysł by to na pewno była tablica (może to prowdzić do
        //  fragmentacji sterty, bo w sumie będzie ona miała 320kB) jednak eh?
        /// <summary>
        /// Lista (a technicznie rzecz biorąc - tablica) incydencji grafu.
        /// </summary>
        public readonly Street[,] crossovers;
        public PriorityQueue<Street> streetPriority;

        /// <summary>
        /// Builds graph loading it's data from file filename.
        /// </summary>
        /// <param name="filename"></param>
        public Graph(string filename)
        {
            sumSensationOfGraph = 0;

            // Loading graph from file:
            try
            {
                // Tutaj musi być cokolwiek przypisane ponieważ później kompilator się pulta :/
                byte[] streetsInCrossover = new byte[0];
                bool firstLine_ = true;
                int newStreetIndex = 0;
                string line_;
                int[] dataFromLine_;
                Street_FreeAccess newStreetToSeal_;
                Street newStreet;
                using (var sr = new StreamReader(filename))
                {
                    if (firstLine_)
                    {
                        crossoverCount = Convert.ToInt32(sr.ReadLine());
                        crossovers = new Street[crossoverCount, 4];
                        streetPriority = new PriorityQueue<Street>(StreetCount, PriorityMode.Half);
                        streetsInCrossover = new byte[crossoverCount];
                        firstLine_ = false;
                    }
                    // 640k cycles for 320kB of memory means: 0.3ms@2.4GHz
                    // Powyższe to obliczenia ile zajmuje zadeklarowanie 320kB pamięci w CLR (czyli co za tym idzie C#)
                    //  nie pamiętam co one tutaj robią...

                    while ((line_ = sr.ReadLine()) != null)
                    {
                        dataFromLine_ = line_.Split(' ').Select(int.Parse).ToArray();

                        newStreet = new Street(newStreetIndex++, dataFromLine_[0] - 1, dataFromLine_[1] - 1, dataFromLine_[2], dataFromLine_[3]);
                        crossovers[newStreet.end0, streetsInCrossover[newStreet.end0]++] = newStreet;
                        crossovers[newStreet.end1, streetsInCrossover[newStreet.end1]++] = newStreet;

                        streetPriority.Add(newStreet);

                        sumSensationOfGraph += newStreet.NetSensation;
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                // It's not the end of the programm, just relay this exception further.
                throw e;
            }
        }

        public Tour CalculateTour()
        {
            if (sumSensationOfGraph < 0)
                return new Tour();

            // Te zmienne są zapamiętywane, bo przydają się później do zwrócenia wyniku.
            int pathStart, startDirection;
            List<Street> path;
            int sensationOfPath;
            // By próbować ścieżki w obie strony.
            bool oneWay = false;
            int DEBUG_iterationCount = 0;
            while (true)
            {
                DEBUG_iterationCount++;
                if(!oneWay)
                {
                    pathStart = streetPriority.Peek.end0;
                    startDirection = streetPriority.Peek.end1;
                    oneWay = true;
                }
                else
                {
                    pathStart = streetPriority.Peek.end1;
                    startDirection = streetPriority.Poll().end0;
                    oneWay = false;
                }
                path = FindEulerCycle_Guided(pathStart);

                sensationOfPath = path[0].HalfSensation;
                for (byte i = 0; i < path.Count; i++)
                {
                    // Potrzebne są 2 if'y bo wycieczkowicze mogą stracić zainteresowanie nawet tuż przed atrakcjąz
                    //  w końcu atrakcja jest w połowie ulicy.
                    sensationOfPath += path[i].lenght;
                    if (sensationOfPath < 0)
                        break;
                    sensationOfPath += path[i].HalfSensation;
                    if (sensationOfPath < 0)
                        break;
                }
                sensationOfPath += path[0].lenght / 2;
                if (sensationOfPath < 0)
                    continue;

                Console.WriteLine($"Algorithm has finished after {DEBUG_iterationCount} iteration{(DEBUG_iterationCount == 1 ? "" : "s")}.");
                return new Tour(tourCrossoverCount, startDirection, path);
            }
            return new Tour();
        }

        #region >>> Euler Cycle Generation <<<

        bool[] visitedStreets_;
        /// <summary>
        /// Wrapper of EulerTool().
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public List<Street> FindEulerCycle(int start)
        {
            tourCrossoverCount = 0;
            visitedStreets_ = new bool[StreetCount];
            List<Street> path = new List<Street>();
            EulerTool(path, start);
            return path;
        }
        List<Street> EulerTool(List<Street> path, int node)
        {
            for (byte i = 0; i < 4; i++)
            {
                if (!visitedStreets_[crossovers[node,i].index])
                {
                    visitedStreets_[crossovers[node, i].index] = true;
                    EulerTool(path, crossovers[node, i].OtherEnd(node));
                    path.Add(crossovers[node, i]);
                }
            }
            //path.Add(node);
            return path;
        }

        // Algorytm zachłannie wybiera trasę w taki sposób by wycieczka najpierw doznała jak największych wrażeń,
        //  a później żeby te wrażenia starczyły do końca trasy.
        /// <summary>
        /// Wrapper of EulerTool_Guided().
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        int tourCrossoverCount;
        public List<Street> FindEulerCycle_Guided(int start)
        {
            tourCrossoverCount = 0;
            visitedStreets_ = new bool[StreetCount];
            List<Street> path = new List<Street>();
            EulerTool_Guided(path, start);
            return path;
        }
        List<Street> EulerTool_Guided(List<Street> path, int node)
        {
            var priority = new PriorityQueue<Street>(4, PriorityMode.InverseFull);
            for(byte i = 0; i < 4; i++)
                priority.Add(crossovers[node, i]);
            while(!priority.IsEmpty)
            {
                if (visitedStreets_[priority.Peek.index])
                    priority.Poll();
                else
                {
                    visitedStreets_[priority.Peek.index] = true;
                    EulerTool_Guided(path, priority.Peek.OtherEnd(node));
                    path.Add(priority.Poll());
                }
            }
            tourCrossoverCount++;
            //path.Add(node);
            return path;
        }

        #endregion

        #region >>> Debug Methods <<<

        public void Debug_WritePathSensation(List<Street> path)
        {
            int sensation = path[0].HalfSensation;
            Console.Write($"{path[0].Number}.({sensation})->");
            for (int i = 1; i < path.Count; i++)
            {
                Console.Write($"{path[i].Number}.({sensation +=path[i].NetSensation})->");
            }
            Console.Write($"{path[0].Number}.({sensation -= path[0].lenght/2})");
        }
        public void Debug_WriteToConsole()
        {
            for (int y = 0; y < crossoverCount; y++)
            {
                Console.Write("{");
                for (int x = 0; x < 4; x++)
                    Console.Write($" <{crossovers[y, x]}> ");
                Console.WriteLine("}");
            }
        }

        #endregion

        public class Tour
        {
            public readonly bool success;
            public readonly int crossoverCount;
            public readonly int startDirection;
            /// <summary>
            /// First index of this list is street on which building tour agency is advised.
            /// </summary>
            public readonly List<Street> streets;

            public Tour()
            {
                this.success = false;
            }
            public Tour(int crossoverCount, int startDirection, List<Street> streets)
            {
                this.success = true;
                this.crossoverCount = crossoverCount;
                this.startDirection = startDirection;
                this.streets = streets;
            }

            public override string ToString()
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                if(success)
                {
                    builder.Append("TAK");
                    builder.Append($"\n{crossoverCount}");
                    builder.Append($"\n{streets[0].Number} {startDirection+1}");
                    for(int i = 1; i < streets.Count; i++)
                        builder.Append($"\n{streets[i].Number}");
                }
                else
                    builder.Append("NIE");
                return builder.ToString();
            }
        }
    }
}
