using System;
using System.Collections.Generic;
using System.Linq;

namespace marmot
{
	internal class Helpers
	{

		private double ConstraintProportion(double a, double b)
		{
			// disincentivize long, narrow rooms
			if (a == 0) a = 0.01;
			if (b == 0) b = 0.01;
			return Math.Max(a, b) / Math.Min(a, b) - 2;
		}


		public static Tuple<List<double>, double> NelderMead(
		Func<List<double>, double> f,
		List<double> xStart,
		double step = 0.1,
		double noImproveThr = 10E-6,
		int noImprovBreak = 10,
		int maxIter = 0,
		double alpha = 1.0,
		double gamma = 2.0,
		double rho = -0.5,
		double sigma = 0.5,
		double prevBestScore = 0)
		{
			int dim = xStart.Count;
			double prevBest = f(xStart);
			int noImprov = 0;
			List<List<double>> res = new List<List<double>>();
			List<double> temp;

			for (int i = 0; i <= dim; i++)
			{
				if (i == 0)
				{
					temp = new List<double>(xStart);
					temp.Add(prevBest);
					res.Add(temp);
				}
				else
				{
					temp = new List<double>(xStart);
					temp[i - 1] += step;
					double score = f(temp);
					temp.Add(score);
					res.Add(temp);
				}
			}

			int iter = 0;
			while (true)
			{
				res = res.OrderBy(x => x.Last()).ToList();
				double best = res[0].Last();

				if (prevBestScore != 0 && iter >= noImprovBreak && best >= prevBestScore)
				{
					return new Tuple<List<double>, double>(res[0].Take(dim).ToList(), res[0].Last());
				}

				if (maxIter != 0 && iter >= maxIter)
				{
					return new Tuple<List<double>, double>(res[0].Take(dim).ToList(), res[0].Last());
				}

				iter++;

				if (best < prevBest - noImproveThr)
				{
					noImprov = 0;
					prevBest = best;
				}
				else
				{
					noImprov++;
				}

				if (noImprov >= noImprovBreak)
				{
					return new Tuple<List<double>, double>(res[0].Take(dim).ToList(), res[0].Last());
				}

				List<double> x0 = new List<double>(new double[dim]);
				for (int i = 0; i < dim; i++)
				{
					x0[i] = res.Take(dim).Average(x => x[i]);
				}

				List<double> xr = x0.Zip(res.Last(), (xi, xLasti) => xi + alpha * (xi - xLasti)).ToList();
				double rscore = f(xr);

				if (res[0].Last() <= rscore && rscore < res[res.Count - 2].Last())
				{
					res[res.Count - 1] = xr;
					res[res.Count - 1].Add(rscore);
					continue;
				}

				if (rscore < res[0].Last())
				{
					List<double> xe = x0.Zip(res.Last(), (xi, xLasti) => xi + gamma * (xi - xLasti)).ToList();
					double escore = f(xe);

					if (escore < rscore)
					{
						res[res.Count - 1] = xe;
						res[res.Count - 1].Add(escore);
						continue;
					}
					else
					{
						res[res.Count - 1] = xr;
						res[res.Count - 1].Add(rscore);
						continue;
					}
				}

				List<double> xc = x0.Zip(res.Last(), (xi, xLasti) => xi + rho * (xi - xLasti)).ToList();
				double cscore = f(xc);
				if (cscore < res.Last().Last())
				{
					res[res.Count - 1] = xc;
					res[res.Count - 1].Add(cscore);
					continue;
				}

				List<double> x1 = res[0].Take(dim).ToList();
				List<List<double>> nres = new List<List<double>>();
				foreach (List<double> tup in res)
				{
					List<double> redx = x1.Zip(tup, (x1i, tupi) => x1i + sigma * (tupi - x1i)).ToList();
					double score = f(redx);
					redx.Add(score);
					nres.Add(redx);
				}
				res = nres;
			}
		}

	}
}
