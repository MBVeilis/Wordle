using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Wordle;

namespace Wordle.WPF
{
    public partial class MainWindow : Window
    {
        private readonly WordleGame game = new();
        private int currentColumn = 0;
        private int currentRow = 0;
        private bool isAnimating = false;
        public MainWindow()
        {
            InitializeComponent();

            CreateBoard();

            PreviewKeyDown += MainWindow_PreviewKeyDown;

            Focusable = true;
            Focus();
        }

        private void CreateBoard()
        {
            GameBoard.Children.Clear();

            for (int i = 0; i < 30; i++)
            {
                Border tile = new()
                {
                    Style = (Style)FindResource("TileStyle"),

                    RenderTransformOrigin = new Point(0.5, 0.5),

                    RenderTransform = new ScaleTransform(1, 1)
                };

                TextBlock letter = new()
                {
                    Style = (Style)FindResource("TileTextStyle")
                };

                tile.Child = letter;

                GameBoard.Children.Add(tile);
            }
        }

        private async void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key >= System.Windows.Input.Key.A &&
                e.Key <= System.Windows.Input.Key.Z)
            {
                e.Handled = true;
                
                string letter = e.Key.ToString().ToLower();

                AddLetter(letter);

                return;
            }

            if (e.Key == System.Windows.Input.Key.Back)
            {
                e.Handled = true;
                
                RemoveLetter();

                return;
            }

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                
                await SubmitGuess();

                return;
            }
        }

        private async void KeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender; 
            
            string key = button.Tag?.ToString() ?? ""; 
            
            if (key == "enter") 
            { 
                await SubmitGuess(); 
            }
            else if (key == "backspace") 
            { 
                RemoveLetter();
            }
            else
            {
                AddLetter(key);
            }

            Focus();
        }

        private void AddLetter(string letter)
        {
            if (isAnimating)
            {
                return;
            }

            if (game.HasWon || game.IsGameOver)
            {
                return;
            }

            if (currentColumn >= 5)
            {
                return;
            }

            //int row = game.Attempts;

            Border tile = (Border)GameBoard.Children[
                currentRow * 5 + currentColumn
            ];
            
            TextBlock text = (TextBlock)tile.Child;

            text.Text = letter.ToUpper();

            currentColumn++;
        }

        private void RemoveLetter()
        {
            if (isAnimating)
            {
                return;
            }

            if (game.HasWon || game.IsGameOver)
            {
                return;
            }

            if (currentColumn <= 0)
            {
                return;
            }

            currentColumn--;

            //int row = game.Attempts;

            Border tile = (Border)GameBoard.Children[
                currentRow * 5 + currentColumn
            ];

            TextBlock text = (TextBlock)tile.Child;

            text.Text = "";
        }

        private string GetCurrentGuess()
        {
            //int row = game.Attempts;

            string guess = "";

            for (int column = 0; column < 5; column++)
            {
                Border tile = (Border)GameBoard.Children[
                    currentRow * 5 + column
                ];

                TextBlock text = (TextBlock)tile.Child;

                guess += text.Text.ToLower();
            }

            return guess;
        }

        private async Task SubmitGuess()
        {
            if (isAnimating)
            {
                return;
            }

            if (game.HasWon || game.IsGameOver)
            {
                return;
            }

            if (currentColumn < 5)
            {
                MessageText.Text = "Ordet skal være 5 bogstaver.";

                Focus();
                
                return;
            }

            isAnimating = true;

            string guess = GetCurrentGuess();

            try
            {
                LetterResult[] results = game.MakeGuess(guess); 
                
                await DisplayGuess(currentRow, guess, results); 
                
                UpdateKeyboard(guess, results); 
                
                currentColumn = 0;
                currentRow++;

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
            finally
            {
                isAnimating = false; // isAnimating bliver nulstillet her, også hvis backend kaster en exception.
            }

            Focus();
        }

        private async Task DisplayGuess(int row, string guess, LetterResult[] results)
        {
            for (int column = 0; column < guess.Length; column++)
            {
                Border tile = (Border)GameBoard.Children[
                    row * 5 + column
                ];

                TextBlock letter = (TextBlock)tile.Child;

                letter.Text = guess[column].ToString().ToUpper();

                /*switch (results[column])
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
                }*/

                await AnimateTileFlip(tile, results[column]);

                await Task.Delay(80);
            }
        }

        private void UpdateKeyboard(string guess, LetterResult[] results)
        {
            for (int i = 0; i < guess.Length; i++)
            {
                char letter = guess[i];

                Button? button = FindKeyboardButton(letter);

                if (button == null)
                {
                    continue;
                }

                switch (results[i])
                {
                    case LetterResult.Correct:
                        
                        button.Background = new SolidColorBrush(
                            Color.FromRgb(83, 141, 78)
                        );

                        break;

                    case LetterResult.WrongPosition:
                        
                        if (button.Background is SolidColorBrush brush && brush.Color == Color.FromRgb(83, 141, 78))
                        {
                            continue;
                        }
                        
                        button.Background = new SolidColorBrush(
                            Color.FromRgb(181, 159, 59)
                        );
                        
                        break;

                    case LetterResult.NotInWord:

                        if (button.Background is SolidColorBrush existingBrush &&
                            (existingBrush.Color == Color.FromRgb(83, 141, 78) ||
                            existingBrush.Color == Color.FromRgb(181, 159, 59)))
                        {
                            continue;
                        }

                        button.Background = new SolidColorBrush(
                            Color.FromRgb(58, 58, 60)
                        );

                        break;
                }
            }
        }

        private Button? FindKeyboardButton(char letter)
        {
            foreach (object child in GetVisualChildren(Keyboard))
            {
                if (child is Button button && button.Tag?.ToString() == letter.ToString())
                {
                    return button;
                }
            }
            
            return null;
        }

        private static IEnumerable<DependencyObject> GetVisualChildren(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                yield return child;

                foreach (DependencyObject descendant in GetVisualChildren(child))
                {
                    yield return descendant;
                }
            }
        }

        private void ResetKeyboard()
        {
            foreach (object child in GetVisualChildren(Keyboard))
            {
                if (child is Button button &&
                    button.Tag?.ToString()?.Length == 1)
                {
                    button.Background = new SolidColorBrush(
                        Color.FromRgb(129, 131, 132)
                    );
                }
            }   
        }

        private static async Task AnimateTileFlip(Border tile, LetterResult result)
        {
            ScaleTransform transform = (ScaleTransform)tile.RenderTransform;

            DoubleAnimation shrinkAnimation = new() // <- DoubleAnimation bliver brugt fordi ScaleY er en double-property, og fordi vi går fra 1 -> 0 -> 1.
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseIn
                }
            };

            TaskCompletionSource<bool> firstHalf = new();

            shrinkAnimation.Completed += (_, _) =>
            {
                firstHalf.SetResult(true);
            };

            transform.BeginAnimation(ScaleTransform.ScaleYProperty, shrinkAnimation); // <- ScaleY bruges her.

            await firstHalf.Task;

            // Når tilen er flad, ændres farven
            switch(result)
            {
                case LetterResult.Correct:
                    
                    tile.Background = new SolidColorBrush(Color.FromRgb(83, 141, 78));

                    break;
                
                case LetterResult.WrongPosition:
                
                    tile.Background = new SolidColorBrush(Color.FromRgb(181, 159, 59));
                    
                    break;
                
                case LetterResult.NotInWord:
                    
                    tile.Background = new SolidColorBrush(Color.FromRgb(58, 58, 60));
                    
                    break;
            }

            DoubleAnimation expandAnimation = new()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            TaskCompletionSource<bool> secondHalf = new();

            expandAnimation.Completed += (_, _) =>
            {
                secondHalf.SetResult(true);
            };

            transform.BeginAnimation(ScaleTransform.ScaleYProperty, expandAnimation);

            await secondHalf.Task;
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            game.NewGame();

            CreateBoard();

            ResetKeyboard();

            currentRow = 0;
            currentColumn = 0;

            MessageText.Text = "";

            Focus();
        }
    }
}