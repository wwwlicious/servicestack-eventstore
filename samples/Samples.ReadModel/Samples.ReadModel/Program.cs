namespace Samples.ReadModel
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            new TestAppHost("Samples.ReadModel", typeof (TestAppHost).Assembly)
                .Init();

            Console.ReadKey();
        }
    }
}
