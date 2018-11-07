using System;
using System.Diagnostics;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases
{
    public static class DatabaseSeeder
    {
        public static void SeedRandomly<M>(IDatabaseApi databaseApi, ICreateModel createModel, int amount) where M : IModel, new()
        {
            var random = new Random();
            var listOfNewModels = new M[amount];
            for (int i = 0; i < amount; i++)
            {
                var newModel = new M();
                newModel.Randomize(i, random);

                listOfNewModels[i] = newModel;
            }
            databaseApi.Create(listOfNewModels, createModel);
        }
    }
}
