namespace Samples.ReadModel.Types
{
    using System;

    public class OrderViewModel
    {
        public Guid Id { get; }

        public OrderViewModel(Guid id)
        {
            Id = id;
        }

        public string OrderStatus { get; set; }

        public int LineItemCount { get; set; }
    }
}
