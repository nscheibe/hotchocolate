﻿using System;
using System.Collections.Generic;
using System.Reflection;
using HotChocolate.Configuration;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors.Definitions;

namespace HotChocolate
{
    public delegate DocumentNode LoadSchemaDocument(IServiceProvider services);

    public delegate INamedType CreateNamedType(IServiceProvider services);


    public interface ITypeSystemObject
        : Types.IHasName
        , ITypeSystemObjectEvents
    {

    }

    public interface ITypeSystemObjectEvents
    {
        /// <summary>
        /// Register dependencies.
        /// </summary>
        void OnInitialize(IInitializationContext context);

        /// <summary>
        /// Completes the name of this object.
        /// </summary>
        void OnCompleteName();

        /// <summary>
        /// Completes the object and makes the object immutable.
        /// </summary>
        void OnCompleteObject();
    }

    public interface ITypeSystemObjectContext
    {
        ITypeSystemObject Type { get; }

        bool IsType { get; }

        bool IsIntrospectionType { get; }

        bool IsDirective { get; }

        IServiceProvider Services { get; }

        void ReportError(ISchemaError error);
    }

    public interface IInitializationContext
        : ITypeSystemObjectContext
    {
        void RegisterDependency(
            ITypeReference reference,
            TypeDependencyKind kind);

        void RegisterDependency(IDirectiveReference reference);

        void RegisterResolver(IFieldReference reference);

        void RegisterMiddleware(
            IFieldReference reference,
            IEnumerable<FieldMiddleware> components);
    }

    public interface ICompletionContext
        : ITypeSystemObjectContext
    {
        INamedType GetType(ITypeReference reference);

        IDirective GetDirective(IDirectiveReference reference);

        FieldResolver GetResolver(IFieldReference reference);

        FieldDelegate GetCompiledMiddleware(IFieldReference reference);

        IReadOnlyCollection<ObjectType> GetPossibleTypes();
    }


    public enum TypeDependencyKind
    {
        /// <summary>
        /// The dependency instance does not be completed.
        /// </summary>
        Default,

        /// <summary>
        /// The dependency instance needs to have it`s name completed.
        /// </summary>
        Named,

        /// <summary>
        /// The dependency instance needs to be fully completed.
        /// </summary>
        Completed
    }


    // TODO : work in progress new schmea builder interface
    public interface ISchemaBuilder
    {
        ISchemaBuilder Use(FieldMiddleware middleware);

        ISchemaBuilder AddDocument(
            LoadSchemaDocument loadSchemaDocument);

        // ISchemaBuilder AddType(Type type);

        // ISchemaBuilder AddRootType(Type type, OperationType operation);

        ISchemaBuilder AddType(
            CreateNamedType createNamedType);

        ISchemaBuilder AddRootType(
            CreateNamedType createNamedType,
            OperationType operation);

        ISchemaBuilder AddResolver(IFieldReference fieldReference);

        ISchemaBuilder AddBinding(object binding);

        ISchemaBuilder AddServices(IServiceProvider services);

        ISchema Create();
    }

    public interface ISchemaBuilderContext
    {

    }



    internal static class SchemaBuilderExtensions
    {
        public static ISchemaBuilder AddQueryType(
            this ISchemaBuilder builder,
            Type type)
        {
            return builder;
            // return builder.AddRootType(type, OperationType.Query);
        }

        public static ISchemaBuilder AddQueryType(
            this ISchemaBuilder builder,
            ObjectType queryType)
        {
            return builder;
            // return builder.AddRootType(queryType, OperationType.Query);
        }

        public static ISchemaBuilder AddQueryType<TQuery>(
            this ISchemaBuilder builder)
        {
            return builder;
            // return builder.AddRootType(typeof(TQuery), OperationType.Query);
        }
    }
}
