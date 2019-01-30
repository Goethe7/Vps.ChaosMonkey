using System;
using System.Collections.Generic;

namespace Vps.Chaos.Helpers
{
    public static class ChaosHelper
    {
        private static readonly Random Rnd = new Random();

        public static T RandomCandidateForChaos<T>(this IList<T> list)
        {
            return list[Rnd.Next(0, list.Count)];
        }
    }
}