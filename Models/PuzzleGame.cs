using System.Collections.Generic;
using System.Linq;

namespace BanglaNumberPuzzle.Models
{
    public class PuzzleLevel
    {
        public int Max { get; init; }
        public int Rows { get; init; }
        public int Cols { get; init; }
        public int Blanks => Rows * Cols - Max;
    }

    public static class PuzzleGame
    {
        public const int Empty = 0;

        // 1-10: 4×3 = 12 cells, 2 blanks
        // 1-50: 10×6 = 60 cells, 10 blanks (one row per ten, like a number chart)
        // 1-100: 10×11 = 110 cells, 10 blanks
        public static readonly Dictionary<int, PuzzleLevel> Levels = new()
        {
            [10]  = new PuzzleLevel { Max = 10,  Rows = 3,  Cols = 4  },
            [50]  = new PuzzleLevel { Max = 50,  Rows = 6,  Cols = 10 },
            [100] = new PuzzleLevel { Max = 100, Rows = 11, Cols = 10 },
        };

        public const int DefaultLevel = 10;

        private static readonly char[] BanglaDigits = { '০','১','২','৩','৪','৫','৬','৭','৮','৯' };

        // Index = number (index 0 unused)
        public static readonly string[] BanglaWords =
        {
            "",
            "এক","দুই","তিন","চার","পাঁচ","ছয়","সাত","আট","নয়","দশ",
            "এগারো","বারো","তেরো","চৌদ্দ","পনেরো","ষোলো","সতেরো","আঠারো","উনিশ","বিশ",
            "একুশ","বাইশ","তেইশ","চব্বিশ","পঁচিশ","ছাব্বিশ","সাতাশ","আটাশ","ঊনত্রিশ","ত্রিশ",
            "একত্রিশ","বত্রিশ","তেত্রিশ","চৌত্রিশ","পঁয়ত্রিশ","ছত্রিশ","সাঁইত্রিশ","আটত্রিশ","ঊনচল্লিশ","চল্লিশ",
            "একচল্লিশ","বিয়াল্লিশ","তেতাল্লিশ","চুয়াল্লিশ","পঁয়তাল্লিশ","ছেচল্লিশ","সাতচল্লিশ","আটচল্লিশ","ঊনপঞ্চাশ","পঞ্চাশ",
            "একান্ন","বাহান্ন","তিপ্পান্ন","চুয়ান্ন","পঞ্চান্ন","ছাপ্পান্ন","সাতান্ন","আটান্ন","ঊনষাট","ষাট",
            "একষট্টি","বাষট্টি","তেষট্টি","চৌষট্টি","পঁয়ষট্টি","ছেষট্টি","সাতষট্টি","আটষট্টি","ঊনসত্তর","সত্তর",
            "একাত্তর","বাহাত্তর","তিয়াত্তর","চুয়াত্তর","পঁচাত্তর","ছিয়াত্তর","সাতাত্তর","আটাত্তর","ঊনআশি","আশি",
            "একাশি","বিরাশি","তিরাশি","চুরাশি","পঁচাশি","ছিয়াশি","সাতাশি","অষ্টাশি","ঊননব্বই","নব্বই",
            "একানব্বই","বিরানব্বই","তিরানব্বই","চুরানব্বই","পঁচানব্বই","ছিয়ানব্বই","সাতানব্বই","আটানব্বই","নিরানব্বই","একশো"
        };

        public static PuzzleLevel GetLevel(int level) =>
            Levels.TryGetValue(level, out var l) ? l : Levels[DefaultLevel];

        public static string ToBanglaNumeral(int n) =>
            new string(n.ToString().Select(c => BanglaDigits[c - '0']).ToArray());

        public static List<int> GetSolvedBoard(PuzzleLevel level)
        {
            var b = Enumerable.Range(1, level.Max).ToList();
            for (int i = 0; i < level.Blanks; i++) b.Add(Empty);
            return b;
        }

        // With 2+ blanks every arrangement is reachable, so a plain shuffle is always solvable.
        public static List<int> GetShuffledBoard(PuzzleLevel level)
        {
            var b = GetSolvedBoard(level);
            var rng = new System.Random();
            do
            {
                for (int i = b.Count - 1; i > 0; i--)
                {
                    int j = rng.Next(i + 1);
                    (b[i], b[j]) = (b[j], b[i]);
                }
            } while (CheckSolved(b, level));
            return b;
        }

        public static bool CheckSolved(List<int> board, PuzzleLevel level) =>
            board.SequenceEqual(GetSolvedBoard(level));
    }
}
