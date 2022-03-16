using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace console_snake
{
   class Program
   {
      // ┃━┏┓┗┛╋┫┣
      // ❶❷❸❹❺❻❼❽❾
      // ♠♣♥♦
      // ╭╮╯╰╷╶╵╴│─● ╱╲┴┼┬┤├

      private const int NOTHING = 0;
      private const int BORDER_TOP_LEFT = 1;
      private const int BORDER_TOP_RIGHT = 2;
      private const int BORDER_BOTTOM_LEFT = 3;
      private const int BORDER_BOTTOM_RIGHT = 4;
      private const int BORDER_HORIZONTAL = 5;
      private const int BORDER_VERTICAL = 6;
      private const int FRUIT = 7;
      private const int BONUS = 16;
      private const int SNAKE = 20;

      private const int DIR_NONE = -1;
      private const int DIR_LEFT = 0;
      private const int DIR_RIGHT = 1;
      private const int DIR_UP = 2;
      private const int DIR_DOWN = 4;

      private const string FG_BLACK = "\u001b[0m\u001b[30m";
      private const string FG_DARKRED = "\u001b[0m\u001b[31m";
      private const string FG_DARKGREEN = "\u001b[0m\u001b[32m";
      private const string FG_DARKYELLOW = "\u001b[0m\u001b[33m";
      private const string FG_DARKBLUE = "\u001b[0m\u001b[34m";
      private const string FG_DARKMAGENTA = "\u001b[0m\u001b[35m";
      private const string FG_DARKCYAN = "\u001b[0m\u001b[36m";
      private const string FG_GRAY = "\u001b[0m\u001b[37m";
      private const string FG_DARKGRAY = "\u001b[0m\u001b[30;1m";
      private const string FG_RED = "\u001b[0m\u001b[31;1m";
      private const string FG_GREEN = "\u001b[0m\u001b[32;1m";
      private const string FG_YELLOW = "\u001b[0m\u001b[33;1m";
      private const string FG_BLUE = "\u001b[0m\u001b[34;1m";
      private const string FG_MAGENTA = "\u001b[0m\u001b[35;1m";
      private const string FG_CYAN = "\u001b[0m\u001b[36;1m";
      private const string FG_WHITE = "\u001b[0m\u001b[37;1m";

      //private const string BG_BLACK = "\u001b[40m";
      //private const string BG_DARKRED = "\u001b[41m";
      //private const string BG_DARKGREEN = "\u001b[42m";
      //private const string BG_DARKYELLOW = "\u001b[43m";
      //private const string BG_DARKBLUE = "\u001b[44m";
      //private const string BG_DARKMAGENTA = "\u001b[45m";
      //private const string BG_DARKCYAN = "\u001b[46m";
      //private const string BG_GRAY = "\u001b[47m";
      //private const string BG_DARKGRAY = "\u001b[40;1m";
      //private const string BG_RED = "\u001b[41;1m";
      //private const string BG_GREEN = "\u001b[42;1m";
      //private const string BG_YELLOW = "\u001b[43;1m";
      //private const string BG_TBLUE = "\u001b[44;1m";
      //private const string BG_MAGENTA = "\u001b[45;1m";
      //private const string BG_CYAN = "\u001b[46;1m";
      //private const string BG_WHITE = "\u001b[47;1m";

      private static readonly char[] bonuses = new char[] { '♠', '♣', '♥', '♦' };
      private static readonly char[] fruits = new char[] { '❶', '❷', '❸', '❹', '❺', '❻', '❼', '❽', '❾' };
      private static readonly char[] snake = new char[] { '╭', '╮', '╯', '╰', '╷', '╶', '╵', '╴', '│', '─', '●' };
      private static readonly char[] borders = new char[] { '┃', '━', '┏', '┓', '┗', '┛' };
      private static readonly Random rnd = new Random();

      private static int[,] playfield = null;
      private static int delayHorizontal = 75;
      private static int delayVertical = 125; // to compensate for characters being taller than wide
      private static Point location = Point.Empty;
      private static int direction = DIR_RIGHT;
      private static int length = 5;
      private static int currentFruit = FRUIT;
      private static int score = 0;
      private static int scoreIncrease = 100;
      private static int scoreIncreaseIncrease = 100;
      private static int bonusIncrease = 500;
      private static int bonusIncreaseIncrease = 200;
      private static bool hasBonus = false;
      private static int currentBonus = BONUS;
      private static int startLength = 5;
      private static int growth = 5;
      private static List<ConsoleKey> keyInputs = new List<ConsoleKey>();

      static void Main(string[] args)
      {
         Setup();
         
         if (DisplayIntro())
         {
            DrawPlayfield();
            MainLoop();
         }

         Console.Clear();
         Console.SetCursorPosition(0, 0);
         Console.CursorVisible = true;
      }

      static void Setup()
      {
         playfield = new int[Console.WindowWidth, Console.WindowHeight - 1];

         ResetSnake();

         playfield[0, 0] = BORDER_TOP_LEFT;
         playfield[playfield.GetUpperBound(0), 0] = BORDER_TOP_RIGHT;
         playfield[0, playfield.GetUpperBound(1)] = BORDER_BOTTOM_LEFT;
         playfield[playfield.GetUpperBound(0), playfield.GetUpperBound(1)] = BORDER_BOTTOM_RIGHT;

         for (int x = 1; x < playfield.GetUpperBound(0); x++)
         {
            playfield[x, 0] = BORDER_HORIZONTAL;
            playfield[x, playfield.GetUpperBound(1)] = BORDER_HORIZONTAL;
         }

         for (int y = 1; y < playfield.GetUpperBound(1); y++)
         {
            playfield[0, y] = BORDER_VERTICAL;
            playfield[playfield.GetUpperBound(0), y] = BORDER_VERTICAL;
         }

         PlaceFruit();

         Console.OutputEncoding = Encoding.UTF8;
         Console.InputEncoding = Encoding.UTF8;
         Console.CursorVisible = false;
      }

      static void MainLoop()
      {
         var dt = DateTime.Now;
         var needsNewFruit = false;

         for (; ; )
         {
            var dt2 = DateTime.Now;
            var ts = dt2.Subtract(dt);

            if ((ts.TotalMilliseconds >= delayHorizontal && (direction == DIR_LEFT || direction == DIR_RIGHT)) || (ts.TotalMilliseconds >= delayVertical && (direction == DIR_UP || direction == DIR_DOWN)))
            {
               dt = dt2;

               if (keyInputs.Count != 0)
               {
                  switch (keyInputs[0])
                  {
                     case ConsoleKey.LeftArrow: if (direction != DIR_RIGHT) direction = DIR_LEFT; break;
                     case ConsoleKey.RightArrow: if (direction != DIR_LEFT) direction = DIR_RIGHT; break;
                     case ConsoleKey.UpArrow: if (direction != DIR_DOWN) direction = DIR_UP; break;
                     case ConsoleKey.DownArrow: if (direction != DIR_UP) direction = DIR_DOWN; break;
                  }

                  keyInputs.RemoveAt(0);
               }

               var item = 0;

               switch (direction)
               {
                  case DIR_LEFT: item = playfield[location.X - 1, location.Y]; break;
                  case DIR_RIGHT: item = playfield[location.X + 1, location.Y]; break;
                  case DIR_UP: item = playfield[location.X, location.Y - 1]; break;
                  case DIR_DOWN: item = playfield[location.X, location.Y + 1]; break;
               }

               if (item >= SNAKE || (item >= BORDER_TOP_LEFT && item <= BORDER_VERTICAL)) // lose life
               {
                  if (DisplayGameOver()) ResetGame();
                  else break;
               }
               else if (item >= BONUS) // eat bonus
               {
                  score += bonusIncrease;
                  bonusIncrease += bonusIncreaseIncrease;
                  hasBonus = false;
               }
               else if (item >= FRUIT) // eat fruit
               {
                  needsNewFruit = true;
                  score += scoreIncrease;
                  scoreIncrease += scoreIncreaseIncrease;
                  length += growth;
               }

               // move snake
               for (int x = 0; x < playfield.GetLength(0); x++)
               {
                  for (int y = 0; y < playfield.GetLength(1); y++)
                  {
                     if (playfield[x, y] == SNAKE + length - 1) playfield[x, y] = NOTHING;
                     else if (playfield[x, y] >= SNAKE) playfield[x, y]++;
                  }
               }

               switch (direction)
               {
                  case DIR_LEFT:
                     playfield[location.X - 1, location.Y] = SNAKE;
                     location = new Point(location.X - 1, location.Y);
                     break;
                  case DIR_RIGHT:
                     playfield[location.X + 1, location.Y] = SNAKE;
                     location = new Point(location.X + 1, location.Y);
                     break;
                  case DIR_UP:
                     playfield[location.X, location.Y - 1] = SNAKE;
                     location = new Point(location.X, location.Y - 1);
                     break;
                  case DIR_DOWN:
                     playfield[location.X, location.Y + 1] = SNAKE;
                     location = new Point(location.X, location.Y + 1);
                     break;
               }

               if (needsNewFruit)
               {
                  needsNewFruit = false;

                  if (!PlaceFruit())
                  {
                     ResetSnake(); // make snake short again.
                     PlaceFruit();
                  }
               }

               DrawPlayfield();
            }

            if (Console.KeyAvailable)
            {
               var key = Console.ReadKey(true);

               switch (key.Key)
               {
                  case ConsoleKey.LeftArrow:
                  case ConsoleKey.RightArrow:
                  case ConsoleKey.UpArrow:
                  case ConsoleKey.DownArrow: 
                     if (keyInputs.Count == 0 || keyInputs[keyInputs.Count  -1] != key.Key) 
                        keyInputs.Add(key.Key); 

                     break;
                  case ConsoleKey.Escape: if (AskQuitGame()) return; break;
               }
            }
         }
      }

      static bool DisplayGameOver()
      {
         //┏━━━━━━━━━━━━━━━━━━━┓
         //┃     GAME OVER     ┃
         //┣━━━━━━━━━━━━━━━━━━━┫
         //┃     New Game?     ┃
         //┃        Y/N        ┃
         //┗━━━━━━━━━━━━━━━━━━━┛

         var x = playfield.GetLength(0) / 2 - 10;
         var y = playfield.GetLength(1) / 2 - 3;

         Console.SetCursorPosition(x, y);
         Console.Write(FG_WHITE);
         Console.Write("┏━━━━━━━━━━━━━━━━━━━┓");
         Console.SetCursorPosition(x, y + 1);
         Console.Write("┃     " + FG_DARKYELLOW + "GAME OVER" + FG_WHITE + "     ┃");
         Console.SetCursorPosition(x, y + 2);
         Console.Write("┣━━━━━━━━━━━━━━━━━━━┫");
         Console.SetCursorPosition(x, y + 3);
         Console.Write("┃     New Game?     ┃");
         Console.SetCursorPosition(x, y + 4);
         Console.Write("┃        Y/N        ┃");
         Console.SetCursorPosition(x, y + 5);
         Console.Write("┗━━━━━━━━━━━━━━━━━━━┛");

         var key = Console.ReadKey(true);

         while (key.Key != ConsoleKey.Y && key.Key != ConsoleKey.N)
            key = Console.ReadKey(true);

         return key.Key == ConsoleKey.Y;
      }

      static bool AskQuitGame()
      {
         //┏━━━━━━━━━━━━━━━━━━━┓
         //┃     QUIT GAME     ┃
         //┣━━━━━━━━━━━━━━━━━━━┫
         //┃        Y/N        ┃
         //┗━━━━━━━━━━━━━━━━━━━┛

         var x = playfield.GetLength(0) / 2 - 10;
         var y = playfield.GetLength(1) / 2 - 3;

         Console.SetCursorPosition(x, y);
         Console.Write(FG_WHITE);
         Console.Write("┏━━━━━━━━━━━━━━━━━━━┓");
         Console.SetCursorPosition(x, y + 1);
         Console.Write("┃     " + FG_DARKYELLOW + "QUIT GAME" + FG_WHITE + "     ┃");
         Console.SetCursorPosition(x, y + 2);
         Console.Write("┣━━━━━━━━━━━━━━━━━━━┫");
         Console.SetCursorPosition(x, y + 3);
         Console.Write("┃        Y/N        ┃");
         Console.SetCursorPosition(x, y + 4);
         Console.Write("┗━━━━━━━━━━━━━━━━━━━┛");

         var key = Console.ReadKey(true);

         while (key.Key != ConsoleKey.Y && key.Key != ConsoleKey.N && key.Key != ConsoleKey.Escape)
            key = Console.ReadKey(true);

         return key.Key == ConsoleKey.Y;
      }

      static void ResetGame()
      {
         score = 0;
         hasBonus = false;

         for (int x = 1; x < playfield.GetUpperBound(0); x++)
            for (int y = 1; y < playfield.GetUpperBound(1); y++)
               if (playfield[x, y] != NOTHING) playfield[x, y] = NOTHING;

         Console.Clear();
         ResetSnake();
         PlaceFruit();
      }

      static void ResetSnake()
      {
         direction = DIR_RIGHT;
         length = startLength;
         scoreIncrease = 100;
         bonusIncrease = 500;
         var startX = length + 2;

         for (int x = 1; x < playfield.GetUpperBound(0); x++)
            for (int y = 1; y < playfield.GetUpperBound(1); y++)
               if (playfield[x, y] >= SNAKE) playfield[x, y] = NOTHING;

         for (int x = startX; x > startX - length; x--)
         {
            if (playfield[x, playfield.GetLength(1) / 2] >= FRUIT) playfield[x, playfield.GetLength(1) / 2 - 1] = playfield[x, playfield.GetLength(1) / 2];
            playfield[x, playfield.GetLength(1) / 2] = SNAKE + (startX - x);
         }
         
         location = new Point(startX, playfield.GetLength(1) / 2);
      }

      static bool PlaceFruit()
      {
         var possibleValues = new List<Point>();
         for (int x = 2; x < playfield.GetUpperBound(0) - 1; x++)
            for (int y = 2; y < playfield.GetUpperBound(1) - 2; y++)
               if (playfield[x, y] == NOTHING)
                  possibleValues.Add(new Point(x, y));

         if (possibleValues.Count != 0)
         {
            var i = rnd.Next(0, possibleValues.Count);
            playfield[possibleValues[i].X, possibleValues[i].Y] = currentFruit;
            currentFruit++;
            if (currentFruit >= FRUIT + fruits.Length) currentFruit = FRUIT;

            if (!hasBonus && rnd.Next(0, 4) == 0) // place new bonus
            {
               hasBonus = true;
               possibleValues.RemoveAt(i);
               i = rnd.Next(0, possibleValues.Count);

               playfield[possibleValues[i].X, possibleValues[i].Y] = currentBonus;
               currentBonus++;
               if (currentBonus >= BONUS + bonuses.Length) currentBonus = BONUS;
            }
         }

         return possibleValues.Count != 0;
      }

      static void DrawPlayfield()
      {
         var lastColor = FG_GRAY;

         var sb = new StringBuilder(playfield.Length);
         sb.Append(lastColor);

         for (int y = 0; y < playfield.GetLength(1); y++) 
         {
            for (int x = 0; x < playfield.GetLength(0); x++)
            {
               string newColor;

               if (playfield[x, y] >= SNAKE) newColor = FG_GREEN;
               else if (playfield[x, y] >= BONUS) newColor = FG_DARKYELLOW;
               else if (playfield[x, y] >= FRUIT) newColor = FG_RED;
               else if (playfield[x, y] >= BORDER_TOP_LEFT) newColor = FG_GRAY;
               else newColor = lastColor;

               if (newColor != lastColor)
               {
                  sb.Append(newColor);
                  lastColor = newColor;
               }

               if (playfield[x, y] == NOTHING) sb.Append(' ');
               else if (playfield[x, y] == BORDER_TOP_LEFT) sb.Append(borders[2]);
               else if (playfield[x, y] == BORDER_TOP_RIGHT) sb.Append(borders[3]);
               else if (playfield[x, y] == BORDER_BOTTOM_LEFT) sb.Append(borders[4]);
               else if (playfield[x, y] == BORDER_BOTTOM_RIGHT) sb.Append(borders[5]);
               else if (playfield[x, y] == BORDER_HORIZONTAL) sb.Append(borders[1]);
               else if (playfield[x, y] == BORDER_VERTICAL) sb.Append(borders[0]);
               else if (playfield[x, y] >= SNAKE)
               {
                  var prev = DIR_NONE;
                  if (playfield[x - 1, y] == playfield[x, y] - 1) prev = DIR_LEFT;
                  else if (playfield[x + 1, y] == playfield[x, y] - 1) prev = DIR_RIGHT;
                  else if (playfield[x, y - 1] == playfield[x, y] - 1) prev = DIR_UP;
                  else if (playfield[x, y + 1] == playfield[x, y] - 1) prev = DIR_DOWN;

                  var next = DIR_NONE;
                  if (playfield[x - 1, y] == playfield[x, y] + 1) next = DIR_LEFT;
                  else if (playfield[x + 1, y] == playfield[x, y] + 1) next = DIR_RIGHT;
                  else if (playfield[x, y - 1] == playfield[x, y] + 1) next = DIR_UP;
                  else if (playfield[x, y + 1] == playfield[x, y] + 1) next = DIR_DOWN;

                  if (prev == DIR_NONE) sb.Append(snake[10]);
                  else if (prev == DIR_LEFT)
                  {
                     switch (next)
                     {
                        case DIR_RIGHT: sb.Append(snake[9]); break;
                        case DIR_UP: sb.Append(snake[2]); break;
                        case DIR_DOWN: sb.Append(snake[1]); break;
                        case DIR_NONE: sb.Append(snake[7]); break;
                     }
                  }
                  else if (prev == DIR_RIGHT)
                  {
                     switch (next)
                     {
                        case DIR_LEFT: sb.Append(snake[9]); break;
                        case DIR_UP: sb.Append(snake[3]); break;
                        case DIR_DOWN: sb.Append(snake[0]); break;
                        case DIR_NONE: sb.Append(snake[5]); break;
                     }
                  }
                  else if (prev == DIR_UP)
                  {
                     switch (next)
                     {
                        case DIR_LEFT: sb.Append(snake[2]); break;
                        case DIR_RIGHT: sb.Append(snake[3]); break;
                        case DIR_DOWN: sb.Append(snake[8]); break;
                        case DIR_NONE: sb.Append(snake[6]); break;
                     }
                  }
                  else if (prev == DIR_DOWN)
                  {
                     switch (next)
                     {
                        case DIR_LEFT: sb.Append(snake[1]); break;
                        case DIR_RIGHT: sb.Append(snake[0]); break;
                        case DIR_UP: sb.Append(snake[8]); break;
                        case DIR_NONE: sb.Append(snake[4]); break;
                     }
                  }
               }
               else if (playfield[x, y] >= BONUS) sb.Append(bonuses[playfield[x, y] - BONUS]);
               else if (playfield[x, y] >= FRUIT) sb.Append(fruits[playfield[x, y] - FRUIT]);
            }
         }

         sb.Append("Length: ");
         sb.Append(length);
         sb.Append(" Score: ");
         sb.Append(score);

         Console.SetCursorPosition(0, 0);
         Console.Write(sb.ToString());
      }

      static bool DisplayIntro()
      {
         //╭──╮         ╷
         //│            │ ╭
         //╰───╮╭──╮╭──╮├─┤ ╭──╮
         //    ││  ││  ││ ╰╮├──╯
         //╰───╯╵  ╵╰──┴╵  ╵╰──╯
         // 
         //    [P] Play Game
         //    [Q] Quit

         var x = playfield.GetLength(0) / 2 - 10;
         var y = playfield.GetLength(1) / 2 - 4;

         Console.Clear();
         Console.SetCursorPosition(x, y);
         Console.Write(FG_DARKYELLOW);
         Console.Write("╭──╮         ╷");
         Console.SetCursorPosition(x, y + 1);
         Console.Write("│            │ ╭");
         Console.SetCursorPosition(x, y + 2);
         Console.Write("╰───╮╭──╮╭──╮├─┤ ╭──╮");
         Console.SetCursorPosition(x, y + 3);
         Console.Write("    ││  ││  ││ ╰╮├──╯");
         Console.SetCursorPosition(x, y + 4);
         Console.Write("╰───╯╵  ╵╰──┴╵  ╵╰──╯");
         Console.SetCursorPosition(x, y + 6);
         Console.Write(FG_GRAY);
         Console.Write("    [P] Play Game");
         Console.SetCursorPosition(x, y + 7);
         Console.Write("    [Q] Quit");

         var key = Console.ReadKey(true);

         while (key.Key != ConsoleKey.P && key.Key != ConsoleKey.Q)
            key = Console.ReadKey(true);

         return key.Key == ConsoleKey.P;
      }
   }
}

