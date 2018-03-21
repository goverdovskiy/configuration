﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Convertions;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class JsonFileSource_Tests
    {
        private const string TestFileName = "test_JsonFileSource.json";

        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void Cleanup()
        {
            File.Delete(TestFileName);
        }

        private static void CreateTextFile(string text)
        {
            using (var file = new StreamWriter(TestFileName, false))
                file.WriteLine(text);
        }

        [Test]
        public void Should_return_null_if_file_not_exists()
        {
            new JsonFileSource(TestFileName).Get().Should().BeNull();
        }
        
        [Test]
        public void Should_parse_String_value()
        {
            CreateTextFile("{ \"StringValue\": \"string\" }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "StringValue", new RawSettings("string") }
                        }));
        }

        [Test]
        public void Should_parse_Integer_value()
        {
            CreateTextFile("{ \"IntValue\": 123 }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "IntValue", new RawSettings("123") }
                        }));
        }

        [Test]
        public void Should_parse_Double_value()
        {
            CreateTextFile("{ \"DoubleValue\": 123.321 }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "DoubleValue", new RawSettings("123,321") }
                        }));
        }

        [Test]
        public void Should_parse_Boolean_value()
        {
            CreateTextFile("{ \"BooleanValue\": true }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "BooleanValue", new RawSettings("True") }
                        }));
        }

        [Test]
        public void Should_parse_Null_value()
        {
            CreateTextFile("{ \"NullValue\": null }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "NullValue", new RawSettings(null) }
                        }));
        }

        [Test]
        public void Should_parse_Array_value()
        {
            CreateTextFile("{ \"IntArray\": [1, 2, 3] }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "IntArray", new RawSettings(new List<RawSettings>
                            {
                                new RawSettings("1"),
                                new RawSettings("2"),
                                new RawSettings("3"),
                            }) }
                        }));
        }

        [Test]
        public void Should_parse_Object_value()
        {
            CreateTextFile("{ \"Object\": { \"StringValue\": \"str\" } }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "Object", new RawSettings(
                                new Dictionary<string, RawSettings>
                                {
                                    { "StringValue", new RawSettings("str") }
                                }) }
                        }));
        }

        [Test]
        public void Should_parse_ArrayOfObjects_value()
        {
            CreateTextFile("{ \"Array\": [{ \"StringValue\": \"str\" }, { \"IntValue\": 123 }] }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "Array", new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings(new Dictionary<string, RawSettings>
                                    {
                                        { "StringValue", new RawSettings("str") }
                                    }),
                                    new RawSettings(new Dictionary<string, RawSettings>
                                    {
                                        { "IntValue", new RawSettings("123") }
                                    })
                                }) }
                        }));
        }

        [Test]
        public void Should_parse_ArrayOfNulls_value()
        {
            CreateTextFile("{ \"Array\": [null, null] }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "Array", new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings(null),
                                    new RawSettings(null)
                                }) }
                        }));
        }

        [Test]
        public void Should_parse_ArrayOfArrays_value()
        {
            CreateTextFile("{ \"Array\": [[\"s\", \"t\"], [\"r\"]] }");

            new JsonFileSource(TestFileName).Get()
                .Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            { "Array", new RawSettings(
                                new List<RawSettings>
                                {
                                    new RawSettings(new List<RawSettings>
                                    {
                                        new RawSettings("s"),
                                        new RawSettings("t"),
                                    }),
                                    new RawSettings(new List<RawSettings>
                                    {
                                        new RawSettings("r"),
                                    })
                                }) }
                        }));
        }

        [Test]
        public void Should_Observe_file()
        {
            new Action(() => Should_Observe_file_test().Should().Be(2)).ShouldPassIn(2.Seconds());
        }

        private int Should_Observe_file_test()
        {
            var val = 0;
            var jcs = new JsonFileSource(TestFileName, 300.Milliseconds());
            var sub1 = jcs.Observe().Subscribe(settings =>
            {
                val++;
                settings.Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            {"Param2", new RawSettings("set2")}
                        }));
            });

            CreateTextFile("{ \"Param2\": \"set2\" }");

            var sub2 = jcs.Observe().Subscribe(settings =>
            {
                val++;
                settings.Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            {"Param2", new RawSettings("set2")}
                        }));
            });

            Thread.Sleep(1.Seconds());

            sub1.Dispose();
            sub2.Dispose();
            return val;
        }

        [Test]
        public void Should_not_Observe_file_twice()
        {
            var sec = 1;
            new Action(() => Should_not_Observe_file_twice_test(sec).Should().Be(1)).ShouldPassIn((sec*3).Seconds());
        }

        public int Should_not_Observe_file_twice_test(int sec)
        {
            var val = 0;
            var jcs = new JsonFileSource(TestFileName, 300.Milliseconds());
            var sub = jcs.Observe().Subscribe(settings =>
            {
                val++;
                settings.Should().BeEquivalentTo(
                    new RawSettings(
                        new Dictionary<string, RawSettings>
                        {
                            {"Param1", new RawSettings("set1")}
                        }));
            });

            CreateTextFile("{ \"Param1\": \"set1\" }");
            Thread.Sleep(TimeSpan.FromSeconds(sec));

            CreateTextFile("{ \"Param1\": \"set1\" }");
            Thread.Sleep(TimeSpan.FromSeconds(sec));

            sub.Dispose();
            return val;
        }
    }
}