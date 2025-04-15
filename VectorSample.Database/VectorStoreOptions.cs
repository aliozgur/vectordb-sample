using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorSample.Database
{
    public class VectorStoreOptions
    {
        public string ConnectionString { get; private set; }
        public VectorStoreOptions(string connStr)
        {
            ConnectionString = connStr;
        }
    }
}
