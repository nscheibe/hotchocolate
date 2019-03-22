using System;
using HotChocolate.Configuration;

namespace HotChocolate.Configuration
{
    public interface ISchemaConfiguration
        : ISchemaFirstConfiguration
        , ICodeFirstConfiguration
        , IMiddlewareConfiguration
    {
        ISchemaOptions Options { get; }

        void RegisterServiceProvider(IServiceProvider serviceProvider);
    }
}