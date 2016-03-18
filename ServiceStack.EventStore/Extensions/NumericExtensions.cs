namespace ServiceStack.EventStore.Extensions
{
    public static class NumericExtensions
    {
        public static int Add(this int firstAddend, int secondAddend)
        {
            return firstAddend + secondAddend;
        }

        public static int Subtract(this int minuend, int subtrahend)
        {
            return minuend - subtrahend;
        }

        public static int MultiplyBy(this int multiplicand, int multiplier)
        {
            return multiplicand * multiplier;
        }

        public static int DivideBy(this int dividend, int divisor)
        {
            return dividend/divisor;
        }
    }
}
