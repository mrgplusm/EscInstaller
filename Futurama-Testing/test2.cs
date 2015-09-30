using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Futurama_Testing
{
    /// <summary>
    ///This is a test class for LibraryDataTest and is intended
    ///to contain all LibraryDataTest Unit Tests
    ///</summary>
    [TestClass]
    public class LibraryDataTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }


        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class

        /*

            [ClassInitialize]
            public static void MyClassInitialize(TestContext testContext)
            {


                _dispatcher = new Dispatcher();
                _dispatcher.Connect("COM12", Assert.Fail);
                var waitHandle = new AutoResetEvent(false);

                _dispatcher.AddData(new InstallationMode(true)
                {
                    Received = (list, o) =>
                    {
                        if (list == null) Assert.Fail("no return data");
                        Assert.AreEqual(list[4], McuDat.InstallModeRuns);
                        waitHandle.Set();
                    }
                });

                if (!waitHandle.WaitOne(5000, true))
                {
                    Assert.Fail("Connection timed out");
                }

                _dispatcher.AddData(new Password("5", false)
                {
                    Received = (list, o) =>
                    {
                        if (list == null) Assert.Fail("no return data");
                        Assert.AreEqual(list[4], McuDat.DataOk);
                        waitHandle.Set();
                    }
                });

                if (!waitHandle.WaitOne(5000, true))
                {
                    Assert.Fail("password response timed out");
                }
            }
            //
            //Use ClassCleanup to run code after all tests in a class have run
            [ClassCleanup]
            public static void MyClassCleanup()
            {
            }
            //
            //Use TestInitialize to run code before running each test
            //[TestInitialize()]
            //public void MyTestInitialize()
            //{
            //}
            //
            //Use TestCleanup to run code after each test has run
            //[TestCleanup()]
            //public void MyTestCleanup()
            //{
            //}
            //
            #endregion


            [TestMethod]
            public void RouteToAuxDemux()
            {
                var padloc = new object();

                for (int i = 0; i < 12 * 32; i += 24)
                {
                    _dispatcher.AddData(new AuxDemux(i % 2 > 0, i)
                    {
                        Received = (list, o) =>
                        {
                            lock (padloc)
                            {
                                _counter++;
                                Assert.IsNotNull(list);
                            }
                        }
                    });

                }
                while (_counter < (16))
                {
                    Thread.Sleep(500);
                }

                Assert.IsTrue(_counter == 16);

            }




            [TestMethod]
            public void RedundantThrowAwayData()
            {

                var waitHandle = new AutoResetEvent(false);

                for (int i = 0; i < 100; i++)
                {
                    int i1 = i;
                    _dispatcher.AddData(new NameUpdate(0, "lala" + i, NameType.Output) 
                    {AdditionalData = i, Received = (list, o)  =>
                            {
                                if(list == null) Assert.Fail("NO returndata for repetition {0}, nameupdate",i1 );
                                if (int.Parse(o.ToString()) == 99) waitHandle.Set();
                            }});
                }

                if (!waitHandle.WaitOne(5000, false))
                {
                    Assert.Fail("vuReceive timed out");
                }

                for (int i = 0; i < 5; i++)
                {
                    for (int j = -80; j < 7; j++)
                    {
                        int i1 = i;
                        _dispatcher.AddData(new GainSlider(0, j, SliderType.Input)
                        {
                            AdditionalData = i,
                            Received = (list, o) =>
                                {
                                    if (list == null) Assert.Fail("NO returndata for repetition {0}, sliderupdate", i1);
                                    if (int.Parse(o.ToString()) == 6) waitHandle.Set();
                                }
                        }
                            );
                    }

                }
                if (!waitHandle.WaitOne(5000, false))
                {
                    Assert.Fail("vuReceive timed out");
                }
            }

            [TestMethod]
            public void PeqAddParams()
            {
                var peqModel = new SpeakerLibraryViewModel(LibraryData.DesignDataModel, _dispatcher)
                                   {

                                   };

                for (int i = 0; i < 30; i++)
                {
                    peqModel.AddNewParam.Execute(null);
                    peqModel.AddNewParam.Execute(null);
                    peqModel.AddNewParam.Execute(null);
                    peqModel.AddNewParam.Execute(null);
                    peqModel.ClearParams.Execute(null);
                }

                Assert.AreEqual(1, peqModel.SpeakerPeqs.PeqDataViewModels.Count);
            }

            [TestMethod]
            public void InputNameString()
            {
                var ipnm = new InputNameViewModel(LibraryData.DesignDataModel, _dispatcher) { NameOfInput = "test123" };
            }

            [TestMethod]
            public void InputNameVu()
            {
                var waitHandle = new AutoResetEvent(false);
                var inputNameViewModel = new InputNameViewModel(LibraryData.DesignDataModel, _dispatcher);


                inputNameViewModel.VuValueChanged += () => waitHandle.Set();

                inputNameViewModel.ActivateVu = true;

                if (!waitHandle.WaitOne(56000, false))
                {
                    Assert.Fail("vuReceive timed out");
                }
                inputNameViewModel.ActivateVu = false;
            }



            private int _counter;

            [TestMethod]
            public void MuteBlocks()
            {
                var padloc = new object();

                for (int i = 0; i < 12 * 32; i += 24)
                {
                    _dispatcher.AddData(new MuteBlock(i, i % 2 > 0)
                    {
                        Received = (list, o) =>
                                       {
                                           lock (padloc)
                                           {
                                               _counter++;
                                               Assert.IsNotNull(list);
                                           }
                                       }
                    });

                }
                while (_counter < (16))
                {
                    Thread.Sleep(500);
                }

                Assert.IsTrue(_counter == 16);

            }

            [TestMethod]
            public void VuMeterModule()
            {
                _counter = 0;
                var locker = new object();
                for (int i = 0; i < 12 * 32; i += 24)
                {
                    _dispatcher.AddData(new GetVU(i)
                    {
                        Received = (list, o) =>
                        {
                            lock (locker)
                            {
                                _counter++;
                                Assert.IsNotNull(list);
                            }
                        }
                    });
                }
                while (_counter < (16))
                {
                    Thread.Sleep(500);
                }

                Assert.IsTrue(_counter == 16);
            }


        } */
    }
}
