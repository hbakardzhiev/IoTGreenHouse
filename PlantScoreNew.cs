using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoTGreenHouse
{
    class PlantScoreNew : TableEntity
    {
        public Int32 Score { get; set; }

        public PlantScoreNew() : base(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()) { }
    }
}
