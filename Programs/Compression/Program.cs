using Simulation;

class Program
{
    static bool[] GenerateRandomBoolString(int length)
    {
        Random rng = new();
        bool[] ret = new bool[length];
        for(int i = 0; i < length; i++) ret[i] = rng.Next() % 2 == 0;
        return ret;
    }

    static void PrintBoolString(bool[] input)
    {
        Console.WriteLine(
            String.Join("", input.Select(b => b ? 1 : 0))
        );
    }

    static void Main(string[] args)
    {
        int length = 90;
        bool[] original = GenerateRandomBoolString(length);

        Console.WriteLine("original");
        PrintBoolString(original);

        var compressed = new CompressedBuffer(original);

        Console.WriteLine("inflated");
        PrintBoolString(compressed.Decompress());
    }
}