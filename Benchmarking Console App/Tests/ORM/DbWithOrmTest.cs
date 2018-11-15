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

        public override TestReport Test<M>(IDatabaseType databaseType, bool scaled)
        {
            double timeSpentInsertingModels = 0;
            double timeSpentGettingAllModels = 0;
            double timeSpentUpdatingModels = 0;
            double timeSpentDeletingModels = 0;

            var databaseTypeEnum = databaseType.ToEnum();

            if (!(databaseTypeEnum.Equals(EDatabaseType.PostgreSQL) || databaseTypeEnum.Equals(EDatabaseType.MySQL)))
            {
                throw new Exception("ORM is only supported for SQL databases.");
            }

            var connectionStringForDatabase =
                DatabaseConnectionStringFactory.GetDatabaseConnectionString(databaseType.ToEnum());
            var ormApiForDatabase = new SqlOrmDatabaseApi(connectionStringForDatabase);

            var randomizedStartingModels = base.GetRandomModels<M>(amountOfModelsToCreate)
                .ToList();


            // Truncating all first in order to start fresh
            ormApiForDatabase.Truncate<M>();


            // Testing inserting
            timeSpentInsertingModels = base.GetTimeSpentOnActionMs(() =>
            {
                ormApiForDatabase.Create(randomizedStartingModels);
            });


            // Testing getting all
            timeSpentGettingAllModels = base.GetTimeSpentOnActionMs(() => { ormApiForDatabase.GetAll<M>(); });


            // Testing getting by primary key TODO
            // Testing getting by content TODO


            // Testing deleting all. This is done BEFORE updating etc 
            // so we wont have to keep track of changes to the original models
            timeSpentDeletingModels = base.GetTimeSpentOnActionMs(() =>
            {
                ormApiForDatabase.Delete(randomizedStartingModels);
            });


            // Testing updating 
            var random = new Random();
            foreach (var model in randomizedStartingModels)
            {
                // re-randomizing so that the ORM will detect a change
                model.RandomizeValuesExceptPrimaryKey(random);
            }

            timeSpentUpdatingModels = base.GetTimeSpentOnActionMs(() =>
            {
                ormApiForDatabase.Update(randomizedStartingModels);
            });



            // TODO: DUPLICATION WITH SIMPLE DRIVER TEST
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
