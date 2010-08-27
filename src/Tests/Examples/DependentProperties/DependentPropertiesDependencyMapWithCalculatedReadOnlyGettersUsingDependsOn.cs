using System.ComponentModel;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Tests.Util;
using Container = StructureMap.Container;

namespace Tests.Examples.DependentProperties
{
    [TestFixture]
    public class DependentPropertiesDependencyMapWithCalculatedReadOnlyGettersUsingDependsOn
    {
        [Test]
        public void UsingDependentPropertiesUpdatesRelatedWhenUsingDependsOnInDependencyMap()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With(new AutoNotifyAttrConvention());
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var project = container.GetInstance<Project>();
            var projectTracker = new EventTracker<PropertyChangedEventHandler>();

            (project as INotifyPropertyChanged).PropertyChanged += projectTracker;
            project.Files = new[] { "src1", "src2" };

            Assert.That(projectTracker.CallCount, Is.EqualTo(2)); // one for Files, one for FileCount
        }

        [AutoNotify(DependencyMap = typeof(ProjectDependency))]
        public class Project
        {
            public virtual string Name { get; set; }
            public virtual string[] Files { get; set; }
            public virtual int FileCount { get { return Files.Length; } }
        }

        class ProjectDependency : DependencyMap<Project>
        {
            public ProjectDependency()
            {
                Property(x => x.FileCount).DependsOn(x => x.Files);
            }
        }
    }
}