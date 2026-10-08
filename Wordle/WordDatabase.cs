namespace Wordle
{
    public class WordDatabase
    {
        private readonly string filePath = Path.Combine(
            AppContext.BaseDirectory,                       // Points to the program's folder.
            "Data",
            "words.txt"
        );

        public List<string> GetWords()
        {
            if(!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Could not find words.txt",
                    filePath
                );
            }

            return [.. File.ReadAllLines(filePath) // Uses "[.." instead of ".ToList()"
                .Where(word => word.Length == 5)
                .Select(word => word.ToLowerInvariant())];
        }
    }
}
