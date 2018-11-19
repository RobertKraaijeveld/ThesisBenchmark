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


        public TestReport GetTestReport<M>(IDatabaseType databaseType, bool scaled, bool wipeExistingDatabase) where M : class, IModel, new()
        {
            var actions = GetActionsToMeasure<M>(databaseType, wipeExistingDatabase);

            this.MeasureCRUDTimes(actions);

            var scaledOrNotStr = scaled ? "(scaled)" : "(unscaled)";
            return new TestReport()
            {
                DatabaseTypeUsedStr = $"{databaseType.GetName()} {scaledOrNotStr}",
                ModelTypeName = typeof(M).Name,

                TimeSpentDeletingAllModels = timeSpentDeletingModels,
                TimeSpentInsertingModels = timeSpentInsertingModels,
                TimeSpentRetrievingAllModels = timeSpentGettingAllModels,
                //TimeSpentRetrievingModelsByContent = timeSpentGettingModelsByContent, TODO
                TimeSpentRetrievingModelsByPrimaryKey = timeSpentGettingModelsByPrimaryKey,
                TimeSpentUpdatingModels = timeSpentUpdatingModels,

                AmountOfModelsInserted = amountOfModelsToCreate,
                AmountOfModelsRetrievedByContent = amountOfModelsToRetrieveByContent,
                AmountOfModelsRetrievedByPrimaryKey = amountOfModelsToRetrieveByPrimaryKey,
                AmountOfModelsUpdated = amountOfModelsToUpdate
            };
        }

        protected abstract ActionsToMeasure GetActionsToMeasure<M>(IDatabaseType databaseType, bool wipeExistingDatabase) where M: class, IModel, new();


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

        protected struct ActionsToMeasure
        {
            public Action CreateAction;
            public Action DeleteAction;
            public Action GetAllAction;
            public Action GetByPkAction;
            public Action UpdateAction;
            public Action TruncateAction;
            public Action RandomizeAction;

            public bool WipeExistingDatabase; 
        }

        private void MeasureCRUDTimes(ActionsToMeasure actions)
        {
            if (actions.WipeExistingDatabase)
            {
                actions.TruncateAction.Invoke();
                this.timeSpentInsertingModels = GetTimeSpentOnActionMs(actions.CreateAction);
            }

            this.timeSpentGettingAllModels = GetTimeSpentOnActionMs(actions.GetAllAction);
            this.timeSpentGettingModelsByPrimaryKey = GetTimeSpentOnActionMs(actions.GetByPkAction);

            // Doing deleting before updating so we wont have to keep track of changes to the original models
            this.timeSpentDeletingModels = GetTimeSpentOnActionMs(actions.DeleteAction);

            // Re-randomizing models before updating so some actual changes are made.
            actions.RandomizeAction.Invoke();
            this.timeSpentUpdatingModels = GetTimeSpentOnActionMs(actions.UpdateAction);

            // Forcing garbage collection to ensure old models are not retained in memory.
            // This would skew the performance (especially memory) measurements.
            GC.Collect();
        }

    }

}
