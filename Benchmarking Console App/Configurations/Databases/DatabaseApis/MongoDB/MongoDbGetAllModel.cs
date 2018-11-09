using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseApis.MongoDB
{
    public class MongoDbGetAllModel<M> : IGetAllModel<M> where M : IModel, new()
    {
        public string CreateGetAllString()
        {
            // empty filter and empty projection is the same as SELECT * without where-clause
            return "{{},{}}"; 
        }
    }
}
