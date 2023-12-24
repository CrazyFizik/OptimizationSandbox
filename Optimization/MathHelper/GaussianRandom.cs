using System;

namespace Optimization.MathHelper
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GaussianRandom
    {
        private bool _hasDeviation;
        private double _deviation;
        private readonly Random _random;

        public GaussianRandom(Random random = null)
        {
            _random = random ?? new Random();
        }

        /// <summary>
        /// Obtains normally (Gaussian) distributed random numbers, using the Box-Muller
        /// transformation with Marsaglia polar method. 
        /// This transformation takes two uniformly distributed deviates
        /// within the unit circle, and transforms them into two independently
        /// distributed normal deviates.
        /// </summary>
        /// <param name="mean">The mean of the distribution.  Default is zero.</param>
        /// <param name="sigma">The standard deviation of the distribution.  Default is one.</param>
        /// <returns></returns>
        public double NextGaussian(double mean = 0.0, double sigma = 1.0)
        {
            if (sigma <= 0.0)
                throw new ArgumentOutOfRangeException("sigma", "Must be greater than zero.");

            if (_hasDeviation)
            {
                _hasDeviation = false;
                return _deviation * sigma + mean;
            }

            double u, v, rSquared;
            do
            {
                // two random values between -1.0 and 1.0
                u = 2 * _random.NextDouble() - 1.0;
                v = 2 * _random.NextDouble() - 1.0;
                rSquared = u * u + v * v;
                // ensure within the unit circle
            } while (rSquared >= 1.0 || rSquared == 0.0);

            // calculate polar tranformation for each deviate
            var polar = Math.Sqrt(-2 * Math.Log(rSquared) / rSquared);

            // return second deviate
            var deviation = u * polar;

            // store first deviate
            _deviation = v * polar;
            _hasDeviation = true;

            //return normal random number
            return deviation * sigma + mean;
        }
    }
}
