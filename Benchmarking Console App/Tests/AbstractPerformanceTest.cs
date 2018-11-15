using System;
using System.Collections.Generic;
using System.Diagnostics;
using Benchmarking_Console_App.Configurations.Databases.DatabaseTypes;
using Benchmarking_Console_App.Tests;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Testing
{
    public abstract class AbstractPerformanceTest
    {
        protected int amountOfModelsToCreate;
        protected int amountOfModelsToRetrieveByPrimaryKey;
        protected int amountOfModelsToRetrieveByContent;
        protected int amountOfModelsToUpdate;

        protected double timeSpentInsertingModels = 0;
        protected double timeSpentGettingAllModels = 0;
        protected double timeSpentGettingModelsByPrimaryKey = 0;
        protected double timeSpentUpdatingModels = 0;
        protected double timeSpentDeletingModels = 0;


        protected AbstractPerformanceTest(int amountOfModelsToCreate, int amountOfModelsToRetrieveByPrimaryKey,
                                          int amountOfModelsToRetrieveByContent, int amountOfModelsToUpdate)
        {
            this.amountOfModelsToCreate = amountOfModelsToCreate;
            this.amountOfModelsToRetrieveByPrimaryKey = amountOfModelsToRetrieveByPrimaryKey;
            this.amountOfModelsToRetrieveByContent = amountOfModelsToRetrieveByContent;
            this.amountOfModelsToUpdate = amountOfModelsToUpdate;
        }


        public abstract TestReport GetTestReport<M>(IDatabaseType databaseType, bool scaled) where M : class, IModel, new();

        protected void MeasureCRUDTimes(Action createModelsAction, Action deleteModelsAction, Action getAllModelsAction,
                                        Action getModelsByPkAction, Action updateModelsAction, Action truncateAction)
        {
            // Truncating to make sure no left-overs from previous tests remain
            truncateAction.Invoke();

            this.timeSpentInsertingModels = GetTimeSpentOnActionMs(createModelsAction);
            this.timeSpentGettingAllModels = GetTimeSpentOnActionMs(getAllModelsAction);
            this.timeSpentGettingModelsByPrimaryKey = GetTimeSpentOnActionMs(getModelsByPkAction);

            // Doing deleting before updating so we wont have to keep track of changes to the original models
            this.timeSpentDeletingModels = GetTimeSpentOnActionMs(deleteModelsAction);

            this.timeSpentUpdatingModels = GetTimeSpentOnActionMs(updateModelsAction);


            // Forcing garbage collection to ensure old models are not retained in memory.
            // This would skew the performance (especially memory) measurements.
            GC.Collect();
        }

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
    }
}
