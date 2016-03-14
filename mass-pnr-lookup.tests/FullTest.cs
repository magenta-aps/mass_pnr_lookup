using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mass_pnr_lookup.Controllers;
using mass_pnr_lookup.Models;
using mass_pnr_lookup.Queues;
using System.IO;
using CprBroker.Engine.Queues;
using CprBroker.Schemas.Part;
using CprBroker.Engine;
using CprBroker.Tests.PartInterface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace mass_pnr_lookup.tests
{
    [TestClass]
    public class FullTest
    {
        public class DummySearchDataProvider : IPartSearchListDataProvider, IPartPersonMappingDataProvider
        {
            public Version Version { get { throw new NotImplementedException(); } }

            Dictionary<string, Guid> UUIDs = new Dictionary<string, Guid>();

            public Guid? GetPersonUuid(string cprNumber)
            {
                if (!UUIDs.ContainsKey(cprNumber))
                    UUIDs[cprNumber] = Guid.NewGuid();
                return UUIDs[cprNumber];
            }

            public Guid?[] GetPersonUuidArray(string[] cprNumberArray)
            {
                return cprNumberArray.Select(c => GetPersonUuid(c)).ToArray();
            }

            public bool IsAlive() { return true; }

            public LaesResultatType[] SearchList(SoegInputType1 searchCriteria)
            {
                var name = searchCriteria.SoegObjekt.SoegAttributListe.SoegEgenskab.First().NavnStruktur;
                var cpr = searchCriteria.SoegObjekt.SoegAttributListe.SoegRegisterOplysning.First().Item as CprBorgerType;
                cpr.PersonCivilRegistrationIdentifier = "2107497164"; // CprBroker.Tests.PartInterface.Utilities.RandomCprNumber();

                return new LaesResultatType[]
                {
                    new LaesResultatType()
                    {
                        Item = new RegistreringType1()
                        {
                            AttributListe=new AttributListeType()
                            {
                                Egenskab = new EgenskabType[] {
                                    new EgenskabType(){
                                         NavnStruktur = name,
                                    }
                                },
                                RegisterOplysning = new RegisterOplysningType[] {
                                    new RegisterOplysningType() {
                                        Item = cpr
                                    }
                                }
                            }
                        }
                    }
                };
            }
        }


        TestBase testBase;

        //[ClassInitialize]
        public void ClassInitialize()
        {
            //testBase = new TestBase();
            //testBase.CreateDatabases();
            //testBase.RegisterDataProviderType<DummySearchDataProvider>();
        }

        /*
        [ClassCleanup]
        public void ClassCleanup()
        {
            testBase.DeleteDatabases();
        }*/

        [TestInitialize]
        public void TestInitialize()
        {
            testBase = new TestBase();
            testBase.CreateDatabases();
            testBase.RegisterDataProviderType<DummySearchDataProvider>();
            testBase.InsertLookups();
            mass_pnr_lookup.QueueRegistration.Register();
        }
        [TestCleanup]
        public void TestCleanup()
        {
            testBase.DeleteAllData();
            testBase.DeleteDatabases();
        }


        [TestMethod]
        public void Run()
        {
            string fileName = Guid.NewGuid().ToString();


            var userToken = "";
            var appToken = CprBroker.Utilities.Constants.BaseApplicationToken.ToString();
            CprBroker.Engine.BrokerContext.Initialize(appToken, userToken);

            using (var myContext = new BatchContext())
            {
                Batch _batch = null;
                Func<Batch> batch = () =>
                {
                    if (_batch == null)
                        _batch = myContext.Batches.Where(b => b.FileName == fileName).SingleOrDefault();
                    else
                        myContext.Entry(_batch).Reload();
                    return _batch;
                };

                var controller = new FilesController();
                var bytes = Encoding.UTF8.GetBytes(Properties.Resources.Test_Opslag);

                controller.EnqueueFile(new MemoryStream(bytes), fileName, bytes.Length, "dummyUser");

                Assert.IsNotNull(batch());

                // Extract
                var extractQueue = Queue.GetQueues<ExtractionQueue>().Single();
                var extractQueueItem = extractQueue.GetNext(1000).Where(qi0 => qi0.BatchId == batch().BatchId).SingleOrDefault();
                Assert.IsNotNull(extractQueueItem);

                var extractResult = extractQueue.Process(new BatchQueueItem[] { extractQueueItem });
                Assert.AreEqual(1, extractResult.Length);
                extractQueue.Remove(extractResult);

                // Search
                var searchQueue = Queue.GetQueues<SearchQueue>().Single();
                searchQueue.SourceUsageOrder = CprBroker.Schemas.SourceUsageOrder.LocalOnly;
                searchQueue.Impl.MaxRetry = 1;

                var searchQueueItems = searchQueue.GetNext(1000).Where(qi => batch().Lines.Select(l => l.BatchElementId).Contains(qi.BatchLineId)).ToArray();
                Assert.AreEqual(batch().Lines.Count, searchQueueItems.Length);

                var searchResult = searchQueue.Process(searchQueueItems);
                //Assert.AreEqual(batch().Lines.Count, searchResult.Length);
                searchQueue.Remove(searchResult);

                // Output generation
                var outputGenerationQueue = Queue.GetQueues<OutputGenerationQueue>().Single();
                var outputGenerationQueueItem = outputGenerationQueue.GetNext(1000).Where(qi => qi.BatchId == batch().BatchId).SingleOrDefault();
                Assert.IsNotNull(outputGenerationQueueItem);
                var outputGenerationResult = outputGenerationQueue.Process(new BatchQueueItem[] { outputGenerationQueueItem });
                Assert.AreEqual(1, outputGenerationResult.Length);
                var generated = Encoding.UTF8.GetString(batch().GeneratedContents);
                Assert.AreEqual(Properties.Resources.Test_Opslag_output, generated);

                // Notification
                var notificationQueue = Queue.GetQueues<UserNotificationQueue>().Single();
                var notificationQueueItem = notificationQueue.GetNext(1000).Where(qi => qi.BatchId == batch().BatchId).SingleOrDefault();
                Assert.IsNotNull(notificationQueueItem);
                var notificationResult = notificationQueue.Process(new BatchQueueItem[] { notificationQueueItem });
                Assert.AreEqual(1, notificationResult.Length);

            }
        }
    }
}