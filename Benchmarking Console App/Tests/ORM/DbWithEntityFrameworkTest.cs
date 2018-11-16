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
    public class DbWithEntityFrameworkTest : AbstractPerformanceTest
    {
        public DbWithEntityFrameworkTest(int amountOfModelsToCreate, int amountOfModelsToRetrieveByPrimaryKey,
                                         int amountOfModelsToRetrieveByContent, int amountOfModelsToUpdate)
            : base(amountOfModelsToCreate, amountOfModelsToRetrieveByPrimaryKey,
                   amountOfModelsToRetrieveByContent, amountOfModelsToUpdate)
        {
        }

        protected override ActionsToMeasure GetActionsToMeasure<M>(IDatabaseType databaseType) 
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
            var getByPkAction = new Action(() =>
                ormApiForDatabase.Search(new Func<M, bool>(x => randomizedStartingModels.Contains(x))));
            var deleteAllAction = new Action(() => ormApiForDatabase.Delete(randomizedStartingModels));
            var updateAction = new Action(() => ormApiForDatabase.Update(randomizedStartingModels));

            var randomizeAction = new Action(() =>
            {
                var random = new Random();
                foreach (var model in randomizedStartingModels)
                {
                    model.RandomizeValuesExceptPrimaryKey(random);
                }
            });
            var truncateAction = new Action(() => ormApiForDatabase.Truncate<M>());


            return new ActionsToMeasure()
            {
                createAction = createAction,
                deleteAction = deleteAllAction,
                getByPkAction = getByPkAction,
                randomizeAction = randomizeAction,
                updateAction = updateAction,
                getAllAction = getAllAction,
                truncateAction = truncateAction
            };
        }
    }
}
