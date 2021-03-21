using System.Collections.Generic;

namespace ObjectGraphPathTest.TestGraphModels
{
    public class Product
    {
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public ICollection<DependentPart> Dependencies { get; set; }
    }
}
