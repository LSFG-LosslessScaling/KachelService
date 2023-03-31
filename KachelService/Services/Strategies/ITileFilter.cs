using KachelService.Model.Customer;

namespace KachelService.Services.Strategies
{
    public interface ITileFilter
    {
        public string ActionName { get; set; }

        public bool Show(Customer customer);
    }
}
