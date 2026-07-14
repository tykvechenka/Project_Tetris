using System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConsoleTetris
{
    public class Tetromino
    {
        public int[,] Shape { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Tetromino(int[,] shape)
        {
            Shape = shape;
        }

        public void Rotate()
        {
            Shape = GetRotatedShape();
        }

        public int[,] GetRotatedShape()
        {
            int rows = Shape.GetLength(0);
            int cols = Shape.GetLength(1);
            int[,] rotated = new int[cols, rows];

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    rotated[x, rows - 1 - y] = Shape[y, x];
                }
            }

            return rotated;
        }
    }

    public class GameField
    {
        public int Width { get; }
        public int Height { get; }

        private readonly int[,] grid;
        private readonly StringBuilder buffer;

        public GameField(int width, int height)
        {
            Width = width;
            Height = height;
            grid = new int[height, width];
            buffer = new StringBuilder();
        }

        public bool IsValidPosition(Tetromino piece, int newX, int newY, int[,] testShape = null)
        {
            int[,] shape = testShape ?? piece.Shape;

            for (int y = 0; y < shape.GetLength(0); y++)
            {
                for (int x = 0; x < shape.GetLength(1); x++)
                {
                    if (shape[y, x] == 0)
                    {
                        continue;
                    }

                    int fieldX = newX + x;
                    int fieldY = newY + y;

                    if (fieldX < 0 || fieldX >= Width || fieldY < 0 || fieldY >= Height)
                    {
                        return false;
                    }

                    if (grid[fieldY, fieldX] == 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void Merge(Tetromino piece)
        {
            for (int y = 0; y < piece.Shape.GetLength(0); y++)
            {
                for (int x = 0; x < piece.Shape.GetLength(1); x++)
                {
                    if (piece.Shape[y, x] == 1)
                    {
                        grid[piece.Y + y, piece.X + x] = 1;
                    }
                }
            }
        }

        public int ClearLines()
        {
            int clearedLines = 0;

            for (int y = Height - 1; y >= 0; y--)
            {
                bool full = true;

                for (int x = 0; x < Width; x++)
                {
                    if (grid[y, x] == 0)
                    {
                        full = false;
                        break;
                    }
                }

                if (!full)
                {
                    continue;
                }

                for (int row = y; row > 0; row--)
                {
                    for (int col = 0; col < Width; col++)
                    {
                        grid[row, col] = grid[row - 1, col];
                    }
                }

                for (int col = 0; col < Width; col++)
                {
                    grid[0, col] = 0;
                }

                clearedLines++;
                y++;
            }

            return clearedLines;
        }

        public bool IsGameOver()
        {
            for (int x = 0; x < Width; x++)
            {
                if (grid[0, x] == 1)
                {
                    return true;
                }
            }

            return false;
        }

        public void Draw(Tetromino currentPiece, int score)
        {
            buffer.Clear();
            buffer.AppendLine("ТЕТРИС");
            buffer.AppendLine("Управление: <- -> вниз - движение, вверх - поворот");
            buffer.AppendLine("Esc - выход");
            buffer.AppendLine("Счёт: " + score);
            buffer.AppendLine();

            for (int y = 0; y < Height; y++)
            {
                buffer.Append("|");

                for (int x = 0; x < Width; x++)
                {
                    bool isCurrentPiece = false;

                    if (currentPiece != null)
                    {
                        for (int py = 0; py < currentPiece.Shape.GetLength(0); py++)
                        {
                            for (int px = 0; px < currentPiece.Shape.GetLength(1); px++)
                            {
                                if (currentPiece.Shape[py, px] == 1 &&
                                    currentPiece.X + px == x &&
                                    currentPiece.Y + py == y)
                                {
                                    isCurrentPiece = true;
                                }
                            }
                        }
                    }

                    if (isCurrentPiece || grid[y, x] == 1)
                    {
                        buffer.Append("[]");
                    }
                    else
                    {
                        buffer.Append("  ");
                    }
                }

                buffer.AppendLine("|");
            }

            buffer.Append("+");
            for (int i = 0; i < Width * 2; i++)
            {
                buffer.Append("-");
            }

            buffer.AppendLine("+");

            Console.SetCursorPosition(0, 0);
            Console.Write(buffer.ToString());
        }
    }

    public class TetrisGame
    {
        private readonly Random random;
        private GameField field;
        private Tetromino currentPiece;
        private bool gameOver;
        private int score;
        private DateTime lastFallTime;

        private readonly List<int[,]> shapes = new List<int[,]>
        {
            new int[,] { { 1, 1, 1, 1 } },
            new int[,] { { 1, 1 }, { 1, 1 } },
            new int[,] { { 0, 1, 0 }, { 1, 1, 1 } },
            new int[,] { { 1, 0, 0 }, { 1, 1, 1 } },
            new int[,] { { 0, 0, 1 }, { 1, 1, 1 } },
            new int[,] { { 0, 1, 1 }, { 1, 1, 0 } },
            new int[,] { { 1, 1, 0 }, { 0, 1, 1 } }
        };

        public TetrisGame()
        {
            random = new Random();
        }

        public void Run()
        {
            Console.CursorVisible = false;

            try
            {
                Console.SetWindowSize(50, 30);
                Console.SetBufferSize(50, 30);
            }
            catch
            {
            }

            ShowStartMenu();

            bool playAgain = true;

            while (playAgain)
            {
                StartNewGame();
                GameLoop();
                playAgain = ShowGameOverMenu();
            }

            Console.Clear();
            Console.SetCursorPosition(0, 0);
        }

        private void ShowStartMenu()
        {
            Console.Clear();
            Console.WriteLine("ТЕТРИС");
            Console.WriteLine();
            Console.WriteLine("Правила:");
            Console.WriteLine("Нужно складывать фигуры так, чтобы заполнять целые линии.");
            Console.WriteLine("Заполненная линия исчезает и приносит очки.");
            Console.WriteLine();
            Console.WriteLine("Управление:");
            Console.WriteLine("Стрелка влево  - движение влево");
            Console.WriteLine("Стрелка вправо - движение вправо");
            Console.WriteLine("Стрелка вниз   - ускорить падение");
            Console.WriteLine("Стрелка вверх  - поворот фигуры");
            Console.WriteLine("Esc            - выход");
            Console.WriteLine();
            Console.WriteLine("Нажмите любую клавишу, чтобы начать...");
            Console.ReadKey(true);
        }

        private void StartNewGame()
        {
            field = new GameField(10, 20);
            currentPiece = null;
            gameOver = false;
            score = 0;
            lastFallTime = DateTime.Now;
            Console.Clear();
            SpawnPiece();
        }

        private void GameLoop()
        {
            const int fallDelayMs = 350;
            const int frameDelayMs = 30;

            while (!gameOver)
            {
                HandleInput();

                if ((DateTime.Now - lastFallTime).TotalMilliseconds >= fallDelayMs)
                {
                    Update();
                    lastFallTime = DateTime.Now;
                }

                field.Draw(currentPiece, score);
                Thread.Sleep(frameDelayMs);
            }
        }

        private void SpawnPiece()
        {
            int[,] shape = (int[,])shapes[random.Next(shapes.Count)].Clone();
            currentPiece = new Tetromino(shape);
            currentPiece.X = field.Width / 2 - currentPiece.Shape.GetLength(1) / 2;
            currentPiece.Y = 0;

            if (!field.IsValidPosition(currentPiece, currentPiece.X, currentPiece.Y))
            {
                gameOver = true;
            }
        }

        private void HandleInput()
        {
            while (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.LeftArrow)
                {
                    if (field.IsValidPosition(currentPiece, currentPiece.X - 1, currentPiece.Y))
                    {
                        currentPiece.X--;
                    }
                }
                else if (key == ConsoleKey.RightArrow)
                {
                    if (field.IsValidPosition(currentPiece, currentPiece.X + 1, currentPiece.Y))
                    {
                        currentPiece.X++;
                    }
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    if (field.IsValidPosition(currentPiece, currentPiece.X, currentPiece.Y + 1))
                    {
                        currentPiece.Y++;
                        lastFallTime = DateTime.Now;
                    }
                }
                else if (key == ConsoleKey.UpArrow)
                {
                    int[,] rotated = currentPiece.GetRotatedShape();

                    if (field.IsValidPosition(currentPiece, currentPiece.X, currentPiece.Y, rotated))
                    {
                        currentPiece.Rotate();
                    }
                }
                else if (key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
            }
        }

        private void Update()
        {
            if (field.IsValidPosition(currentPiece, currentPiece.X, currentPiece.Y + 1))
            {
                currentPiece.Y++;
            }
            else
            {
                field.Merge(currentPiece);

                int clearedLines = field.ClearLines();
                score += clearedLines * 100;

                if (field.IsGameOver())
                {
                    gameOver = true;
                }
                else
                {
                    SpawnPiece();
                }
            }
        }

        private bool ShowGameOverMenu()
        {
            Console.SetCursorPosition(0, field.Height + 7);
            Console.WriteLine("Игра окончена!                 ");
            Console.WriteLine("Ваш счёт: " + score + "                 ");
            Console.WriteLine("Нажмите R, чтобы начать заново.");
            Console.WriteLine("Нажмите Esc, чтобы выйти.      ");

            while (true)
            {
                ConsoleKey key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.R)
                {
                    return true;
                }

                if (key == ConsoleKey.Escape)
                {
                    return false;
                }
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            TetrisGame game = new TetrisGame();
            game.Run();
        }
    }
}