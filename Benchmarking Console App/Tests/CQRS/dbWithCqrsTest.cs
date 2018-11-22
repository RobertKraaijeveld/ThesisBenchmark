using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarking_Console_App.Configurations.Databases.DatabaseTypes;
using Benchmarking_Console_App.Models.DatabaseModels;
using Benchmarking_Console_App.Testing;
using Benchmarking_program.Configurations.Databases.Interfaces;

namespace Benchmarking_Console_App.Tests.CQRS
{
    public class DbWithCqrsTest : AbstractPerformanceTest
    {
        private readonly IDatabaseType readDatabaseType;

        public DbWithCqrsTest(IDatabaseType readDatabaseType)
        {    
            this.readDatabaseType = readDatabaseType;
        }

        protected override ActionsToMeasure GetActionsToMeasure<M>(IDatabaseType writeDatabaseType, bool wipeExistingDatabase)
        {
            var randomizedStartingModels = base.GetRandomModels<M>(amountOfModelsToCreate)
                                               .ToList();

            var currentModelTypePrimaryKeyName = randomizedStartingModels.First().GetPrimaryKeyFieldName();


            // Getting api's for both read and write db's, creating CqrsReader and CqrsWriter which use these api's.
            var apiForReadDatabase = readDatabaseType.GetDatabaseApis().NormalDatabaseApi;
            var apiForWriteDatabase = writeDatabaseType.GetDatabaseApis().NormalDatabaseApi;

            var readDatabaseCrudModels = readDatabaseType.GetCrudModelsForDatabaseType<M>();
            var writeDatabaseCrudModels = writeDatabaseType.GetCrudModelsForDatabaseType<M>();

            var cqrsReader = new CqrsReader<M>(apiForReadDatabase, readDatabaseCrudModels);
            var cqrsWriter = new CqrsWriter<M>(apiForWriteDatabase, writeDatabaseCrudModels);


            // Creating read database actions
            var modelsToSearchFor = randomizedStartingModels.Take(amountOfModelsToRetrieveByPk).ToList();
            var primaryKeyAndValuePerModel = base.GetPrimaryKeyAndValuePerModel(modelsToSearchFor);

            Action getByPkAction = () =>
            {
                var primaryKeySearchModels = new List<ISearchModel<M>>();

                for (int i = 0; i < primaryKeyAndValuePerModel.Count; i++)
                {
                    var primaryKeyAndValueOfThisModel = primaryKeyAndValuePerModel[i];

                    // We update the ISearchModel of the CqrsReader with the PK name and value of the current model, then search for that specific model. 
                    readDatabaseCrudModels.SearchModel.IdentifiersAndValuesToSearchFor = new Dictionary<string, object>();
                    readDatabaseCrudModels.SearchModel.IdentifiersAndValuesToSearchFor.Add(primaryKeyAndValueOfThisModel.Key, 
                                                                                           primaryKeyAndValueOfThisModel.Value);

                    primaryKeySearchModels.Add(readDatabaseCrudModels.SearchModel.Clone());
                }

                cqrsReader.OpenConnectionToApi();
                cqrsReader.Search(primaryKeySearchModels);
                cqrsReader.CloseConnectionToApi();
            };


            // Get by value action
            var firstNonPkAttributePerModel = base.GetFirstNonPrimaryKeyAttributePerModel(modelsToSearchFor);
            Action getByValueAction = () =>
            {
                List<ISearchModel<M>> valueSearchModels = new List<ISearchModel<M>>();

                for (int i = 0; i < firstNonPkAttributePerModel.Count; i++)
                {
                    var firstNonPkAttributeNameAndValueOfCurrModel = firstNonPkAttributePerModel[i];

                    // Again, updating the ISearchModel of the CqrsReader, but using the name/value of the first non-primary key attribute this time.
                    readDatabaseCrudModels.SearchModel.IdentifiersAndValuesToSearchFor = new Dictionary<string, object>();
                    readDatabaseCrudModels.SearchModel.IdentifiersAndValuesToSearchFor.Add(firstNonPkAttributeNameAndValueOfCurrModel.Key,
                                                                                           firstNonPkAttributeNameAndValueOfCurrModel.Value);
                    valueSearchModels.Add(readDatabaseCrudModels.SearchModel.Clone());
                }

                cqrsReader.OpenConnectionToApi();
                cqrsReader.Search(valueSearchModels);
                cqrsReader.CloseConnectionToApi();
            };


            // Create action
            Action createAction = () =>
            {
                cqrsWriter.OpenConnectionToApi();
                cqrsWriter.Create(randomizedStartingModels);
                cqrsWriter.CloseConnectionToApi();
            };


            // Delete action
            var columnsToDeleteOn = new string[] { currentModelTypePrimaryKeyName };
            Action deleteAllAction = () =>
            {
                cqrsWriter.OpenConnectionToApi();

                cqrsWriter.crudModels.DeleteModel.IdentifiersToDeleteOn = columnsToDeleteOn;
                cqrsReader.crudModels.DeleteModel.IdentifiersToDeleteOn = columnsToDeleteOn;

                cqrsWriter.Delete(randomizedStartingModels);

                cqrsWriter.CloseConnectionToApi();
            };


            // Update action
            var columnsToUpdateOn = columnsToDeleteOn;
            var updateAction = new Action(() =>
            {
                cqrsWriter.OpenConnectionToApi();

                cqrsWriter.crudModels.UpdateModel.IdentifiersToFilterOn = columnsToUpdateOn;
                cqrsReader.crudModels.UpdateModel.IdentifiersToFilterOn = columnsToUpdateOn;

                cqrsWriter.Update(randomizedStartingModels);

                cqrsWriter.CloseConnectionToApi();
            });


            // Truncation action
            var randomizeAction = new Action(() =>
            {
                var random = new Random();
                foreach (var model in randomizedStartingModels)
                {
                    model.RandomizeValuesExceptPrimaryKey(random);
                }
            });

            var truncateAction = new Action(() =>
            {
                cqrsWriter.OpenConnectionToApi();
                cqrsWriter.Truncate();
                cqrsWriter.CloseConnectionToApi();
            });


            return new ActionsToMeasure()
            {
                CreateAction = createAction,
                DeleteAction = deleteAllAction,
                GetByPkAction = getByPkAction,
                RandomizeAction = randomizeAction,
                UpdateAction = updateAction,
                GetByValueAction = getByValueAction,
                TruncateAction = truncateAction,

                WipeExistingDatabase = wipeExistingDatabase
            };
        }

        protected override string GetDatabaseTypeString(IDatabaseType writeDatabaseType)
        {
            return $"CQRS (Read DB {readDatabaseType.ToEnum()} with Write DB {writeDatabaseType.ToEnum()})";
        }
    }
}
