using System;

using DotNetDBF;
using FirebirdSql.Data.FirebirdClient;

namespace Cron.Classes.DBF
{
    /// <summary>
    /// Дефектура МДЛП
    /// </summary>
    public class Def : DbfStruct<Def.Item>
    {
        public Def() : base() { }

        protected override string GetAnswerFileName()
        {
            string ndok = "";
            int? did = 0;
            if (this.AnswerItems.Count > 0)
            {
                ndok = this.AnswerItems[0].ndok;
                did = Additional.Helper.TryConvertToInt32(this.AnswerItems[0].did, false) - 100000000;
                did = Math.Abs((int)did);
            }
            if (string.IsNullOrEmpty(ndok))
                ndok = "";
            else
                ndok = ndok.Substring(ndok.Length > 4 ? ndok.Length - 4 : 0).PadLeft(4, '0');
            return $"md({ndok}){(did).ToString()}";
        }

        protected override string GetAnswerLocalDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.LocalPrihAnswerDir;
        }

        public class Item : IDbfStructItem
        {
            public string ndok;
            public DateTime? datapr;
            public int? did;
            public int? codegood;
            public string gtin;
            public string sscc;
            public string kiz;
            public int? obj_type;
            public string ser;

            public DBFField[] GetDBFFields()
            {
                return new DBFField[] {
                    new DBFField("NDOK", NativeDbType.Char, 80),
                    new DBFField("DATAPR", NativeDbType.Date),
                    new DBFField("DID", NativeDbType.Numeric, 12, 2),
                    new DBFField("CODEGOOD", NativeDbType.Numeric, 10, 0),
                    new DBFField("GTIN", NativeDbType.Char, 14),
                    new DBFField("SSCC", NativeDbType.Char, 18),
                    new DBFField("KIZ", NativeDbType.Char, 27),
                    new DBFField("OBJ_TYPE", NativeDbType.Numeric, 10, 0),
                    new DBFField("SER", NativeDbType.Char, 80)
                };
            }

            public void Mapping(FbDataReader dr_did)
            {
                ndok = dr_did["NDOK"].ToString();                                            // Номер расходного документа
                datapr = (dr_did["DATAPR"] != null) ? (DateTime?)dr_did["DATAPR"] : null;    // Дата расходного документа
                did = Additional.Helper.TryConvertToInt32(dr_did["DID"].ToString());
                codegood = Additional.Helper.TryConvertToInt32(dr_did["CODEGOOD"]);          // Код товара
                gtin = dr_did["GTIN"].ToString();                                            // GTIN
                sscc = dr_did["SSCC"].ToString();                                            // SSCC
                kiz = dr_did["KIZ"].ToString();                                              // SGTIN
                obj_type = Additional.Helper.TryConvertToInt32(dr_did["OBJECT_TYPE"]);       // [ 1 "SSCC" | 2 "SGTIN" ]
                ser = dr_did["SER_NAME"].ToString();
            }
        }
    }
}
