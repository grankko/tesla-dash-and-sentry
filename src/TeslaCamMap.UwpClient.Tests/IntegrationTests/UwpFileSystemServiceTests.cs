using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeslaCamMap.Lib.Model;
using TeslaCamMap.UwpClient.Services;
using Windows.ApplicationModel;
using Windows.Storage.Search;

namespace TeslaCamMap.UwpClient.Tests.IntegrationTests
{
    [TestClass]
    public class UwpFileSystemServiceTests
    {
        [TestMethod]
        public void ParseEventFilesIntegrationTest()
        {
            Task.Run(async () =>
            {
                var sut = new UwpFileSystemService();
                var files = await Package.Current.InstalledLocation.GetFilesAsync(CommonFileQuery.OrderByName);
                var result = await sut.ParseFiles(files);

                Assert.AreEqual(3, result.Count);

                // Testdata for event 2020-02-01
                var firstEvent = result[0];
                Assert.AreEqual(DateTime.Parse("2020-02-01T13:10:25"), firstEvent.Timestamp);
                Assert.IsTrue(firstEvent.Segments.Count == 2);
                // The event timestamp should be in the last segment
                Assert.IsFalse(firstEvent.Segments.First().ContainsEventTimestamp);
                Assert.IsTrue(firstEvent.Segments.Last().ContainsEventTimestamp);
                Assert.AreEqual("Solna", firstEvent.City);
                Assert.AreEqual(EventReason.UserInteractionHonk, firstEvent.Reason);

                // Testdata for event 2020-02-02
                var secondEvent = result[1];
                Assert.AreEqual(DateTime.Parse("2020-02-02T13:10:25"), secondEvent.Timestamp);
                Assert.IsTrue(secondEvent.Segments.Count == 2);
                // The event timestamp should be in the last segment
                Assert.IsFalse(secondEvent.Segments.First().ContainsEventTimestamp);
                Assert.IsTrue(secondEvent.Segments.Last().ContainsEventTimestamp);
                Assert.AreEqual("Upplands Väsby", secondEvent.City);
                Assert.AreEqual(EventReason.UserInteractionDashCamTapped, secondEvent.Reason);

                // Testdata for event 2020-02-03
                var thirdEvent = result[2];
                Assert.AreEqual(DateTime.Parse("2020-02-03T13:10:05"), thirdEvent.Timestamp);
                Assert.IsTrue(thirdEvent.Segments.Count == 2);
                // The event timestamp should be in the first segment
                Assert.IsTrue(thirdEvent.Segments.First().ContainsEventTimestamp);
                Assert.IsFalse(thirdEvent.Segments.Last().ContainsEventTimestamp);
                Assert.AreEqual("Sollentuna", thirdEvent.City);
                Assert.AreEqual(EventReason.SentryAwareObjectDetection, thirdEvent.Reason);

            }).GetAwaiter().GetResult();
        }
    }
}
