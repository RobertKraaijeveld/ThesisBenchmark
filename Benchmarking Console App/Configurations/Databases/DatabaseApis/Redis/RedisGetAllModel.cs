﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Benchmarking_Console_App.Configurations.Databases.Interfaces;
using Benchmarking_program.Models.DatabaseModels;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseApis.MongoDB
{
    public class RedisGetAllModel<M> : IGetAllModel<M> where M : IModel, new()
    {
        public string CreateGetAllString()
        {
            return $"KEYS *";
        }
    }
}