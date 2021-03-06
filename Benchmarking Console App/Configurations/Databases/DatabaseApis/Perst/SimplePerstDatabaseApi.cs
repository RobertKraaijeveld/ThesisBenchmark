using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;
using Perst;


namespace Benchmarking_program.Configurations.Databases.DatabaseApis.SQL
{
    /// <summary>
    /// Provides basic CRUD functionality for the Perst KV-database.
    /// Each KV in the DB is: { Key: a string key, Value: A JSON representation of an IModel }
    /// </summary>
    public class SimplePerstDatabaseApi : IObjectOrientedDatabaseApi
    {
        private readonly Storage _storage;
        private readonly Database _database;

        private Dictionary<Type, MultidimensionalIndex> _multiDimensionalIndexesPerType = new Dictionary<Type, MultidimensionalIndex>();

        public SimplePerstDatabaseApi(string databaseFileName)
        {
            _storage = StorageFactory.Instance.CreateStorage();
            _storage.Open(databaseFileName, pagePoolSize: 2000); // TODO MAKE SURE THIS MATCHES DOCKER CONTAINERS

            _database = new Database(_storage, multithreaded: true);
        }


        public IEnumerable<M> GetAll<M>() where M : IModel, new()
        {
            return ExecuteDatabaseCommandsWithinTransaction(() =>
            {
                var results = new List<M>();
                var records = _database.GetRecords(typeof(M));

                foreach (var record in records)
                {
                    results.Add((M)record);
                }
                return results;
            });
        }

        public IEnumerable<M> GetByComparison<M>(M patternObject) where M : IModel, new()
        {
            return ExecuteDatabaseCommandsWithinTransaction<IEnumerable<M>>(() =>
            {
                var results = new List<M>();
                var typeOfModel = typeof(M);

                if (!_multiDimensionalIndexesPerType.ContainsKey(typeOfModel))
                {
                    // Adding a multi-dimensional index for this type if it doesn't yet exist.
                    AddMultidimensionalIndexForType<M>();
                }

                var indexForType = _multiDimensionalIndexesPerType[typeOfModel];
                var objs = indexForType.QueryByExample(patternObject).ToList();
                foreach (var obj in objs)
                {
                    results.Add((M)obj);
                }
                return results;
            });
        }

        public IEnumerable<M> GetByRange<M>(M low, M high) where M : IModel, new()
        {
            _database.BeginTransaction();
            try
            {
                var results = new List<M>();
                var typeOfModel = typeof(M);

                if (!_multiDimensionalIndexesPerType.ContainsKey(typeOfModel))
                {
                    // Adding a multi-dimensional index for this type if it doesn't yet exist.
                    AddMultidimensionalIndexForType<M>();
                }

                var indexForType = _multiDimensionalIndexesPerType[typeOfModel];
                var objs = indexForType.QueryByExample(low, high).ToList();
                foreach (var obj in objs)
                {
                    results.Add((M)obj);
                }
                return results;
            }
            catch (Exception e)
            {
                _database.RollbackTransaction();
                throw;
            }
        }

        public int Amount<M>() where M : IModel, new()
        {
            return ExecuteDatabaseCommandsWithinTransaction<int>(() => _database.CountRecords(typeof(M)));
        }

        public void Create<M>(IEnumerable<M> newModels) where M : IModel, new()
        {
            // Adding a multi-dimensional index for this type if it doesn't yet exist.
            AddMultidimensionalIndexForType<M>();

            // Then, adding the new models to the DB and updating their index.
            ExecuteDatabaseCommandsWithinTransaction<object>(() =>
            {
                var modelType = typeof(M);
                var multiDimensionalIndexForModelType = _multiDimensionalIndexesPerType[modelType];

                foreach (var model in newModels)
                {
                    multiDimensionalIndexForModelType.Add(model);
                    _database.AddRecord(modelType, model);
                }
                return null; // Have to return 'something' from func, even if return value isnt used
            });
        }

        public void Update<M>(IEnumerable<M> modelsWithNewValues) where M : IModel, new()
        {
            var modelType = typeof(M);

            // Adding the new models to the DB and updating their index. The new models will simply replace the old.
            ExecuteDatabaseCommandsWithinTransaction<object>(() =>
            {
                var multiDimensionalIndexForModelType = _multiDimensionalIndexesPerType[modelType];

                foreach (var model in modelsWithNewValues)
                {
                    _database.AddRecord(modelType, model);
                    multiDimensionalIndexForModelType.Add(model);
                }

                return null; // Have to return 'something' from func, even if return value isnt used 
            });
        }


        public void Delete<M>(IEnumerable<M> modelsToDelete) where M : IModel, new()
        {
            ExecuteDatabaseCommandsWithinTransaction<object>(() =>
            {
                var modelType = typeof(M);

                foreach (var model in modelsToDelete)
                {
                    _database.DeleteRecord(modelType, model);
                }
                return null;
            });
        }

        public void DeleteAll()
        {
            ExecuteDatabaseCommandsWithinTransaction<object>(() =>
            {
                var types = _multiDimensionalIndexesPerType.Keys.ToList();
                foreach (var type in types)
                {
                    _database.DropTable(type);
                    _multiDimensionalIndexesPerType.Remove(type);
                }
                return null;
            });
        }


        private void AddMultidimensionalIndexForType<M>() where M : IModel, new()
        {
            var modelType = typeof(M);
            var publicFieldsOfModel = modelType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            string[] searchableFields = publicFieldsOfModel.Select(x => x.Name)
                                                           .ToArray();

            var index = _storage.CreateMultidimensionalIndex(modelType, searchableFields, treateZeroAsUndefinedValue: true);

            if (!_multiDimensionalIndexesPerType.ContainsKey(modelType))
            {
                _multiDimensionalIndexesPerType.Add(modelType, index);
            }
        }

        private T ExecuteDatabaseCommandsWithinTransaction<T>(Func<T> actionToExecute)
        {
            _database.BeginTransaction();
            try
            {
                return actionToExecute.Invoke();
            }
            catch (Exception e)
            {
                _database.RollbackTransaction();
                throw e;
            }
        }
    }
}
