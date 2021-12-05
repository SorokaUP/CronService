using System;

using DotNetDBF;
using FirebirdSql.Data.FirebirdClient;

namespace Cron.Classes.DBF
{
    /// <summary>
    /// Возвраты
    /// </summary>
    public class Vzvr : DbfStruct<Vzvr.Item>
    {
        public Vzvr() : base() { }

        protected override string GetAnswerFileName()
        {
            return (this.AnswerItems.Count > 0)
                ? this.AnswerItems[0].NDOC.Replace("-", "_").Replace("*", "_").Replace("/", "_").Replace("__", "_")
                : "_";
        }

        protected override string GetAnswerLocalDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.LocalVzvrDir;
        }

        public class Item : IDbfStructItem
        {
            public int? IDP;
            public double? CENA;
            public int? KOL;
            public string NDOC;
            public DateTime DDATE;
            public string ORD_DOC;
            public DateTime ORD_DATE;
            public string ORD_INS;
            public string GOODCODE;
            public string GES;
            public int? KOL_ALL;

            public DBFField[] GetDBFFields()
            {
                return new DBFField[] {
                    new DBFField("IDP", NativeDbType.Numeric, 15, 0),
                    new DBFField("CENA", NativeDbType.Numeric, 15, 2),
                    new DBFField("KOL", NativeDbType.Numeric, 15, 0),
                    new DBFField("NDOC", NativeDbType.Char, 40),
                    new DBFField("DDATE", NativeDbType.Date),
                    new DBFField("ORD_DOC", NativeDbType.Char, 40),
                    new DBFField("ORD_DATE", NativeDbType.Date),
                    new DBFField("ORD_INS", NativeDbType.Char, 40),
                    new DBFField("GOODCODE", NativeDbType.Char, 80),
                    new DBFField("GES", NativeDbType.Char, 80),
                    new DBFField("KOL_ALL", NativeDbType.Numeric, 15, 0)
                };
            }

            public void Mapping(FbDataReader dr_did)
            {
                IDP = Convert.ToInt32(dr_did["IDP"]);
                CENA = Convert.ToDouble(dr_did["CENA"]);
                KOL = Convert.ToInt32(dr_did["KOL"]);
                NDOC = dr_did["NDOC"].ToString();
                DDATE = Convert.ToDateTime(dr_did["DDATE"]);
                ORD_DOC = dr_did["ORD_DOC"].ToString();
                ORD_DATE = Convert.ToDateTime(dr_did["ORD_DATE"]);
                ORD_INS = dr_did["ORD_INS"].ToString();
                GOODCODE = dr_did["GOODCODE"].ToString();
                GES = dr_did["GES"].ToString();
                KOL_ALL = Convert.ToInt32(dr_did["KOL_ALL"]);
            }
        }
    }

    /// <summary>
    /// Возвраты
    /// </summary>
    public class VzvrMdlp : DbfStruct<VzvrMdlp.Item>
    {
        public VzvrMdlp() : base() { }

        protected override string GetAnswerFileName()
        {
            string name = (AnswerItems.Count > 0)
                    ? Additional.Helper.GenerateNameMdFile(AnswerItems[0].ndok, AnswerItems[0].did)
                    : Additional.Helper.GenerateNameMdFile("", 0);
            return name;
        }

        protected override string GetAnswerLocalDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.LocalVzvrDir;
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
                    new DBFField("NDOK", NativeDbType.Char, 13),            // Номер расходного документа
                    new DBFField("DATAPR", NativeDbType.Date),              // Дата расходного документа
                    new DBFField("DID", NativeDbType.Numeric, 12, 2),       // DID расходного документа
                    new DBFField("CODEGOOD", NativeDbType.Numeric, 10, 0),  // Код товара
                    new DBFField("GTIN", NativeDbType.Char, 14),            // GTIN товара
                    new DBFField("SSCC", NativeDbType.Char, 18),            // SSCC
                    new DBFField("KIZ", NativeDbType.Char, 27),             // SGTIN
                    new DBFField("OBJ_TYPE", NativeDbType.Numeric, 10, 0),  // [ 1 "SSCC" | 2 "SGTIN" ]
                    new DBFField("SER", NativeDbType.Char, 80)              // Серия
                };
            }

            public void Mapping(FbDataReader dr_did)
            {
                ndok = dr_did["NDOK"].ToString();
                datapr = (dr_did["DATAPR"] != DBNull.Value) ? (DateTime?)dr_did["DATAPR"] : null;
                did = Additional.Helper.TryConvertToInt32(dr_did["O$I$DID"]);
                codegood = Additional.Helper.TryConvertToInt32(dr_did["CODEGOOD"]);
                gtin = dr_did["GTIN"].ToString();
                sscc = dr_did["SSCC"].ToString();
                kiz = dr_did["KIZ"].ToString();
                obj_type = Additional.Helper.TryConvertToInt32(dr_did["OBJECT_TYPE"]);
                ser = dr_did["SER_NAME"].ToString();
            }
        }
    }
}
