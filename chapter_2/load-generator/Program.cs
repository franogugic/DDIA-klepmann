using System.Diagnostics;

var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
var loopNumber = 10000;
var msDelay = 100;
var url = "http://localhost:5048/product/4324";
var tasksOpen = new List<Task<double>>();
var tasksClose = new List<double>();

var failed = 0;

async Task<double> SendOne(HttpClient client, string url)
{
    var timer = Stopwatch.StartNew();
    try
    {
        await client.GetAsync(url);
    }
    catch
    {
        Interlocked.Increment(ref failed);
    }
    timer.Stop();
    return timer.Elapsed.TotalMilliseconds;
}

void Results(List<double> times, string loopType, int heavyReqNumber)
{
    Console.WriteLine($"Results for {loopType}, has {heavyReqNumber} heavy requests");
    var average = times.Average();
    var max = times.Max();
    Console.WriteLine("p999: " + times[(int)Math.Ceiling(loopNumber * 0.999) - 1] + "ms");
    Console.WriteLine("p99: " + times[(int)Math.Ceiling(loopNumber * 0.99) - 1] + "ms");
    Console.WriteLine("p95: " + times[(int)Math.Ceiling(loopNumber * 0.95) - 1] + "ms");
    Console.WriteLine("p50: " + times[(int)Math.Ceiling(loopNumber * 0.5) - 1] + "ms");
    Console.WriteLine("Average: " + average + "ms");
    Console.WriteLine("Highest: " + max + "ms");
}

//ZAGRIJAVANJE
var warmingTimer = Stopwatch.StartNew();
for (int i = 0; i < 40; i++)
{
    await SendOne(client, url);
}
warmingTimer.Stop();
Console.WriteLine("warming: " + warmingTimer.Elapsed.TotalMilliseconds);

// open loop
async Task OpenLoop()
{
    for (var i = 0; i < loopNumber; i++)
    {
        await Task.Delay(msDelay);
        var task = SendOne(client, url);
        tasksOpen.Add(task);
    }

    await Task.WhenAll(tasksOpen);
    var times = tasksOpen.Select(t => t.Result).OrderBy(x => x).ToList();
    var heavyReqNumber = times.Count(t => t > 500);
    Results(times, "open loop", heavyReqNumber);
}
await OpenLoop();
Console.WriteLine("=====================================================");
async Task CloseLoop()
{
    for (var i = 0; i < loopNumber; i++)
    {
        var time = await SendOne(client, url);
        tasksClose.Add(time);
    }
    var times = tasksClose.OrderBy(x => x).ToList();
    var heavyReqNumber = times.Count(t => t > 500);
    Results(times, "close loop", heavyReqNumber);
}

await CloseLoop();