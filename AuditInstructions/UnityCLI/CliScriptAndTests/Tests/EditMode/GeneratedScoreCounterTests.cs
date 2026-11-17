using System;
using System.Reflection;
using NUnit.Framework;

namespace UnityBestPractices.Samples.Cli.Tests
{
    public sealed class GeneratedScoreCounterTests
    {
        private Type _type;
        private object _counter;

        [SetUp]
        public void SetUp()
        {
            // Reflection keeps the test assembly compilable before the first generation command.
            _type = Type.GetType("UnityBestPractices.Samples.Cli.Generated.SampleScoreCounter, Starter.Cli.Generated");
            Assert.That(_type, Is.Not.Null, "Run starter_generate_score_counter with dry_run=false and wait for compilation.");
            _counter = Activator.CreateInstance(_type);
        }

        [Test]
        public void AddScore_AccumulatesFromZero()
        {
            PropertyInfo score = _type.GetProperty("Score");
            MethodInfo add = _type.GetMethod("AddScore");
            Assert.That(score.GetValue(_counter), Is.EqualTo(0));
            add.Invoke(_counter, new object[] { 2 });
            add.Invoke(_counter, new object[] { 3 });
            Assert.That(score.GetValue(_counter), Is.EqualTo(5));
        }

        [Test]
        public void NegativeAmount_IsRejectedWithoutChangingScore()
        {
            MethodInfo add = _type.GetMethod("AddScore");
            add.Invoke(_counter, new object[] { 4 });
            TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
                () => add.Invoke(_counter, new object[] { -1 }));
            Assert.That(exception.InnerException, Is.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(_type.GetProperty("Score").GetValue(_counter), Is.EqualTo(4));
        }
    }
}
