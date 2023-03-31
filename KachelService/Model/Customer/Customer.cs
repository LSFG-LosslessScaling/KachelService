using System.Net;

namespace KachelService.Model.Customer
{
    public class Customer
    {
        public string Ident { get; set; }
        public int? Company { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerPayment { get; set; }
        public int DeliveryAddress { get; set; }
        public int CustomerWarehouseNumber { get; set; }
        public string SalesTerritory { get; set; }
        public string TargetGroup { get; set; }
        public string Origin { get; set; }
        public List<CustomerFlag> Flags { get; set; }

        public Address Address { get; set; }

        public User User { get; set; }

        public string GetIdent()
        {
            if (!string.IsNullOrEmpty(Ident) && Ident.Length > 2 && Ident[1] == '.')
                return Ident.Substring(2);

            return Ident;
        }
    }

    public class CustomerFlag
    {
        public string FlagNumber { get; set; }

        public string Flag { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public string Attribute { get; set; }

        public string FlagString
        {
            get
            {
                //if (!FlagNumber.HasValue)
                //    return string.Empty;

                if (string.IsNullOrEmpty(Attribute))
                    return $"{FlagNumber}";

                return $"{FlagNumber} {Attribute}";
            }
        }

        public string ShortFlagString
        {
            get
            {
                if (string.IsNullOrEmpty(Attribute))
                    return $"{FlagNumber}";

                return $"{FlagNumber}{Attribute}".ToLower();
            }
        }
    }

    public class Address
    {
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string Street { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string EMail { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
    }

    public class User
    {
        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string EmailAddress { get; set; }
    }
}
