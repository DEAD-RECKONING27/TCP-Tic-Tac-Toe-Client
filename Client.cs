using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _3_In_A_Row_Client
{
    class Client
    {
        private static readonly string ipAddr = "192.168.1.234";
        private static readonly string path = AppDomain.CurrentDomain.BaseDirectory + "Records.txt";

        private static readonly string SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "win.wav";
        static char[,] grid = new char[3, 3]
            {
                {' ',' ',' ' },
                {' ',' ',' ' },
                {' ',' ',' ' }
            };
        static bool gameOver = false;
        static char turn = 'X';
        static System.Media.SoundPlayer player = new System.Media.SoundPlayer(SoundLocation);

        public static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(ipAddr, 80);
            IPAddress ip = IPAddress.Parse(ipAddr);

            Stream strm = tcpClient.GetStream();

            byte[] b = new byte[1024];

            Console.WriteLine("");
            RecordBoard("New Game on " + DateTime.Now.ToLongDateString());
            PrintBoard();
            while (!gameOver)
            {
                if (turn == 'O')
                {
                    Console.WriteLine("\nIt is {0}'s turn.", (turn == 'O' ? 'O' : 'X'));
                    Console.Write("Choose a row: ");
                    int row = Convert.ToInt32(Console.ReadLine()) - 1;
                    Console.WriteLine("");
                    Console.Write("Choose a column: ");
                    int column = Convert.ToInt32(Console.ReadLine()) - 1;
                    Console.WriteLine("");
                    UpdateIndex(row, column,strm);
                    //byte[] send = EncodeData(GridToString(grid));
                    //strm.Write(send, 0, send.Length);
                }
                else
                {
                    Console.WriteLine("Awaiting Opponent's response...");
                    strm.Read(b, 0, b.Length);
                    grid = StringToGrid(DecodeData(b));
                    turn = (turn == 'X' ? 'O' : 'X');
                }
                Console.Clear();
                Console.WriteLine("");
                RecordBoard();
                PrintBoard();
                CheckForWin();
            }
            RecordBoard("Game over " + (turn == 0 ? 'X' : 'O') + "'s won.");
            RecordBoard("");
            Console.ReadKey();
            tcpClient.Dispose(); ;
        }

        public static string GridToString(char[,] charArray)
        {
            string concat = "";
            for (int i = 0; i < charArray.GetLength(0); i++)
            {
                for (int j = 0; j < charArray.GetLength(1); j++)
                {
                    concat += charArray[i, j];
                }
            }
            return concat;
        }

        public static char[,] StringToGrid(string str)
        {
            char[,] grid = new char[3, 3];
            int row = 0, col = 0;
            foreach (char c in str)
            {
                if (col < 2 || row == 2 & col == 2)
                {
                    grid[row, col] = c;
                    col++;
                }
                else if (row < 2)
                {
                    grid[row, col] = c;
                    row++;
                    col = 0;
                }
            }
            return grid;
        }

        public static byte[] EncodeData(string arrData)
        {
            return Encoding.ASCII.GetBytes(arrData);
        }

        public static string DecodeData(byte[] data)
        {
            return System.Text.Encoding.ASCII.GetString(data);
        }

        public static void PrintBoard()
        {
            Console.WriteLine
                ("     1   2   3        \n" +
                 "   -------------      \n" +
                 " 1 | {0} | {1} | {2} |\n" +
                 "   |-----------|      \n" +
                 " 2 | {3} | {4} | {5} |\n" +
                 "   |-----------|      \n" +
                 " 3 | {6} | {7} | {8} |\n" +
                 "   -------------",
                 grid[0, 0], grid[0, 1], grid[0, 2],
                 grid[1, 0], grid[1, 1], grid[1, 2],
                 grid[2, 0], grid[2, 1], grid[2, 2]);
        }

        public static void RecordBoard()
        {
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("                      \n" +
                             "{0}'s turn at {1} {2}: ", turn == 'X' ? 'X' : 'O', DateTime.Now.ToLongTimeString(), DateTime.Now.ToShortDateString());

                sw.WriteLine("     1   2   3        \n" +
                             "   -------------      \n" +
                             " 1 | {0} | {1} | {2} |\n" +
                             "   |-----------|      \n" +
                             " 2 | {3} | {4} | {5} |\n" +
                             "   |-----------|      \n" +
                             " 3 | {6} | {7} | {8} |\n" +
                             "   -------------      \n" +
                             "                        ",
                             grid[0, 0], grid[0, 1], grid[0, 2],
                             grid[1, 0], grid[1, 1], grid[1, 2],
                             grid[2, 0], grid[2, 1], grid[2, 2]);

                sw.WriteLine("");
            }
        }

        public static void RecordBoard(string str)
        {
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(str);
            }
        }

        public static void UpdateIndex(int row, int column, Stream strm)
        {
            try
            {
                if (grid[row, column].Equals(' '))
                {
                    grid[row, column] = (turn == 'X' ? 'X' : 'O');
                    byte[] send = EncodeData(GridToString(grid));
                    strm.Write(send, 0, send.Length);
                    turn = (turn == 'X' ? 'O' : 'X');
                }
                else if (!grid[row, column].Equals(' '))
                {
                    Console.WriteLine("That square is already taken. Pick again.");
                    Console.ReadKey();
                }
            }
            catch (System.IndexOutOfRangeException)
            {
                Console.WriteLine("That is not a valid choice. Try again you cheeky buggar!");
                Console.ReadKey();
            }
        }

        public static void CheckForWin()
        {
            if ((grid[0, 0].Equals(grid[0, 1]) & grid[0, 1].Equals(grid[0, 2]) && grid[0, 1] != ' ') || (grid[1, 0].Equals(grid[1, 1]) & grid[1, 1].Equals(grid[1, 2]) && grid[1, 1] != ' ') ||
               (grid[2, 0].Equals(grid[2, 1]) & grid[2, 1].Equals(grid[2, 2]) && grid[2, 1] != ' ') || (grid[0, 0].Equals(grid[1, 0]) & grid[1, 0].Equals(grid[2, 0]) && grid[1, 0] != ' ') ||
               (grid[0, 1].Equals(grid[1, 1]) & grid[1, 1].Equals(grid[2, 1]) && grid[1, 1] != ' ') || (grid[0, 2].Equals(grid[1, 2]) & grid[1, 2].Equals(grid[2, 2]) && grid[1, 2] != ' ') ||
               (grid[0, 0].Equals(grid[1, 1]) & grid[1, 1].Equals(grid[2, 2]) && grid[1, 1] != ' ') || (grid[2, 0].Equals(grid[1, 1]) & grid[1, 1].Equals(grid[0, 2]) && grid[1, 1] != ' '))
            {
                gameOver = true;
                Console.WriteLine("");
                Console.WriteLine("   --Game Over--\n" +
                                  "      {0}'s Win!   ", turn);
                if (turn == 'O')
                    player.Play();
            }
            //turn = (turn == 'X' ? 'O' : 'X');
        }
    }
}
