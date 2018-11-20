using System;
using System.Collections.Generic;
using System.Linq;
using Benchmarking_Console_App.Configurations.Databases.DatabaseTypes;
using Benchmarking_Console_App.Models.DatabaseModels;
using Benchmarking_Console_App.Testing;

namespace Benchmarking_Console_App.Tests.CQRS
{
    public class DbWithCqrsTest : AbstractPerformanceTest
    {
        private readonly IDatabaseType readDatabaseType;

        public DbWithCqrsTest(int amountOfModelsToCreate, int amountOfModelsToRetrieveByPrimaryKey,
                              int amountOfModelsToRetrieveByContent, int amountOfModelsToUpdate, IDatabaseType readDatabaseType)
            : base(amountOfModelsToCreate, amountOfModelsToRetrieveByPrimaryKey,
                   amountOfModelsToRetrieveByContent, amountOfModelsToUpdate)
        {
            this.readDatabaseType = readDatabaseType;
        }

        protected override ActionsToMeasure GetActionsToMeasure<M>(IDatabaseType writeDatabaseType, bool wipeExistingDatabase)
        {
            var randomizedStartingModels = base.GetRandomModels<MinuteAveragesRow>(amountOfModelsToCreate)
                                               .ToList();

            var currentModelTypePrimaryKeyName = randomizedStartingModels.First().GetPrimaryKeyFieldName();


            // Getting api's for both read and write db's, creating CqrsReader and CqrsWriter which use these api's.
            var apiForReadDatabase = readDatabaseType.GetDatabaseApis().NormalDatabaseApi;
            var apiForWriteDatabase = readDatabaseType.GetDatabaseApis().NormalDatabaseApi;

            var readDatabaseCrudModels = readDatabaseType.GetCrudModelsForDatabaseType<M>();
            var writeDatabaseCrudModels = writeDatabaseType.GetCrudModelsForDatabaseType<M>();

            var cqrsReader = new CqrsReader<M>(apiForReadDatabase, readDatabaseCrudModels);
            var cqrsWriter = new CqrsWriter<M>(apiForWriteDatabase, writeDatabaseCrudModels);


            // Creating read database actions
            Action getAllAction = () => cqrsReader.GetAll();



            var modelsToSearchForByPk = randomizedStartingModels.Take(amountOfModelsToRetrieveByPrimaryKey)
                                                                .ToList();
            Action getByPkAction = () =>
            {
                // Each model has it's own combo of Primary Key: Value.
                foreach (var model in modelsToSearchForByPk)
                {
                    // So we update the ISearchModel of the CqrsReader with the PK of the current model, then search for that specific model. 
                    var columnsAndValuesToSearchFor = new Dictionary<string, object>();

                    var primaryKeyName = model.GetPrimaryKeyFieldName();
                    var primaryKeyValue = model.GetFieldsWithValues()[primaryKeyName];

                    columnsAndValuesToSearchFor.Add(primaryKeyName, primaryKeyValue);

                    cqrsReader.crudModels.SearchModel.IdentifiersAndValuesToSearchFor = columnsAndValuesToSearchFor;
                    cqrsReader.Search(cqrsReader.crudModels.SearchModel);
                }
            };


            // Creating write database actions
            Action createAction = () => cqrsWriter.Create((IEnumerable<M>) randomizedStartingModels);

            var columnsToDeleteOn = new string[] { currentModelTypePrimaryKeyName };
            Action deleteAllAction = () =>
            {
                cqrsWriter.crudModels.DeleteModel.IdentifiersToDeleteOn = columnsToDeleteOn;
                cqrsWriter.Delete((IEnumerable<M>) randomizedStartingModels);
            };


            //TODO
            //var updateAction = new Action(() =>
            //{
            //    ormApiForDatabase.Update(randomizedStartingModels);
            //});

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
                cqrsWriter.Truncate();
            });


            return new ActionsToMeasure()
            {
                CreateAction = createAction,
                DeleteAction = deleteAllAction,
                GetByPkAction = getByPkAction,
                RandomizeAction = randomizeAction,
                UpdateAction = () => {}, // TODO
                GetAllAction = getAllAction,
                TruncateAction = truncateAction,

                WipeExistingDatabase = wipeExistingDatabase
            };
        }

        protected override string GetDatabaseTypeString(IDatabaseType writeDatabaseType)
        {
            return $"CQRS (Read DB {readDatabaseType.ToEnum()}, Write DB {writeDatabaseType.ToEnum()}";
        }
    }
}
