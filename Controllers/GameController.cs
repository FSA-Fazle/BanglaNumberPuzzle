using BanglaNumberPuzzle.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BanglaNumberPuzzle.Controllers
{
    public class GameController : Controller
    {
        private const string SessionKeyPrefix = "NumberBoard_";

        public IActionResult Index(int level = PuzzleGame.DefaultLevel)
        {
            if (!PuzzleGame.Levels.ContainsKey(level)) level = PuzzleGame.DefaultLevel;
            var lvl = PuzzleGame.GetLevel(level);
            var board = LoadOrCreate(level);
            var vm = new GameViewModel
            {
                Level = level,
                Rows = lvl.Rows,
                Cols = lvl.Cols,
                Board = board,
                IsSolved = PuzzleGame.CheckSolved(board, lvl),
                SolvedBoard = PuzzleGame.GetSolvedBoard(lvl),
                Numerals = Enumerable.Range(0, lvl.Max + 1)
                    .Select(n => n == 0 ? "" : PuzzleGame.ToBanglaNumeral(n)).ToList(),
                Words = PuzzleGame.BanglaWords.Take(lvl.Max + 1).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Move([FromBody] MoveRequest req)
        {
            if (!PuzzleGame.Levels.ContainsKey(req.Level)) return BadRequest();
            var lvl = PuzzleGame.GetLevel(req.Level);
            var board = LoadOrCreate(req.Level);
            int tileIdx = req.TileIndex;

            if (tileIdx < 0 || tileIdx >= board.Count || board[tileIdx] == PuzzleGame.Empty)
                return Json(new { board, solved = PuzzleGame.CheckSolved(board, lvl) });

            int tileRow = tileIdx / lvl.Cols;
            int tileCol = tileIdx % lvl.Cols;

            // Use the blank index from the client if provided and valid
            int targetIdx = -1;
            if (req.BlankIndex >= 0 && req.BlankIndex < board.Count
                && board[req.BlankIndex] == PuzzleGame.Empty)
            {
                int blankRow = req.BlankIndex / lvl.Cols;
                int blankCol = req.BlankIndex % lvl.Cols;
                bool adjacent = (tileRow == blankRow && System.Math.Abs(tileCol - blankCol) == 1)
                             || (tileCol == blankCol && System.Math.Abs(tileRow - blankRow) == 1);
                if (adjacent) targetIdx = req.BlankIndex;
            }

            // Fallback: find first adjacent blank
            if (targetIdx == -1)
            {
                int[] dr = { -1, 1, 0, 0 };
                int[] dc = { 0, 0, -1, 1 };
                for (int d = 0; d < 4; d++)
                {
                    int nr = tileRow + dr[d];
                    int nc = tileCol + dc[d];
                    if (nr < 0 || nr >= lvl.Rows || nc < 0 || nc >= lvl.Cols) continue;
                    int ni = nr * lvl.Cols + nc;
                    if (board[ni] == PuzzleGame.Empty) { targetIdx = ni; break; }
                }
            }

            if (targetIdx != -1)
            {
                (board[tileIdx], board[targetIdx]) = (board[targetIdx], board[tileIdx]);
                SaveBoard(req.Level, board);
            }

            bool solved = PuzzleGame.CheckSolved(board, lvl);
            return Json(new { board, solved });
        }

        [HttpPost]
        public IActionResult NewGame(int level = PuzzleGame.DefaultLevel)
        {
            if (!PuzzleGame.Levels.ContainsKey(level)) return BadRequest();
            var board = PuzzleGame.GetShuffledBoard(PuzzleGame.GetLevel(level));
            SaveBoard(level, board);
            return Json(new { board, solved = false });
        }

        private List<int> LoadOrCreate(int level)
        {
            var json = HttpContext.Session.GetString(SessionKeyPrefix + level);
            if (json != null)
                return JsonSerializer.Deserialize<List<int>>(json)!;
            var board = PuzzleGame.GetShuffledBoard(PuzzleGame.GetLevel(level));
            SaveBoard(level, board);
            return board;
        }

        private void SaveBoard(int level, List<int> board)
        {
            HttpContext.Session.SetString(SessionKeyPrefix + level, JsonSerializer.Serialize(board));
        }
    }

    public class GameViewModel
    {
        public int Level { get; set; }
        public int Rows { get; set; }
        public int Cols { get; set; }
        public List<int> Board { get; set; } = new();
        public bool IsSolved { get; set; }
        public List<int> SolvedBoard { get; set; } = new();
        public List<string> Numerals { get; set; } = new();
        public List<string> Words { get; set; } = new();
    }

    public class MoveRequest
    {
        public int Level { get; set; } = PuzzleGame.DefaultLevel;
        public int TileIndex { get; set; }
        public int BlankIndex { get; set; } = -1;
    }
}
