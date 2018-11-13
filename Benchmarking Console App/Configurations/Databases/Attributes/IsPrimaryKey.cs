using System;

namespace Benchmarking_Console_App.Configurations.Databases.Attributes
{
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)
    ]
    public class IsPrimaryKey : System.Attribute
    {
    }
}
