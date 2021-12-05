using System;

using DotNetDBF;
using FirebirdSql.Data.FirebirdClient;

namespace Cron.Classes.DBF
{
    /// <summary>
    /// Приходы
    /// </summary>
    public class Prih : DbfStruct<Prih.Item>
    {
        public Prih() : base() { }

        protected override string GetAnswerFileName()
        {
            try
            {
                int? did = (this.AnswerItems.Count > 0) ? AnswerItems[0].did : 0;
                did = Math.Abs((int)(did - 100000000));
                return did.ToString();
            }
            catch
            {
                return "_";
            }
        }

        protected override string GetAnswerLocalDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.LocalPrihAnswerDir;
        }

        public class Item : IDbfStructItem
        {
            public string idges;
            public string naim;
            public string proizv;
            public string ser;
            public DateTime srok;
            public string idp;
            public double? cenapr;
            public double? cenapro;
            public double? zarc;
            public int? stnds;
            public double? nds;
            public double? summ;
            public int? kol;
            public double? kolk;
            public double? dlk;
            public double? shk;
            public double? vsk;
            public double? dlu;
            public double? shu;
            public double? vsu;
            public double? vesk;
            public double? vesn;
            public string uslh;
            public string ndok;
            public DateTime datapr;

            //-------------------------------------------------------------
            // Поля только для ответа

            public int codegood;
            public int did;

            public DBFField[] GetDBFFields()
            {
                return new DBFField[] {
                    new DBFField("IDGES", NativeDbType.Char, 10),           // lid товара
                    new DBFField("NAIM", NativeDbType.Char, 100),           // Товар
                    new DBFField("PROIZV", NativeDbType.Char, 80),          // Производитель
                    new DBFField("SER", NativeDbType.Char, 80),             // Серия
                    new DBFField("SROK", NativeDbType.Date),                // Срок годности
                    new DBFField("IDP", NativeDbType.Char, 12),             // eid строки документа
                    new DBFField("CENAPR", NativeDbType.Numeric, 12, 2),    // Цена продажи
                    new DBFField("CENAPRO", NativeDbType.Numeric, 12, 2),   // Отпускная цена производителя
                    new DBFField("ZARC", NativeDbType.Numeric, 12, 2),
                    new DBFField("STNDS", NativeDbType.Numeric, 3, 0),
                    new DBFField("NDS", NativeDbType.Numeric, 12, 2),       // НДС
                    new DBFField("SUMM", NativeDbType.Numeric, 15, 2),      // enum14 Сумма по строке
                    new DBFField("KOL", NativeDbType.Numeric, 10, 0),
                    new DBFField("KOLK", NativeDbType.Numeric, 12, 2),
                    new DBFField("DLK", NativeDbType.Numeric, 7, 3),
                    new DBFField("SHK", NativeDbType.Numeric, 7, 3),
                    new DBFField("VSK", NativeDbType.Numeric, 7, 3),
                    new DBFField("DLU", NativeDbType.Numeric, 7, 3),
                    new DBFField("SHU", NativeDbType.Numeric, 7, 3),
                    new DBFField("VSU", NativeDbType.Numeric, 7, 3),
                    new DBFField("VESK", NativeDbType.Numeric, 7, 3),
                    new DBFField("VESN", NativeDbType.Numeric, 7, 3),
                    new DBFField("USLH", NativeDbType.Char, 15),
                    new DBFField("NDOK", NativeDbType.Char, 13),            // Номер прихода КРОНа
                    new DBFField("CODEGOOD", NativeDbType.Numeric, 10, 0)   // Код товара 
                };
            }

            public void Mapping(FbDataReader dr_did)
            {
                did = Convert.ToInt32(dr_did["O$I$DID"]);

                idges = dr_did["O$IDGES"].ToString();             
                naim = dr_did["O$NAIM"].ToString();               
                proizv = dr_did["O$PROIZV"].ToString();           
                ser = dr_did["O$SER"].ToString();                 
                srok = Convert.ToDateTime(dr_did["O$SROK"]);      
                idp = dr_did["O$IDP"].ToString();                 
                cenapr = Convert.ToDouble(dr_did["O$CENAPR"]);    
                cenapro = Convert.ToDouble(dr_did["O$CENAPRO"]);  
                zarc = Convert.ToDouble(dr_did["O$ZARC"]);
                stnds = Convert.ToInt32(dr_did["O$STNDS"]);
                nds = Convert.ToDouble(dr_did["O$NDS"]);          
                summ = Convert.ToDouble(dr_did["O$SUMM"]);        
                kol = Convert.ToInt32(dr_did["O$KOL"]);
                kolk = Convert.ToDouble(dr_did["O$KOLK"]);
                ndok = dr_did["O$NDOC"].ToString();               
                codegood = Convert.ToInt32(dr_did["O$CODEGOOD"]);                
            }
        }
    }
    /// <summary>
    /// Приходы МДЛП
    /// </summary>
    public class PrihMdlp : DbfStruct<PrihMdlp.Item>
    {
        public PrihMdlp() : base() { }

        protected override string GetAnswerFileName()
        {
            string name = (AnswerItems.Count > 0)
                    ? Additional.Helper.GenerateNameMdFile(AnswerItems[0].ndok, AnswerItems[0].did)
                    : Additional.Helper.GenerateNameMdFile("", 0);
            return name;
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
                    new DBFField("NDOK", NativeDbType.Char, 13),            // Номер расходного документа
                    new DBFField("DATAPR", NativeDbType.Date),              // Дата расходного документа
                    new DBFField("DID", NativeDbType.Numeric, 12, 2),       // DID приходного документа
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
                datapr = (dr_did["DATAPR"] != null) ? (DateTime?)dr_did["DATAPR"] : null;    
                did = Additional.Helper.TryConvertToInt32(dr_did["DID"].ToString());
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
