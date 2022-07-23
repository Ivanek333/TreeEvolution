using System;
using System.Linq;
using System.Collections.Generic;

namespace TreeEvolution
{
	public static class d //Debug.Log
	{
		public static bool debug;
		public static void l(object s, bool newline = true)
		{
			if (debug)
				if (newline)
					Console.WriteLine(s);
				else
					Console.Write(s);
		}
		public static void c() { Console.Clear(); }
	}

	public struct vec2
	{
		public int x, y;

		public vec2(int x, int y)
		{
			this.x = x; this.y = y;
		}
	}

	public class FieldCell
	{
		public int energy, mass, block, shadow;
		public CellType ctype;

		public FieldCell(int energy, int mass = 0, int block = 0, CellType ctype = CellType.none, int shadow = 0)
		{
			this.energy = energy; this.mass = mass; this.block = block; this.ctype = ctype; this.shadow = shadow;
		}

		public FieldCell()
		{
			energy = 0; mass = 0; block = 0; ctype = CellType.none; shadow = 0;
		}
	}
}