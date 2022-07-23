using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeEvolution
{
	public enum CellType
	{
		leaf = 0, root, branch, seed, none
	}
	public class Cell
	{
		public CellType ctype;
		public vec2 pos;
		public bool growed;

		public Cell(vec2 pos, CellType ctype, bool growed = false)
		{
			this.pos = pos; this.ctype = ctype; this.growed = growed;
		}

		public static bool CanGrow(CellType parent, CellType child, vec2 pos)
		{
			bool ret = true;
			switch (parent)
			{
				case CellType.seed:
					ret = false; break;
				case CellType.leaf:
					ret = false; break;
				case CellType.root:
					if (child == CellType.leaf)
						ret = false;
					if (child == CellType.seed)
						ret = false;
					break;
				case CellType.branch:
					if ((child == CellType.leaf) ||
						(child == CellType.seed))
						if (Program.world[pos.x][pos.y].mass != 0)
							ret = false;
					if (child == CellType.root)
						if (Program.world[pos.x][pos.y].mass == 0)
							ret = false;
					break;
			}
			return ret;
		}
	}
}
