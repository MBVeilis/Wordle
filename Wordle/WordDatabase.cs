namespace Wordle
{
    public class WordDatabase
    {
        private readonly string filePath = Path.Combine(
            AppContext.BaseDirectory,                       // Peger på programmets mappe
            "Data",
            "words.txt"
        );

        public List<string> GetWords()
        {
            if(!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Kunne ikke finde words.txt",
                    filePath
                );
            }

            return [.. File.ReadAllLines(filePath) // Bruger "[.." i stedet for ".ToList()"
                .Where(word => word.Length == 5)
                .Select(word => word.ToLowerInvariant())];
        }
    }
}
