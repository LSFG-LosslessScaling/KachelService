using KachelService.Model.Customer;

namespace KachelService.Services.Strategies.File
{
    public class FileUserAdressStrategie : ITileFilter
    {
        private readonly ISet<string> _customerIds = new SortedSet<string>();

        public readonly int _mandant;
        public readonly string _mandantName;

        public FileUserAdressStrategie(string fileName, string actionName, int mandant)
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
                var parts = line.Split('_');
                if (parts.Length == 2)
                {
                    var ident = $"{parts[0]}_{parts[1]}";
                    _customerIds.Add(ident);
                }
            }
        }

        public string ActionName { get; set; }
        public bool Show(Customer customer)
        {
            if (customer.Origin != _mandantName)
                return true; // nicht für diesen Mandent...

            var ident = $"{customer.CustomerNumber}_{customer.DeliveryAddress}";

            return _customerIds.Contains(ident);
        }

    }
}
