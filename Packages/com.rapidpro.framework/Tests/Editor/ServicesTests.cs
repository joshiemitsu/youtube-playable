using System;
using NUnit.Framework;

namespace RapidPro.Core.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="Services"/>.
    /// </summary>
    public class ServicesTests
    {
        private interface IFakeService { }

        private class FakeService : IFakeService { }

        [SetUp]
        public void SetUp()
        {
            Services.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            Services.Clear();
        }

        [Test]
        public void Get_AfterRegister_ReturnsSameInstance()
        {
            var service = new FakeService();

            Services.Register<IFakeService>(service);

            Assert.AreSame(service, Services.Get<IFakeService>());
        }

        [Test]
        public void TryGet_WhenMissing_ReturnsFalseAndNull()
        {
            bool found = Services.TryGet(out IFakeService service);

            Assert.IsFalse(found);
            Assert.IsNull(service);
        }

        [Test]
        public void Get_WhenMissing_ThrowsWithTypeName()
        {
            var exception = Assert.Throws<InvalidOperationException>(() => Services.Get<IFakeService>());

            StringAssert.Contains(nameof(IFakeService), exception.Message);
        }

        [Test]
        public void Unregister_RemovesService()
        {
            Services.Register<IFakeService>(new FakeService());

            Services.Unregister<IFakeService>();

            Assert.IsFalse(Services.TryGet(out IFakeService _));
        }

        [Test]
        public void Clear_RemovesAllServices()
        {
            Services.Register<IFakeService>(new FakeService());

            Services.Clear();

            Assert.IsFalse(Services.TryGet(out IFakeService _));
        }
    }
}
