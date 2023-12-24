using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Heuristic.GeneticlAlgorithm
{
    public enum eStrategyType
    {
        Canonical,
        Simple,
        SteadyState,
        Separate,
        Spices
    }

    public enum eSelectionType
    {
        RouletteWheelRank,              //Fitness's rank version
        RouletteWheel,                  //Classical
        Tournament,                     //Tornament
        StohoasticUniversalSampling,
        LinearRankSelection,
        ExponentialRankSelection,
        TruncationSelection
    }

    public enum eCrossoverType
    {
        OnePoint,       //classic
        TwoPoint,
        Flat,
        Uniform,        //fast noised
        ArithmeticUniform,     //slow
        ArithmeticNonUniform,
        Heuristic,       //fast
        Intermediate,
        Line,
        BLX_alpha,
        SBX
    }

    public enum eMutationType
    {
        Gaussian,           //classic real-value approach
        FixedUniform,            //mutate 1 gene
        Uniform,    //mutate N genes
        NonUniform,
        Muhleblein,
        Heuristic,           //fast for real-values
        Polyniminal
    }
}
