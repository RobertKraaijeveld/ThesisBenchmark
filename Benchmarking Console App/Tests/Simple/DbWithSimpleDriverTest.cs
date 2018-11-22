using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Benchmarking_Console_App.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Configurations.Databases.Interfaces;

namespace Benchmarking_Console_App.Testing
{
    public class DbWithSimpleDriverTest : AbstractPerformanceTest
    {
        protected override ActionsToMeasure GetActionsToMeasure<M>(IDatabaseType databaseType,
            bool wipeExistingDatabase)
        {
            var randomModelsToInsert = base.GetRandomModels<M>(amountOfModelsToCreate)
                                           .ToList();

            Action createAction;
            Action deleteAction;
            Action getByPkAction;
            Action getByValueAction;
            Action updateAction;
            Action randomizeAction;
            Action truncateAction;

            if (databaseType.ToEnum().Equals(EDatabaseType.Perst)) // Perst is an OO-DB so we use the OO-api. 
            {
                var ooDatabaseApi = databaseType.GetDatabaseApis().ObjectOrientedDatabaseApi;

                truncateAction = () => ooDatabaseApi.DeleteAll();
                createAction = () => ooDatabaseApi.Create(randomModelsToInsert);
                deleteAction = () => ooDatabaseApi.Delete(randomModelsToInsert);

                var modelsToRetrieveByPk = randomModelsToInsert.Take(this.amountOfModelsToRetrieveByPk)
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


                var modelsToRetrieveByValue = randomModelsToInsert.Take(this.amountOfModelsToRetrieveByContent);
                getByValueAction = () =>
                {
                    foreach (var model in modelsToRetrieveByValue)
                    {
                        ooDatabaseApi.GetByComparison<M>(model);
                    }
                };


                var modelsWithUpdatedValues = randomModelsToInsert.Take(this.amountOfModelsToUpdate).ToList();

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

                // Truncate, create, get all Actions
                truncateAction = () => 
                {
                    apiForDatabaseType.OpenConnection();
                    apiForDatabaseType.Truncate<M>();
                    apiForDatabaseType.CloseConnection();
                };

                createAction = () => 
                {
                    apiForDatabaseType.OpenConnection();
                    apiForDatabaseType.Create(randomModelsToInsert, crudModelsForDatabaseType.CreateModel);
                    apiForDatabaseType.CloseConnection();
                };


                // Deleting Action
                var columnsToDeleteOn = new string[] {currentModelTypePrimaryKeyName};
                crudModelsForDatabaseType.DeleteModel.IdentifiersToDeleteOn = columnsToDeleteOn;

                deleteAction = () => 
                {
                    apiForDatabaseType.OpenConnection();
                    apiForDatabaseType.Delete(randomModelsToInsert, crudModelsForDatabaseType.DeleteModel);
                    apiForDatabaseType.CloseConnection();
                };

                // Getting by primary key Action
                var modelsToSearchFor = randomModelsToInsert.Take(amountOfModelsToRetrieveByPk)
                                                                .ToList();
                var primaryKeyAndValuePerModel = base.GetPrimaryKeyAndValuePerModel(modelsToSearchFor);

                getByPkAction = () =>
                {
                    apiForDatabaseType.OpenConnection();

                    for (int i = 0; i < primaryKeyAndValuePerModel.Count; i++)
                    {
                        var primaryKeyAndValueOfThisModel = primaryKeyAndValuePerModel[i];

                        // We update the ISearchModel with the PK name and value of the current model, then search for that specific model. 
                        crudModelsForDatabaseType.SearchModel.IdentifiersAndValuesToSearchFor = new Dictionary<string, object>();
                        crudModelsForDatabaseType.SearchModel.IdentifiersAndValuesToSearchFor.Add(primaryKeyAndValueOfThisModel.Key,
                                                                                                  primaryKeyAndValueOfThisModel.Value);

                        apiForDatabaseType.Search(new List<ISearchModel<M>> { crudModelsForDatabaseType.SearchModel });
                    }

                    apiForDatabaseType.CloseConnection();
                };


                // Getting by Value Action
                var firstNonPkAttributePerModel = base.GetFirstNonPrimaryKeyAttributePerModel(modelsToSearchFor);
                getByValueAction = () =>
                {
                    var searchModels = new List<ISearchModel<M>>();

                    for (int i = 0; i < firstNonPkAttributePerModel.Count; i++)
                    {
                        var firstNonPkAttributeNameAndValueOfCurrModel = firstNonPkAttributePerModel[i];

                        // Again, updating the ISearchModel of the CqrsReader, but using the name/value of the first non-primary key attribute this time.
                        crudModelsForDatabaseType.SearchModel.IdentifiersAndValuesToSearchFor = new Dictionary<string, object>();
                        crudModelsForDatabaseType.SearchModel.IdentifiersAndValuesToSearchFor.Add(firstNonPkAttributeNameAndValueOfCurrModel.Key,
                                                                                                  firstNonPkAttributeNameAndValueOfCurrModel.Value);
                        searchModels.Add(crudModelsForDatabaseType.SearchModel.Clone());
                    }

                    apiForDatabaseType.OpenConnection();
                    apiForDatabaseType.Search(searchModels);
                    apiForDatabaseType.CloseConnection();
                };


                // Updating action
                var modelsWithUpdatedValues = randomModelsToInsert.Take(this.amountOfModelsToUpdate).ToList();
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

                updateAction = () => 
                {
                    apiForDatabaseType.OpenConnection();
                    apiForDatabaseType.Update(modelsWithUpdatedValues, crudModelsForDatabaseType.UpdateModel);
                    apiForDatabaseType.CloseConnection();
                };
            }

            return new ActionsToMeasure()
            {
                CreateAction = createAction,
                DeleteAction = deleteAction,
                GetByPkAction = getByPkAction,
                GetByValueAction = getByValueAction,
                RandomizeAction = randomizeAction,
                UpdateAction = updateAction,
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
