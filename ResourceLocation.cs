using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VowpalWabbitIncrementalTraining
{
    public class ResourceLocation
    {
        public string Name { get; set; }

        public AzureBlobDataReference Location { get; set; }
    }

    public class ResourceLocations
    {
        public ResourceLocation[] Resources { get; set; }
    }
}
