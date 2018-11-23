using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Benchmarking_Console_App.Configurations.Databases.DatabaseTypes;
using Benchmarking_Console_App.Tests;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Testing
{
    public abstract class AbstractPerformanceTest
    {
        protected int amountOfModelsToCreate;
        protected int amountOfModelsToRetrieveByPk;
        protected int amountOfModelsToRetrieveByContent;
        protected int amountOfModelsToUpdate;

        protected double timeSpentInsertingModels = 0;
        protected double timeSpentGettingModelsByValue = 0;
        protected double timeSpentGettingModelsByPrimaryKey = 0;
        protected double timeSpentUpdatingModels = 0;
        protected double timeSpentDeletingModels = 0;


        public TestReport GetTestReport<M>(IDatabaseType databaseType, bool scaled, bool wipeExistingDatabase) where M : class, IModel, new()
        {
            var actions = GetActionsToMeasure<M>(databaseType, wipeExistingDatabase);

            this.MeasureCRUDTimes(actions);

            var scaledOrNotStr = scaled ? "(scaled)" : "(unscaled)";
            return new TestReport()
            {
                DatabaseTypeUsedStr = $"{GetDatabaseTypeString(databaseType)} {scaledOrNotStr}",
                ModelTypeName = typeof(M).Name,

                TimeSpentDeletingAllModels = timeSpentDeletingModels,
                TimeSpentInsertingModels = timeSpentInsertingModels,
                TimeSpentRetrievingModelsByValue = timeSpentGettingModelsByValue,
                TimeSpentRetrievingModelsByPrimaryKey = timeSpentGettingModelsByPrimaryKey,
                TimeSpentUpdatingModels = timeSpentUpdatingModels,

                AmountOfModelsInserted = amountOfModelsToCreate,
                AmountOfModelsRetrievedByContent = amountOfModelsToRetrieveByContent,
                AmountOfModelsRetrievedByPrimaryKey = amountOfModelsToRetrieveByPk,
                AmountOfModelsUpdated = amountOfModelsToUpdate
            };
        }

        public void SetAmounts(int AmountOfModelsToCreate, int AmountOfModelsToRetrieveByPk,
                               int AmountOfModelsToRetrieveByContent, int AmountOfModelsToUpdate)
        {
            this.amountOfModelsToCreate = AmountOfModelsToCreate;
            this.amountOfModelsToRetrieveByPk = AmountOfModelsToRetrieveByPk;
            this.amountOfModelsToRetrieveByContent = AmountOfModelsToRetrieveByContent;
            this.amountOfModelsToUpdate = AmountOfModelsToUpdate;
        }

        protected abstract ActionsToMeasure GetActionsToMeasure<M>(IDatabaseType databaseType, bool wipeExistingDatabase) where M: class, IModel, new();

        protected abstract string GetDatabaseTypeString(IDatabaseType dbType);

        protected List<KeyValuePair<string, object>> GetPrimaryKeyAndValuePerModel<M>(List<M> models) where M: class, IModel, new()
        {
            var primaryKeyAndValuePerModel = new List<KeyValuePair<string, object>>();

            // Each model has it's own combo of Primary Key: Value. 
            // We retrieve it for each model, and add it to the list as a dictionary.
            foreach (var model in models)
            {
                var primaryKeyName = model.GetPrimaryKeyFieldName();
                var primaryKeyValue = model.GetFieldsWithValues()[primaryKeyName];

                var columnsAndValuesToSearchFor = new KeyValuePair<string, object>(primaryKeyName, primaryKeyValue);

                primaryKeyAndValuePerModel.Add(columnsAndValuesToSearchFor);
            }

            return primaryKeyAndValuePerModel;
        }

        protected List<KeyValuePair<string, object>> GetFirstNonPrimaryKeyAttributePerModel<M>(List<M> models) where M : class, IModel, new()
        {
            var attributeNameAndValuePerModel = new List<KeyValuePair<string, object>>();

            string firstNonPrimaryKeyAttributeName = null;
            foreach (var model in models)
            {
                if (firstNonPrimaryKeyAttributeName == null)
                {
                    var modelPrimaryKeyName = model.GetPrimaryKeyFieldName();
                    var modelFieldsAndValues = model.GetFieldsWithValues();

                    firstNonPrimaryKeyAttributeName = modelFieldsAndValues.Keys
                                                                          .First(x => x.Equals(modelPrimaryKeyName));
                }

                var valueOfFirstNonPkAttribute = model.GetFieldsWithValues()[firstNonPrimaryKeyAttributeName];
                var columnsAndValuesToSearchFor = new KeyValuePair<string, object>(firstNonPrimaryKeyAttributeName, valueOfFirstNonPkAttribute);

                attributeNameAndValuePerModel.Add(columnsAndValuesToSearchFor);
            }

            return attributeNameAndValuePerModel;
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

        protected struct ActionsToMeasure
        {
            public Action CreateAction;
            public Action DeleteAction;
            public Action GetByValueAction;
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

            this.timeSpentGettingModelsByValue = GetTimeSpentOnActionMs(actions.GetByValueAction);
            this.timeSpentGettingModelsByPrimaryKey = GetTimeSpentOnActionMs(actions.GetByPkAction);

            if (actions.WipeExistingDatabase)
            {
                // Doing deleting before updating so we wont have to keep track of changes to the original models
                this.timeSpentDeletingModels = GetTimeSpentOnActionMs(actions.DeleteAction);
            }

            // Re-randomizing models and re-inserting before updating so some actual changes are made.
            actions.RandomizeAction.Invoke();
            this.timeSpentUpdatingModels = GetTimeSpentOnActionMs(actions.UpdateAction); 
        }

    }

}
