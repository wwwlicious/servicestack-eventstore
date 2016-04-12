namespace Samples.WriteModel
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new TestAppHost("Samples.WriteModel", typeof(TestAppHost).Assembly);
            host.Init();
        }
    }
}
