using ASD;
using ASD.Graphs;
using System;
using System.Collections.Generic;

namespace Lab06
{
    public class HeroesSolver : MarshalByRefObject
    {
        /// <summary>
        /// Etap 1 - stwierdzenie, czy rozwiązanie istnieje
        /// </summary>
        /// <param name="g">graf przedstawiający mapę</param>
        /// <param name="keymasterTents">tablica krotek zawierająca pozycje namiotów klucznika - pierwsza liczba to kolor klucznika, druga to numer skrzyżowania</param>
        /// <param name="borderGates">tablica krotek zawierająca pozycje bram granicznych - pierwsza liczba to kolor bramy, dwie pozostałe to numery skrzyżowań na drodze między którymi znajduje się brama</param>
        /// <param name="p">ilość występujących kolorów (występujące kolory to 1,2,...,p)</param>
        /// <returns>bool - wartość true jeśli rozwiązanie istnieje i false wpp.</returns>
        public bool Lab06Stage1(Graph<int> g, (int color, int city)[] keymasterTents, (int color, int cityA, int cityB)[] borderGates, int p)
        {
            if (g.VertexCount == 2)
                return true;
            // edge values
            var edges = new Dictionary<(int, int), ulong>();
            foreach (var i in borderGates) // O(m * p) < O(m * 2^p)
            {
                var x = (i.cityA < i.cityB ? (i.cityA, i.cityB) : (i.cityB, i.cityA));
                if (!edges.ContainsKey(x))
                    edges[x] = (ulong)(1 << i.color);
                else
                    edges[x] |= (ulong)(1 << i.color);
            }

            ulong[] cities = new ulong[g.VertexCount + 1];
            foreach (var key in keymasterTents) // O(n * p) < O(n * 2^p)
            {
                cities[key.city] |= (ulong)(1 << key.color);
            }
    
            ulong last;
            ulong available = 0;

            Queue<int> queue = new Queue<int>();
            var visited = new bool[g.VertexCount + 1];
            visited[1] = true;
            do // <= O(2^p)
            {
                last = available;
                for (int i = 0; i <= g.VertexCount; i++) // do/while + for <= O(n * 2^p)
                {
                    if(visited[i])
                        queue.Enqueue(i);
                }
                while (queue.Count > 0)
                {
                    var curr = queue.Dequeue();
                    if ((cities[curr] | available) != available)
                        available |= cities[curr];
                    visited[curr] = true;
                    foreach (var e in g.OutEdges(curr)) // while + for <= O(n + m)
                    {
                        if (visited[e.To])
                            continue;
                        var x = e.From < e.To ? (e.From, e.To) : (e.To, e.From);
                        if (edges.ContainsKey(x) && (edges[x] | available) != available)
                            continue;
                        if (e.To == g.VertexCount - 1)
                            return true;
                        queue.Enqueue(e.To);
                    }
                }
            } while (last != available);
            return false;
        }

        /// <summary>
        /// Etap 2 - stwierdzenie, czy rozwiązanie istnieje
        /// </summary>
        /// <param name="g">graf przedstawiający mapę</param>
        /// <param name="keymasterTents">tablica krotek zawierająca pozycje namiotów klucznika - pierwsza liczba to kolor klucznika, druga to numer skrzyżowania</param>
        /// <param name="borderGates">tablica krotek zawierająca pozycje bram granicznych - pierwsza liczba to kolor bramy, dwie pozostałe to numery skrzyżowań na drodze między którymi znajduje się brama</param>
        /// <param name="p">ilość występujących kolorów (występujące kolory to 1,2,...,p)</param>
        /// <returns>krotka (bool solutionExists, int solutionLength) - solutionExists ma wartość true jeśli rozwiązanie istnieje i false wpp. SolutionLenth zawiera długość optymalnej trasy ze skrzyżowania 1 do n</returns>
        public (bool solutionExists, int solutionLength) Lab06Stage2(Graph<int> g, (int color, int city)[] keymasterTents, (int color, int cityA, int cityB)[] borderGates, int p)
        {
            int n = g.VertexCount - 1; // wierzchołek 0 nie występuje w zadaniu

            return (false, 0);
        }
    }
}
