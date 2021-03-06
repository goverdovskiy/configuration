﻿using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Tests.Binders
{
    [TestFixture]
    public class DefaultSettingsBinder_Tests
    {
        private DefaultSettingsBinder binder;
        private ISettingsBinderProvider provider;

        [SetUp]
        public void TestSetup()
        {
            var dummyBinder = Substitute.For<ISafeSettingsBinder<MyClass>>();
            dummyBinder.Bind(Arg.Any<ISettingsNode>()).Returns(SettingsBindingResult.Success<MyClass>(default));
            provider = Substitute.For<ISettingsBinderProvider>();
            provider.CreateFor<MyClass>().Returns(dummyBinder);

            binder = new DefaultSettingsBinder(provider);
        }

        [Test]
        public void Should_add_default_parsers_to_given_SettingsBinderProvider()
        {
            provider.Received().SetupParserFor<bool>(Arg.Any<ITypeParser>());
        }

        [Test]
        public void Bind_should_select_appropriate_binder_for_given_type()
        {
            binder.Bind<MyClass>(new ObjectNode(""));

            provider.CreateFor<MyClass>().Received().Bind(Arg.Any<ISettingsNode>());
        }

        [Test]
        public void WithParserFor_should_setup_parser()
        {
            binder.WithParserFor<MyClass>(MyClass.TryParse);

            provider.Received().SetupParserFor<MyClass>(Arg.Any<ITypeParser>());
        }

        [Test]
        public void WithParserFor_should_return_self()
        {
            binder.WithParserFor<MyClass>(MyClass.TryParse).Should().BeSameAs(binder);
        }

        [Test]
        public void WithCustomBinder_should_setup_binder()
        {
            var customBinder = Substitute.For<ISettingsBinder<int>>();
            binder.WithCustomBinder(customBinder);

            provider.Received().SetupCustomBinder(Arg.Is<SafeBinderWrapper<int>>(b => ReferenceEquals(b.Binder, customBinder)));
        }

        [Test]
        public void WithCustomBinder_should_return_self()
        {
            binder.WithCustomBinder(Substitute.For<ISettingsBinder<int>>()).Should().BeSameAs(binder);
        }

        public class MyClass
        {
            public static bool TryParse(string s, out MyClass v)
            {
                v = new MyClass();
                return true;
            }
        }
    }
}