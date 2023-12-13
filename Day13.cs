using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using UnityEngine;
using Unity.EditorCoroutines.Editor;

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

namespace SpoonmanGames.CodingAdventures.AdventOfCode2023
{
	public class Day13 : Exercise
	{
		// ----------------------------------------------------------------------------------
		// FIELDS
		// ----------------------------------------------------------------------------------
		public class Data
		{
			public List<char[,]> maps;
		}

		// ----------------------------------------------------------------------------------
		// PARSER
		// ----------------------------------------------------------------------------------
		public void ParseData()
		{
			data = new Data();

			data.maps = new List<char[,]>();

			var premap = new List<string>();

			for (var i = 0; i < lines.Count; i++)
			{
				var line = lines[i];

				if (line == "" || i == lines.Count - 1)
				{
					var map = new char[premap.Count, premap[0].Length];

					for (var row = 0; row < premap.Count; row++)
					{
						for (var column = 0; column < premap[0].Length; column++)
						{
							map[row, column] = premap[row][column];
						}
					}
					
					data.maps.Add(map);
					premap.Clear();
				}
				else
				{
					premap.Add(line);
				}
			}
		}






		// ----------------------------------------------------------------------------------
		// METHODS
		// ----------------------------------------------------------------------------------
		private bool AreRowsEqual(char[,] map, int row1, int row2)
		{
			for (var column = 0; column < map.GetLength(1); column++)
			{
				if (map[row1,column] != map[row2,column]) return false;
			}

			return true;
		}

		private bool AreColumnsEqual(char[,] map, int column1, int column2)
		{
			for (var row = 0; row < map.GetLength(0); row++)
			{
				if (map[row, column1] != map[row, column2]) return false;
			}

			return true;
		}

		private long RowsDifference(char[,] map, int row1, int row2)
		{
			var count = 0L;
			for (var column = 0; column < map.GetLength(1); column++)
			{
				if (map[row1,column] != map[row2,column])
				{
					count ++;
				}
			}

			return count;
		}

		private long ColumnsDifference(char[,] map, int column1, int column2)
		{
			var count = 0L;
			for (var row = 0; row < map.GetLength(0); row++)
			{
				if (map[row, column1] != map[row, column2])
				{
					count ++;
				}
			}

			return count;
		}

		// ----------------------------------------------------------------------------------
		// PART 1
		// ----------------------------------------------------------------------------------
		private IEnumerator ExecutePart1(RunSettings settings, Action<string> resultCallback)
		{
			ParseData();

			var total = 0L;

			foreach (var map in data.maps)
			{
				var mirrorRow = -1;
				var mirrorColumn = -1;

				// Horizontal
				for (var row = 0; row < map.GetLength(0)-1; row++)
				{
					var row1 = row;
					var row2 = row+1;
					var isMirror = true;

					do
					{
						if (!AreRowsEqual(map, row1, row2))
						{
							isMirror = false;
							break;
						}

						row1--;
						row2++;
					} while (row1 >= 0 && row2 < map.GetLength(0));

					if (isMirror) mirrorRow = row;
				}

				if (mirrorRow == -1)
				{
					// Vertical
					for (var column = 0; column < map.GetLength(1)-1; column++)
					{
						var column1 = column;
						var column2 = column+1;
						var isMirror = true;

						do
						{
							if (!AreColumnsEqual(map, column1, column2))
							{
								isMirror = false;
								break;
							}

							column1--;
							column2++;
						} while (column1 >= 0 && column2 < map.GetLength(1));

						if (isMirror) mirrorColumn = column;
					}
				}

				mirrorColumn++;
				mirrorRow++;

				total += mirrorColumn + mirrorRow * 100;
			}

			if (isDebugging) yield return null;

			resultCallback(total.ToString());
		}










		// ----------------------------------------------------------------------------------
		// PART 2
		// ----------------------------------------------------------------------------------
		private IEnumerator ExecutePart2(RunSettings settings, Action<string> resultCallback)
		{
			ParseData();

			var total = 0L;

			foreach (var map in data.maps)
			{
				var mirrorRow = -1;
				var mirrorColumn = -1;

				// Horizontal
				for (var row = 0; row < map.GetLength(0)-1; row++)
				{
					var row1 = row;
					var row2 = row+1;
					var smudges = 0L;

					do
					{
						smudges += RowsDifference(map, row1, row2);

						if (smudges > 1)
						{
							break;
						}

						row1--;
						row2++;
					} while (row1 >= 0 && row2 < map.GetLength(0));

					if (smudges == 1)
					{
						mirrorRow = row;
						break;
					}
				}

				if (mirrorRow == -1)
				{
					// Vertical
					for (var column = 0; column < map.GetLength(1)-1; column++)
					{
						var column1 = column;
						var column2 = column+1;
						var smudges = 0L;

						do
						{
							smudges += ColumnsDifference(map, column1, column2);

							if (smudges > 1)
							{
								break;
							}

							column1--;
							column2++;
						} while (column1 >= 0 && column2 < map.GetLength(1));

						if (smudges == 1) 
						{
							mirrorColumn = column;
							break;
						}
					}
				}

				mirrorColumn++;
				mirrorRow++;

				total += mirrorColumn + mirrorRow * 100;
			}

			if (isDebugging) yield return null;

			resultCallback(total.ToString());
		}
	}
}