using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SnakeGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer timer;
        int score;
        bool pause;

        const int step = 20;                // size of step = size bit of snake
        int moveDirectionX;                 // -1   0   +1
        int moveDirectionY;                 // -1   0   +1

        Border currentFreeBit;              // current bit for snake food
        Queue<Key> queuKeyboardCommands;    // Up, Down, Left, Right

        List<Border> snake;

        double currentXPositionOfHead;      // x coordinate of LeftTop angle snake head 
        double currentYPositionOfHead;      // y coordinate of LeftTop angle snake head

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (ClashIsOccurred(Canvas.GetLeft(snake[0]), Canvas.GetTop(snake[0]), true))  // if head with tail clash detected
                {
                    timer.Close();
                    txtScore.Text = "GAME OVER! Your score: " + (score).ToString();
                    return;
                }

                if (SnakeIsAteBit())       
                {
                    snake.Insert(0, currentFreeBit);

                    currentXPositionOfHead = Canvas.GetLeft(currentFreeBit);
                    currentYPositionOfHead = Canvas.GetTop(currentFreeBit);

                    currentFreeBit = GenerateFreeBit();

                    if (timer.Interval > 100)
                    {
                        timer.Interval -= timer.Interval * 0.02;        // reduce timer interval on 2 %
                    }
                    txtScore.Text = "Lenght: " + (++score).ToString();
                }

                if (queuKeyboardCommands.Count > 0)                    // if keyboard arrow(s) was pushed, change direction
                {
                    Key keyboardCommand = GetNextSuitableKeyboardCommand();

                    // change direction if get suitable command 
                    if (keyboardCommand == Key.Right && moveDirectionY != 0)
                    {
                        moveDirectionX = +1;
                        moveDirectionY = 0;
                    }
                    else if (keyboardCommand == Key.Left && moveDirectionY != 0)
                    {
                        moveDirectionX = -1;
                        moveDirectionY = 0;
                    }
                    else if (keyboardCommand == Key.Up && moveDirectionX != 0)
                    {
                        moveDirectionY = -1;
                        moveDirectionX = 0;
                    }
                    else if (keyboardCommand == Key.Down && moveDirectionX != 0)
                    {
                        moveDirectionY = +1;
                        moveDirectionX = 0;
                    }
                }

                Border snakeHead = snake[0];

                // сhange the current location of head if snake will move beyond the borders of canvas (mirror effect)
                if (moveDirectionX == -1 && Canvas.GetLeft(snakeHead) < 20) //  if head on left edge of canvas and negative X direction
                {
                    currentXPositionOfHead = 380;
                }
                else if (moveDirectionX == 1 && Canvas.GetLeft(snakeHead) > 360)  // TODO: why > 360 ? instead 380
                {
                    currentXPositionOfHead = 0;
                }
                else if (moveDirectionY == -1 && Canvas.GetTop(snakeHead) < 20)
                {
                    currentYPositionOfHead = 380;
                }
                else if (moveDirectionY == 1 && Canvas.GetTop(snakeHead) > 360)
                {
                    currentYPositionOfHead = 0;
                }
                else   // сhange the current location of the head for further movement(on this location) and change direction if need (by *)
                {
                    currentXPositionOfHead += step * moveDirectionX;
                    currentYPositionOfHead += step * moveDirectionY;
                }
                
                SnakeMoveStep();
            });
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
            {
                queuKeyboardCommands.Enqueue(e.Key);
            }
            else if (e.Key == Key.Pause)
            {
                MenuPause_Click(null, null);
            }
        }

        private Key GetNextSuitableKeyboardCommand()              // example: if snake move to right, remove next Right keyboardCommand(s)
        {
            bool sucess = false;
            Key keyboardCommand;

            do
            {
                if (queuKeyboardCommands.Count < 1) return Key.None;

                keyboardCommand = queuKeyboardCommands.Dequeue();

                if (moveDirectionX == 0 && (keyboardCommand == Key.Left || keyboardCommand == Key.Right))  // if vertical move
                {
                    sucess = true;
                }
                else if (moveDirectionY == 0 && (keyboardCommand == Key.Up || keyboardCommand == Key.Down)) // if horizontal move
                {
                    sucess = true;
                }

            } while (sucess == false);

            return keyboardCommand;
        }

        private void SnakeMoveStep()
        {
            double XPositionForNextSnakeBit = Canvas.GetLeft(snake[0]);
            double YPositionForNextSnakePart = Canvas.GetTop(snake[0]);

            for (int i = 0; i < snake.Count; i++)  // move each part of snake
            {
                if (i == 0)     // move head
                {
                    Canvas.SetLeft(snake[i], currentXPositionOfHead);
                    Canvas.SetTop(snake[i], currentYPositionOfHead);
                    snake[i].Background = Brushes.Red;
                }
                else     // if next part of tailSnake
                {
                    double x = Canvas.GetLeft(snake[i]);
                    double y = Canvas.GetTop(snake[i]);

                    Canvas.SetLeft(snake[i], XPositionForNextSnakeBit);
                    Canvas.SetTop(snake[i], YPositionForNextSnakePart);

                    XPositionForNextSnakeBit = x;
                    YPositionForNextSnakePart = y;
                    snake[i].Background = Brushes.Green;
                }
            }
        }

        private Border GenerateFreeBit()
        {
            int x;
            int y;

            do
            {
                x = new Random().Next(1, 19) * 20;
                y = new Random().Next(1, 19) * 20;

            } while (ClashIsOccurred(x, y));      // the free part should not be generated in the body of the snake

            Border part = new Border();
            part.CornerRadius = new CornerRadius(5);
            part.Background = Brushes.Yellow;
            part.Height = 20;
            part.Width = 20;

            Canvas.SetLeft(part, x);
            Canvas.SetTop(part, y);
            canvas.Children.Add(part);
            return part;
        }

        // check the bit with the coordinates x and y is in the body of the snake
        // if checkTailOnly = true - check the head is hit tail (x and y in this case must be coordinates of head)
        private bool ClashIsOccurred(double x, double y, bool checkTailOnly = false) 
        {
            foreach (var item in snake)
            {
                if (checkTailOnly)
                {
                    if (item.Equals(snake[0])) continue;  // do not chech head
                }

                if (Canvas.GetLeft(item) == x && Canvas.GetTop(item) == y)
                {
                    return true;
                }
            }
            return false;
        }

        private bool SnakeIsAteBit()
        {
            double XPositionOfFreePart = Canvas.GetLeft(currentFreeBit);
            double YPositionOfFreePart = Canvas.GetTop(currentFreeBit);

            double XPositionOfSnakeHead = Canvas.GetLeft(snake[0]);
            double YPositionOfSnakeHead = Canvas.GetTop(snake[0]);

            if (moveDirectionX == -1)
            {
                if (XPositionOfFreePart == XPositionOfSnakeHead - 20 && YPositionOfFreePart == YPositionOfSnakeHead)
                {
                    return true;
                }
                else if (XPositionOfFreePart == XPositionOfSnakeHead && YPositionOfFreePart == YPositionOfSnakeHead + 20 && GetNextSuitableKeyboardCommand() == Key.Down)
                {
                    moveDirectionX = 0;
                    moveDirectionY = 1;
                    return true;
                }
                else if (XPositionOfFreePart == XPositionOfSnakeHead && YPositionOfFreePart == YPositionOfSnakeHead - 20 && GetNextSuitableKeyboardCommand() == Key.Up)
                {
                    moveDirectionX = 0;
                    moveDirectionY = -1;
                    return true;
                }
            }
            else if (moveDirectionX == 1)
            {
                if (XPositionOfFreePart == XPositionOfSnakeHead + 20 && YPositionOfFreePart == YPositionOfSnakeHead)
                {
                    return true;
                }
                else if (XPositionOfFreePart == XPositionOfSnakeHead && YPositionOfFreePart == YPositionOfSnakeHead + 20 && GetNextSuitableKeyboardCommand() == Key.Down)
                {
                    moveDirectionX = 0;
                    moveDirectionY = 1;
                    return true;
                }
                else if (XPositionOfFreePart == XPositionOfSnakeHead && YPositionOfFreePart == YPositionOfSnakeHead - 20 && GetNextSuitableKeyboardCommand() == Key.Up)
                {
                    moveDirectionX = 0;
                    moveDirectionY = -1;
                    return true;
                }
            }
            else if (moveDirectionY == -1)
            {
                if (XPositionOfFreePart == XPositionOfSnakeHead && YPositionOfFreePart == YPositionOfSnakeHead - 20)
                {
                    return true;
                }
                else if (YPositionOfFreePart == YPositionOfSnakeHead && XPositionOfFreePart == XPositionOfSnakeHead + 20 && GetNextSuitableKeyboardCommand() == Key.Right)
                {
                    moveDirectionX = 1;
                    moveDirectionY = 0;
                    return true;
                }
                else if (YPositionOfFreePart == YPositionOfSnakeHead && XPositionOfFreePart == XPositionOfSnakeHead - 20 && GetNextSuitableKeyboardCommand() == Key.Left)
                {
                    moveDirectionX = -1;
                    moveDirectionY = 0;
                    return true;
                }
            }
            else if (moveDirectionY == 1)
            {
                if (XPositionOfFreePart == XPositionOfSnakeHead && YPositionOfFreePart == YPositionOfSnakeHead + 20)
                {
                    return true;
                }
                else if (YPositionOfFreePart == YPositionOfSnakeHead && XPositionOfFreePart == XPositionOfSnakeHead + 20 && GetNextSuitableKeyboardCommand() == Key.Right)
                {
                    moveDirectionX = 1;
                    moveDirectionY = 0;
                    return true;
                }
                else if (YPositionOfFreePart == YPositionOfSnakeHead && XPositionOfFreePart == XPositionOfSnakeHead - 20 && GetNextSuitableKeyboardCommand() == Key.Left)
                {
                    moveDirectionX = -1;
                    moveDirectionY = 0;
                    return true;
                }
            }

            return false;
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Menu_NewGame_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children?.Clear();
            snake?.Clear();
            queuKeyboardCommands?.Clear();
            timer?.Close();
            currentFreeBit = null;

            timer = new Timer(500);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            score = 0;
            queuKeyboardCommands = new Queue<Key>();

            snake = new List<Border>();

            Border headSnake = GenerateFreeBit();
            currentXPositionOfHead = Canvas.GetLeft(headSnake);
            currentYPositionOfHead = Canvas.GetTop(headSnake);
            headSnake.Background = Brushes.Red;
            snake.Add(headSnake);

            currentFreeBit = GenerateFreeBit();

            // random initial direction
            moveDirectionX = 0;         // -1   0   +1
            moveDirectionY = 0;

            Random rnd = new Random();

            if (new Random().Next(2) == 0)
            {
                moveDirectionX = rnd.Next(2) == 0 ? -1 : 1;
            }
            else
            {
                moveDirectionY = rnd.Next(2) == 0 ? -1 : 1;
            }
        }

        private void MenuPause_Click(object sender, RoutedEventArgs e)
        {
            pause = !pause;

            if (pause)
            {
                timer.Stop();
            }
            else
            {
                timer.Start();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}