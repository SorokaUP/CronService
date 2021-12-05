using System.Data;
using System.Collections.Generic;

using DotNetDBF;
using Extensions;
using FirebirdSql.Data.FirebirdClient;

namespace Cron.Classes.DBF
{
    /// <summary>
    /// Резервы
    /// </summary>
    public class Rez : DbfStruct<Rez.Item>
    {
        public string FileName { get; set; }
        public bool isSn { get; set; }
        public List<Item> Items { get; set; }
        public string ErrorMessage { get; set; }
        public FtpCron ftp { get; set; }
        public string AnswerDir { get; set; }
        public string LocalDir { get; set; }
        public string LocalAnswerDir { get; set; }

        public Rez() : base()
        {
            this.Items = new List<Item>();
        }

        protected override string GetAnswerFileName()
        {
            return (this.AnswerItems.Count > 0)
                ? this.AnswerItems[0].ID_RESERV
                : "_";
        }

        protected override string GetAnswerLocalDir()
        {
            return LocalAnswerDir;
        }

        /// <summary>
        /// Чтение файла снятия/постановки резерва DBF
        /// </summary>
        /// <returns></returns>
        private List<Item> ReadDbfToItems()
        {
            List<Item> res = new List<Item>();
            DataTable data = ReadDataTable(LocalDir + "\\" + FileName);

            //--------------------------------------------------------------------------------------
            // Разбираем прочитанные строки

            foreach (DataRow row in data.Rows)
            {
                res.Add(new Item
                {
                    IDGES = row.FldByName("idges").ToString(),
                    IDP = row.FldByName("idp").ToString(),
                    KOL = row.FldByName("kol").ToString(),
                    AGES = row.FldByName("ages").ToString(),
                    ADR = row.FldByName("adr").ToString(),
                    URLICO = row.FldByName("urlico").ToString(),
                    ID_RESERV = row.FldByName("id_reserv").ToString()
                });
            }

            return res;
        }

        /// <summary>
        /// Выполнить
        /// </summary>
        public void Execute()
        {
            this.Items = ReadDbfToItems();
            Firebird Fb = new Firebird();
            if (isSn)
                Fb.SnRez((int)Additional.Helper.TryConvertToInt32(this.Items[0].ID_RESERV, false));
            else
                Fb.Rez(this, ftp);
        }

        public class Item : IDbfStructItem
        {
            public string IDGES;
            public string IDP;
            public string KOL;
            public string AGES;
            public string ADR;
            public string URLICO;
            public string ID_RESERV;

            public DBFField[] GetDBFFields()
            {
                return new DBFField[] {
                    new DBFField("IDGES", NativeDbType.Char, 10),
                    new DBFField("IDP", NativeDbType.Char, 10),
                    new DBFField("KOL", NativeDbType.Numeric, 10, 0),
                    new DBFField("AGES", NativeDbType.Numeric, 10, 0),
                    new DBFField("ADR", NativeDbType.Char, 200),
                    new DBFField("URLICO", NativeDbType.Char, 200),
                    new DBFField("ID_RESERV", NativeDbType.Char, 6)
                };
            }

            public void Mapping(FbDataReader dr_did)
            {
                IDGES = dr_did["IDGES"].ToString();
                IDP = dr_did["IDP"].ToString();
                KOL = dr_did["KOL"].ToString();
                AGES = dr_did["AGES"].ToString();
                ADR = dr_did["ADR"].ToString();
                URLICO = dr_did["URLICO"].ToString();
                ID_RESERV = dr_did["ID_RESERV"].ToString();
            }
        }
    }
}
