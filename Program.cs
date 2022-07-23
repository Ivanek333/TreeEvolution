using System;
using System.Linq;
using System.Collections.Generic;

namespace TreeEvolution
{
	public static class Program
	{

		public static List<List<FieldCell>> world;
		public static List<Tree> trees;
		public static vec2 worldSize;
		public static int alive;

		public static void DrawWorld(int layer)
		{
			string s = $"Generation ~ {alive / Tree.maxLifeTime}";
			s += $"  Lifetime: {Tree.maxLifeTime}  Iteration: {alive}";
			for (int j = worldSize.y - 1; j >= 0; j--)
			{
				for (int i = 0; i < worldSize.x; i++)
				{
					switch (layer)
					{
						case 0:
							s += world[i][j].mass;
							break;
						case 1:
							s += world[i][j].energy;
							break;
						case 2:
							s += world[i][j].block;
							break;
						case 3:
							switch (world[i][j].ctype)
							{
								case CellType.none:
									if (world[i][j].mass > 0)
										s += "%";
									//else if (world[i][j].shadow != 0)
									//    s += world[i][j].shadow; // для просмотра уровня теней
									else
										s += " ";
									break;
								case CellType.leaf:
									s += "~"; break;
								case CellType.branch:
									s += "@"; break;
								case CellType.root:
									s += "#"; break;
								case CellType.seed:
									s += "*"; break;
							}
							break;
					}
				}
				s += "\n";
			}
			Console.Write(s);
		}
		
		public static void UpdateShadows()
		{
			for (int i = 0; i < worldSize.x; i++)
			{
				int sh = Tree.maxShadow;
				for (int j = worldSize.y - 1; j >= 0; j--)
				{
					world[i][j].shadow = sh;
					if ((world[i][j].block == 1) && (sh > 0))
					    sh--;
				}
			}
		}

		public static void Main()
		{
			int[] energyVertical = new int[]
			{ 0, 0, 0, 0, 0, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};
			int[] massVertical = new int[]
			{ 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

			Tree.MtoEconvert = 1;  //масса превращается в энергию, если она закончилась
			Tree.defaultEnergy = 10; //энергия семени
			Tree.defaultMass = 10;    //масса семени
			Tree.energyConsume = 3;  //расход энергии за один шаг
			Tree.massToGrow = 1;     //расход массы для роста новых клеток
			Tree.seedEMscale = 5;    //увеличение расхода для семени
			Tree.maxLifeTime = 100;  //максимальная продолжительность жизни
			Tree.maxShadow = 3;      //максимальный уровень теней (сколько преград может преодолеть)
            Tree.level3mass = 3; // коэфицент увеличение стоимости роста и поддержания жизни на этом уровне
            
			Gen.rand = new Random();
			Gen.randWgenAmp = 2;      //амплитуда при случаном заполнении гена
			Gen.randWeditAmp = 1;     //амплитуда при мутации
			Gen.randWeditChance = 5.0;  //шанс мутации в %, можно ставить дробные значения
			Gen.lowIQmode = false;    //убирает возможность ничего не ставить (хз зачем надо)

			worldSize = new vec2(100, energyVertical.Count());

			int treeSpawnCount = worldSize.x / 15;

			world = new List<List<FieldCell>>();
			for (int i = 0; i < worldSize.x; i++)
			{
				world.Add(new List<FieldCell>());
				for (int j = 0; j < worldSize.y; j++)
				{
					world[i].Add(new FieldCell(energyVertical[j], massVertical[j]));
				}
			}
			DrawWorld(0);
			Console.WriteLine();
			DrawWorld(1);
			Console.WriteLine();
			DrawWorld(2);

			trees = new List<Tree>();
			for (int i = 0; i < treeSpawnCount; i++)
			{
				vec2 p = new vec2(worldSize.x / treeSpawnCount * i, 3);
				Tree t = new Tree(p, false);
				trees.Add(t);
			}
			
			//Требования по кол-ву
			Tree.reqLeaf = 2;   // листьев
			Tree.reqRoot = 1;   // корней
			Tree.reqBranch = 2; // стеблей
			Tree.reqSeed = 0;   // семян
			
			d.debug = false;       //дебаг моде он/офф
			bool manual = false;   //тыкать на кнопки для следующего шага
			bool autoreset = true; //если все деревья умерли, автоматически генерировать заново
			bool matchreq = false; //необоходимо соответствовать требованиям (на определенном шаге)
			int fastdelay = 1;    //задержка быстрого просмотра
			int slowdelay = 500;   //задержка медленного просмотра
			int delay = fastdelay; //текущая задержка
			alive = 0;             //счетчик шагов
			int autor = 0;         //счетчик перезапусков (ограничен 50)
			int skip = 0000;      //сколько шагов скипнуть без отрисовки(супер быстро)
			int slowtime = 99000;  //время для включения медленной скорости
			int reqafter = 500;    //после скольких шагов начинать проверять требования
			while (true)
			{
				alive++;

				if (manual && (alive >= skip))
				{
					var key = (char)Console.Read();
					if (key == 'b') break;
				}

				int tcount = trees.Count;

				d.l("tcount: " + tcount);
				for (int i = 0; i < tcount; i++)
				{
					trees[i].UpdateTree();
				}
				for (int i = 0; i < tcount; i++)
				{
					d.l("tree: " + i);
					trees[i].GrowTree();
					d.l("tree: " + i + " end");
				}
				for (int i = 0; i < tcount; i++)
				{
					if (trees[i].needToDie)
					{
						if ((alive > reqafter) && !trees[i].destroy && matchreq)
						{
							if (!trees[i].MatchRequest(Tree.reqLeaf, Tree.reqRoot, Tree.reqBranch, Tree.reqSeed))
								trees[i].destroy = true;
						}
						trees[i].Die();
						Program.trees.RemoveAt(i);
						i--;
						tcount--;
					}
				}
				UpdateShadows();
				if (alive > skip)
				{
					//Console.Clear();
					DrawWorld(3);
					System.Threading.Thread.Sleep(delay);
				}
				else
				{
					if (alive % Math.Max((skip / 100), 500) == 0)
						Console.WriteLine("alive: " + alive + " (" + (alive / (skip / 100)) + "%)");
				}

				if (trees.Count == 0)
				{
					Console.WriteLine("No trees left");
					if (autoreset)
					{
						autor++;
						if (autor > 50)
						{
							Console.WriteLine("Too much resets");
							break;
						}
						else
						{
							alive = 0;
							delay = fastdelay;
							for (int i = 0; i < treeSpawnCount; i++)
							{
								vec2 p = new vec2(worldSize.x / treeSpawnCount * i, 3);
								Tree t = new Tree(p, false);
								trees.Add(t);
							}
						}
					}
					else
						break;
				}
				else if (alive > slowtime)
				{
					delay = slowdelay;
				}
			}
			Console.WriteLine("end");
		}
	}
}