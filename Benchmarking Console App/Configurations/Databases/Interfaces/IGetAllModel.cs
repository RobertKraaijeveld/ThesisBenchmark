using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.Interfaces
{
    public interface IGetAllModel<M> where M: IModel, new()
    {
        string CreateGetAllString();
    }
}
