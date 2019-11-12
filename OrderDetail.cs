namespace AkkaExample
{
    public class OrderDetail
    {
        public int Id { get; set; } 
        public int OrderId { get; set; }
        public string Message { get; internal set; }
        public Order Order { get; set; }
    }
}