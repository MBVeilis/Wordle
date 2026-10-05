namespace Wordle
{
    public class GuessResult // Gemmer historikken over gæt
    {
        public string Guess { get; }
        public IReadOnlyList<LetterResult> Results { get; } // IReadOnlyList<> gør så arrayet ikke kan ændres udefra.
        public GuessResult(string guess, LetterResult[] results)
        {
            Guess = guess;
            Results = results;
        }
    }
}
