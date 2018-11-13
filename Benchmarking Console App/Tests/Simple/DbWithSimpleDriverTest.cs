using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Benchmarking_Console_App.Configurations.Databases.DatabaseTypes;
using Benchmarking_Console_App.Tests.TestReport;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;

namespace Benchmarking_Console_App.Testing
{
    public class DbWithSimpleDriverTest : AbstractPerformanceTest
    {
        public DbWithSimpleDriverTest(int amountOfModelsToCreate, int amountOfModelsToRetrieveByPrimaryKey,
                                      int amountOfModelsToRetrieveByContent, int amountOfModelsToUpdate)
            : base(amountOfModelsToCreate, amountOfModelsToRetrieveByPrimaryKey,
                   amountOfModelsToRetrieveByContent, amountOfModelsToUpdate)
        { }

        public override TestReport Test<M>(IDatabaseType databaseType)
        {
            double timeSpentInsertingModels = 0;
            double timeSpentGettingAllModels = 0;
            double timeSpentGettingModelsByPk = 0;
            double timeSpentGettingModelsByContent = 0;
            double timeSpentUpdatingModels = 0;
            double timeSpentDeletingModels = 0;


            var randomModelsToInsert = base.GetRandomModels<M>(amountOfModelsToCreate)
                                           .ToList();

            // Perst is OO-DB so we use the OO-api. 
            if (databaseType.ToEnum().Equals(EDatabaseType.Perst))
            {
                var ooDatabaseApi = databaseType.GetDatabaseApi().ObjectOrientedDatabaseApi;


                // Truncating all first in order to start fresh
                ooDatabaseApi.DeleteAll();


                // Testing inserting
                timeSpentInsertingModels = base.GetTimeSpentOnActionMs(() =>
                {
                    ooDatabaseApi.Create(randomModelsToInsert);
                });


                // Testing getting all
                timeSpentGettingAllModels = base.GetTimeSpentOnActionMs(() => { ooDatabaseApi.GetAll<M>(); });


                // Testing getting by pk
                var modelsToRetrieveByPk = randomModelsToInsert.Take(this.amountOfModelsToRetrieveByPrimaryKey)
                    .Select(m =>
                    {
                        // Perst will search for the PK only if all other fields are null.
                        // So we set each field which isnt the PK to null via reflection.
                        var primaryKeyFieldName = m.GetPrimaryKeyFieldName();

                        foreach (var field in m.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
                        {
                            if (!field.Name.Equals(primaryKeyFieldName))
                            {
                                field.SetValue(m, null);
                            }
                        }

                        return m;
                    })
                    .ToList();

                timeSpentGettingModelsByPk = base.GetTimeSpentOnActionMs(() =>
                {
                    foreach (var model in modelsToRetrieveByPk)
                    {
                        ooDatabaseApi.GetByComparison<M>(model);
                    }
                });


                // Testing getting by content ??
                // ohgodno


                // Testing deleting all. This is done BEFORE updating etc 
                // so we wont have to keep track of changes to the original models
                timeSpentDeletingModels = base.GetTimeSpentOnActionMs(() =>
                {
                    ooDatabaseApi.Delete(randomModelsToInsert);
                });


                // Re-inserting the old models so we can test updating.
                ooDatabaseApi.Create(randomModelsToInsert);


                // Testing updating
                var modelsWithUpdatedValues = randomModelsToInsert.Take(this.amountOfModelsToUpdate)
                                                                   .ToList();

                var random = new Random();
                modelsWithUpdatedValues.ForEach(x => x.RandomizeValuesExceptPrimaryKey(random));

                timeSpentUpdatingModels = base.GetTimeSpentOnActionMs(() =>
                {
                    ooDatabaseApi.Update(modelsWithUpdatedValues);
                });
            }
            else
            {
                var crudModelsForDatabaseType = databaseType.GetCrudModelsForDatabaseType<M>();
                var apiForDatabaseType = databaseType.GetDatabaseApi().NormalDatabaseApi;

                var currentModelTypePrimaryKeyName = randomModelsToInsert.First().GetPrimaryKeyFieldName();



                // Truncating all first in order to start fresh
                apiForDatabaseType.Truncate<M>();


                // Testing inserting
                timeSpentInsertingModels = base.GetTimeSpentOnActionMs(() =>
                {
                    apiForDatabaseType.Create(randomModelsToInsert, crudModelsForDatabaseType.CreateModel);
                });


                // Testing getting all
                timeSpentGettingAllModels = base.GetTimeSpentOnActionMs(() =>
                {
                    apiForDatabaseType.GetAll(crudModelsForDatabaseType.GetAllModel);
                });


                // Testing getting by primary key
                var modelsToSearchForByPk = randomModelsToInsert.Take(amountOfModelsToRetrieveByPrimaryKey)
                                                                .ToList();

                timeSpentGettingModelsByPk = base.GetTimeSpentOnActionMs(() =>
                {
                    // Each model has it's own combo of Primary Key: Value.
                    // So we update the ISearchModel for each model, then search for that specific model. 
                    foreach (var model in modelsToSearchForByPk)
                    {
                        var columnsAndValuesToSearchFor = new Dictionary<string, object>();

                        var primaryKeyName = model.GetPrimaryKeyFieldName();
                        var primaryKeyValue = model.GetFieldsWithValues()[primaryKeyName];

                        columnsAndValuesToSearchFor.Add(primaryKeyName, primaryKeyValue);

                        crudModelsForDatabaseType.SearchModel.IdentifiersAndValuesToSearchFor = columnsAndValuesToSearchFor;
                        apiForDatabaseType.Search(crudModelsForDatabaseType.SearchModel);
                    }
                });


                // Testing getting by content TODO:


                // Testing deleting all. This is done BEFORE updating etc 
                // so we wont have to keep track of changes to the original models
                var columnsToDeleteOn = new string[] { currentModelTypePrimaryKeyName };
                crudModelsForDatabaseType.DeleteModel.IdentifiersToDeleteOn = columnsToDeleteOn;

                timeSpentDeletingModels = base.GetTimeSpentOnActionMs(() =>
                {
                    apiForDatabaseType.Delete(randomModelsToInsert, crudModelsForDatabaseType.DeleteModel);
                });


                // Testing updating 
                var columnsToUpdateOn = columnsToDeleteOn;
                crudModelsForDatabaseType.UpdateModel.IdentifiersToFilterOn = columnsToUpdateOn;

                var random = new Random();
                var modelsWithUpdatedValues = randomModelsToInsert.Take(this.amountOfModelsToUpdate)
                                                                  .Select(m =>
                                                                  {
                                                                      m.RandomizeValuesExceptPrimaryKey(random);
                                                                      return m;
                                                                  })
                                                                  .ToList();

                timeSpentUpdatingModels = base.GetTimeSpentOnActionMs(() =>
                {
                    apiForDatabaseType.Update(modelsWithUpdatedValues, crudModelsForDatabaseType.UpdateModel);
                });
            }

            return new TestReport()
            {
                DatabaseTypeUsed = databaseType.ToEnum(),
                ModelTypeUsed = typeof(M),

                TimeSpentDeletingAllModels = timeSpentDeletingModels,
                TimeSpentInsertingModels = timeSpentInsertingModels,
                TimeSpentRetrievingAllModels = timeSpentGettingAllModels,
                TimeSpentRetrievingModelsByContent = timeSpentGettingModelsByContent,
                TimeSpentRetrievingModelsByPrimaryKey = timeSpentGettingModelsByPk,
                TimeSpentUpdatingModels = timeSpentUpdatingModels,

                AmountOfModelsInserted = amountOfModelsToCreate,
                AmountOfModelsRetrievedByContent = amountOfModelsToRetrieveByContent,
                AmountOfModelsRetrievedByPrimaryKey = amountOfModelsToRetrieveByPrimaryKey,
                AmountOfModelsUpdated = amountOfModelsToUpdate
            };
        }
    }
}
