using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LargeXlsx;

namespace CStat.Common
{
    public class CSSpreadSheet : IDisposable
    {
        public enum SSType {
            NONE =  0x00000000,
            TEXT =  0x00000001,
            PDF =   0x00000020,
            MSDOC = 0x00000040,
            EXCEL = 0x00000080,
            CSV =   0x00000100
            // VALUES MUST MATCH CmdMgr.CmdFormat : TBD Cleanup
        };

        public static SSType GetSSType(int type)
        {
            return Enum.IsDefined(typeof(SSType), type) ? (SSType)type : SSType.TEXT;
        }

        public static ReaderWriterLockSlim fLock = new ReaderWriterLockSlim();
        private bool _disposed;
        private bool _inFLock = false;
        private FileStream _stream = null;
        public XlsxWriter _xlsxWriter = null;
        private SSType _type = SSType.NONE;
        StreamWriter _wFile = null;
        public string FileName = "";
        private int P1=0,P2=0,P3=0,P4=0,P5=0,P6=0,P7=0,P8=0,P9=0,P10=0,P11=0,P12=0,P13=0,P14=0,P15=0,P16=0;

        public CSSpreadSheet (string filebase, SSType ssType) 
        {
            _inFLock = fLock.TryEnterWriteLock(5000);
            var TempPath = Path.GetTempPath();
            _type = ssType;
            FileName = Path.Combine(TempPath, filebase);
            switch (ssType)
            {
                case SSType.EXCEL:
                    FileName += ".xlsx";
                    _stream = new FileStream(FileName, FileMode.Create, FileAccess.Write);
                    _xlsxWriter = new XlsxWriter(_stream);
                    _xlsxWriter.BeginWorksheet("Sheet 1");
                    break;
                case SSType.CSV:
                    FileName += ".csv";
                    _wFile = new StreamWriter(FileName);
                    break;
                case SSType.TEXT:
                    FileName += ".txt";
                    _wFile = new StreamWriter(FileName);
                    break;
            }
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~CSSpreadSheet()
        {
            this.Dispose(false);
        }

        public void SetPad(int p1, int p2 = 0, int p3 = 0, int p4 = 0, int p5 = 0, int p6 = 0, int p7 = 0, int p8 = 0, int p9 = 0, int p10 = 0, int p11 = 0, int p12 = 0, int p13 = 0, int p14 = 0, int p15 = 0, int p16 = 0)
        {
            P1 = p1; P2 = p2; P3 = p3; P4 = p4; P5 = p5; P6 = p6; P7 = p7; P8 = p8; P9 = p9; P10 = p10; P11 = p11; P12 = p12; P13 = p13; P14 = p14; P15 = p15; P16 = p16;
        }

        public void AddRow(string s1, string s2 = "", string s3 = "", string s4 = "", string s5 = "", string s6 = "", string s7 = "", string s8 = "", string s9 = "", string s10 = "", string s11 = "", string s12 = "", string s13 = "", string s14 = "", string s15 = "", string s16 = "")
        {
            // Determine the number of specified strings
            int NumCols = 0;
            do {
                if (!string.IsNullOrEmpty(s16)) { NumCols = 16; break;} if (!string.IsNullOrEmpty(s15)) { NumCols = 15; break;} if (!string.IsNullOrEmpty(s14)) { NumCols = 14; break;} if (!string.IsNullOrEmpty(s13)) { NumCols = 13; break;}
                if (!string.IsNullOrEmpty(s12)) { NumCols = 12; break;} if (!string.IsNullOrEmpty(s11)) { NumCols = 11; break;} if (!string.IsNullOrEmpty(s10)) { NumCols = 10; break;} if (!string.IsNullOrEmpty(s9)) { NumCols = 9; break;}
                if (!string.IsNullOrEmpty(s8)) { NumCols = 8; break;} if (!string.IsNullOrEmpty(s7)) { NumCols = 7; break;} if (!string.IsNullOrEmpty(s6)) { NumCols = 6; break;} if (!string.IsNullOrEmpty(s5)) { NumCols = 5; break;}
                if (!string.IsNullOrEmpty(s4)) { NumCols = 4; break;} if (!string.IsNullOrEmpty(s3)) { NumCols = 3; break;} if (!string.IsNullOrEmpty(s2)) { NumCols = 2; break;} if (!string.IsNullOrEmpty(s1)) { NumCols = 1; break;}
            } while (false);
            if (NumCols == 0) return;

            switch (_type)
            {
                case SSType.EXCEL:
                    {
                        switch (NumCols)
                        {
                            case 1: _xlsxWriter.BeginRow().Write(s1); break;
                            case 2: _xlsxWriter.BeginRow().Write(s1).Write(s2); break;
                            case 3: _xlsxWriter.BeginRow().Write(s1).Write(s2).Write(s3); break;
                            case 4: _xlsxWriter.BeginRow().Write(s1).Write(s2).Write(s3).Write(s4); break;
                            case 5: _xlsxWriter.BeginRow().Write(s1).Write(s2).Write(s3).Write(s4).Write(s5); break;
                            case 6: _xlsxWriter.BeginRow().Write(s1).Write(s2).Write(s3).Write(s4).Write(s5).Write(s6); break;
                            case 7: _xlsxWriter.BeginRow().Write(s1).Write(s2).Write(s3).Write(s4).Write(s5).Write(s6).Write(s7); break;
                            case 8: _xlsxWriter.BeginRow().Write(s1).Write(s2).Write(s3).Write(s4).Write(s5).Write(s6).Write(s7).Write(s8); break;
                            case 9: _xlsxWriter.BeginRow().Write(s1).Write(s2).Write(s3).Write(s4).Write(s5).Write(s6).Write(s7).Write(s8).Write(s9); break;
                            case 10: _xlsxWriter.BeginRow().Write(s1).Write(s2).Write(s3).Write(s4).Write(s5).Write(s6).Write(s7).Write(s8).Write(s9).Write(s10); break;
                            case 11: _xlsxWriter.BeginRow().Write(s1).Write(s2).Write(s3).Write(s4).Write(s5).Write(s6).Write(s7).Write(s8).Write(s9).Write(s10).Write(s11); break;
                            case 12: _xlsxWriter.BeginRow().Write(s1).Write(s2).Write(s3).Write(s4).Write(s5).Write(s6).Write(s7).Write(s8).Write(s9).Write(s10).Write(s11).Write(s12); break;
                            case 13: _xlsxWriter.BeginRow().Write(s1).Write(s2).Write(s3).Write(s4).Write(s5).Write(s6).Write(s7).Write(s8).Write(s9).Write(s10).Write(s11).Write(s12).Write(s13); break;
                            case 14: _xlsxWriter.BeginRow().Write(s1).Write(s2).Write(s3).Write(s4).Write(s5).Write(s6).Write(s7).Write(s8).Write(s9).Write(s10).Write(s11).Write(s12).Write(s13).Write(s14); break;
                            case 15: _xlsxWriter.BeginRow().Write(s1).Write(s2).Write(s3).Write(s4).Write(s5).Write(s6).Write(s7).Write(s8).Write(s9).Write(s10).Write(s11).Write(s12).Write(s13).Write(s14).Write(s15); break;
                            case 16: _xlsxWriter.BeginRow().Write(s1).Write(s2).Write(s3).Write(s4).Write(s5).Write(s6).Write(s7).Write(s8).Write(s9).Write(s10).Write(s11).Write(s12).Write(s13).Write(s14).Write(s15).Write(s16); break;
                            default:
                                break;
                        }
                    }
                    break;
                case SSType.CSV:
                    {
                        switch (NumCols)
                        {
                            case 1: _wFile.WriteLine(CSVEnc(s1)); break;
                            case 2: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2)); break;
                            case 3: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2) + "," + CSVEnc(s3)); break;
                            case 4: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2) + "," + CSVEnc(s3) + "," + CSVEnc(s4)); break;
                            case 5: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2) + "," + CSVEnc(s3) + "," + CSVEnc(s4) + "," + CSVEnc(s5)); break;
                            case 6: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2) + "," + CSVEnc(s3) + "," + CSVEnc(s4) + "," + CSVEnc(s5) + "," + CSVEnc(s6)); break;
                            case 7: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2) + "," + CSVEnc(s3) + "," + CSVEnc(s4) + "," + CSVEnc(s5) + "," + CSVEnc(s6) + "," + CSVEnc(s7)); break;
                            case 8: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2) + "," + CSVEnc(s3) + "," + CSVEnc(s4) + "," + CSVEnc(s5) + "," + CSVEnc(s6) + "," + CSVEnc(s7) + "," + CSVEnc(s8)); break;
                            case 9: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2) + "," + CSVEnc(s3) + "," + CSVEnc(s4) + "," + CSVEnc(s5) + "," + CSVEnc(s6) + "," + CSVEnc(s7) + "," + CSVEnc(s8) + "," + CSVEnc(s9)); break;
                            case 10: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2) + "," + CSVEnc(s3) + "," + CSVEnc(s4) + "," + CSVEnc(s5) + "," + CSVEnc(s6) + "," + CSVEnc(s7) + "," + CSVEnc(s8) + "," + CSVEnc(s9) + "," + CSVEnc(s10)); break;
                            case 11: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2) + "," + CSVEnc(s3) + "," + CSVEnc(s4) + "," + CSVEnc(s5) + "," + CSVEnc(s6) + "," + CSVEnc(s7) + "," + CSVEnc(s8) + "," + CSVEnc(s9) + "," + CSVEnc(s10) + "," + CSVEnc(s11)); break;
                            case 12: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2) + "," + CSVEnc(s3) + "," + CSVEnc(s4) + "," + CSVEnc(s5) + "," + CSVEnc(s6) + "," + CSVEnc(s7) + "," + CSVEnc(s8) + "," + CSVEnc(s9) + "," + CSVEnc(s10) + "," + CSVEnc(s11) + "," + CSVEnc(s12)); break;
                            case 13: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2) + "," + CSVEnc(s3) + "," + CSVEnc(s4) + "," + CSVEnc(s5) + "," + CSVEnc(s6) + "," + CSVEnc(s7) + "," + CSVEnc(s8) + "," + CSVEnc(s9) + "," + CSVEnc(s10) + "," + CSVEnc(s11) + "," + CSVEnc(s12) + "," + CSVEnc(s13)); break;
                            case 14: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2) + "," + CSVEnc(s3) + "," + CSVEnc(s4) + "," + CSVEnc(s5) + "," + CSVEnc(s6) + "," + CSVEnc(s7) + "," + CSVEnc(s8) + "," + CSVEnc(s9) + "," + CSVEnc(s10) + "," + CSVEnc(s11) + "," + CSVEnc(s12) + "," + CSVEnc(s13) + "," + CSVEnc(s14)); break;
                            case 15: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2) + "," + CSVEnc(s3) + "," + CSVEnc(s4) + "," + CSVEnc(s5) + "," + CSVEnc(s6) + "," + CSVEnc(s7) + "," + CSVEnc(s8) + "," + CSVEnc(s9) + "," + CSVEnc(s10) + "," + CSVEnc(s11) + "," + CSVEnc(s12) + "," + CSVEnc(s13) + "," + CSVEnc(s14) + "," + CSVEnc(s15)); break;
                            case 16: _wFile.WriteLine(CSVEnc(s1) + "," + CSVEnc(s2) + "," + CSVEnc(s3) + "," + CSVEnc(s4) + "," + CSVEnc(s5) + "," + CSVEnc(s6) + "," + CSVEnc(s7) + "," + CSVEnc(s8) + "," + CSVEnc(s9) + "," + CSVEnc(s10) + "," + CSVEnc(s11) + "," + CSVEnc(s12) + "," + CSVEnc(s13) + "," + CSVEnc(s14) + "," + CSVEnc(s15) + "," + CSVEnc(s16)); break;
                            default:
                                break;
                        }
                    }
                    break;
                case SSType.TEXT:
                    {
                        switch (NumCols)
                        {
                            case 1: _wFile.WriteLine(s1.PadLeft(P1)); break;
                            case 2: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2)); break;
                            case 3: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2) + "," + s3.PadLeft(P3)); break;
                            case 4: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2) + "," + s3.PadLeft(P3) + "," + s4.PadLeft(P4)); break;
                            case 5: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2) + "," + s3.PadLeft(P3) + "," + s4.PadLeft(P4) + "," + s5.PadLeft(P5)); break;
                            case 6: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2) + "," + s3.PadLeft(P3) + "," + s4.PadLeft(P4) + "," + s5.PadLeft(P5) + "," + s6.PadLeft(P6)); break;
                            case 7: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2) + "," + s3.PadLeft(P3) + "," + s4.PadLeft(P4) + "," + s5.PadLeft(P5) + "," + s6.PadLeft(P6) + "," + s7.PadLeft(P7)); break;
                            case 8: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2) + "," + s3.PadLeft(P3) + "," + s4.PadLeft(P4) + "," + s5.PadLeft(P5) + "," + s6.PadLeft(P6) + "," + s7.PadLeft(P7) + "," + s8.PadLeft(P8)); break;
                            case 9: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2) + "," + s3.PadLeft(P3) + "," + s4.PadLeft(P4) + "," + s5.PadLeft(P5) + "," + s6.PadLeft(P6) + "," + s7.PadLeft(P7) + "," + s8.PadLeft(P8) + "," + s9.PadLeft(P9)); break;
                            case 10: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2) + "," + s3.PadLeft(P3) + "," + s4.PadLeft(P4) + "," + s5.PadLeft(P5) + "," + s6.PadLeft(P6) + "," + s7.PadLeft(P7) + "," + s8.PadLeft(P8) + "," + s9.PadLeft(P9) + "," + s10.PadLeft(P10)); break;
                            case 11: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2) + "," + s3.PadLeft(P3) + "," + s4.PadLeft(P4) + "," + s5.PadLeft(P5) + "," + s6.PadLeft(P6) + "," + s7.PadLeft(P7) + "," + s8.PadLeft(P8) + "," + s9.PadLeft(P9) + "," + s10.PadLeft(P10) + "," + s11.PadLeft(P11)); break;
                            case 12: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2) + "," + s3.PadLeft(P3) + "," + s4.PadLeft(P4) + "," + s5.PadLeft(P5) + "," + s6.PadLeft(P6) + "," + s7.PadLeft(P7) + "," + s8.PadLeft(P8) + "," + s9.PadLeft(P9) + "," + s10.PadLeft(P10) + "," + s11.PadLeft(P11) + "," + s12.PadLeft(P12)); break;
                            case 13: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2) + "," + s3.PadLeft(P3) + "," + s4.PadLeft(P4) + "," + s5.PadLeft(P5) + "," + s6.PadLeft(P6) + "," + s7.PadLeft(P7) + "," + s8.PadLeft(P8) + "," + s9.PadLeft(P9) + "," + s10.PadLeft(P10) + "," + s11.PadLeft(P11) + "," + s12.PadLeft(P12) + "," + s13.PadLeft(P13)); break;
                            case 14: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2) + "," + s3.PadLeft(P3) + "," + s4.PadLeft(P4) + "," + s5.PadLeft(P5) + "," + s6.PadLeft(P6) + "," + s7.PadLeft(P7) + "," + s8.PadLeft(P8) + "," + s9.PadLeft(P9) + "," + s10.PadLeft(P10) + "," + s11.PadLeft(P11) + "," + s12.PadLeft(P12) + "," + s13.PadLeft(P13) + "," + s1.PadLeft(P14)); break;
                            case 15: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2) + "," + s3.PadLeft(P3) + "," + s4.PadLeft(P4) + "," + s5.PadLeft(P5) + "," + s6.PadLeft(P6) + "," + s7.PadLeft(P7) + "," + s8.PadLeft(P8) + "," + s9.PadLeft(P9) + "," + s10.PadLeft(P10) + "," + s11.PadLeft(P11) + "," + s12.PadLeft(P12) + "," + s13.PadLeft(P13) + "," + s1.PadLeft(P14) + "," + s15.PadLeft(P15)); break;
                            case 16: _wFile.WriteLine(s1.PadLeft(P1) + "," + s2.PadLeft(P2) + "," + s3.PadLeft(P3) + "," + s4.PadLeft(P4) + "," + s5.PadLeft(P5) + "," + s6.PadLeft(P6) + "," + s7.PadLeft(P7) + "," + s8.PadLeft(P8) + "," + s9.PadLeft(P9) + "," + s10.PadLeft(P10) + "," + s11.PadLeft(P11) + "," + s12.PadLeft(P12) + "," + s13.PadLeft(P13) + "," + s1.PadLeft(P14) + "," + s15.PadLeft(P15) + "," + s16.PadLeft(P16)); break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private static string CSVEnc(string instr)
        {
            if (!String.IsNullOrEmpty(instr) && instr.Contains(","))
            {
                return "\"" + instr + "\"";
            }
            return instr;
        }

        /// <summary>
        /// The dispose method that implements IDisposable.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The virtual dispose method that allows
        /// classes inherithed from this one to dispose their resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here.
                    if (_xlsxWriter != null)
                    {
                        _xlsxWriter.Dispose();
                        _xlsxWriter = null;
                    }

                    if (_stream != null)
                    {
                        _stream.Dispose();
                        _stream = null;
                    }

                    if (_wFile != null)
                    {
                        _wFile.Close();
                        _wFile.Dispose();
                        _wFile = null; 
                    }

                    if (_inFLock)
                    {
                        _inFLock = false;
                        fLock.ExitWriteLock();
                    }
                }
                // Dispose unmanaged resources here.

                _disposed = true;
            }
        }
    }
}
