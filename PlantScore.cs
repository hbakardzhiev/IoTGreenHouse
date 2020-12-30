using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoTGreenHouse
{
    class PlantScore : TableEntity
    {
        public Int32 Score { get; set; }

        public PlantScore(string guid, string picUri) : base(guid, picUri) { }
    }
}
