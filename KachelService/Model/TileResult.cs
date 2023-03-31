namespace KachelService.Model
{
    public class TileResult
    {
        public IList<TileAction> Actions { get; set; }

        public Customer.Customer Customer { get; set; }
    }
}
