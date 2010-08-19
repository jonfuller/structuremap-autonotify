﻿using System.ComponentModel;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Container = StructureMap.Container;

namespace Tests
{
    [TestFixture]
    public class DependentProperties
    {
        [Test]
        public void UsingDependentPropertiesUpdatesRelatedProps()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With<AutoNotifyScanner>();
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var book = container.GetInstance<Book>();
            var bookTracker = new EventTracker<PropertyChangedEventHandler>();

            (book as INotifyPropertyChanged).PropertyChanged += bookTracker;
            book.Authors = new[] { "Stephen King", "Jon Fuller" };

            Assert.That(book.AuthorCount, Is.EqualTo(2));
        }

        [Test]
        public void UsingDependentPropertiesFiresNotifyForAllRelatedProps()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With<AutoNotifyScanner>();
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var book = container.GetInstance<Book>();
            var bookTracker = new EventTracker<PropertyChangedEventHandler>();

            (book as INotifyPropertyChanged).PropertyChanged += bookTracker;
            book.Authors = new[] { "Stephen King", "Jon Fuller" };

            Assert.That(bookTracker.CallCount, Is.EqualTo(2));
        }

        [Test]
        public void UsingDependentPropertiesUpdatesRelatedWhenChangedOnFireOnlyChange()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With<AutoNotifyScanner>();
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var contact = container.GetInstance<Contact>();
            var bookTracker = new EventTracker<PropertyChangedEventHandler>();

            contact.FirstName = "Jon";
            contact.LastName = "Fuller";
            (contact as INotifyPropertyChanged).PropertyChanged += bookTracker;
            contact.FirstName = "Jonathon";

            Assert.That(contact.Name, Is.EqualTo("Jonathon Fuller"));
        }

        [Test]
        public void UsingDependentPropertiesUpdatesRelatedWhenUsingDependsOnInDependencyMap()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With<AutoNotifyScanner>();
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var project = container.GetInstance<Project>();
            var projectTracker = new EventTracker<PropertyChangedEventHandler>();

            (project as INotifyPropertyChanged).PropertyChanged += projectTracker;
            project.Files = new[]{"src1", "src2"};

            Assert.That(projectTracker.CallCount, Is.EqualTo(2)); // one for Files, one for FileCount
        }

        [Test]
        public void UsingDependentPropertiesUpdatesRelatedWhenUsingDependsOnAttribute()
        {
            var container = new Container(config => config.Scan(scanConfig =>
            {
                scanConfig.With<AutoNotifyScanner>();
                scanConfig.TheCallingAssembly();
                scanConfig.WithDefaultConventions();
            }));

            var account = container.GetInstance<Account>();
            var projectTracker = new EventTracker<PropertyChangedEventHandler>();

            (account as INotifyPropertyChanged).PropertyChanged += projectTracker;
            account.AccountName = "big customer";
            account.ClientId = "1234";

            Assert.That(projectTracker.CallCount, Is.EqualTo(4));
        }
    }

    [AutoNotify(DependencyMap = typeof(BookDependency))]
    public class Book
    {
        public virtual string[] Authors { get; set; }
        public virtual string Title { get; set; }
        public virtual int AuthorCount { get; private set; }
    }

    public class BookDependency : DependencyMap<Book>
    {
        public BookDependency()
        {
            Property(x => x.Authors).Updates(x => x.AuthorCount).With(x => x.Authors.Length);
        }
    }

    [AutoNotify(Fire = FireOptions.OnlyOnChange, DependencyMap = typeof(ContactDependency))]
    public class Contact
    {
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Name { get; private set; }
    }

    public class ContactDependency : DependencyMap<Contact>
    {
        public ContactDependency()
        {
            Property(x => x.FirstName).Updates(x => x.Name).With(UpdateName);
            Property(x => x.LastName).Updates(x => x.Name).With(UpdateName);
        }

        public string UpdateName(Contact contact)
        {
            return contact.FirstName + " " + contact.LastName;
        }
    }

    [AutoNotify]
    public class Account
    {
        public virtual string AccountName { get; set; }
        public virtual string ClientId { get; set; }

        [DependsOn("AccountName", "ClientId")]
        public virtual string AccountId
        {
            get { return ClientId + "/" + AccountName; }
        }
    }

    [AutoNotify(DependencyMap = typeof(ProjectDependency))]
    public class Project
    {
        public virtual string Name { get; set; }
        public virtual string[] Files { get; set; }
        public virtual int FileCount { get { return Files.Length; } }
    }

    public class ProjectDependency : DependencyMap<Project>
    {
        public ProjectDependency()
        {
            Property(x => x.FileCount).DependsOn(x => x.Files);
        }
    }
}