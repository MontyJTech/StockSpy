using System;
using System.ComponentModel;
using System.Text.Json;

class StockSpy
{
    private const string apiConfigFilename = "key.txt";
    private static readonly HttpClient httpClient = new HttpClient();
    private static string alphaKey = string.Empty;

    public static async Task FetchStockPrice(string stockSymbol, string source)
    {
        try
        {
            decimal price;
            switch (source.ToLower())
            {
                case "yahoo":
                    price = await HandleYahooSymbolRequest(stockSymbol);
                    break;
                case "alpha":
                    price = await HandleAlphaSymbolRequest(stockSymbol);
                    break;
                default:
                    throw new Exception("Invalid Source. Use 'yahoo' or 'alpha'.");
            }

            Console.WriteLine($"Stock: {stockSymbol} | Source: {source} | Price: {price}");
        }
        catch(Exception ex) 
        {
            Console.WriteLine($"Error: {ex.Message}");        
        }
    }

    private async static Task<decimal> HandleYahooSymbolRequest(string symbol)
    {
        string url = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={symbol}";
        string json = await GetJsonResponseFromURL(url);
        return ParseYahooResponse(json);
    }

    private async static Task<decimal> HandleAlphaSymbolRequest(string symbol)
    {
        if (string.IsNullOrEmpty(alphaKey) && !InitializeAlphaAPI())
        {
            throw new Exception("Please ensure API key is initialized correctly. (See README.md)");
        }

        string url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={alphaKey}";
        string json = await GetJsonResponseFromURL(url);
        decimal test = ParseAlphaResponse(json);
        return test;
    }

    private async static Task<string> GetJsonResponseFromURL(string url)
    {
        HttpResponseMessage response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private static decimal ParseYahooResponse(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        return root.GetProperty("quoteResponse").GetProperty("Result")[0].GetProperty("regularMarketPrice").GetDecimal();
    }

    private static decimal ParseAlphaResponse(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        JsonElement property = root.GetProperty("Global Quote").GetProperty("05. price");

        decimal.TryParse(property.GetString(), out decimal temp);
        return temp;
    }

    public static bool InitializeAlphaAPI()
    {
        try
        {
            alphaKey = File.ReadAllText(apiConfigFilename);
            //Check that the apikey is valid.

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to read Alpha API key: {ex.Message}");
            return false;
        }
    }
}