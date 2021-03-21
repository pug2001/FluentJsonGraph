
using System.Collections.Generic;

namespace ObjectGraphPathTest.TestGraphModels
{
    public class DependentPart
    {
        public Product ParentPart { get; set; }
        public Product RequiresChildPart { get; set; }
        public bool Optional { get; set; }
    }
}
