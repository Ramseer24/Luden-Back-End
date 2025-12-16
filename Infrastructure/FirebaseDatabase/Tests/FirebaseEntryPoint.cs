namespace Infrastructure.FirebaseDatabase.Tests;

public class FirebaseEntryPoint
{
    //тестовая чистая точка входа для теста файрбазе
    public static async Task Main(string[] args)
    {
        var demo = new DemoFirebaseTest();
        await demo.RunDemoAsync();
    }
}