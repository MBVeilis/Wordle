namespace Wordle
{
    public class GuessResult // Stores the history of guesses
    {
        public string Guess { get; }
        public IReadOnlyList<LetterResult> Results { get; } // IReadOnlyList<> makes it so the array cannot be changed from outside.
        public GuessResult(string guess, LetterResult[] results)
        {
            Guess = guess;
            Results = results;
        }
    }
}
