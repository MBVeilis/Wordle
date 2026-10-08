namespace Wordle
{
    public class WordleGame
    {
        public int Attempts { get; private set; }

        public bool IsGameOver { get; private set; }

        public bool HasWon { get; private set; }

        public IReadOnlyList<char> ExcludedLetters => excludedLetters;

        static readonly WordDatabase database = new();

        readonly List<string> words = database.GetWords();

        private string secretWord = "";

        private readonly int maxAttempts = 6;

        private readonly List<char> excludedLetters = new();

        private readonly List<GuessResult> guesses = new();

        public IReadOnlyList<GuessResult> Guesses => guesses;

        public int MaxAttempts => maxAttempts;

        public int WordLength => secretWord.Length;

        public WordleGame()
        {
            NewGame();
        }

        private LetterResult[] CheckGuess(string guess)
        {
            LetterResult[] results = [.. Enumerable.Repeat(  // "[.." i stedet for ".ToList()"
                LetterResult.NotInWord,
                WordLength
            )];

            bool[] usedLetters = new bool[WordLength];

            for (int i = 0; i < WordLength; i++)
            {
                if (guess[i] == secretWord[i])
                {
                    results[i] = LetterResult.Correct;
                    usedLetters[i] = true;
                }
            }

            for (int i = 0; i < WordLength; i++)
            {
                if (results[i] == LetterResult.Correct)
                {
                    continue;
                }

                bool found = false;

                for (int j = 0; j < WordLength; j++)
                {
                    if (!usedLetters[j] && guess[i] == secretWord[j])
                    {
                        results[i] = LetterResult.WrongPosition;
                        usedLetters[j] = true;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    results[i] = LetterResult.NotInWord;
                }
            }

            return results;
        }

        private void UpdateExcludedLetters(string guess, LetterResult[] results)
        {
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i] == LetterResult.NotInWord)
                {
                    if (!excludedLetters.Contains(guess[i]))
                    {
                        excludedLetters.Add(guess[i]);
                    }
                }
            }
        }

        public LetterResult[] MakeGuess(string guess)
        {
            if (IsGameOver)
            {
                throw new InvalidOperationException("Spillet er slut.");
            }

            guess = guess.ToLower();

            if (guess.Length != WordLength)
            {
                throw new ArgumentException(
                    $"Ordet skal være {WordLength} bogstaver langt."
                );
            }

            if (!words.Contains(guess))
            {
                throw new ArgumentException(
                    "Det ord findes ikke i ordlisten."
                );
            }

            LetterResult[] results = CheckGuess(guess);

            UpdateExcludedLetters(guess, results);

            guesses.Add(new GuessResult(guess, results));

            Attempts++;

            if (guess == secretWord)
            {
                HasWon = true;
                IsGameOver = true;
            }
            else if (Attempts >= MaxAttempts)
            {
                IsGameOver = true;
            }

            return results;
        }

        public void NewGame()
        {
            Random rand = new();

            int index = rand.Next(words.Count);

            secretWord = words[index];

            Attempts = 0;
            IsGameOver = false;
            HasWon = false;

            excludedLetters.Clear();
            guesses.Clear();
        }
    }
}
