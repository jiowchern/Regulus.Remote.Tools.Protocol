using System;
using System.Reactive.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using NUnit.Framework;
using Regulus.Remote.Tools.Protocol.Sources.TestCommon.MultipleNotices;

namespace Regulus.Remote.Tools.Protocol.Sources.TestCommon.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {

           
            var multipleNotices = new MultipleNotices.MultipleNotices();

            var env = new TestEnv<Entry<IMultipleNotices>>(new Entry<IMultipleNotices>(multipleNotices));

            var n1 = new Regulus.Remote.Tools.Protocol.Sources.TestCommon.Number(1);

            multipleNotices.Numbers1.Items.Add(n1);
            multipleNotices.Numbers1.Items.Add(n1);
            multipleNotices.Numbers2.Items.Add(n1);

            var supplyn1Obs = from mn in env.Queryable.QueryNotifier<IMultipleNotices>().SupplyEvent()
                              from n in mn.Numbers1.Base.SupplyEvent()
                              select n.Value.Value;

            var supplyn2Obs = from mn in env.Queryable.QueryNotifier<IMultipleNotices>().SupplyEvent()
                              from n in mn.Numbers2.Base.SupplyEvent()
                              select n.Value.Value;

            var unsupplyn1Obs = from mn in env.Queryable.QueryNotifier<IMultipleNotices>().SupplyEvent()
                                from n in mn.Numbers1.Base.UnsupplyEvent()
                                select n.Value.Value;

            var unsupplyn2Obs = from mn in env.Queryable.QueryNotifier<IMultipleNotices>().SupplyEvent()
                                from n in mn.Numbers2.Base.UnsupplyEvent()
                                select n.Value.Value;


            var num1s =  supplyn1Obs.Buffer(2).FirstAsync().Wait();
            var num2s =  supplyn2Obs.Buffer(1).FirstAsync().Wait();


            
            NUnit.Framework.Assert.AreEqual(1, num1s[0]);
            NUnit.Framework.Assert.AreEqual(1, num1s[1]);
            NUnit.Framework.Assert.AreEqual(1, num2s[0]);




            var removeNums = new System.Collections.Generic.List<int>();

            unsupplyn1Obs.Subscribe(removeNums.Add);
            unsupplyn2Obs.Subscribe(removeNums.Add);

            var count1Obs = from mn in env.Queryable.QueryNotifier<IMultipleNotices>().SupplyEvent()
                from count in mn.GetNumber1Count().RemoteValue()
                select count;

            var count2Obs = from mn in env.Queryable.QueryNotifier<IMultipleNotices>().SupplyEvent()
                from count in mn.GetNumber2Count().RemoteValue()
                select count;

           
            
            multipleNotices.Numbers2.Items.Remove(n1);
            multipleNotices.Numbers1.Items.Remove(n1);
            multipleNotices.Numbers1.Items.Remove(n1);

            var count1 =  count1Obs.FirstAsync().Wait();
            var count2 =  count2Obs.FirstAsync().Wait();

            NUnit.Framework.Assert.AreEqual(0, count1);
            NUnit.Framework.Assert.AreEqual(0, count2);

            var ar = new Regulus.Utility.AutoPowerRegulator(new Utility.PowerRegulator());
            while (removeNums.Count < 3)
            {
                ar.Operate();
            }
            NUnit.Framework.Assert.AreEqual(1, removeNums[0]);
            NUnit.Framework.Assert.AreEqual(1, removeNums[1]);
            NUnit.Framework.Assert.AreEqual(1, removeNums[2]);


            env.Dispose();
        }
    }
}