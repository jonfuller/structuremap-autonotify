using NUnit.Framework;
using StructureMap.AutoNotify;

namespace Tests.UnitTests
{
    [TestFixture]
    public class DependencyMapTests
    {
        [Test]
        public void CanExtractSourceMemberFromExpression()
        {
            var map = new FooMap();

            Assert.That(map.Map[0].SourcePropName, Is.EqualTo("Value"));
        }

        [Test]
        public void CanExtractTargetMemberFromExpression()
        {
            var map = new FooMap();

            Assert.That(map.Map[0].TargetPropName, Is.EqualTo("StringValue"));
        }

        [Test]
        public void CanExtractSourceMemberFromNestedPropExpression()
        {
            var map = new FooMap();

            Assert.That(map.Map[1].SourcePropName, Is.EqualTo("Bar.Value"));
        }

        class Foo
        {
            public virtual int Value{ get; set; }
            public virtual string StringValue { get { return Value.ToString(); } }

            public virtual Bar Bar { get; set; }
            public virtual int CombinedValue { get { return Value + Bar.Value; } }
        }

        class Bar
        {
            public virtual int Value { get; set; }
        }

        class FooMap : DependencyMap<Foo>
        {
            public FooMap()
            {
                Property(x => x.StringValue).DependsOn(x => x.Value);
                Property(x => x.CombinedValue).DependsOn(x => x.Bar.Value);
            }
        }
    }
}
