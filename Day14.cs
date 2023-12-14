// ----------------------------------------------------------------------------------
// Disclaimer: This code may not compile or execute without errors as is.
//     There is some overhead from my framework and Unity that I'm not adding.
//     For example:
//     - lines is a List<string> that contains the lines of the input file with the exercise.
//     - exercises are executed as Coroutines in the Unity editor.
//     - the result is notified through a callback to the framework triggering a solution run.
//     - isDebugging is a flag that allows me to do a run basically printing logs and doing extra stuff.
//     - etc.
//     Please interpret and use accordingly.
// ----------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using System.Text;

namespace SpoonmanGames.CodingAdventures.AdventOfCode2023
{
	public class Day14 : Exercise
	{
		// ----------------------------------------------------------------------------------
		// FIELDS
		// ----------------------------------------------------------------------------------
		[Serializable]
		public class Data
		{
			public char[,] map;
		}









		// ----------------------------------------------------------------------------------
		// PARSER
		// ----------------------------------------------------------------------------------
		public void ParseData()
		{
			data = new Data();
			data.map = new char[lines.Count, lines[0].Length];

			for (var row = 0; row < lines.Count; row++)
			{
				for (var column = 0; column < lines[0].Length; column++)
				{
					data.map[row, column] = lines[row][column];
				}
			}
		}






		// ----------------------------------------------------------------------------------
		// METHODS
		// ----------------------------------------------------------------------------------
		private long CalculateNorthLoad(char[,] map, bool moveRocks)
		{
			var loadPerColumn = Enumerable.Range(0, map.GetLength(1))
				.Select(l => map.GetLength(0))
				.ToArray();

			var totalLoad = 0L;

			for (var row = 0; row < map.GetLength(0); row++)
			{
				for (var column = 0; column < map.GetLength(1); column++)
				{
					var target = map[row, column];

					switch (target)
					{
						case 'O':
						{
							if (moveRocks)
							{
								totalLoad += loadPerColumn[column];
								loadPerColumn[column] --;
							}
							else
							{
								totalLoad += map.GetLength(0) - row;
							}
							break;
						}
						case '#':
						{
							loadPerColumn[column] = map.GetLength(0) - row - 1;
							break;
						}
					}
				}
			}

			return totalLoad;
		}

		private void MoveMapNorth(char[,] map)
		{
			for (var row = 0; row < map.GetLength(0); row++)
			{
				for (var column = 0; column < map.GetLength(1); column++)
				{
					if (map[row, column] == 'O')
					{
						var move = 0;

						for (var i = row; i >= 0; i--)
						{
							if (map[i, column] == '.') move ++;
							else if (map[i, column] == '#')	break;
						}

						map[row, column] = '.';
						map[row - move, column] = 'O';
					}
				}
			}
		}

		private void MoveMapWest(char[,] map)
		{
			for (var column = 0; column < map.GetLength(1); column++)
			{
				for (var row = 0; row < map.GetLength(0); row++)
				{
					if (map[row, column] == 'O')
					{
						var move = 0;

						for (var i = column; i >= 0; i--)
						{
							if (map[row, i] == '.') move ++;
							else if (map[row, i] == '#')	break;
						}

						map[row, column] = '.';
						map[row, column - move] = 'O';
					}
				}
			}
		}

		private void MoveMapSouth(char[,] map)
		{
			for (var row = map.GetLength(0) - 1; row >= 0; row--)
			{
				for (var column = 0; column < map.GetLength(1); column++)
				{
					if (map[row, column] == 'O')
					{
						var move = 0;

						for (var i = row; i < map.GetLength(0); i++)
						{
							if (map[i, column] == '.') move ++;
							else if (map[i, column] == '#')	break;
						}

						map[row, column] = '.';
						map[row+move, column] = 'O';
					}
				}
			}
		}

		private void MoveMapEast(char[,] map)
		{
			for (var column = map.GetLength(1) - 1; column >= 0; column--)
			{
				for (var row = 0; row < map.GetLength(0); row++)
				{
					if (map[row, column] == 'O')
					{
						var move = 0;

						for (var i = column; i < map.GetLength(1); i++)
						{
							if (map[row, i] == '.') move ++;
							else if (map[row, i] == '#')	break;
						}

						map[row, column] = '.';
						map[row, column + move] = 'O';
					}
				}
			}
		}

		private void LogMap(char[,] map)
		{
			var builder = new StringBuilder();

			for (var row = 0; row < map.GetLength(0); row++)
			{
				for (var column = 0; column < map.GetLength(1); column++)
				{
					builder.Append(map[row, column]);
				}

				builder.AppendLine();
			}
		}

		private long SearchForCycle(List<char[,]> cycles)
		{
			var result = -1L;
			var first = cycles[cycles.Count - 1];

			for (var i = cycles.Count - 2; i >= 0; i--)
			{
				if (AreEqual(first, cycles[i]))
				{
					result = i;
					break;
				}
			}

			return result;
		}

		private bool AreEqual(char[,] first, char[,] second)
		{
			for (var row = 0; row < first.GetLength(0); row++)
			{
				for (var column = 0; column < first.GetLength(1); column++)
				{
					if (first[row, column] != second[row, column]) return false;
				}
			}

			return true;
		}


		// ----------------------------------------------------------------------------------
		// PART 1
		// ----------------------------------------------------------------------------------
		private IEnumerator ExecutePart1(RunSettings settings, Action<string> resultCallback)
		{
			ParseData();

			var totalLoad = CalculateNorthLoad(data.map, true);

			resultCallback(totalLoad.ToString());
		}










		// ----------------------------------------------------------------------------------
		// PART 2
		// ----------------------------------------------------------------------------------
		private IEnumerator ExecutePart2(RunSettings settings, Action<string> resultCallback)
		{
			ParseData();

			var totalCycles = 1000000000;
			//var totalCycles = 20;
			var cycles = new List<char[,]>();

			var map = (char[,])data.map.Clone();

			for (var i = 0L; i < totalCycles; i++)
			{
				MoveMapNorth(map);
				MoveMapWest(map);
				MoveMapSouth(map);
				MoveMapEast(map);

				cycles.Add((char[,])map.Clone());

				var sameAt = SearchForCycle(cycles);

				if (sameAt != -1)
				{
					var repeatCycle = i - sameAt;

					i += Mathf.FloorToInt((totalCycles - 1 - i) / repeatCycle) * repeatCycle;
				}
			}

			var totalLoad = CalculateNorthLoad(map, false);

			resultCallback(totalLoad.ToString());
		}

	}
}