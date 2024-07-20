using System;
using System.Collections.Generic;

namespace MazeGenerator
{
    public static class RandomExtention
    {
        public static T GetRandomFrom<T>(this Random random, List<T> list)
        {
            var index = random.Next(list.Count);
            return list[index];
        }

    }
}
