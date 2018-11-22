using System;
using System.Linq;
using Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_Console_App.Configurations.Databases.DatabaseTypes;
using Benchmarking_Console_App.Models.DatabaseModels;
using Benchmarking_Console_App.Testing;
using Benchmarking_program.Configurations.Databases.DatabaseApis;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;

namespace Benchmarking_Console_App.Tests.ORM
{
    public class DbWithEntityFrameworkTest : AbstractPerformanceTest
    {
        protected override ActionsToMeasure GetActionsToMeasure<M>(IDatabaseType databaseType, bool wipeExistingDatabase) 
        {
            var databaseTypeEnum = databaseType.ToEnum();
            if (!(databaseTypeEnum.Equals(EDatabaseType.PostgreSQL) || databaseTypeEnum.Equals(EDatabaseType.MySQL)))
            {
                throw new Exception("ORM is only supported for SQL databases.");
            }

            var connectionStringForDatabase = DatabaseConnectionStringFactory.GetDatabaseConnectionString(databaseType.ToEnum());
            var ormApiForDatabase = new SqlEntityFrameworkOrmDatabaseApi(connectionStringForDatabase);
            var randomizedStartingModels = base.GetRandomModels<MinuteAveragesRow>(amountOfModelsToCreate)
                                               .ToList();


            var createAction = new Action(() =>
            {
                ormApiForDatabase.Create(randomizedStartingModels);
            }); 

            var getAllAction = new Action(() =>
            {
                ormApiForDatabase.GetAll<M>();
            });

            var getByPkAction = new Action(() =>
            {
                ormApiForDatabase.Search<MinuteAveragesRow>(new Func<MinuteAveragesRow, bool>(x => randomizedStartingModels.Contains(x)));
            });

            var modelsValues = randomizedStartingModels.Select(x => x.startid).ToList();
            var getByValueAction = new Action(() =>
            {
                ormApiForDatabase.Search<MinuteAveragesRow>(new Func<MinuteAveragesRow, bool>(x => modelsValues.Contains(x.startid)));
            });

            var deleteAllAction = new Action(() =>
            {
                ormApiForDatabase.Delete(randomizedStartingModels);
            });

            var updateAction = new Action(() =>
            {
                ormApiForDatabase.Update(randomizedStartingModels);
            });

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
                ormApiForDatabase.Truncate<M>();
            });


            return new ActionsToMeasure()
            {
                CreateAction = createAction,
                DeleteAction = deleteAllAction,
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
            return "Entity Framework " + dbType.ToEnum().ToString();
        }
    }
}
