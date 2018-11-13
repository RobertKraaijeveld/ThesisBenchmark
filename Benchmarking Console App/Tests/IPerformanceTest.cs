using System;
using System.Collections.Generic;
using System.Diagnostics;
using Benchmarking_Console_App.Configurations.Databases.DatabaseTypes;
using Benchmarking_Console_App.Tests.TestReport;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Testing
{
    public abstract class AbstractPerformanceTest
    {
        protected int amountOfModelsToCreate;
        protected int amountOfModelsToRetrieveByPrimaryKey;
        protected int amountOfModelsToRetrieveByContent;
        protected int amountOfModelsToUpdate;

        protected AbstractPerformanceTest(int amountOfModelsToCreate, int amountOfModelsToRetrieveByPrimaryKey,
                                          int amountOfModelsToRetrieveByContent, int amountOfModelsToUpdate)
        {
            this.amountOfModelsToCreate = amountOfModelsToCreate;
            this.amountOfModelsToRetrieveByPrimaryKey = amountOfModelsToRetrieveByPrimaryKey;
            this.amountOfModelsToRetrieveByContent = amountOfModelsToRetrieveByContent;
            this.amountOfModelsToUpdate = amountOfModelsToUpdate;
        }


        public abstract TestReport Test<M>(IDatabaseType databaseType) where M : IModel, new();


        protected IEnumerable<M> GetRandomModels<M>(int amountToCreate) where M : IModel, new()
        {
            var random = new Random();
            var listOfNewModels = new M[amountToCreate];
            for (int i = 0; i < amountToCreate; i++)
            {
                var newModel = new M();
                newModel.Randomize(amountOfExistingModels: i, randomGenerator: random);

                listOfNewModels[i] = newModel;
            }
            return listOfNewModels;
        }

        protected long GetTimeSpentOnActionMs(Action actionToExecute)
        {
            var sw = new Stopwatch();
            sw.Start();
            actionToExecute.Invoke();
            sw.Stop();

            return sw.ElapsedMilliseconds;
        }

        protected void RunGC()
        {
            // Forcing garbage collection to ensure old models are not retained in memory.
            // This would skew the performance (especially memory) measurements.
            GC.Collect();
        }
    }
}
