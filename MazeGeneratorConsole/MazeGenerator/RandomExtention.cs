using MazeGenerator.Models.GenerationModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MazeGenerator
{
    public static class RandomExtention
    {
        public static T GetRandomFrom<T>(this Random random, List<T> list)
        {
            var index = random.Next(list.Count);
            return list[index];
        }

        public static T GetRandomFromByWeight<T>(this Random random, List<OptionWithWeight<T>> list)
        {
            var fullWeight = list.Sum(x => x.Weight);
            var point = random.NextDouble() * fullWeight;

            var lengthOfPath = 0d;
            foreach (var option in list)
            {
                lengthOfPath += option.Weight;
                if (point <= lengthOfPath)
                {
                    return option.Option;
                }
            }

            throw new Exception("Random if broken. We can't get option");
        }
    }
}
