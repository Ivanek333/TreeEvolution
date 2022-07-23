using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeEvolution
{
	public class Tree
	{
		public static double MtoEconvert;
		public static int energyConsume, massToGrow;
		public static int defaultEnergy, defaultMass;
		public static int maxLifeTime, maxShadow;
		public static int level3mass, seedEMscale;

		public static int reqLeaf, reqRoot, reqBranch, reqSeed;

		public List<Cell> cells;
		public int energy, mass;
		public Gen gen;
		public bool seedState, needToDie, destroy;
		public int lifeTime;

		public Tree(vec2 pos, bool seed = false, int energy = 0, int mass = 0, List<double> tw = null)
		{
			lifeTime = 0;
			needToDie = false;
			destroy = false;
			cells = new List<Cell>();
			if (seed)
			{
				this.seedState = true;
				cells.Add(new Cell(pos, CellType.seed));
				Program.world[pos.x][pos.y].block = 1;
				Program.world[pos.x][pos.y].ctype = CellType.seed;
				this.energy = energy;
				this.mass = mass;
				this.gen = new Gen(tw);
				this.gen.MutateGen();
			}
			else
			{
				this.seedState = false;
				cells.Add(new Cell(pos, CellType.branch));
				Program.world[pos.x][pos.y].block = 1;
				Program.world[pos.x][pos.y].ctype = CellType.branch;
				this.energy = defaultEnergy;
				this.mass = defaultMass;
				this.gen = new Gen();
			}
		}

		public bool MatchRequest(int leafc = 0, int rootc = 0, int branchc = 0, int seedc = 0)
		{
			int tleaf = 0, troot = 0, tbranch = 0, tseed = 0;
			foreach (var cell in cells)
			{
				switch (cell.ctype)
				{
					case CellType.leaf:
						tleaf++; break;
					case CellType.root:
						troot++; break;
					case CellType.branch:
						tbranch++; break;
					case CellType.seed:
						tseed++; break;
				}
			}
			if ((tleaf >= leafc) &&
				(troot >= rootc) &&
				(tbranch >= branchc) &&
				(tseed >= seedc))
				return true;
			return false;
		}

		public void UpdateTree()
		{
			foreach (var cell in cells)
			{
				switch (cell.ctype)
				{
					case CellType.leaf:
						energy += Program.world[cell.pos.x][cell.pos.y].energy *
								  Program.world[cell.pos.x][cell.pos.y].shadow;
						break;
					case CellType.branch:
						//energy += Program.world[cell.pos.x][cell.pos.y].energy / 2;
						break;
					case CellType.root:
						mass += Program.world[cell.pos.x][cell.pos.y].mass;
						break;
				}
				energy -= energyConsume;
				if (cell.ctype == CellType.seed) energy -= energyConsume * (seedEMscale - 1);
			}
			if (mass < 0)
			{
				needToDie = true;
			}
			else if (energy < 0)
			{
				if (mass * MtoEconvert >= -energy)
				{
					int t = (int)((-energy) / MtoEconvert);
					if ((-energy) % MtoEconvert != 0) t++;
					mass -= t;
					energy += (int)(t * MtoEconvert);
				}
				else
				{
					needToDie = true;
				}
			}
		}

		public void GrowTree()
		{
			if (seedState)
			{
				vec2 topos = new vec2(cells[0].pos.x, cells[0].pos.y - 1);
				if (topos.y < 0) topos.y = 0;
				if (Program.world[topos.x][topos.y].block == 1)
				{
					needToDie = true;
					destroy = true;
				}
				else if (Program.world[topos.x][topos.y].mass != 0)
				{
					if (Program.world[topos.x][topos.y].mass == 1)
					{
						cells[0].ctype = CellType.branch;
						Program.world[cells[0].pos.x][cells[0].pos.y].ctype = CellType.branch;
						Program.world[cells[0].pos.x][cells[0].pos.y].block = 1;
						seedState = false;
					}
					else
					{
						needToDie = true;
						destroy = true;
					}
				}
				else
				{
					Program.world[cells[0].pos.x][cells[0].pos.y].block = 0;
					Program.world[cells[0].pos.x][cells[0].pos.y].ctype = CellType.none;
					Program.world[topos.x][topos.y].block = 1;
					Program.world[topos.x][topos.y].ctype = CellType.seed;
					cells[0].pos = topos;
				}
			}
			else if (lifeTime > maxLifeTime)
			{
				needToDie = true;
			}
			else
			{
				lifeTime++;
				int count = cells.Count;
				d.l(count);
				for (int l = 0; l < count; l++)
				{
					if (!cells[l].growed)
					{
						var cell = cells[l];
						cell.growed = true;
						List<int> engs = new List<int>();
						List<int> mss = new List<int>();
						List<int> wls = new List<int>();
						List<vec2> neigs = Neigbours(cell.pos, Program.worldSize);
						foreach (var n in neigs)
						{
							engs.Add(Program.world[n.x][n.y].energy);
							mss.Add(Program.world[n.x][n.y].mass);
							wls.Add(Program.world[n.x][n.y].block);
						}
						List<CellType> t = gen.CalculateOutput(
							energy, mass,
							(cell.ctype == CellType.root) ? 1 : 0,
							(cell.ctype == CellType.branch) ? 1 : 0,
							engs, mss, wls);
						for (int i = 0; i < 4; i++)
						{
							if ((wls[i] == 0) && (mass > massToGrow) &&
								(t[i] != CellType.none) &&
								(Math.Abs(neigs[i].y - cell.pos.y) <= 1) &&
								Cell.CanGrow(cell.ctype, t[i], neigs[i]) &&
								!(((i == 0) || (i == 2)) && (neigs[i].y == 3) && (mass < level3mass)))
							{
								int minusmass = 0;
								if (((i == 0) || (i == 2)) && (neigs[i].y == 3))
									minusmass = level3mass;
								else
									minusmass = massToGrow;
								if (t[i] == CellType.seed) minusmass *= seedEMscale;
								mass -= minusmass;
								Cell c = new Cell(neigs[i], t[i]);
								Program.world[c.pos.x][c.pos.y].block = 1;
								Program.world[c.pos.x][c.pos.y].ctype = c.ctype;
								cells.Add(c);
							}
						}
					}
				}
			}
		}

		public void Die()
		{
			foreach (var cell in cells)
			{
				Program.world[cell.pos.x][cell.pos.y].block = 0;
				Program.world[cell.pos.x][cell.pos.y].ctype = CellType.none;
				if (!destroy && (cell.ctype == CellType.seed))
				{
					Program.trees.Add(new Tree(cell.pos, true, defaultEnergy, defaultMass, gen.w));
				}
			}
		}

		public List<vec2> Neigbours(vec2 pos, vec2 size)
		{
			List<vec2> ret = new List<vec2>();
			ret.Add(new vec2((pos.x - 1 + size.x) % size.x, pos.y));
			ret.Add(new vec2(pos.x, (pos.y + 1) % size.y));
			ret.Add(new vec2((pos.x + 1) % size.x, pos.y));
			ret.Add(new vec2(pos.x, (pos.y - 1 + size.y) % size.y));
			return ret;
		}
	}
}
