using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Globalization;
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
			int curClr = -1;
			int cur = target;
			
			bool possible = false;
			
			List<int> path = new List<int>();
			Queue<int> queue = new Queue<int>();
			
			bool[,] visited = new bool[g.VertexCount, n];
			int[,] prev = new int[g.VertexCount, n];
			
			for (int i = 0; i < n; i++)
				visited[start, i] = true;
			
			queue.Enqueue(start);
			while (queue.Count > 0 && !possible)
			{
				var v = queue.Dequeue();

				foreach (var edge in g.OutEdges(v))
				{
					int nbr = edge.To;
					int color = edge.Weight;
					
					if (visited[nbr, color])
						continue;
					
					if (visited[v, color] || CheckColorChange(visited, v, color, c) != -1)
					{
						visited[nbr, color] = true;
						prev[nbr, color] = v;
						
						if (nbr == target)
						{
							curClr = color;
							possible = true;
							break;
						}
						
						queue.Enqueue(nbr);
					}
				}
			}

			if (!possible)
				return (false, new int[0]);

			int pr = prev[cur, curClr];
			path.Add(cur);
			while (cur != start)
			{
				int tempClr = CheckColorChange(visited, pr, curClr, c);
				if (tempClr != -1)
					curClr = tempClr;
				cur = pr;
				path.Add(cur);
				if (curClr != -1)
					pr = prev[cur, curClr];
			}
			path.Reverse();

			return (possible, path.ToArray());
		}

		int CheckColorChange(bool[,] colors, int vertexFrom, int targetColor, DiGraph<int> c)
		{
			for (int i = 0; i < colors.GetLength(1); i++)
				if (i != targetColor && colors[vertexFrom, i] && c.HasEdge(i, targetColor))
					return i;

			return -1;
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
			
			int[,] globalCosts = new int[g.VertexCount, n];
			for (int i = 0; i < g.VertexCount; i++)
				for (int j = 0; j < n; j++)
					globalCosts[i, j] = Int32.MaxValue;
			for (int i = 0; i < starts.Length; i++)
				for (int j = 0; j < n; j++)
					globalCosts[i, j] = 0;
			
			// int currentCost = 0;
			int minCost = Int32.MaxValue;
			// int minStart = -1;

			// Loop to check every start point
			for (int i = 0; i < starts.Length; i++)
			{
				// New priority queue and new visited array for each starting point
				SafePriorityQueue<int, int> priorityQueue = new SafePriorityQueue<int, int>(g.VertexCount);
				bool[,] visited = new bool[g.VertexCount, n];
				for (int k = 0; k < n; k++)
					visited[starts[i], k] = true;
				
				// Filling queue with all vertices with max priorities
				for (int j = 0; j < g.VertexCount; j++)
					priorityQueue.Insert(j, Int32.MaxValue);
				
				// Setting start priority to 0 so that it will be first
				priorityQueue.UpdatePriority(starts[i], 0);
				
				// Loop for Dijkstra algorithm
				while (priorityQueue.Count > 0)
				{
					// Save vertex with best priority and it's priority (cost)
					var priority = priorityQueue.BestPriority();
					var v = priorityQueue.Extract();
					
					// If I got from one start to another it means that it's better to start from the other one
					// Also if priority is still max, it means that all reachable vertices where used
					if (priority == Int32.MaxValue)
						break;

					// Iterating threw all the neighbours
					foreach (var edge in g.OutEdges(v))
					{
						int nbr = edge.To;
						int color = edge.Weight;
						int cost = priority + 1;
						int tempC = -1;
						
						// Checking the same conditions as in the first stage (if vertex is reachable)
						if (visited[v, color] || (tempC = CheckBestColorChange(visited, v, color, c)) != -1)
						{
							// If color change is needed add the additional cost of the change
							if (tempC != -1)
							{
								// tempC is the best possible color change, so I just need to add it to cost
								cost += c.GetEdgeWeight(tempC, color);
							}

							// Check if cost is greater than the globally best cost for all starts, if so skip
							if (cost >= globalCosts[nbr, color])
								continue;
							
							// Update visited array
							visited[nbr, color] = true;
							
							// Update global costs array
							globalCosts[nbr, color] = cost;
							
							// Update priority in queue
							if (priorityQueue.Contains(nbr))
								priorityQueue.UpdatePriority(nbr, cost);
							else
								priorityQueue.Insert(nbr, cost);
						}
					}
				}
			}

			minCost = Int32.MaxValue;
			for (int i = 0; i < n; i++)
			{
				if (globalCosts[target, i] < minCost)
					minCost = globalCosts[target, i];
			}

			if (minCost == Int32.MaxValue)
				return (null, new int[0]);
			
			return (minCost, new int[0]);
		}
		
		int CheckBestColorChange(bool[,] colors, int vertexFrom, int targetColor, DiGraph<int> c)
		{
			int min = 0;
			int minCost = Int32.MaxValue;
			for (int i = 0; i < colors.GetLength(1); i++)
				if (i != targetColor && colors[vertexFrom, i] && c.HasEdge(i, targetColor) && minCost > c.GetEdgeWeight(i, targetColor))
				{
					min = i;
					minCost = c.GetEdgeWeight(i, targetColor);
				}

			if (minCost != Int32.MaxValue)
				return min;
			
			return -1;
		}
	}
}
