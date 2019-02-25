﻿using System;
using System.Reflection;
using HotChocolate.Language;
using HotChocolate.Utilities;

namespace HotChocolate.Types.Descriptors
{
    internal static class DescriptorHelpers
    {
        internal static T ExecuteFactory<T>(
            Func<T> descriptionFactory)
        {
            if (descriptionFactory == null)
            {
                throw new ArgumentNullException(nameof(descriptionFactory));
            }

            return descriptionFactory();
        }

        internal static void AcquireNonNullStatus(
            this FieldDescriptionBase fieldDescription,
            MemberInfo member)
        {
            if (member.IsDefined(typeof(GraphQLNonNullTypeAttribute)))
            {
                var attribute =
                    member.GetCustomAttribute<GraphQLNonNullTypeAttribute>();
                fieldDescription.IsTypeNullable = attribute.IsNullable;
                fieldDescription.IsElementTypeNullable = attribute.IsElementNullable;
            }
        }

        internal static void RewriteClrType(
            this FieldDescriptionBase fieldDescription,
            Func<Type, TypeReference> createContext)
        {
            if (fieldDescription.IsTypeNullable.HasValue
                    && fieldDescription.Type.IsClrTypeReference())
            {
                fieldDescription.Type = createContext(
                    DotNetTypeInfoFactory.Rewrite(
                        fieldDescription.Type.ClrType,
                        !fieldDescription.IsTypeNullable.Value,
                        !fieldDescription.IsElementTypeNullable.Value));
            }
        }

        public static ITypeReference SetMoreSpecificType<TDescription>(
            this TDescription description,
            Type type,
            TypeContext context)
            where TDescription : FieldDescriptionBase
        {
            throw new NotImplementedException();
        }

        public static ITypeReference SetMoreSpecificType<TDescription>(
            this TDescription description,
            ITypeNode typeNode)
            where TDescription : FieldDescriptionBase
        {
            throw new NotImplementedException();
        }
    }
}