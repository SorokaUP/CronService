using System;
using System.Text;
using System.Data;
using System.IO;

using DotNetDBF;
using Extensions;

namespace Cron.Classes.DBF
{
    /// <summary>
    /// Прайс-листы (остатки)
    /// </summary>
    public class Ostatki
    {
        /// <summary>
        /// Полный путь к файлу ответа
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Формирует файл ostatki_new.dbf
        /// </summary>
        /// <param name="dt"></param>
        public void CreateOstatkiNewDbf(DataTable dt)
        {
            // SP$CRON_MAKE_PRICE_52569

            string dir = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.LocalPriceDir;

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            FileName = dir + "\\ostatki_new.dbf";
            FileName = FileName.FixFilePath();

            try
            {
                if (File.Exists(FileName))
                    File.Delete(FileName);
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Не удалось удалить прежний файл {0}, по причине: {1}", FileName, ex.Message));
            }

            using (Stream fos = File.Open(FileName, FileMode.CreateNew, FileAccess.ReadWrite))
            using (var writer = new DBFWriter())
            {
                writer.CharEncoding = Encoding.GetEncoding(866);
                writer.Signature = 3;// DBFSigniture.DBase3;
                writer.LanguageDriver = 0x26; // кодировка 866
                writer.Fields = new[] {
                    new DBFField("IDP", NativeDbType.Numeric, 80, 0),
                    new DBFField("GES", NativeDbType.Numeric, 12, 0),
                    new DBFField("PLANTPRICE", NativeDbType.Numeric, 12, 2),
                    new DBFField("REG_PRICE", NativeDbType.Numeric, 12, 2),
                    new DBFField("REG_DATE", NativeDbType.Date),
                    new DBFField("QNT", NativeDbType.Numeric, 12, 0),
                    new DBFField("PRICE", NativeDbType.Numeric, 12, 2),
                    new DBFField("BBEFORE", NativeDbType.Date),
                    new DBFField("SERIAL", NativeDbType.Char, 80),
                    new DBFField("NAME", NativeDbType.Char, 80),
                    new DBFField("STATE", NativeDbType.Char, 80),
                    new DBFField("N_DOC", NativeDbType.Char, 80),
                    new DBFField("D_DOC", NativeDbType.Date),
                    new DBFField("READY", NativeDbType.Char, 80),
                    new DBFField("REASON", NativeDbType.Char, 80),
                    new DBFField("NUM_DOC", NativeDbType.Char, 80),
                    new DBFField("DATE_DOC", NativeDbType.Date),
                    new DBFField("ID_RESERV", NativeDbType.Numeric, 12, 0),
                    new DBFField("NAPR_SPIS", NativeDbType.Char, 80),
                    new DBFField("SER_READY", NativeDbType.Char, 80),
                    new DBFField("SKLAD", NativeDbType.Char, 80),
                    new DBFField("BLOCKSER", NativeDbType.Numeric, 12, 0),
                    new DBFField("BLOCKPST", NativeDbType.Numeric, 12, 0),
                    new DBFField("CODEPST", NativeDbType.Numeric, 12, 0),
                    new DBFField("MARKER", NativeDbType.Numeric, 12, 0),
                    new DBFField("IS_MDLP", NativeDbType.Numeric, 12, 0),
                    new DBFField("BARCODE", NativeDbType.Char, 20),
                    new DBFField("MDLP_READY", NativeDbType.Numeric, 12, 0)
            };

                foreach (DataRow row in dt.Rows)
                {
                    writer.AddRecord(
                        row.FldByName("IDP"),
                        row.FldByName("GES"),
                        row.FldByName("PLANT_PRICE"),
                        row.FldByName("REG_PRICE"),
                        row.FldByName("REG_DATE"),
                        row.FldByName("QNT"),
                        row.FldByName("PRICE"),
                        row.FldByName("BEST_BEFORE"),
                        row.FldByName("SERIAL"),
                        row.FldByName("NAME"),
                        row.FldByName("DSTATE"),
                        row.FldByName("DCODE"),
                        row.FldByName("DDATE"),
                        row.FldByName("READY"),
                        row.FldByName("REASON"),
                        row.FldByName("NUM_DOC"),
                        row.FldByName("DATE_DOC"),
                        row.FldByName("ID_RESERV"),
                        row.FldByName("NAPR_SPIS"),
                        row.FldByName("SER_READY"),
                        row.FldByName("SKLAD"),
                        row.FldByName("BLOCKSER"),
                        row.FldByName("BLOCKPST"),
                        row.FldByName("CODEPST"),
                        row.FldByName("MARKER"),
                        row.FldByName("IS_MDLP"),
                        row.FldByName("BARCODE"),
                        row.FldByName("MDLP_READY")
                    );
                }
                writer.Write(fos);
            }
        }

        /// <summary>
        /// Формирует файл ostatki.dbf
        /// </summary>
        /// <param name="dt"></param>
        public void CreateOstatkiDbf(DataTable dt)
        {
            // SP$CRON_MAKE_PRICE

            string dir = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.LocalPriceDir;

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            FileName = dir + "\\ostatki.dbf";

            try
            {
                if (File.Exists(FileName))
                    File.Delete(FileName);
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("Не удалось удалить прежний файл {0}, по причине: {1}", FileName, ex.Message));
            }

            using (Stream fos = File.Open(FileName, FileMode.Create, FileAccess.ReadWrite))
            using (var writer = new DBFWriter())
            {
                writer.CharEncoding = Encoding.GetEncoding(866);
                writer.Signature = 3;//DBFSigniture.DBase3;
                writer.LanguageDriver = 0x26; // кодировка 866
                writer.Fields = new[] {
                    new DBFField("GES", NativeDbType.Numeric, 12, 0),
                    new DBFField("IDP", NativeDbType.Numeric, 80, 0),
                    new DBFField("NAME", NativeDbType.Char, 80),
                    new DBFField("PRICE", NativeDbType.Numeric, 12, 2),
                    new DBFField("PPRICE", NativeDbType.Numeric, 12, 2),
                    new DBFField("REG_PRICE", NativeDbType.Numeric, 12, 2),
                    new DBFField("SERIAL", NativeDbType.Char, 80),
                    new DBFField("BBEFORE", NativeDbType.Date),
                    new DBFField("QNT", NativeDbType.Numeric, 12, 0)
            };

                foreach (DataRow row in dt.Rows)
                {
                    writer.AddRecord(
                        row.FldByName("O$GES"),
                        row.FldByName("O$IDP"),
                        row.FldByName("O$NAME"),
                        row.FldByName("O$PRICE"),
                        row.FldByName("O$PLANT_PRICE"),
                        row.FldByName("O$REG_PRICE"),
                        row.FldByName("O$SERIAL"),
                        row.FldByName("O$BEST_BEFORE"),
                        row.FldByName("O$QNT")
                    );
                }
                writer.Write(fos);
            }
        }
    }
}
