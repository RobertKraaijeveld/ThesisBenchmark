using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Benchmarking_Console_App.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;

namespace Benchmarking_Console_App.Testing
{
    public class DbWithSimpleDriverTest : AbstractPerformanceTest
    {
        public DbWithSimpleDriverTest(int amountOfModelsToCreate, int amountOfModelsToRetrieveByPrimaryKey,
            int amountOfModelsToRetrieveByContent, int amountOfModelsToUpdate)
            : base(amountOfModelsToCreate, amountOfModelsToRetrieveByPrimaryKey,
                amountOfModelsToRetrieveByContent, amountOfModelsToUpdate)
        {
        }

        protected override ActionsToMeasure GetActionsToMeasure<M>(IDatabaseType databaseType,
            bool wipeExistingDatabase)
        {
            var randomModelsToInsert = base.GetRandomModels<M>(amountOfModelsToCreate)
                                           .ToList();

            Action createAction;
            Action deleteAction;
            Action getAllAction;
            Action getByPkAction;
            Action updateAction;
            Action randomizeAction;
            Action truncateAction;

            if (databaseType.ToEnum().Equals(EDatabaseType.Perst)) // Perst is an OO-DB so we use the OO-api. 
            {
                var ooDatabaseApi = databaseType.GetDatabaseApis().ObjectOrientedDatabaseApi;

                truncateAction = () => ooDatabaseApi.DeleteAll();
                createAction = () => ooDatabaseApi.Create(randomModelsToInsert);
                getAllAction = () => ooDatabaseApi.GetAll<M>();
                deleteAction = () => ooDatabaseApi.Delete(randomModelsToInsert);

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

                getByPkAction = () =>
                {
                    foreach (var model in modelsToRetrieveByPk)
                    {
                        ooDatabaseApi.GetByComparison<M>(model);
                    }
                };


                var modelsWithUpdatedValues = randomModelsToInsert.Take(this.amountOfModelsToUpdate)
                    .ToList();

                randomizeAction = new Action(() =>
                {
                    var random = new Random();
                    modelsWithUpdatedValues.ForEach(x => x.RandomizeValuesExceptPrimaryKey(random));
                });

                updateAction = () => ooDatabaseApi.Update(modelsWithUpdatedValues);
            }
            else
            {
                var crudModelsForDatabaseType = databaseType.GetCrudModelsForDatabaseType<M>();
                var apiForDatabaseType = databaseType.GetDatabaseApis().NormalDatabaseApi;

                var currentModelTypePrimaryKeyName = randomModelsToInsert.First().GetPrimaryKeyFieldName();

                truncateAction = () => apiForDatabaseType.Truncate<M>();
                createAction = () => apiForDatabaseType.Create(randomModelsToInsert, crudModelsForDatabaseType.CreateModel);
                getAllAction = () => apiForDatabaseType.GetAll(crudModelsForDatabaseType.GetAllModel);


                var columnsToDeleteOn = new string[] {currentModelTypePrimaryKeyName};
                crudModelsForDatabaseType.DeleteModel.IdentifiersToDeleteOn = columnsToDeleteOn;

                deleteAction = () => apiForDatabaseType.Delete(randomModelsToInsert, crudModelsForDatabaseType.DeleteModel);


                var modelsToSearchForByPk = randomModelsToInsert.Take(amountOfModelsToRetrieveByPrimaryKey)
                                                                .ToList();
                getByPkAction = () =>
                {
                    // Each model has it's own combo of Primary Key: Value.
                    foreach (var model in modelsToSearchForByPk)
                    {
                        // So we update the ISearchModel with the PK of the current model, then search for that specific model. 
                        var columnsAndValuesToSearchFor = new Dictionary<string, object>();

                        var primaryKeyName = model.GetPrimaryKeyFieldName();
                        var primaryKeyValue = model.GetFieldsWithValues()[primaryKeyName];

                        columnsAndValuesToSearchFor.Add(primaryKeyName, primaryKeyValue);

                        crudModelsForDatabaseType.SearchModel.IdentifiersAndValuesToSearchFor = columnsAndValuesToSearchFor;
                        apiForDatabaseType.Search(crudModelsForDatabaseType.SearchModel);
                    }
                };


                var modelsWithUpdatedValues = randomModelsToInsert.Take(this.amountOfModelsToUpdate);
                randomizeAction = () =>
                {
                    var random = new Random();

                    modelsWithUpdatedValues = modelsWithUpdatedValues
                                                    .Select(m =>
                                                    {
                                                        m.RandomizeValuesExceptPrimaryKey(random);
                                                        return m;
                                                    })
                                                    .ToList();
                };


                var columnsToUpdateOn = columnsToDeleteOn;
                crudModelsForDatabaseType.UpdateModel.IdentifiersToFilterOn = columnsToUpdateOn;

                updateAction = () => apiForDatabaseType.Update(modelsWithUpdatedValues, crudModelsForDatabaseType.UpdateModel);
            }

            return new ActionsToMeasure()
            {
                CreateAction = createAction,
                DeleteAction = deleteAction,
                GetByPkAction = getByPkAction,
                RandomizeAction = randomizeAction,
                UpdateAction = updateAction,
                GetAllAction = getAllAction,
                TruncateAction = truncateAction,

                WipeExistingDatabase = wipeExistingDatabase
            };
        }

        protected override string GetDatabaseTypeString(IDatabaseType dbType)
        {
            return dbType.ToEnum().ToString();
        }
    }
}
