﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HotChocolate.Utilities;
using HotChocolate.Language;
using HotChocolate.Types.Descriptors.Definitions;

namespace HotChocolate.Types.Descriptors
{
    internal class ObjectTypeDescriptor
        : DescriptorBase<ObjectTypeDefinition>
        , IObjectTypeDescriptor
    {
        public ObjectTypeDescriptor(IDescriptorContext context, Type clrType)
            : base(context)
        {
            if (clrType == null)
            {
                throw new ArgumentNullException(nameof(clrType));
            }

            Definition.ClrType = clrType;
            Definition.Name = context.Naming.GetTypeName(clrType);
            Definition.Description = context.Naming.GetTypeDescription(clrType);
        }

        public ObjectTypeDescriptor(IDescriptorContext context, NameString name)
            : base(context)
        {
            Definition.ClrType = typeof(object);
            Definition.Name = name.EnsureNotEmpty(nameof(name));
        }

        protected override ObjectTypeDefinition Definition { get; } =
            new ObjectTypeDefinition();

        protected ICollection<ObjectFieldDescriptor> Fields { get; } =
            new List<ObjectFieldDescriptor>();

        protected ICollection<Type> ResolverTypes { get; } =
            new HashSet<Type>();

        protected override void OnCreateDefinition(
            ObjectTypeDefinition definition)
        {
            var fields = new Dictionary<NameString, ObjectFieldDefinition>();
            var handledMembers = new HashSet<MemberInfo>();

            foreach (ObjectFieldDefinition fieldDefinition in
                Fields.Select(t => t.CreateDefinition()))
            {
                if (!fieldDefinition.Ignore)
                {
                    fields[fieldDefinition.Name] = fieldDefinition;
                }

                if (fieldDefinition.Member != null)
                {
                    handledMembers.Add(fieldDefinition.Member);
                }
            }

            OnCompleteFields(fields, handledMembers);

            Definition.Fields.AddRange(fields.Values);
        }

        protected virtual void OnCompleteFields(
            IDictionary<NameString, ObjectFieldDefinition> fields,
            ISet<MemberInfo> handledMembers)
        {
            DiscoverResolvers(fields);
        }
        protected void DiscoverResolvers(
            IDictionary<NameString, ObjectFieldDefinition> fields)
        {
            var processed = new HashSet<string>();

            if (Definition.ClrType != typeof(object))
            {
                foreach (Type resolverType in Context.Inspector
                    .GetResolverTypes(Definition.ClrType))
                {
                    ResolverTypes.Add(resolverType);
                }
            }

            foreach (Type resolverType in ResolverTypes)
            {
                AddResolvers(
                    fields,
                    processed,
                    Definition.ClrType ?? typeof(object),
                    resolverType);
            }
        }

        private void AddResolvers(
            IDictionary<NameString, ObjectFieldDefinition> fields,
            ISet<string> processed,
            Type sourceType,
            Type resolverType)
        {
            foreach (MemberInfo member in Context.Inspector
                .GetMembers(resolverType))
            {
                if (IsResolverRelevant(sourceType, member))
                {
                    var fieldDefinition = ObjectFieldDescriptor
                        .New(Context, member, resolverType)
                        .CreateDefinition();

                    if (processed.Add(fieldDefinition.Name))
                    {
                        fields[fieldDefinition.Name] = fieldDefinition;
                    }
                }
            }
        }

        private static bool IsResolverRelevant(
            Type sourceType,
            MemberInfo resolver)
        {
            if (resolver is PropertyInfo)
            {
                return true;
            }

            if (resolver is MethodInfo m)
            {
                ParameterInfo parent = m.GetParameters()
                    .FirstOrDefault(t => t.IsDefined(typeof(ParentAttribute)));
                return parent == null
                    || parent.ParameterType.IsAssignableFrom(sourceType);
            }

            return false;
        }

        public IObjectTypeDescriptor SyntaxNode(
            ObjectTypeDefinitionNode objectTypeDefinitionNode)
        {
            Definition.SyntaxNode = objectTypeDefinitionNode;
            return this;
        }

        public IObjectTypeDescriptor Name(NameString value)
        {
            Definition.Name = value.EnsureNotEmpty(nameof(value));
            return this;
        }

        public IObjectTypeDescriptor Description(string value)
        {
            Definition.Description = value;
            return this;
        }

        public IObjectTypeDescriptor Interface<TInterface>()
            where TInterface : InterfaceType
        {
            if (typeof(TInterface) == typeof(InterfaceType))
            {
                // TODO : resources
                throw new ArgumentException(
                    "The interface type has to be inherited.");
            }

            Definition.Interfaces.Add(typeof(TInterface).GetOutputType());
            return this;
        }

        public IObjectTypeDescriptor Interface<TInterface>(
            TInterface interfaceType)
            where TInterface : InterfaceType
        {
            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            Definition.Interfaces.Add(new SchemaTypeReference(interfaceType));
            return this;
        }

        public IObjectTypeDescriptor Interface(
            NamedTypeNode namedTypeNode)
        {
            if (namedTypeNode == null)
            {
                throw new ArgumentNullException(nameof(namedTypeNode));
            }

            Definition.Interfaces.Add(new SyntaxTypeReference(
                namedTypeNode, TypeContext.Output));
            return this;
        }

        public IObjectTypeDescriptor Include<TResolver>()
        {
            if (typeof(IType).IsAssignableFrom(typeof(TResolver)))
            {
                // TODO : resources
                throw new ArgumentException(
                    "Schema types cannot be used as resolver types.");
            }

            ResolverTypes.Add(typeof(TResolver));
            return this;
        }

        public IObjectTypeDescriptor IsOfType(IsOfType isOfType)
        {
            Definition.IsOfType = isOfType
                ?? throw new ArgumentNullException(nameof(isOfType));
            return this;
        }

        public IObjectFieldDescriptor Field(NameString name)
        {
            var fieldDescriptor = new ObjectFieldDescriptor(Context, name);
            Fields.Add(fieldDescriptor);
            return fieldDescriptor;
        }

        public IObjectFieldDescriptor Field<TResolver>(
            Expression<Func<TResolver, object>> propertyOrMethod)
        {
            if (propertyOrMethod == null)
            {
                throw new ArgumentNullException(nameof(propertyOrMethod));
            }

            MemberInfo member = propertyOrMethod.ExtractMember();
            if (member is PropertyInfo || member is MethodInfo)
            {
                var fieldDescriptor = new ObjectFieldDescriptor(
                    Context, member, typeof(TResolver));
                Fields.Add(fieldDescriptor);
                return fieldDescriptor;
            }

            // TODO : resources
            throw new ArgumentException(
                "A field of an entity can only be a property or a method.",
                nameof(member));
        }

        public IObjectTypeDescriptor Directive<T>(T directive)
            where T : class
        {
            Definition.AddDirective(directive);
            return this;
        }

        public IObjectTypeDescriptor Directive<T>()
            where T : class, new()
        {
            Definition.AddDirective(new T());
            return this;
        }

        public IObjectTypeDescriptor Directive(
            NameString name,
            params ArgumentNode[] arguments)
        {
            Definition.AddDirective(name, arguments);
            return this;
        }
    }
}
