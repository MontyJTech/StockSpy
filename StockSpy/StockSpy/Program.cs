class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Sanity check");

        if(args.Length == 2)
        {
            await StockSpy.FetchStockPrice(args[0], args[1]);
        }
        else
        {
            return;
        }
    }
}