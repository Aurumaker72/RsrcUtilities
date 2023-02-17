﻿namespace RsrcUtilities.Serializers.Extensions;

internal static class ArrayExtensions
{
    public static void ForEach(this Array array, Action<Array, int[]> action)
    {
        if (array.LongLength == 0) return;
        var walker = new ArrayTraverse(array);
        do
        {
            action(array, walker.Position);
        } while (walker.Step());
    }


    internal class ArrayTraverse
    {
        private readonly int[] maxLengths;
        public int[] Position;

        public ArrayTraverse(Array array)
        {
            maxLengths = new int[array.Rank];
            for (var i = 0; i < array.Rank; ++i) maxLengths[i] = array.GetLength(i) - 1;
            Position = new int[array.Rank];
        }

        public bool Step()
        {
            for (var i = 0; i < Position.Length; ++i)
                if (Position[i] < maxLengths[i])
                {
                    Position[i]++;
                    for (var j = 0; j < i; j++) Position[j] = 0;
                    return true;
                }

            return false;
        }
    }
}