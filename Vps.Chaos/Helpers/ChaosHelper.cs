using System;
using System.Collections.Generic;

namespace Vps.Chaos.Helpers
{
    public static class ChaosHelper
    {
        private static readonly Random rnd = new Random();

        public static T RandomCandidateForChaos<T>(this IList<T> list)
        {
            return list[rnd.Next(0, list.Count)];
        }
    }
}