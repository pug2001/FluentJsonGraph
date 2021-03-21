using System.Collections.Generic;

namespace ObjectGraphPathTest.TestGraphModels
{
    public class Manufacturer
    {
        public string ManufacturerName { get; set; }
        public ICollection<Product> Products { get; set; }

    }
}
