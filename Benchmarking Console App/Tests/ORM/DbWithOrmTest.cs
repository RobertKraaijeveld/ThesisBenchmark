using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_Console_App.Configurations.Databases.DatabaseTypes;
using Benchmarking_Console_App.Testing;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Tests.ORM
{
    public class DbWithOrmTest : AbstractPerformanceTest
    {
        public DbWithOrmTest(int amountOfModelsToCreate, int amountOfModelsToRetrieveByPrimaryKey,
            int amountOfModelsToRetrieveByContent, int amountOfModelsToUpdate)
            : base(amountOfModelsToCreate, amountOfModelsToRetrieveByPrimaryKey,
                amountOfModelsToRetrieveByContent, amountOfModelsToUpdate)
        {
        }

        public override TestReport GetTestReport<M>(IDatabaseType databaseType, bool scaled)
        {
            var databaseTypeEnum = databaseType.ToEnum();
            if (!(databaseTypeEnum.Equals(EDatabaseType.PostgreSQL) || databaseTypeEnum.Equals(EDatabaseType.MySQL)))
            {
                throw new Exception("ORM is only supported for SQL databases.");
            }

            var connectionStringForDatabase = DatabaseConnectionStringFactory.GetDatabaseConnectionString(databaseType.ToEnum());
            var ormApiForDatabase = new SqlEntityFrameworkOrmDatabaseApi(connectionStringForDatabase);
            var randomizedStartingModels = base.GetRandomModels<M>(amountOfModelsToCreate)
                                               .ToList();

            // Creating action lambdas
            var createAction = new Action(() => ormApiForDatabase.Create(randomizedStartingModels));
            var getAllAction = new Action(() => ormApiForDatabase.GetAll<M>());
            var deleteAllAction = new Action(() => ormApiForDatabase.Delete(randomizedStartingModels));
            var truncateAction = new Action(() => ormApiForDatabase.Truncate<M>());
            // Testing getting by primary key TODO
            // Testing getting by content TODO


            // Re-Randomizing values of models for update action
            var random = new Random();
            foreach (var model in randomizedStartingModels)
            {
                model.RandomizeValuesExceptPrimaryKey(random);
            }
            var updateAction = new Action(() => ormApiForDatabase.Update(randomizedStartingModels));

            
            // Setting values of TimeSpentInsertingModels, TimeSpentDeletingModels etc
            base.MeasureCRUDTimes(createAction, deleteAllAction, getAllAction, 
                                  () => {}, updateAction, truncateAction);


            var scaledOrNotStr = scaled ? "(scaled)" : "(unscaled)";
            return new TestReport() 
            {
                DatabaseTypeUsed = databaseType.ToEnum() + scaledOrNotStr,
                ModelTypeName = typeof(M).Name,

                TimeSpentDeletingAllModels = timeSpentDeletingModels,
                TimeSpentInsertingModels = timeSpentInsertingModels,
                TimeSpentRetrievingAllModels = timeSpentGettingAllModels,
                //TimeSpentRetrievingModelsByContent = timeSpentGettingModelsByContent,
                //TimeSpentRetrievingModelsByPrimaryKey = timeSpentGettingModelsByPk,
                TimeSpentUpdatingModels = timeSpentUpdatingModels,

                AmountOfModelsInserted = amountOfModelsToCreate,
                AmountOfModelsRetrievedByContent = amountOfModelsToRetrieveByContent,
                AmountOfModelsRetrievedByPrimaryKey = amountOfModelsToRetrieveByPrimaryKey,
                AmountOfModelsUpdated = amountOfModelsToUpdate
            };
        }
    }
}
