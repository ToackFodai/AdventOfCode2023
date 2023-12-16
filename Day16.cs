// ----------------------------------------------------------------------------------
// Disclaimer: This code may not compile or execute without errors as is.
//     There is some overhead from my framework and Unity that I'm not adding.
//     For example:
//     - lines is a List<string> that contains the lines of the input file with the exercise.
//     - exercises are executed as Coroutines in the Unity editor.
//     - the result is notified through a callback to the framework triggering a solution run.
//     - etc.
//     Please interpret and use accordingly.
// ----------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Gamelogic.Extensions;
using SpoonmanGames.Creator;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using System.Text;

namespace SpoonmanGames.CodingAdventures.AdventOfCode2023
{
	public class Day16 : Exercise
	{
		// ----------------------------------------------------------------------------------
		// FIELDS
		// ----------------------------------------------------------------------------------
		[Serializable]
		public class Data
		{
			public Tile[,] map;
		}

		[Serializable]
		public class Tile
		{
			public char element;
			public BeamDirection beamDirection;
		}

		public class Beam
		{
			public int row;
			public int column;
			public BeamDirection direction;

			public Beam(int row, int column, BeamDirection direction)
			{
				this.row = row;
				this.column = column;
				this.direction = direction;
			}
		}

		[Flags]
		public enum BeamDirection
		{
			Up         = 1 << 0,
			Right      = 1 << 1,
			Down       = 1 << 2,
			Left       = 1 << 3,

			Horizontal = Left | Right,
			Vertical   = Up | Down,

			None       = 0,
			All        = Up | Right | Down | Left
		}










		// ----------------------------------------------------------------------------------
		// PARSER
		// ----------------------------------------------------------------------------------
		public void ParseData()
		{
			data = new Data();
			data.map = new Tile[lines.Count, lines[0].Length];

			for (var row = 0; row < lines.Count; row++)
			{
				for (var column = 0; column < lines[0].Length; column++)
				{
					data.map[row, column] = new Tile()
					{
						element = lines[row][column],
						beamDirection = BeamDirection.None
					};
				}
			}
		}







		// ----------------------------------------------------------------------------------
		// METHODS
		// ----------------------------------------------------------------------------------
		public bool IsInside(int row, int column) => row >= 0 && row < data.map.GetLength(0) && column >= 0 && column < data.map.GetLength(1);

		public Vector2Int TransformDirectionToVector(BeamDirection direction)
		{
			return direction switch
			{
				// Vector2Int are row, column
				BeamDirection.Up    => new Vector2Int(-1, 0),
				BeamDirection.Right => new Vector2Int(0, 1),
				BeamDirection.Down  => new Vector2Int(1, 0),
				BeamDirection.Left  => new Vector2Int(0, -1),
				_ => throw new ArgumentException($"Direction {direction} is not valid.")
			};
		}

		private BeamDirection ReflectDirection(char element, BeamDirection direction)
		{
			return direction switch
			{
				BeamDirection.Up    => element == '/' ? BeamDirection.Right : BeamDirection.Left,
				BeamDirection.Right => element == '/' ? BeamDirection.Up : BeamDirection.Down,
				BeamDirection.Down  => element == '/' ? BeamDirection.Left : BeamDirection.Right,
				BeamDirection.Left  => element == '/' ? BeamDirection.Down : BeamDirection.Up,
				_ => throw new ArgumentException($"Direction {direction} is not valid.")
			};
		}

		private BeamDirection SplitDirections(char element, BeamDirection direction)
		{
			return direction switch
			{
				BeamDirection.Up    => element == '-' ? BeamDirection.Horizontal : direction,
				BeamDirection.Down  => element == '-' ? BeamDirection.Horizontal : direction,
				BeamDirection.Right => element == '|' ? BeamDirection.Vertical : direction,
				BeamDirection.Left  => element == '|' ? BeamDirection.Vertical : direction,
				_ => throw new ArgumentException($"Direction {direction} is not valid.")
			};
		}

		private long ThrowBeam(int startingRow, int startingColumn, BeamDirection startingDirection)
		{
			var beams = new List<Beam>() { new Beam(startingRow, startingColumn, startingDirection) };
			var ignoreFirst = true;

			// Throw beam (and beams)
			while (beams.Count > 0)
			{
				for (var i = beams.Count - 1; i >= 0; i--)
				{
					var beam = beams[i];

					if (ignoreFirst)
					{
						ignoreFirst = false;
					}
					else
					{
						if ((data.map[beam.row, beam.column].beamDirection & beam.direction) != 0)
						{
							beams.RemoveAt(i);
							continue;
						}
						else
						{
							data.map[beam.row, beam.column].beamDirection |= beam.direction;
						}
					}

					var direction = TransformDirectionToVector(beam.direction);

					if (IsInside(beam.row + direction.x, beam.column + direction.y))
					{
						beam.row += direction.x;
						beam.column += direction.y;

						var element = data.map[beam.row, beam.column].element;

						switch (data.map[beam.row, beam.column].element)
						{
							case ',': break;
							case '/': 
							case '\\':
							{
								beam.direction = ReflectDirection(element, beam.direction);
								break;
							}
							case '-':
							case '|':
							{
								var splitDirection = SplitDirections(element, beam.direction);

								if (splitDirection == beam.direction) break;

								beams.RemoveAt(i);

								if (splitDirection == BeamDirection.Horizontal)
								{
									beam.direction = BeamDirection.Left;
									var otherBeam = new Beam(beam.row, beam.column, BeamDirection.Right);
									
									beams.Add(beam);
									beams.Add(otherBeam);
								}
								else if (splitDirection == BeamDirection.Vertical)
								{
									beam.direction = BeamDirection.Up;
									var otherBeam = new Beam(beam.row, beam.column, BeamDirection.Down);
									
									beams.Add(beam);
									beams.Add(otherBeam);
								}
								else
								{
									throw new Exception($"Invalid split direction {beam.direction} {element} {splitDirection}");
								}

								break;
							}
						}
					}
					else
					{
						beams.RemoveAt(i);
					}
				}
			}

			var total = 0L;

			// Count energized
			for (var row = 0; row < data.map.GetLength(0); row++)
			{
				for (var column = 0; column < data.map.GetLength(1); column++)
				{
					if (data.map[row, column].beamDirection != BeamDirection.None)
					{
						total++;
					}

					// Clean up
					data.map[row, column].beamDirection = BeamDirection.None;
				}
			}

			return total;
		}









		// ----------------------------------------------------------------------------------
		// PART 1
		// ----------------------------------------------------------------------------------
		private IEnumerator ExecutePart1(RunSettings settings, Action<string> resultCallback)
		{
			ParseData();

			var total = ThrowBeam(0, -1, BeamDirection.Right);

			resultCallback(total.ToString());
		}









		// ----------------------------------------------------------------------------------
		// PART 2
		// ----------------------------------------------------------------------------------
		private IEnumerator ExecutePart2(RunSettings settings, Action<string> resultCallback)
		{
			ParseData();

			var best = 0L;

			for (var row = 0; row < data.map.GetLength(0); row++)
			{
				var first = ThrowBeam(row, -1, BeamDirection.Right);
				var second = ThrowBeam(row, data.map.GetLength(1), BeamDirection.Left);

				if (first > best) best = first;
				if (second > best) best = second;
			}

			for (var column = 0; column < data.map.GetLength(1); column++)
			{
				var first = ThrowBeam(-1, column, BeamDirection.Down);
				var second = ThrowBeam(data.map.GetLength(1), column, BeamDirection.Up);

				if (first > best) best = first;
				if (second > best) best = second;
			}

			resultCallback(best.ToString());
		}
	}
}

