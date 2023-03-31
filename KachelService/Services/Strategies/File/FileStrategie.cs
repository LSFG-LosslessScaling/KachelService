using KachelService.Model.Customer;
using System.Text;

namespace KachelService.Services.Strategies.File
{
    public class FileStrategie : ITileFilter
    {

        private readonly ISet<string> _customerIds = new SortedSet<string>();

        public readonly int _mandant;
        public readonly string _mandantName;

        public FileStrategie(string fileName, string actionName, int mandant)
        {
            _mandant = mandant;
            ActionName = actionName;

            switch (mandant)
            {
                case 1:
                    _mandantName = "PV";
                    break;
                case 2:
                    _mandantName = "STG";
                    break;
                case 3:
                    _mandantName = "NK";
                    break;
            }

            var lines = System.IO.File.ReadLines(fileName);
            foreach (var line in lines)
            {
                _customerIds.Add(GetIdent(line));
            }

        }

        public string ActionName { get; set; }
        public bool Show(Customer customer)
        {
            if (customer.Origin != _mandantName)
                return true; // nicht für diesen Mandent...

            return _customerIds.Contains(customer.Ident);
        }

        private string GetIdent(string customerId)
        {
            if (int.TryParse(customerId, out var id))
            {
                return id.ToString();
            }

            return customerId;
        }
    }
}
