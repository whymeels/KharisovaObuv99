namespace KharisovaObuv
{
    public class OrderView
    {
        public int OrderID { get; set; }

        public System.DateTime OrderDate { get; set; }

        public string ClientFullName { get; set; }

        public decimal TotalAmount { get; set; }
    }
}