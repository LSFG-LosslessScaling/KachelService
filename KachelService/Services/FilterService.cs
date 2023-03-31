using KachelService.Model.Customer;
using KachelService.Services.Strategies;
using KachelService.Services.Strategies.File;

namespace KachelService.Services
{
    public class FilterService
    {
        private readonly Dictionary<string, ITileFilter> _filter = new();

        public FilterService()
        {
            _filter.Add("Muenchen",new FileStrategie(Configuration.Muenchen, "Muenchen",2));
            _filter.Add("Leipzig", new FileStrategie(Configuration.Leipzig, "Leipzig", 2));
            _filter.Add("Essen", new FileUserAdressStrategie(Configuration.Essen, "Essen", 1));
        }

        public void AddFilter(string actionName, ITileFilter filter)
        {
            _filter.Add(actionName, filter);
        }

        public bool Show(Customer customer, string action)
        {
            if (!_filter.Any())
                return true;

            if (_filter.TryGetValue(action, out var filter))
            {
                return filter.Show(customer);
            }

            return true;
        }
    }
}
