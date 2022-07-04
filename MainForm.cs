using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using CsvHelper;
using System.Globalization;

namespace MappingBJ
{
    public partial class MainForm : Form
    {
        List<mmref> stores = new List<mmref>();
        List<mmdestinationOutput> destinations = new List<mmdestinationOutput>();
        public MainForm()
        {
            InitializeComponent();
            SetupForm();
            try
            {
                GetRefs();
                SuckInFiles();
                MapData();
                SpitOutFile();
                CleanUp();
            }
            catch (Exception ex)
            {
                Logging.Log(null, ex, "MappingBJ");
            }
        }

        private void GetRefs()
        {
            var files = Directory.GetFiles(@"\\dc1\Data\Public\EDI\EDINew\Mastermind\3PL_Lookupdata");

            if (files != null)
            {
                if (files.Length != 0)
                {
                    DB.Execute("truncate table mmref");

                    foreach (var file in files)
                    {
                        List<List<string>> result = new List<List<string>>();
                        string value;

                        CsvHelper.Configuration.CsvConfiguration config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.CurrentCulture);
                        config.Delimiter = ",";
                        config.Quote = '"';
                        config.HasHeaderRecord = true;
                        TextReader reader = File.OpenText(file);
                        var csv = new CsvReader(reader, config);

                        csv.Read();
                        var fieldCount = csv.ColumnCount;

                        while (csv.Read())
                        {
                            List<string> values = new List<string>();
                            for (int i = 0; csv.TryGetField<string>(i, out value); i++)
                            {
                                values.Add(value);
                            }
                            result.Add(values);
                        }

                        reader.Close();
                        reader.Dispose();

                        csv.Dispose();

                        foreach (var row in result)
                        {
                            string vals = "";
                            foreach (var v in row)
                            {
                                vals += "'" + v.Replace("'", "''") + "', ";
                            }
                            if (vals.Length > 0)
                            {
                                vals = vals.Substring(0, vals.Length - 2);
                            }
                            DB.Execute("insert into mmref select " + vals);
                        }

                        FileInfo fi = new FileInfo(file);

                        File.Move(file, @"\\dc1\Data\Public\EDI\EDINew\Mastermind\3PL_Lookupdata\Archive\" + fi.Name);
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                        }
                    }
                }
            }

            ImportEntities ie = new ImportEntities();
            stores = ie.mmrefs.ToList();
        }

        private void SetupForm()
        {
            this.Opacity = 0;
            this.ShowInTaskbar = false;
        }

        private void SuckInFiles()
        {
            var files = Directory.GetFiles(@"\\dc1\Data\Public\EDI\EDINew\Mastermind\Inbound_3PL");
            foreach (var file in files)
            {
                SuckInFile(file);
            }
        }

        private void SuckInFile(string filename)
        {
            List<List<string>> result = new List<List<string>>();
            string value;

            CsvHelper.Configuration.CsvConfiguration config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.CurrentCulture);
            config.Delimiter = ",";
            config.Quote = '"';
            config.HasHeaderRecord = true;
            TextReader reader = File.OpenText(filename);
            var csv = new CsvReader(reader, config);

            csv.Read();
            var fieldCount = csv.ColumnCount;
            
            while (csv.Read())
            {
                List<string> values = new List<string>();
                for (int i = 0; csv.TryGetField<string>(i, out value); i++)
                {
                    values.Add(value);
                }
                result.Add(values);
            }

            reader.Close();
            reader.Dispose();

            csv.Dispose();

            foreach (var row in result)
            {
                string vals = "";
                foreach (var v in row)
                {
                    vals += "'" + v.Replace("'", "''") + "', ";
                }
                if (vals.Length > 0)
                {
                    vals = vals.Substring(0, vals.Length - 2);
                }
                DB.Execute("insert into mmraw select " + vals);
            }

            FileInfo fi = new FileInfo(filename);

            if (File.Exists(@"\\dc1\Data\Public\EDI\EDINew\Mastermind\Inbound_3PL\Archive\" + fi.Name))
            {
                File.Move(filename, @"\\dc1\Data\Public\EDI\EDINew\Mastermind\Inbound_3PL\Archive\" + fi.Name.Substring(0, fi.Name.Length - 4) + "_" + DateTime.Now.Ticks.ToString() + ".csv");
            }
            else
            {
                File.Move(filename, @"\\dc1\Data\Public\EDI\EDINew\Mastermind\Inbound_3PL\Archive\" + fi.Name);
            }
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        private string GetPUDate(string shipdate)
        {
            var dateComponents = shipdate.Split('/');
            var year = dateComponents[2].Substring(2, 2);
            var month = dateComponents[0];
            var day = dateComponents[1];

            return year + month + day;
        }

        private string GetSpecialInstructions(string storeNumber)
        {
            var store = stores.FirstOrDefault(m => m.Store == storeNumber);

            return store.All_year_Daily_Deliveries + " " + store.Start_Window + " " + store.End_Window + " " + store.Length_of_Window + " " + store.Tailgate_Dock + " " + store.Attention + " " + store.Phone_;
        }

        private void MapData()
        {
            ImportEntities ie = new ImportEntities();
            ie.Configuration.AutoDetectChangesEnabled = false;
            ie.Configuration.ValidateOnSaveEnabled = false;

            var raws = ie.mmraws.ToList();

            List<mmdestinationOutput> mmdests = new List<mmdestinationOutput>();

            foreach (var raw in raws)
            {
                mmdestinationOutput mmd = new mmdestinationOutput();
                mmd.Key = raw.TRACKING_NUMBER__REF;
                mmd.DName = raw.SHIPPER_NAME;
                mmd.DAdd1 = raw.CONSIGNEE_ADDRESS_1;
                mmd.DAdd2 = raw.CONSIGNEE_ADDRESS_2;
                mmd.DCity = raw.CONSIGNEE_CITY;
                mmd.DProv = raw.CONSIGNEE_PROVINCE;
                mmd.DCty = "CA";
                mmd.DPostal = raw.CONSIGNEE_POSTAL;
                mmd.DContact = raw.CONSIGNEE;
                mmd.Pcs = "0";
                mmd.Pwgt = "0";
                mmd.Twgt = raw.WEIGHT;
                mmd.Tskid = raw.SKID_COUNT;
                mmd.OName = raw.SHIPPER_NAME;
                mmd.OAdd1 = raw.SHIPPER_ADDRESS;
                mmd.OCity = raw.SHIPPER_CITY;
                mmd.OProv = raw.SHIPPER_PROVINCE;
                mmd.OCty = "CA";
                mmd.OPostal = raw.SHIPPER_POSTAL;
                mmd.PuDate__yy_mm_dd_ = GetPUDate(raw.SHIP_DATE);
                mmd.DelDate__yy_mm_dd_ = "";
                mmd.Special_Instructions = GetSpecialInstructions(raw.KEY_CONSIGNEE_NUMBER);
                
                mmdests.Add(mmd);
            }

            DB.Execute("truncate table mmraw");

            destinations = mmdests;
        }

        private void SpitOutFile()
        {
            CsvHelper.Configuration.CsvConfiguration config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.CurrentCulture);
            config.Delimiter = ",";
            config.Quote = '"';
            config.HasHeaderRecord = true;

            //Have to get rid of the ID field here.
            using (TextWriter writer = new StreamWriter(@"\\dc1\Data\Public\EDI\EDINew\Mastermind\3PL_FLXdata\output_" + DateTime.Now.Ticks.ToString() + ".csv"))
            {
                var csv = new CsvWriter(writer, config);
                
                csv.WriteRecords(destinations); // where values implements IEnumerable
            }
        }

        private void CleanUp()
        {
            DB.Execute("truncate table mmdestination");
        }
    }
}