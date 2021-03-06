﻿using System;
using System.ComponentModel;
using Castle.DynamicProxy;
using NUnit.Framework;
using StructureMap.AutoNotify;
using Tests.Util;

namespace Tests.UnitTests
{
    [TestFixture]
    public class CanMakeNotifiableForInterface
    {
        [TestCase]
        public void ShouldReturnAnINPCForInterfacedObject()
        {
            var greeter = Notifiable.MakeForInterfaceGeneric<IGreeter>(new LolCat(), FireOptions.Always, new ProxyGenerator(), new DependencyMap());

            Assert.That(greeter, Is.InstanceOf<INotifyPropertyChanged>());
        }

        [TestCase]
        public void ShouldFireChangedWhenPropertyChangedOnMadeObject()
        {
            var greeter = Notifiable.MakeForInterfaceGeneric<IGreeter>(new LolCat(), FireOptions.Always, new ProxyGenerator(), new DependencyMap());

            var tracker = new EventTracker<PropertyChangedEventHandler>();

            (greeter as INotifyPropertyChanged).PropertyChanged += tracker;

            greeter.Greeting = "buzz off";

            Assert.That(tracker.WasCalled);
        }

        [TestCase]
        public void ShouldThrowWhenGiveNonInterface()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                Notifiable.MakeForInterfaceGeneric<LolCat>(new LolCat(), FireOptions.Always, new ProxyGenerator(), new DependencyMap());
            });
        }

        public class LolCat : IGreeter
        {
            public LolCat()
            {
                Greeting = "OHai";
            }

            public string Hello()
            {
                return Greeting;
            }

            public string Greeting { get; set; }
        }

        [AutoNotify]
        public interface IGreeter
        {
            string Hello();

            string Greeting { get; set; }
        }
    }
}