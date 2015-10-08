
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common.Model;
using EscInstaller.ViewModel;
using Common;
using Common.Commodules;
using EscInstaller.ViewModel.Settings.Peq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Futurama_Testing
{
    [TestClass]
    public class InstallerTests
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            LibraryData.CreateEmptySystem();
        }

        [TestMethod]
        public void BackupFile()
        {
            LibraryData.CreateEmptySystem();
            SystemFileBackup u = new SystemFileBackup();
            u.Save();
        }

        [TestMethod]
        public void SaveOpenFile()
        {
            const string file = @"C:\Users\asuspc\Documents\q.esc";

            LibraryData.FuturamaSys.MainUnits.First().ExpansionCards = 3;
            FileManagement.SaveSystemFile(LibraryData.FuturamaSys, file);
            var e = FileManagement.OpenSystemFile(file);
            Assert.AreEqual(LibraryData.FuturamaSys.MainUnits.First().ExpansionCards, e.MainUnits.First().ExpansionCards);
        }

        [TestMethod]
        public void ButtonFileProgrammingTest()
        {
            var x = new GetMessageSelection(0);

            foreach (var result in x.CommandValue.Select(n => n.ToString("x2")))
            {
                Debug.Write(result);
                Debug.Write(' ');
            }


        }

        [TestMethod]
        public void SetEeprom()
        {
            var z = new SetE2PromExt(0, new byte[] { 0x01, 0x02, 0x03, 0x04 }, 34816);
            Console.WriteLine(z.ToString());
        }

        [TestMethod]
        public void RedundancyCheck()
        {

            foreach (var q in TestSpeaker().PEQ)
            {                
                var z = new FilterBase(q);
                var data = z.RedundancyToBytes();

                var conv = new PeqDataModel();
                var logic = new FilterBase(conv);
                logic.Parse(data);
                

                Assert.AreEqual(q.Frequency, conv.Frequency, .5);
                Assert.AreEqual(q.Boost, conv.Boost, .5);
                Assert.AreEqual(q.Gain, conv.Gain, .5);
                Assert.AreEqual(q.FilterType, conv.FilterType);
                Assert.AreEqual(q.IsEnabled, conv.IsEnabled);
                Assert.AreEqual(q.BandWidth, conv.BandWidth, .5);
                Assert.AreEqual(q.Order, conv.Order);

                CollectionAssert.AreEqual(q.Biquads.ToArray(), conv.Biquads.ToArray());
            }
        }

        
        [TestMethod]
        public void TestRecentFiles()
        {
            var t = new MainViewModel();
            var q = new RecentFilesLogic(t);

            const int itemsToTest = 10;

            for (int i = 0; i < itemsToTest; i++)
            {
                q.AddFile(i.ToString("N0"));
            }

            var expected = Enumerable.Range(itemsToTest - RecentFilesLogic.MaxRecentFiles, RecentFilesLogic.MaxRecentFiles)
                .Select(n => n.ToString("N0")).ToList();            

            CollectionAssert.AreEqual(EscInstaller.Properties.Settings.Default.RecentFiles.Cast<string>().ToArray(), expected);

            //what if last file is opened twice
            q.AddFile((itemsToTest-1).ToString("N0"));
          
            CollectionAssert.AreEqual(EscInstaller.Properties.Settings.Default.RecentFiles.Cast<string>().ToArray(), 
                expected);
        }


        ///
        /// every 4 bytes, skip 2 bytes address space. Emulate eeprom vs dsp command
        private static List<byte> SkipAdress(IEnumerable<byte> data1)
        {
            var data = new List<byte>(data1);
            var templist = new List<byte>();

            for (int i = 0; i < data.Count(); i += 6)
            {
                templist.AddRange(data.Skip(i + 2).Take(4));
            }
            return templist;
        }

        [TestMethod]
        public void DemuxMux()
        {
            var muz = DspCoefficients.DemuxBytes(555, 3, 2).ToArray();
            var demux = DspCoefficients.DemuxSelected(SkipAdress(muz));
            Assert.AreEqual(2, demux);

            muz = DspCoefficients.DemuxBytes(1000, 10, 9).ToArray();
            var q = SkipAdress(muz);
            demux = DspCoefficients.DemuxSelected(q);
            Assert.AreEqual(9, demux);

            muz = DspCoefficients.DemuxBytes(30, 2, 1).ToArray();
            demux = DspCoefficients.DemuxSelected(SkipAdress(muz));
            Assert.AreEqual(1, demux);
        }

        [TestMethod]
        public void TestCompressor2()
        {
            var values = new List<double>(Enumerable.Range(0, 33).Select(n => (double)n*3));
            var lst = new List<SetCompressor>();
            for (var i = 0; i < 7; i++)
            {
                lst.Add(new SetCompressor(502, i, values));
            }

            foreach (var setCompressor in lst)
            {
                Console.WriteLine(setCompressor.ToString());    
            }
            
        }
         


        [TestMethod]
        public void SendPresetTest()
        {
            //todo:fix this
            var speakerModel = new SpeakerDataModel { SpeakerPeqType = SpeakerPeqType.BiquadsPreset };
            var spdvm = new SpeakerLogic(speakerModel);

            //var z = spdvm.GetPresetData(0);
        }

        [TestMethod]
        public void SpeakerDataViewModelTest()
        {
            //todo: incomplete, untested

            var q = new SpeakerDataModel();
            //var t = new SpeakerDataViewModel();
            //t.AddNewParam.Execute(null);
        }


        [TestMethod]
        public void RequiredBiquadTest()
        {
            var biquads = new List<PeqDataModel>();

            Assert.AreEqual(biquads.RequiredBiquads(), 0);

            var biquads1 = new List<PeqDataModel>()
            {
                new PeqDataModel() {Order = 3, FilterType = FilterType.LinkWitzHp}, //3
                new PeqDataModel() {Order = 3, FilterType = FilterType.ButterworthHp}, //2
                new PeqDataModel() {Order = 3, FilterType = FilterType.Peaking}, //2
            };

            Assert.AreEqual(biquads1.RequiredBiquads(), 7);
        }

        [TestMethod]
        public void DspDbValueTest()
        {
            const double input = 0.0707945823669434;
            var x = input.GetDbValue();
            Console.WriteLine(x);
            var output = x.GainValue();
            Console.WriteLine(output);
            Assert.AreEqual(input, output, .00001);
        }

  
        [TestMethod]
        public void RoutingUpdateTest()
        {
            Assert.IsTrue(LibraryData.FuturamaSys.MatrixSelection.Count < 1);

            //test unicity of hashlist
            for (var i = 0; i < 1000; i++)
            {
                LibraryData.FuturamaSys.MatrixSelection.Add(new MatrixCell
                {
                    BroadcastMessage = BroadCastMessage.Alarm1,
                    ButtonId = i / 12,
                    FlowId = i
                });
                LibraryData.FuturamaSys.MatrixSelection.Add(new MatrixCell { FlowId = 1, ButtonId = 1 });
            }

            Assert.IsTrue(LibraryData.FuturamaSys.MatrixSelection.All(n => n.BroadcastMessage != BroadCastMessage.Alarm2));

            for (int button = 0; button < 18; button++)
            {
                for (int zone = 0; zone < 384; zone++)
                {
                    var z = new RoutingTable(Enumerable.Range(button * 12, 12).ToArray(), zone / 12,
                        LibraryData.FuturamaSys.MatrixSelection);
                    var data = z.CommandValue.ToArray();


                    for (int i = 0; i < 12; i++)
                    {
                        //button Id is as expected 
                        Assert.AreEqual(button * 12 + i + 1, data[4 + i * 5]);
                        //no high bytes are used in alarm1
                        Assert.IsTrue(0xF0 > data[5 + i * 5]);

                        //alarm2 never set
                        Assert.AreEqual(0x00, data[7 + i * 5]); //A2H
                        Assert.AreEqual(0x00, data[8 + i * 5]); //A2L
                    }
                }
            }
        }

        [TestMethod]
        public void GetMainUnitId()
        {
            Assert.AreEqual(0, GenericMethods.GetMainunitIdForFlowId(500));
            Assert.AreEqual(0, GenericMethods.GetMainunitIdForFlowId(504));
            Assert.AreEqual(1, GenericMethods.GetMainunitIdForFlowId(505));
            Assert.AreEqual(0, GenericMethods.GetMainunitIdForFlowId(0));
            Assert.AreEqual(1, GenericMethods.GetMainunitIdForFlowId(12));
            Assert.AreEqual(0, GenericMethods.GetMainunitIdForFlowId(11));
            Assert.AreEqual(3, GenericMethods.GetMainunitIdForFlowId(36));
        }


        private static PeqDataModel TestPeqParam(FilterType filterType, int freq, int order)
        {

            var peqdata0 = new PeqDataModel
            {
                BandWidth = 1,
                Boost = 1,
                Biquads = null,
                FilterType = filterType,
                Frequency = freq,
                Gain = 1,
                IsEnabled = true,
                Order = order
            };
            return peqdata0;
        }

        private SpeakerDataModel TestSpeaker()
        {
            var speakerdata = new SpeakerDataModel
                {
                    SpeakerPeqType = SpeakerPeqType.BiquadsPreset,
                    PEQ = new List<PeqDataModel>(),
                };

            speakerdata.PEQ.Add(TestPeqParam(FilterType.ButterworthHp, 2000, 6));
            speakerdata.PEQ.Add(TestPeqParam(FilterType.ButterworthLp, 20000, 4));
            speakerdata.PEQ.Add(TestPeqParam(FilterType.LinkWitzHp, 100, 4));
            speakerdata.PEQ.Add(TestPeqParam(FilterType.Peaking, 1000, 4));
            speakerdata.PEQ.Add(TestPeqParam(FilterType.BesselHp, 2000, 4));


            return speakerdata;
        }


        [TestMethod]
        public void SendPresetTemporaryFunction()
        {
            var z = new Common.Commodules.GetMessageSelection(0);

            z.ToString();
        }

        [TestMethod]
        public void RedundancyDataPositionTest()
        {
            //channels, biquads, bytes per channel, bytes total, range
            // preset  12    14    112   1344  34816   36159
            // Aux     3     7     56    168   36160   36327    
            // mic     2     5     40    80    36328   36408            
            var spType = new[] { SpeakerPeqType.BiquadsPreset, SpeakerPeqType.BiquadsAux, SpeakerPeqType.BiquadsMic, };
            var expected = new[,]
            {
                //preset
                { 36152, 34816 ,}, 
                //aux
                { 36320, 36160 ,},
                //mic
                { 36400, 36328 ,},                                 
            };

            var biqaudSpeakerId = new[, ,]
            {
                //preset
                { { 13, 11 }, { 0, 0 } }, 
                //aux
                { { 6, 14 }, { 0, 12 } }, 
                //mic
                { { 4, 16 }, { 0, 15 } },
            };

            for (var i = 0; i < spType.Length; i++)
            {
                for (int test = 0; test < 2; test++)
                {
                    var tp = TestSpeaker();
                    tp.SpeakerPeqType = spType[i];
                    var logig = new SpeakerLogic(tp);
                    var address = logig.RedundancyAddress(biqaudSpeakerId[i, test, 0]);
                    
                    Assert.AreEqual(expected[i, test], address);
                }
            }
        }

        [TestMethod]
        public void LinkWitz6Th()
        {
            var x = DspCoefficients.GetXoverSOS(384, 6, FilterType.LinkWitzLp, 48000).Select(n => n.DspParams());
            var y =
                new[]
                    {
                        new[]
                            {
                                0.000601309320184, 0.001202618640367, 0.000601309320184, 1.901913563002136,
                                -0.904318800282870
                            },
                        new[]
                            {
                                0.000616045308739, 0.001232090617477, 0.000616045308739, 1.948522813110796,
                                -0.950986994345750
                            },

                        new[]
                            {
                                0.000616045308739, 0.001232090617477, 0.000616045308739, 1.948522813110796,
                                -0.950986994345750
                            },

                    };

            var i = 0;
            foreach (var d in x)
            {
                Assert.IsTrue(d.SequenceEqual(y[i++], new DspParamComarer()), "failed on biquad " + (i - 1));
            }
        }

        [TestMethod]
        public void LinkWitz4Th()
        {
            var x = DspCoefficients.GetXoverSOS(512, 4, FilterType.LinkWitzHp, 48000).Select(n => n.DspParams());
            var y =
                new[]
                    {
                        new[]
                            {
                                0.953714080094688, -1.907428160189375, 0.953714080094688, 1.905284625122248,
                                -0.909571695256502

                            },
                        new[]
                            {
                                0.953714080094688, - 1.907428160189375, 0.953714080094688, 1.905284625122248,
                                -0.909571695256502
                            },
                    };

            var i = 0;
            foreach (var d in x)
            {
                Assert.IsTrue(d.SequenceEqual(y[i++], new DspParamComarer()), "failed on biquad " + (i - 1));
            }
        }


        [TestMethod]
        public void LinkWitz2Nd()
        {
            var x = DspCoefficients.GetXoverSOS(512, 2, FilterType.LinkWitzHp, 48000).First().DspParams();
            var y =

                new[]
                    {
                        0.936180987547197, -1.872361975094395, 0.936180987547197, 1.870257846804933, -0.874466103383856
                    };
            Assert.IsTrue(x.SequenceEqual(y, new DspParamComarer()));
        }


        [TestMethod]
        public void BesselH4Th()
        {
            var x = DspCoefficients.GetXoverSOS(384, 4, FilterType.BesselLp, 48000).Select(n => n.DspParams());
            var y =
                new[]
                    {

                        new[]
                            {
                                0.000605188228776, 0.001210376457552, 0.000605188228776, 1.914182371441551,
                                -0.916603124356656
                            },
                        new[]
                            {
                                0.000605188228776, 0.001210376457552, 0.000605188228776, 1.914182371441551,
                                -0.916603124356656
                            },

                    };

            var i = 0;
            foreach (var d in x)
            {
                Assert.IsTrue(d.SequenceEqual(y[i++], new DspParamComarer()));
            }
        }

        [TestMethod]
        public void BesselH3Rd()
        {
            var x = DspCoefficients.GetXoverSOS(10, 3, FilterType.BesselHp, 48000).Select(n => n.DspParams());
            var y =
                new[]
                    {
                        new[]
                            {
                                0.999345929525232, -0.999345929525232, 0, 0.998691859050465, 0
                            },
                        new[]
                            {
                                0.998867231486796, -1.997734462973593, 0.998867231486796, 1.997733607207339,
                                -0.997735318739846
                            }
                    };

            var i = 0;
            foreach (var d in x)
            {
                Assert.IsTrue(d.SequenceEqual(y[i++], new DspParamComarer()));
            }
        }

        [TestMethod]
        public void Bessel2Nd()
        {
            var x = DspCoefficients.GetXoverSOS(10000, 2, FilterType.BesselHp, 48000).First().DspParams();
            var y =

                new[]
                    {
                         0.342719267599336  ,-0.685438535198671   ,0.342719267599336,0.281858695809837   ,-0.089018374587505
                    };
            Assert.IsTrue(x.SequenceEqual(y, new DspParamComarer()));
        }



        [TestMethod]
        public void SosXOver3RdOrder()
        {
            var butter3Lp1000A = DspCoefficients.GetXoverSOS(1000, 3, FilterType.ButterworthLp, 48000).Select(n => n.DspParams());
            var butter3Lp1000B =
                new[]
                {
                    new []{0.061511768503622, 0.061511768503622, 0, 0.876976462992757, 0},
                    new []{0.004015505022858,   0.008031010045716,   0.004015505022858, 1.861408444532108 ,  -0.877470464623539}
                };

            var i = 0;
            foreach (var d in butter3Lp1000A)
            {
                Assert.IsTrue(d.SequenceEqual(butter3Lp1000B[i++], new DspParamComarer()));
            }
        }

        [TestMethod]
        public void BiquadXover2NdOrder()
        {
            var butter2Lp1000A = DspCoefficients.GetXoverSOS(1000, 2, FilterType.ButterworthLp, 48000).First().DspParams();
            var butter2Lp1000B = new[] { 0.003916126660547, 0.007832253321095, 0.003916126660547, 1.815341082704568, -0.831005589346758 };

            Assert.IsTrue(butter2Lp1000A.SequenceEqual(butter2Lp1000B, new DspParamComarer()));
        }

        [TestMethod]
        public void BiquadXover1StOrder()
        {
            var x = DspCoefficients.GetXoverSOS(1000, 1, FilterType.ButterworthLp, 48000).First().DspParams();
            var y = new[] { 0.061511768503622, 0.061511768503622, 0, 0.876976462992757, 0 };
            Assert.IsTrue(x.SequenceEqual(y, new DspParamComarer()));
        }

        [TestMethod]
        public void BiquadSosTest()
        {
            var t = DspCoefficients.GetBiquadSos(10, 14000, 1, true, 48000).DspParams();
            var y = new[]
                {
                    1.599326168924671,
                    0.374162504619094,
                    -0.153673263162329,
                    -0.374162504619094,
                    -0.445652905762343
                };
            Assert.IsTrue(t.SequenceEqual(y, new DspParamComarer()));
        }


    }

    class DbValueComparer : IEqualityComparer<double>
    {
        public bool Equals(double x, double y)
        {
            return Math.Abs(x - y) < 0.1;
        }

        public int GetHashCode(double obj)
        {
            return (int)obj * 17;
        }
    }

    class DspParamComarer : IEqualityComparer<double>
    {
        public bool Equals(double x, double y)
        {
            return Math.Abs(x - y) < 0.00001;
        }

        public int GetHashCode(double obj)
        {
            return (int)obj * 17;
        }
    }

}
