using System;

using DotNetDBF;
using FirebirdSql.Data.FirebirdClient;

namespace Cron.Classes.DBF
{
    /// <summary>
    /// Заказы
    /// </summary>
    public class Order : DbfStruct<Order.Item>
    {
        public Order() : base() { }

        protected override string GetAnswerFileName()
        {
            try
            {
                string res = "";
                if (this.AnswerItems.Count > 0)
                    res = this.AnswerItems[0].id_reserv.ToString();
                return (String.IsNullOrEmpty(res)) ? "_" : res;
            }
            catch
            {
                return "";
            }
        }

        protected override string GetAnswerLocalDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.LocalOrderAnswerDir;
        }

        public class Item : IDbfStructItem
        {
            public string idges;
            public string idp;
            public double? cena;
            public int? stnds;
            public double? nds;
            public double? summ;
            public int? kol;
            public string nzak;
            public int? nstr;
            public int? ages;
            public int? id_reserv;

            public DBFField[] GetDBFFields()
            {
                return new DBFField[] {
                    new DBFField("IDGES", NativeDbType.Char, 10),           // lid товара
                    new DBFField("IDP", NativeDbType.Char, 10),             // eid строки документа
                    new DBFField("CENA", NativeDbType.Numeric, 15, 2),      // Цена продажи
                    new DBFField("STNDS", NativeDbType.Numeric, 3, 0),      // Ставка НДС
                    new DBFField("NDS", NativeDbType.Numeric, 15, 2),       // НДС
                    new DBFField("SUMM", NativeDbType.Numeric, 15, 2),      // Сумма
                    new DBFField("KOL", NativeDbType.Numeric, 10, 0),       // Количество
                    new DBFField("NZAK", NativeDbType.Char, 13),            // Номер заказа
                    new DBFField("NSTR", NativeDbType.Numeric, 5, 0),       // Номер строки?
                    new DBFField("AGES", NativeDbType.Numeric, 6, 0),
                    new DBFField("ID_RESERV", NativeDbType.Numeric, 15, 0)  // ID резерва
                };
            }

            public void Mapping(FbDataReader dr_did)
            {
                idges = dr_did["IDGES"].ToString();
                idp = dr_did["IDP"].ToString();
                cena = Convert.ToDouble(dr_did["CENA"]);
                stnds = Convert.ToInt32(dr_did["STNDS"]);
                nds = Convert.ToDouble(dr_did["NDS"]);
                summ = Convert.ToDouble(dr_did["SUMM"]);
                kol = Convert.ToInt32(dr_did["KOL"]);
                nzak = dr_did["NZAK"].ToString();
                nstr = Convert.ToInt32(dr_did["NSTR"]);
                ages = Convert.ToInt32(dr_did["AGES"]);
                id_reserv = Convert.ToInt32(dr_did["ID_RESERV"]);
            }
        }
    }
    /// <summary>
    /// Заказы МДЛП
    /// </summary>
    public class OrderMdlp : DbfStruct<OrderMdlp.Item>
    {
        public OrderMdlp() : base() { }

        protected override string GetAnswerFileName()
        {
            string name = (AnswerItems.Count > 0)
                    ? Additional.Helper.GenerateNameMdFile(AnswerItems[0].ndok, AnswerItems[0].did)
                    : Additional.Helper.GenerateNameMdFile("", 0);
            return name;
        }

        protected override string GetAnswerLocalDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.LocalOrderAnswerDir;
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
