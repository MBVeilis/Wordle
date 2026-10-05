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
            /*Random rand = new();

            int index = rand.Next(words.Count);

            secretWord = words[index];*/

            NewGame();
        }

        /*public void Start()
        {
            Console.WriteLine("Velkommen til Wordle!");
            Console.WriteLine($"Gæt ordet på 5 bogstaver, du har {maxAttempts} forsøg.");
            Console.WriteLine();

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                Console.WriteLine($"Forsøg {attempt}: ");
                string guess = Console.ReadLine()?.ToLower() ?? "";

                if (guess.Length != WordLength)
                {
                    Console.WriteLine($"Ordet skal være {WordLength} bogstaver langt.");
                    attempt--; // tælles ikke som et forsøg
                    continue;
                }

                if (!words.Contains(guess))
                {
                    Console.WriteLine("Det ord findes ikke i ordlisten");
                    attempt--;
                    continue;
                }

                LetterResult[] results = CheckGuess(guess);

                UpdateExcludedLetters(guess, results);

                string result = "";
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i] == LetterResult.Correct)
                    {
                        result += $"[{guess[i].ToString().ToUpper()}]"; // Korrekt sted
                    }
                    else if (results[i] == LetterResult.WrongPosition)
                    {
                        result += $"({guess[i]})"; // Korrekt bogstav, forkert sted
                    }
                    else
                    {
                        result += guess[i]; // Forkert bogstav
                    }
                }

                Console.WriteLine(result);

                Console.WriteLine();

                Console.WriteLine("Udelukkede bogstaver: ");

                foreach (char letter in excludedLetters)
                {
                    Console.Write($"{char.ToUpper(letter)} ");
                }

                Console.WriteLine("");
                Console.WriteLine("");

                if (guess == secretWord)
                {
                    Console.WriteLine($"Tillykke! Du gættede ordet '{secretWord}' korrekt!");
                    break;
                }

                if (attempt == maxAttempts)
                {
                    Console.WriteLine($"Desværre, du brugte alle forsøg. Ordet var '{secretWord}'.");
                }
            }
        }*/

        private LetterResult[] CheckGuess(string guess)
        {
            //Console.WriteLine($"DEBUG - Secret word: {secretWord}");
            //Console.WriteLine($"DEBUG - Guess: {guess}");

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
