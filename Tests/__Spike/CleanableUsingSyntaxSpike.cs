using System;
using MAVLinkAPI.Tests.Util.Resource;
using Microsoft.Win32.SafeHandles;
using NUnit.Framework;

namespace MAVLinkAPI.Tests.__Spike
{
    public class CleanableUsingSyntaxSpike
    {
        [Test]
        public void Fake()
        {
            var i1 = HandleExample.Counter;

            // using (var obj = new Fake(10, true))
            // using (var obj = new Fake(0, true))
            // using (var obj = new Fake(-1, true)) // won't work
            using (var obj = new HandleExample(true))
            {
                Assert.AreEqual(i1, HandleExample.Counter);
                // do things
            }

            Assert.AreEqual(i1 + 1, HandleExample.Counter);
        }

        [Test]
        public void Real()
        {
            var i1 = CleanableExample.Counter;
            using (var obj = new CleanableExample())
            {
                Assert.AreEqual(i1, CleanableExample.Counter);
                // do things
            }

            Assert.AreEqual(i1 + 1, CleanableExample.Counter);
        }
    }

    public class HandleExample : SafeHandleMinusOneIsInvalid
    {
        public static volatile int Counter;

        public HandleExample(nint handle, bool ownsHandle) : base(ownsHandle)
        {
            SetHandle(handle);
        }

        public HandleExample(bool ownsHandle) : base(ownsHandle)
        {
            var i = new IntPtr(0);

            SetHandle(i);
        }

        protected override bool ReleaseHandle()
        {
            Counter += 1;
            return true;
        }
    }
}