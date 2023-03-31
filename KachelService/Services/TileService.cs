using KachelService.Model;
using KachelService.Services.Strategies;

namespace KachelService.Services
{
    public class TileService
    {

        private readonly FilterService _filterService;
        private readonly CustomerService _customerService;
        private readonly LinkService _linkService;

        public TileService(
            FilterService filterService,
            LinkService linkService,
            CustomerService customerService)
        {
            _linkService = linkService;
            _filterService = filterService;
            _customerService = customerService;
        }

        public async Task<TileResult> GetAction(string token, List<string> actions)
        {
            var customer = await _customerService.GetCustomer(token);

            var result = new TileResult { Actions = new List<TileAction>() };

            if (customer != null)
            {
                result.Customer = customer;

                foreach (var action in actions.Where(action => _filterService.Show(customer, action)))
                {

                    int version = 1;
                    if (action == "Muenchen")
                    {
                        version = 1;
                    }
                    if (action == "Leipzig")
                    {
                        version = 2;
                    }

                    if (action == "Essen")
                    {
                        version = 1;
                    }


                    if (action == "Essen")
                    {
                        var link = await _linkService.GetUriEssen(customer, version);
                        result.Actions.Add(new TileAction()
                        {
                            Show = true,
                            ActionName = action,
                            Link = link
                        });
                    }
                    else
                    {
                        var link = await _linkService.GetUri(customer, version);
                        result.Actions.Add(new TileAction()
                        {
                            Show = true,
                            ActionName = action,
                            Link = link
                        });
                    }

                }
            }

            return result;
        }

    }
}
