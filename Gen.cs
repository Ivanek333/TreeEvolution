using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeEvolution
{
	public class Gen
	{
		public static double randWgenAmp;
		public static double randWeditAmp;
		public static double randWeditChance;
		public static bool lowIQmode;

		public static Random rand;

		public List<double> w;

		public Gen()
		{
			CreateRandomWeights();
		}

		public Gen(List<double> tw)
		{
			w = new List<double>();
			tw.ForEach((double v) => w.Add(v));
		}

		public void CreateRandomWeights()
		{
			w = new List<double>();
			for (int i = 0; i < 16 * 10 + 10 * 20; i++)
			{
				w.Add((rand.Next() % (100 * randWgenAmp * 2)) / 100.0 - randWgenAmp);
			}
		}

		public void MutateGen()
		{
			for (int i = 0; i < w.Count; i++)
			{
				double c = (rand.Next() % 10000) / 100.0;
				if (c <= randWeditChance)
				{
					w[i] += (rand.Next() % (100 * randWeditAmp * 2)) / 100.0 - randWeditAmp;
				}
			}
		}

		public List<CellType> CalculateOutput(int treeE, int treeM, int meRoot, int meBranch, List<int> energies, List<int> masses, List<int> walls)
		{
			List<double> middle = new List<double>();
			List<double> output = new List<double>();
			List<double> input = new List<double>
			{ treeE, treeM, meRoot, meBranch };
			for (int i = 0; i < 4; i++)
			{
				input.Add(energies[i]);
				input.Add(masses[i]);
				input.Add(walls[i]);
			}
			for (int i = 0; i < 10; i++)
			{
				middle.Add(0);
				for (int j = 0; j < input.Count; j++)
				{
					middle[i] += input[j] * w[i * input.Count + j];
				}
			}
			for (int i = 0; i < 20; i++)
			{
				output.Add(0);
				for (int j = 0; j < 10; j++)
				{
					output[i] += middle[j] * w[16 * 10 + i * 10 + j];
				}
			}
			List<CellType> outs = new List<CellType>();
			List<double> max = new List<double>();
			for (int i = 0; i < 4; i++)
			{
				max.Add(output[i]);
				outs.Add((CellType)0);
				for (int j = 1; j < (lowIQmode ? 4 : 5); j++)
				{
					if (output[i + j * 4] > max[i])
					{
						max[i] = output[i + j * 4];
						outs[i] = (CellType)j;
					}
				}
			}
			return outs;
		}
	}
}
