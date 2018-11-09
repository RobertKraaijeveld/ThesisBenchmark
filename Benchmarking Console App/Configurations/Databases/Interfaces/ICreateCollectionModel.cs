using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.Interfaces
{
    public interface ICreateCollectionModel<M> where M : IModel, new()
    {
        String GetCreateCollectionText();
    }
}
