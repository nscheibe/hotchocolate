using ChilliCream.Testing;
using Snapshooter.Xunit;
using Xunit;
using HotChocolate.Language;

namespace HotChocolate.Stitching.Merge
{
    public class SchemaMergerTests
    {
        [Fact]
        public void MergeSimpleSchemaWithDefaultHandler()
        {
            // arrange
            DocumentNode schema_a =
                Parser.Default.Parse("union Foo = Bar | Baz union A = B | C");
            DocumentNode schema_b =
                Parser.Default.Parse("union Foo = Bar | Baz");

            // act
            DocumentNode schema = SchemaMerger.New()
                .AddSchema("A", schema_a)
                .AddSchema("B", schema_b)
                .Merge();

            // assert
            SchemaSyntaxSerializer.Serialize(schema).MatchSnapshot();
        }

        [Fact]
        public void MergeDemoSchemaWithDefaultHandler()
        {
            // arrange
            DocumentNode schema_a =
                Parser.Default.Parse(
                    FileResource.Open("Contract.graphql"));
            DocumentNode schema_b =
                Parser.Default.Parse(
                    FileResource.Open("Customer.graphql"));

            // act
            DocumentNode schema = SchemaMerger.New()
                .AddSchema("A", schema_a)
                .AddSchema("B", schema_b)
                .Merge();

            // assert
            SchemaSyntaxSerializer.Serialize(schema).MatchSnapshot();
        }
    }
}
