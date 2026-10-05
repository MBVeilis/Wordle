using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wordle;

namespace Wordle.WPF
{
    public partial class MainWindow : Window
    {
        private readonly WordleGame game = new();
        public MainWindow()
        {
            InitializeComponent();

            CreateBoard();
        }

        private void CreateBoard()
        {
            GameBoard.Children.Clear();

            for (int i = 0; i < 30; i++)
            {
                Border tile = new()
                {
                    Style = (Style)FindResource("TileStyle")
                };

                TextBlock letter = new()
                {
                    Style = (Style)FindResource("TileTextStyle")
                };

                tile.Child = letter;

                GameBoard.Children.Add(tile);
            }
        }
        
        private void GuessButton_Click(object sender, RoutedEventArgs e)
        {
            string guess = GuessInput.Text.Trim();

            try
            {
                LetterResult[] results = game.MakeGuess(guess);

                DisplayGuess(game.Attempts - 1, guess, results);

                GuessInput.Clear();
                GuessInput.Focus();

                if (game.HasWon)
                {
                    MessageText.Text = "Tillykke! Du gættede ordet!";
                }
                else if (game.IsGameOver)
                {
                    MessageText.Text = "Spillet er slut!";
                }
            }
            catch (ArgumentException ex)
            {
                MessageText.Text = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                MessageText.Text = ex.Message;
            }
        }

        private void DisplayGuess(int row, string guess, LetterResult[] results)
        {
            for (int column = 0; column < guess.Length; column++)
            {
                Border tile = (Border)GameBoard.Children[
                    row * 5 + column
                ];

                TextBlock letter = (TextBlock)tile.Child;

                letter.Text = guess[column].ToString().ToUpper();

                switch (results[column])
                {
                    case LetterResult.Correct:
                        tile.Background = new SolidColorBrush(
                            Color.FromRgb(83, 141, 78)
                        );
                        break;

                    case LetterResult.WrongPosition:
                        tile.Background = new SolidColorBrush(
                            Color.FromRgb(181, 159, 59)
                        );
                        break;

                    case LetterResult.NotInWord:
                        tile.Background = new SolidColorBrush(
                            Color.FromRgb(58, 58, 60)
                        );
                        break;
                }
            }
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            game.NewGame();

            CreateBoard();

            GuessInput.Clear();
            GuessInput.Focus();

            MessageText.Text = "";
        }
    }
}