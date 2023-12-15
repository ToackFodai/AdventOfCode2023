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
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using System.Collections.Specialized;

namespace SpoonmanGames.CodingAdventures.AdventOfCode2023
{
	public class Day15 : Exercise
	{
		// ----------------------------------------------------------------------------------
		// METHODS
		// ----------------------------------------------------------------------------------
		private int IncreaseStep(int step, int value) => ((step + value) * 17) % 256;

		private int CalculateHASHMAP(string label)
		{
			var total = 0;

			for (var i = 0; i < label.Length; i++)
			{
				total = IncreaseStep(total, (int)label[i]);
			}

			return total;
		}









		// ----------------------------------------------------------------------------------
		// PART 1
		// ----------------------------------------------------------------------------------
		private IEnumerator ExecutePart1(RunSettings settings, Action<string> resultCallback)
		{
			// ParseData();

			var line = lines[0];
			var step = 0;
			var total = 0L;

			for (var i = 0; i < line.Length; i++)
			{
				var character = line[i];
				var value = (int)character;

				if (character != ',')
				{
					step = IncreaseStep(step, value);
				}

				if (character == ',' || i == line.Length - 1)
				{
					total += step;
					step = 0;
				}
			}

			resultCallback(total.ToString());
		}








		// ----------------------------------------------------------------------------------
		// PART 2
		// ----------------------------------------------------------------------------------
		private IEnumerator ExecutePart2(RunSettings settings, Action<string> resultCallback)
		{
			// ParseData();

			var operations = lines[0].Split(',');
			var boxes = new Dictionary<int, OrderedDictionary>();

			for (var i = 0; i < 256; i++)
			{
				boxes.Add(i, new OrderedDictionary());
			}

			foreach (var operation in operations)
			{
				var operationIndex = Mathf.Max(operation.IndexOf('='), operation.IndexOf('-'));
				var label = operation.Substring(0, operationIndex);
				var box = CalculateHASHMAP(label);

				if (operation[operationIndex] == '=')
				{
					var value = operation.Substring(operationIndex+1);
		
					if (boxes[box].Contains(label))
						boxes[box][label] =  value;
					else
						boxes[box].Add(label, value);
				}
				else if (operation[operationIndex] == '-')
				{
					if (boxes[box].Contains(label))
						boxes[box].Remove(label);
				}
			}

			var totalFocusingPower = 0L;

			for (var i = 0; i < 256; i++)
			{
				var box = boxes[i];

				for (var j = 0; j < box.Count; j++)
				{
					var focusingPower = (i + 1) * (j + 1) * int.Parse((string)box[j]);
					
					totalFocusingPower += focusingPower;
				}
			}

			resultCallback(totalFocusingPower.ToString());
		}
	}
}