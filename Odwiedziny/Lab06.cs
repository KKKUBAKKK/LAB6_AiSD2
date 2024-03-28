using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;

namespace ASD
{
	public class Lab06 : MarshalByRefObject
	{
		/// <summary>Etap 1</summary>
		/// <param name="n">Liczba kolorów (równa liczbie wierzchołków w c)</param>
		/// <param name="c">Graf opisujący możliwe przejścia między kolorami. Waga to wysiłek.</param>
		/// <param name="g">Graf opisujący drogi w mieście. Waga to kolor drogi.</param>
		/// <param name="target">Wierzchołek docelowy (dom Grzesia).</param>
		/// <param name="start">Wierzchołek startowy (wejście z lasu).</param>
		/// <returns>Pierwszy element pary to informacja, czy rozwiązanie istnieje. Drugi element pary, to droga będąca rozwiązaniem: sekwencja odwiedzanych wierzchołków (pierwszy musi być start, ostatni target). W przypadku, gdy nie ma rozwiązania, ma być tablica o długości 0.</returns>
		public (bool possible, int[] path) Stage1(int n, DiGraph<int> c, Graph<int> g, int target, int start)
		{
			DiGraph<int> layeredGraph = new DiGraph<int>(g.VertexCount * n);
			bool isReachable = false;

			for (int layer = 0; layer < n; layer++)
			{
				foreach (var edge in g.BFS().SearchAll())
				{
					if (layer == edge.Weight || c.HasEdge(layer, edge.Weight))
					{
						layeredGraph.AddEdge(edge.From + g.VertexCount * layer, edge.To + g.VertexCount * edge.Weight, edge.Weight);
					}
				}
			}

			for (int i = 0; i < n; i++)
			{
				foreach (var e in layeredGraph.DFS().SearchFrom(start))
				{
					if (e.To == target + i * g.VertexCount)
					{
						isReachable = true;
						break;
					}
				}
				
				if (isReachable)
					break;
			}

			if (!isReachable)
				return (false, new int[0]);

			return (true, new int[0]);
		}

		/// <summary>Drugi etap</summary>
		/// <param name="n">Liczba kolorów (równa liczbie wierzchołków w c)</param>
		/// <param name="c">Graf opisujący możliwe przejścia między kolorami. Waga to wysiłek.</param>
		/// <param name="g">Graf opisujący drogi w mieście. Waga to kolor drogi.</param>
		/// <param name="target">Wierzchołek docelowy (dom Grzesia).</param>
		/// <param name="starts">Wierzchołki startowe (wejścia z lasu).</param>
		/// <returns>Pierwszy element pary to koszt najlepszego rozwiązania lub null, gdy rozwiązanie nie istnieje. Drugi element pary, tak jak w etapie 1, to droga będąca rozwiązaniem: sekwencja odwiedzanych wierzchołków (pierwszy musi być start, ostatni target). W przypadku, gdy nie ma rozwiązania, ma być tablica o długości 0.</returns>
		public (int? cost, int[] path) Stage2(int n, DiGraph<int> c, Graph<int> g, int target, int[] starts)
		{
		    return (null, new int[0]);
		}
	}
}
