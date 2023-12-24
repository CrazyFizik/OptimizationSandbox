using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.TestFunctions;
using Optimization.Interfaces;
using Optimization.Heuristic.ParticleSwarmOptimization;
using Optimization.Heuristic.GeneticlAlgorithm;
using Optimization.Heuristic.SimulatedAnnealing;

using OptimizationSandbox.Tasks.COA;
using OptimizationSandbox.Tasks.COA.Problem;


namespace OptimizationSandbox
{
    class Program
    {
        //static Random _random = new Random();

        static void Main(string[] args)
        {
            //try
            {
                //TestRandom();

                Console.WriteLine("\nGlobal optimization demo\n");
                TestCOA();
                //TestFunctions();
                Console.WriteLine("\nEnd demo\n");
                Console.WriteLine("\nPlease press any key");
                Console.ReadLine();
            }
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    Console.ReadLine();
            //}
        }

        #region Random shit

        /// <summary>
        ///  Box-Muller transform
        ///  https://en.wikipedia.org/wiki/Box–Muller_transform 
        /// </summary>
        static double BoxMuller(Random random)
        {
            double stddev = 1.0;
            double mean = 0;

            //Box-Muller Begin
            double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - random.NextDouble();

            double randStdNormal =
                Math.Sqrt(-2.0 * Math.Log(u1)) *
                Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)   

            double randomn = randStdNormal * stddev + mean;
            //Box-Muller End

            return randomn;
        }

        /// <summary>
        ///  Marsaglia polar method with random number strong in (-1, 1) interval (ie  sigma = 0.33(3)
        ///  https://en.wikipedia.org/wiki/Marsaglia_polar_method 
        /// </summary>
        static double Marsaglia(Random random)
        {
            double stddev = 1.0;
            double mean = 0.0;

            double u, v, rSquared;
            do
            {
                u = 2.0 * random.NextDouble() - 1.0;
                v = 2.0 * random.NextDouble() - 1.0;
                rSquared = u * u + v * v;
            }
            while (rSquared >= 1.0 || rSquared == 0);

            double polar = Math.Sqrt(-2.0 * Math.Log(rSquared) / rSquared);
            double randomn = u * polar * stddev + mean;

            return randomn;
        }

        delegate double TestFucntion(Random random);
        delegate double TestRandomGenerator(double mean = 0.0, double sigma = 1.0);

        static void TestRandom(TestRandomGenerator generator, int n)
        {
            double[] randn = new double[n];
            for (int i = 0; i < n; i++)
            {
                randn[i] = generator();
            }

            TestRandom(randn);
            Console.WriteLine("Samples: " + n);
        }

        static void TestRandom(TestFucntion generator, Random random, int n)
        {
            double[] randn = new double[n];            
            for (int i = 0; i < n; i++)
            {
                randn[i] = generator(random);
            }

            TestRandom(randn);
            Console.WriteLine("Samples: " + n);
        }

        static void TestRandom(double[] randomn)
        {
            int n = randomn.Length;
            double stddev = 1;
            
            double norm = 0;
            int sigma1, sigma2, sigma3;
            sigma1 = sigma2 = sigma3 = 0;

            double variance = 0;

            for (int i = 0; i < n; i++)
            {
                double r = randomn[i];
                norm += r * r;
                if (r < 1 * stddev && r > -1 * stddev) sigma1++;
                if (r < 2 * stddev && r > -2 * stddev) sigma2++;
                if (r < 3 * stddev && r > -3 * stddev) sigma3++;
                if (Math.Abs(r) > variance) variance = Math.Abs(r) / stddev;
                Console.WriteLine(randomn[i] + " ");
            }
            norm = Math.Sqrt(norm);
            Console.WriteLine("L2-norm = " + norm);
            Console.WriteLine("Variance = " + variance);
            Console.WriteLine("Q1 = " + (double)sigma1 / n + " need 0.68"); // need 0.68
            Console.WriteLine("Q2 = " + (double)sigma2 / n + " need 0.95"); // need 0.95
            Console.WriteLine("Q3 = " + (double)sigma3 / n + " need 0.997"); // need 0.997            
        }

        #endregion

        #region Optimizators        

        static void GA(IProblem problem, double[][] startPop = null)
        {
            Console.WriteLine("\nBegin Evolutionary Optimization demo\n");
            string path = "GA";
            Options options = new Options();
            options._size = 200;// problem._Length * problem._Length;//100
            options._mutationRate = 0.1;// 2.0 / problem._Length;
            options._crossoverRate = 0.75;
            options._eliteRate = 0.05;
            options._maxLoop = 1000;// * model._Length;
            options._crossoverType = eCrossoverType.SBX;//.Flat;//.SBX;//.BLX_alpha;
            options._mutationType = eMutationType.NonUniform;//.Muhleblein;//.Polyniminal;
            options._selectionType = eSelectionType.Tournament;//.StohoasticUniversalSampling;//.RouletteWheel;//.StohoasticUniversalSampling;
            options._strategyType = eStrategyType.Canonical;

            Optimization.Heuristic.GeneticlAlgorithm.Chromosome.ChromosomeDouble ancestor =
                new Optimization.Heuristic.GeneticlAlgorithm.Chromosome.ChromosomeDouble(problem);
            Optimization.Heuristic.GeneticlAlgorithm.DoubleEvolver ga = 
                new DoubleEvolver(problem, ancestor, options);

            ga.Init();
            ga._population._BestChromosome.Evaluate();

            if (startPop != null)
            {
                for (int i = 0; i < startPop.Length; i++)
                {
                    Optimization.Heuristic.GeneticlAlgorithm.Chromosome.ChromosomeDouble y =
                        new Optimization.Heuristic.GeneticlAlgorithm.Chromosome.ChromosomeDouble(problem);

                    for (int j = 0; j < startPop[i].Length; j++)
                    {
                        y._chromosome[j] = startPop[i][j];
                    }

                    y.Evaluate();
                    ga._population[i] = y.Clone();
                }
            }

            problem.Save(path+"0");
            Console.WriteLine("\nPopulation size = " + ga._population._Size);
            Console.WriteLine("Number genes = " + problem._Length);
            Console.WriteLine("minGene value = " + problem._LB[0].ToString("F1"));
            Console.WriteLine("maxGene value = " + problem._UB[0].ToString("F1"));
            Console.WriteLine("Mutants = " + ga._population._MutantsNumber.ToString());
            Console.WriteLine("Xover  = " + ga._population._XoverNumner.ToString());
            Console.WriteLine("Elites  = " + ga._population._ElitesNumber.ToString());
            Console.WriteLine("Maximum generations = " + ga._population._Genertaions);
            Console.WriteLine("Selection type = " + ga._population._SelectionType);
            Console.WriteLine("Xover type = " + ga._population._BestChromosome._CrossoverType);
            Console.WriteLine("Mutation type = " + ga._population._BestChromosome._MutationType);

            //GA part
            double[] bestga = ga.Solve();
            double fitness = problem._ObjectiveFunction(bestga);
            path = fitness.ToString("F0") + "_" + path;
            problem.Save(path);
            Console.WriteLine("=======================\n");
            Console.WriteLine("Solve GA loop complete");
            Console.WriteLine("\nFinal GA solution:");
            Console.WriteLine(ga.ToString());
            Console.WriteLine("\nEnd Evolutionary Optimization demo\n");
            Console.WriteLine("=======================\n");

            //Next
            startPop = new double[ga._population._ElitesNumber][];
            for (int i = 0; i < startPop.Length; i++)
            {
                startPop[i] = new double[ga._population._Elites[i]._Length];
                ga._population._Elites[i].Evaluate();
                startPop[i] = problem._X0;
            }

            //SA part
            string name = "SA";
            Console.WriteLine("\nBegin Simulated annealing optimization demo\n");
            SimulatedAnnealing sa = new SimulatedAnnealing(problem, startPop);
            Console.WriteLine("Entering main solve loop");
            ISolution solution = sa.Solve();
            double[] bestsa = solution._X;
            double bestfaval = solution._Fval;
            int n = sa._Solutions.Count;
            double[][] particles = new double[n][];
            if (particles.Length > 1)
            {
                for (int i = 0; i < n; i++)
                {
                    bestfaval = problem._ObjectiveFunction(sa._Solutions[i]._X);
                    path = name + "_" + i.ToString() + "_" + bestfaval.ToString("F0") + "_" + DateTime.Now.ToString("yyyy.mm.dd.HH.mm");
                    problem.Save(path);

                    particles[i] = new double[problem._Length];
                    for (int j = 0; j < problem._Length; j++)
                    {
                        particles[i][j] = sa._Solutions[i]._X[j];
                    }
                }
            }
            path = name + "_" + bestfaval.ToString("F0") + "_" + DateTime.Now.ToString("yyyy.mm.dd.HH.mm");
            problem.Save(path);
            Console.WriteLine("=======================\n");
            Console.WriteLine("Solve SA loop complete");
            Console.WriteLine("\nFinal SA solution:");
            Console.WriteLine(sa.ToString());
            Console.WriteLine("\nEnd SA Optimization demo\n");
            Console.WriteLine("=======================\n");

            //Console GA
            Console.WriteLine("=======================");
            Console.WriteLine("Best GA solution found:");
            for (int i = 0; i < bestga.Length; ++i)
            {
                Console.Write(bestga[i].ToString("F4") + " ");
            }
            Console.WriteLine("\nFunction value at best solution = " + ga._population._FitnessBest.ToString("F4"));
            //Console.WriteLine("\nEnd Evolutionary Optimization demo\n");
            Console.WriteLine("=======================\n");
            //Console SA     
            Console.WriteLine("=======================");
            Console.WriteLine("Best SA solution found:");
            for (int i = 0; i < problem._Length; ++i)
            {
                Console.Write(bestsa[i].ToString("F4") + " ");
            }
            Console.WriteLine("\nFunction value at best solution = " + sa._Solution._Fval.ToString("F4"));
            //Console.WriteLine("\nEnd SA Optimization demo\n");
            Console.WriteLine("=======================\n");
        }

        static void MSO(IProblem model, double[][] particles = null)
        {
            string path = "mso";

            int maxLoop = 2048;
            int numParticles = 12; // number particles in each swarm
            int numSwarms = 3; // number swarms in multiswarm

            Console.WriteLine("\nBegin Multiple Particle Swarm optimization demo\n");
            Console.WriteLine("Setting number particles in each swarm = "+numParticles);
            Console.WriteLine("Setting number swarms in multiswarm = "+numSwarms);

            Console.WriteLine("\nInitializing all swarms in multiswarm");
            Multiswarm multiSwarm = new Multiswarm(model, numSwarms, numParticles, model._LB[0], model._UB[0]);
            Console.WriteLine("\nInitial multiswarm:");
            Console.WriteLine(multiSwarm.ToString());

            Console.WriteLine("\nSetting maxLoop = " + maxLoop);
            Console.WriteLine("Entering main solve loop");
            multiSwarm.Solve(maxLoop);

            Console.WriteLine("Solve loop complete");
            Console.WriteLine("\nFinal multiswarm:");
            Console.WriteLine(multiSwarm.ToString());

            model._ObjectiveFunction(multiSwarm._bestMultiPos);
            model.Save(path);

            Console.WriteLine("\nBest solution found = " + multiSwarm._bestMultiCost.ToString("F6"));
            Console.Write("at x0 = " + multiSwarm._bestMultiPos[0].ToString("F4"));
            Console.WriteLine(", x1 = " + multiSwarm._bestMultiPos[1].ToString("F4"));
            Console.WriteLine("\nEnd Multi-swarm demo\n");
            Console.WriteLine("=======================\n");
        }

        static void PSO(IProblem model, double[][] particles = null)
        {
            string path = "pso";

            int numParticles = 36; // number particles in each swarm
            int maxLoop = 2048;

            Console.WriteLine("\nBegin Particle Swarm optimization demo\n");
            Console.WriteLine("Setting number particles in each swarm = " + (numParticles).ToString());


            Console.WriteLine("\nInitializing all particles");
            Swarm swarm = new Swarm(model, numParticles, model._LB[0], model._UB[0]);
            Console.WriteLine("\nInitial particles:");
            Console.WriteLine(swarm.ToString());

            Console.WriteLine("\nSetting maxLoop = " + maxLoop);
            Console.WriteLine("Entering main solve loop");
            swarm.Solve(maxLoop);

            model._ObjectiveFunction(swarm._bestSwarmPos);
            model.Save(path);

            Console.WriteLine("Solve loop complete");
            Console.WriteLine("\nFinal swarm:");
            Console.WriteLine(swarm.ToString());

            Console.WriteLine("\nBest solution found = " + swarm._bestSwarmCost.ToString("F6"));
            Console.Write("at x0 = " + swarm._bestSwarmPos[0].ToString("F4"));
            Console.WriteLine(", x1 = " + swarm._bestSwarmPos[1].ToString("F4"));
            Console.WriteLine("\nEnd PSO demo\n");
            Console.WriteLine("=======================\n");
        }

        static void FA(IProblem model, double[][] particles = null)
        {
            string path = "FA";

            int numParticles = 20; // number particles in each swarm
            int maxLoop = 2048;

            Console.WriteLine("\nBegin Firefly optimization demo\n");
            Console.WriteLine("Setting number particles = " + (numParticles).ToString());


            Console.WriteLine("\nInitializing all Fireflyes");
            FireFlyOptimization fa;
            if (particles == null)
            {
                 fa = new FireFlyOptimization(model, numParticles, maxLoop);
            }
            else
            {
                fa = new FireFlyOptimization(model, particles, maxLoop);
            }

            Console.WriteLine("\nInitial particles:");
            Console.WriteLine(fa.ToString());

            Console.WriteLine("\nSetting maxLoop = " + maxLoop);
            Console.WriteLine("Entering main solve loop");
            fa.Solve();

            double fitness = model._ObjectiveFunction(fa._Solution);
            path = fitness.ToString("F0") + "_" + path;
            model.Save(path);

            for (int i = 0; i < fa._population.Length; i++)
            {
                fitness = model._ObjectiveFunction(fa._population[i]._position);
                model.Save(path + "_" + i.ToString() + "_" + fitness.ToString("F0"));
            }

            Console.WriteLine("Solve loop complete");
            Console.WriteLine("\nFinal firefly:");
            Console.WriteLine(fa.ToString());


            Console.WriteLine("\nBest (x,y) solution found:");
            for (int i = 0; i < fa._Solution.Length; ++i)
            {
                Console.Write(fa._Solution[i].ToString("F4") + " ");
            }

            Console.WriteLine("\nFunction value at best solution = " + fa._Cost.ToString("F4"));
            Console.WriteLine("\nEnd FA demo\n");
            Console.WriteLine("=======================\n");
        }

        static void SA(IProblem problem1, IProblem problem2 = null, double[][] startSolutions = null)
        {
            double fitness;
            string path;
            string name = "SA";

            Console.WriteLine("\nBegin Simulated annealing optimization demo\n");

            Optimization.Unconstrained.BFGS bfgs =null;
            SimulatedAnnealing sa = null;

            if (problem2 != null) bfgs = new Optimization.Unconstrained.BFGS(problem2);
            sa = new SimulatedAnnealing(problem1, startSolutions, bfgs);
            

            Console.WriteLine("Entering main solve loop");
            sa.Solve();

            int n = sa._Solutions.Count;
            double[][] particles = new double[n][];
            if (particles.Length > 1)
            {
                for (int i = 0; i < n; i++)
                {
                    fitness = problem1._ObjectiveFunction(sa._Solutions[i]._X);
                    path = name + "_" + i.ToString() + "_" + fitness.ToString("F0") + "_" + DateTime.Now.ToString("yyyy.mm.dd.HH.mm");
                    problem1.Save(path);

                    particles[i] = new double[problem1._Length];
                    for (int j = 0; j < problem1._Length; j++)
                    {
                        particles[i][j] = sa._Solutions[i]._X[j];
                    }
                }
            }

            fitness = problem1._ObjectiveFunction(sa._Solution._X);
            path = name + "_" + fitness.ToString("F0") + "_" + DateTime.Now.ToString("yyyy.mm.dd.HH.mm");
            problem1.Save(path);

            Console.WriteLine("Solve loop complete");
            Console.WriteLine("\nFinal solution:");
            Console.WriteLine(sa.ToString());


            Console.WriteLine("\nBest (x,y) solution found:");
            for (int i = 0; i < problem1._Length; ++i)
            {
                Console.Write(sa._Solution._X[i].ToString("F4") + " ");
            }

            Console.WriteLine("\nFunction value at best solution = " + sa._Solution._Fval.ToString("F4"));
            Console.WriteLine("\nEnd SA Optimization demo\n");
            Console.WriteLine("=======================\n");

            //Console.WriteLine("Start FA");
            //FA(model, particles);

            //Console.WriteLine("Start GA");
            //GA(model, particles);
        }

        static void BFGS(IProblem model, double[] x0 = null)
        {
            string name = "BFGS";

            Console.WriteLine("\nBegin BFGS optimization demo\n");
            Optimization.Unconstrained.BFGS bfgs = new Optimization.Unconstrained.BFGS(model);
            double[] x = bfgs.Solve()._X;

            double fitness = model._ObjectiveFunction(x);
            string path = name + "_" + fitness.ToString("F0") + "_" + DateTime.Now.ToString("yyyy.mm.dd.HH.mm");
            model.Save(path);

            Console.WriteLine("\nFunction value at best solution = " + fitness.ToString("F4"));
            Console.WriteLine("Minimized at x0 = [" + String.Join(",", x) + "]");
            Console.WriteLine("\nEnd BFGS Optimization demo\n");
            Console.WriteLine("=======================\n");
        }

        #endregion

        static void TestFunctions()
        {
            int dim = 20;

            List<IProblem> functions = new List<IProblem>();

            //SimpleFunction simple = new SimpleFunction(dim);
            //functions.Add(simple);
            Himmelblau himmelblau = new Himmelblau();
            functions.Add(himmelblau);
            Spherical spherical = new Spherical(dim);
            functions.Add(spherical);
            Rosenbrok rosenbrok = new Rosenbrok(dim);
            functions.Add(rosenbrok);
            Rastrigin rastring = new Rastrigin(dim);
            functions.Add(rastring);

            //DateTime time;
            //TimeSpan span;
            //double[] x = new double[dim];
            //Random r = new Random();
            //for (int i =0; i < dim; i++)
            //{
            //    x[i] = r.NextDouble();
            //}
            //time = DateTime.Now;
            //for (int i = 0; i < 10000; i++)
            //{
            //    spherical._ObjectiveFunction(x);
            //}            
            //span = DateTime.Now - time;
            //Console.WriteLine(span.TotalMilliseconds.ToString());
            //Console.ReadKey();

            foreach (IProblem model in functions)
            {
                Console.WriteLine("\nPlease press any key");
                Console.ReadLine();
                Console.WriteLine(model.ToString());

                //PSO(model);
                //FA(model);
                //GA(model);
                SA(model);
                //BFGS(model);
                Console.WriteLine(model.ToString());
            }
        }

        static void TestCOA()
        {
            string s;
            Design design1 = Deafult.CraeteDeafult();
            Design design2 = Deafult.CraeteDeafult2();


            var model1 = new Model(design1, 0.5);
            var model2 = new Model(design2, 1.0);

            var polar1 = new Polar(model1);
            var absolute1 = new Absolute(model1);

            var polar2 = new Polar(model2);
            var absolute2 = new Absolute(model2);

            var model3 = new Spherical(20);

            Console.WriteLine("\nPlease press any key");
            Console.ReadLine();

            double[][] x0 = new double[1][];
            //GA(polar2);
            //s = "RESULT\n"
            //    + "Area: " + model2._Out._conflictedArea.ToString() + "\n"
            //    + "MainCoupling: " + model2._Out._mainCouplingConflict.ToString() + "\n"
            //    + "CrossCoupling: " + model2._Out._crossCouplingConflict.ToString() + "\n";
            //Console.WriteLine(s);

            x0 = null;
            //x0[0] = polar2._X0;
            SA(polar2, null, x0);
            s = "RESULT\n"
                + "Area: " + model2._Out._conflictedArea.ToString() + "\n"
                + "MainCoupling: " + model2._Out._mainCouplingConflict.ToString() + "\n"
                + "CrossCoupling: " + model2._Out._crossCouplingConflict.ToString() + "\n";
            Console.WriteLine(s);

            //x0 = null;
            ////x0[0] = polar2._X0;
            //SA(polar2, absolute2, x0);
            //s = "RESULT\n"
            //    + "Area: " + model2._Out._conflictedArea.ToString() + "\n"
            //    + "MainCoupling: " + model2._Out._mainCouplingConflict.ToString() + "\n"
            //    + "CrossCoupling: " + model2._Out._crossCouplingConflict.ToString() + "\n";
            //Console.WriteLine(s);

            //x0[0] = absolute2._X0;
            //SA(absolute2, absolute2, x0);
            //s = "RESULT\n"
            //    + "Area: " + model2._Out._conflictedArea.ToString() + "\n"
            //    + "MainCoupling: " + model2._Out._mainCouplingConflict.ToString() + "\n"
            //    + "CrossCoupling: " + model2._Out._crossCouplingConflict.ToString() + "\n";
            //Console.WriteLine(s);

            //FA(model2);
            //MSO(model2);
            //PSO(model2);
        }

        static void TestRandom()
        {
            int n = 10000;
            Random _random = new Random();

            DateTime time;
            TimeSpan span;

            Console.WriteLine("Box-Muller");
            time = DateTime.Now;
            TestRandom(BoxMuller, _random, n);
            span = DateTime.Now - time;
            Console.WriteLine(span.TotalMilliseconds.ToString());
            Console.ReadKey();

            Console.WriteLine("Box-Muller-Marsagila");
            time = DateTime.Now;
            TestRandom(Marsaglia, _random, n);
            span = DateTime.Now - time;
            Console.WriteLine(span.TotalMilliseconds.ToString());
            Console.ReadKey();

            Console.WriteLine("Box-Muller-Marsagila");
            Optimization.MathHelper.GaussianRandom random = new Optimization.MathHelper.GaussianRandom(_random);
            TestRandomGenerator trg = random.NextGaussian;
            TestRandom(trg, n);
            Console.ReadKey();
        }

    }
}
