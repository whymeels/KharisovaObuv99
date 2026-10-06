namespace KharisovaObuv
{
    public class OrderItemView
    {
        public int StockItemID { get; set; }

        public string ProductName { get; set; }

        public decimal SizeValue { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal LineTotal
        {
            get
            {
                return Quantity * UnitPrice;
            }
        }
    }
}