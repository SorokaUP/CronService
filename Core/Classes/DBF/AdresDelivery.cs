using System;

using DotNetDBF;
using Extensions;
using FirebirdSql.Data.FirebirdClient;

namespace Cron.Classes.DBF
{
    /// <summary>
    /// AdresDelivery
    /// </summary>
    public class AdresDelivery : DbfStruct<AdresDelivery.Item>
    {
        public AdresDelivery() : base() { }

        protected override string GetAnswerFileName()
        {
            return "Adres_delivery";
        }

        protected override string GetAnswerLocalDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.LocalAdresDeliveryDir;
        }

        public class Item : IDbfStructItem
        {
            public string ORGNAME;
            public string INN;
            public string CODE;
            public int? CODE_PM;
            public string ADRES;
            public string ZONE;
            public int? PRIORITY;
            public int? REGNUM;

            public DBFField[] GetDBFFields()
            {
                return new DBFField[] {
                    new DBFField("ORGNAME", NativeDbType.Char, 80),
                    new DBFField("INN", NativeDbType.Char, 80),
                    new DBFField("CODE", NativeDbType.Char, 30),
                    new DBFField("CODE_PM", NativeDbType.Numeric, 9, 0),
                    new DBFField("ADRES", NativeDbType.Char, 255),
                    new DBFField("ZONE", NativeDbType.Char, 30),
                    new DBFField("PRIORITY", NativeDbType.Numeric, 9, 0),
                    new DBFField("REGNUM", NativeDbType.Numeric, 9, 0)
                };
            }

            public void Mapping(FbDataReader dr_did)
            {
                ORGNAME = dr_did["ORGNAME"].ToString().Trim().AnsiToOem();
                INN = dr_did["INN"].ToString().Trim().AnsiToOem();
                CODE = dr_did["CODE"].ToString();
                CODE_PM = Additional.Helper.TryConvertToInt32(dr_did["CODE_PM"]);
                ADRES = dr_did["ADRES"].ToString().Trim().AnsiToOem();
                ZONE = dr_did["ZONE_NAME"].ToString().Trim().AnsiToOem();
                PRIORITY = Additional.Helper.TryConvertToInt32(dr_did["PRIORITY"]);
                REGNUM = Additional.Helper.TryConvertToInt32(dr_did["NUMB_REG_CLN"]);
            }
        }
    }
}
