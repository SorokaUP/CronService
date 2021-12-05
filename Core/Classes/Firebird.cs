using System;
using System.Text;
using FirebirdSql.Data.FirebirdClient;
using System.Data;
using System.Configuration;
using System.Diagnostics;
using System.Collections.Generic;

using Extensions;
using Cron.Classes.DBF;

namespace Cron.Classes
{
    public class Firebird
    {
        // https://www.firebirdsql.org/en/net-examples-of-use/
        //---------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------

        #region Подключение и отключение от базы
        /// <summary>
        /// Получаем строку подключения
        /// </summary>
        /// <returns></returns>
        private string getConnectionString()
        {
            try
            {
                return ConfigurationManager.ConnectionStrings["tw4db"].ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion
               
        //---------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------
        // ОСНОВНЫЕ МЕТОДЫ

        #region Загрузка данных в базу
        /// <summary>
        /// Загрузка в таблицу
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Data"></param>
        public void Load_Process(DataTable Data, string FileName, string Mode)
        {
            using (FbConnection conn = new FbConnection(getConnectionString()))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    Log.Write($"Ошибка подключения (Load_Process): {ex.Message}");
                    return;
                }

                using (FbTransaction tr = conn.BeginTransaction())
                {
                    using (FbCommand cmd = new FbCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = conn;
                        cmd.Transaction = tr;

                        try
                        {
                            //------------------------------------------------------------------------------------------
                            // Предобработка и выполнение

                            Load_PreProcess(cmd, Mode);
                            foreach (DataRow row in Data.Rows)
                            {
                                // Устанавливаем параметры (читаем из DBF)
                                Load_SetParameters(cmd, row, FileName, Mode);
                                cmd.ExecuteScalar();
                            }

                            // Подкатываем транзакцию и закрываем соединение
                            tr.Commit();
                        }
                        catch (Exception ex)
                        {
                            tr.Rollback();
                            Log.Write($"Ошибка (Load_Process): {ex.Message}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Формируем запрос и параметры
        /// </summary>
        /// <param name="cmd">Команда SQL</param>
        /// <param name="Mode">Режим работы</param>
        private void Load_PreProcess(FbCommand cmd, string Mode)
        {
            StringBuilder sqlText = new StringBuilder();
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Clear();
            
            switch (Mode)
            {
                case Modes.GetPrih:
                    #region Приходы
                    sqlText.AppendLine("insert into CRON$INPUT_TABLE");
                    sqlText.AppendLine("(IDGES, NAIM, PROIZV, SER, SROK, IDP, CENAPR, CENAPRO, ZARC, STNDS, NDS, SUMM, KOL, ");
                    sqlText.AppendLine("KOLK, DLK, SHK, VSK, DLU, SHU, VSU, VESK, VESN, USLH, NDOK, FNAME, FDATE, FSTATE, DATAPR)");
                    sqlText.AppendLine("values");
                    sqlText.AppendLine("(@IDGES, @NAIM, @PROIZV, @SER, @SROK, @IDP, @CENAPR, @CENAPRO, @ZARC, @STNDS, @NDS, @SUMM, @KOL, @KOLK, ");
                    sqlText.AppendLine("@DLK, @SHK, @VSK, @DLU, @SHU, @VSU, @VESK, @VESN, @USLH, @NDOK, @FNAME, @FDATE, @FSTATE, @DATAPR);");
                    cmd.CommandText = sqlText.ToString();

                    cmd.Parameters.Add("IDGES", FbDbType.Integer);
                    cmd.Parameters.Add("NAIM", FbDbType.VarChar, 100);
                    cmd.Parameters.Add("PROIZV", FbDbType.VarChar, 70);
                    cmd.Parameters.Add("SER", FbDbType.VarChar, 70);
                    cmd.Parameters.Add("SROK", FbDbType.Date);
                    cmd.Parameters.Add("IDP", FbDbType.VarChar, 10);
                    cmd.Parameters.Add("CENAPR", FbDbType.Numeric, 15); cmd.ParamByName("CENAPR").Precision = 2;
                    cmd.Parameters.Add("CENAPRO", FbDbType.Numeric, 15); cmd.ParamByName("CENAPRO").Precision = 2;
                    cmd.Parameters.Add("ZARC", FbDbType.Numeric, 15); cmd.ParamByName("ZARC").Precision = 2;
                    cmd.Parameters.Add("STNDS", FbDbType.Integer);
                    cmd.Parameters.Add("NDS", FbDbType.Numeric, 15); cmd.ParamByName("NDS").Precision = 2;
                    cmd.Parameters.Add("SUMM", FbDbType.Numeric, 15); cmd.ParamByName("SUMM").Precision = 2;
                    cmd.Parameters.Add("KOL", FbDbType.Numeric, 15); cmd.ParamByName("KOL").Precision = 2;
                    cmd.Parameters.Add("KOLK", FbDbType.Numeric, 15); cmd.ParamByName("KOLK").Precision = 2;
                    cmd.Parameters.Add("DLK", FbDbType.Numeric, 7); cmd.ParamByName("DLK").Precision = 3;
                    cmd.Parameters.Add("SHK", FbDbType.Numeric, 7); cmd.ParamByName("SHK").Precision = 3;
                    cmd.Parameters.Add("VSK", FbDbType.Numeric, 7); cmd.ParamByName("VSK").Precision = 3;
                    cmd.Parameters.Add("DLU", FbDbType.Numeric, 7); cmd.ParamByName("DLU").Precision = 3;
                    cmd.Parameters.Add("SHU", FbDbType.Numeric, 7); cmd.ParamByName("SHU").Precision = 3;
                    cmd.Parameters.Add("VSU", FbDbType.Numeric, 7); cmd.ParamByName("VSU").Precision = 3;
                    cmd.Parameters.Add("VESK", FbDbType.Numeric, 7); cmd.ParamByName("VESK").Precision = 3;
                    cmd.Parameters.Add("VESN", FbDbType.Numeric, 7); cmd.ParamByName("VESN").Precision = 3;
                    cmd.Parameters.Add("USLH", FbDbType.VarChar, 50);
                    cmd.Parameters.Add("NDOK", FbDbType.VarChar, 13);
                    cmd.Parameters.Add("FNAME", FbDbType.VarChar, 50);
                    cmd.Parameters.Add("FDATE", FbDbType.Date);
                    cmd.Parameters.Add("FSTATE", FbDbType.Integer);
                    cmd.Parameters.Add("DATAPR", FbDbType.Date);
                    #endregion
                    break;

                case Modes.GetOrder:
                    #region Заказы
                    sqlText.AppendLine("insert into CRON$ORD_TABLE");
                    sqlText.AppendLine("( IDGES,  IDP,  KOL,  CENA,  STNDS,  NDS,  SUMM,  NZAK,  FNAME,  FDATE,  A5CODE,  N,  RESERVEID)");
                    sqlText.AppendLine("values");
                    sqlText.AppendLine("(@IDGES, @IDP, @KOL, @CENA, @STNDS, @NDS, @SUMM, @NZAK, @FNAME, @FDATE, @A5CODE, @N, @RESERVEID);");
                    cmd.CommandText = sqlText.ToString();

                    cmd.Parameters.Add("IDGES", FbDbType.Integer);
                    cmd.Parameters.Add("IDP", FbDbType.Integer);
                    cmd.Parameters.Add("KOL", FbDbType.Integer);
                    cmd.Parameters.Add("CENA", FbDbType.Numeric, 15); cmd.ParamByName("CENA").Precision = 2;
                    cmd.Parameters.Add("STNDS", FbDbType.Integer);
                    cmd.Parameters.Add("NDS", FbDbType.Numeric, 15); cmd.ParamByName("NDS").Precision = 2;
                    cmd.Parameters.Add("SUMM", FbDbType.Numeric, 15); cmd.ParamByName("SUMM").Precision = 2;
                    cmd.Parameters.Add("NZAK", FbDbType.Integer); 
                    cmd.Parameters.Add("FNAME", FbDbType.VarChar, 80); 
                    cmd.Parameters.Add("FDATE", FbDbType.Date); 
                    cmd.Parameters.Add("A5CODE", FbDbType.Integer); 
                    cmd.Parameters.Add("N", FbDbType.Integer); 
                    cmd.Parameters.Add("RESERVEID", FbDbType.VarChar, 20);
                    #endregion
                    break;
            } 

            cmd.Prepare();
        }

        /// <summary>
        /// Заполняем параметры в соответствии с DBF файлом
        /// </summary>
        /// <param name="cmd">Команда SQL</param>
        /// <param name="row">Строка данных DBF</param>
        /// <param name="FileName">Наименование файла</param>
        /// <param name="Mode">Режим работы</param>
        private void Load_SetParameters(FbCommand cmd, DataRow row, string FileName, string Mode)
        {
            string[] fields = null;
            switch (Mode)
            {
                case Modes.GetPrih:
                    #region Приходы
                    fields = new string[] {
                        "IDGES", "NAIM", "PROIZV", "SER", "SROK", "IDP", "CENAPR", "CENAPRO",
                        "ZARC", "STNDS", "NDS", "SUMM", "KOL", "KOLK", "DLK", "SHK",
                        "VSK", "DLU", "SHU", "VSU", "VESK", "VESN", "USLH", "NDOK",
                        "DATAPR"
                    };
                    foreach (string field in fields)
                        cmd.ParamByName(field).Value = row.FldByName(field);

                    cmd.ParamByName("FNAME").Value = FileName;
                    cmd.ParamByName("FDATE").Value = DateTime.Now;
                    cmd.ParamByName("FSTATE").Value = 0;
                    #endregion
                    break;

                case Modes.GetOrder:
                    #region
                    fields = new string[] { "IDGES", "IDP", "CENA", "STNDS", "NDS", "SUMM", "KOL", "NZAK" };
                    foreach (string field in fields)
                        cmd.ParamByName(field).Value = row.FldByName(field);

                    cmd.ParamByName("N").Value = row.FldByName("NSTR");
                    cmd.ParamByName("A5CODE").Value = row.FldByName("AGES");
                    cmd.ParamByName("RESERVEID").Value = row.FldByName("ID_RESERV");
                    cmd.ParamByName("FNAME").Value = FileName;
                    cmd.ParamByName("FDATE").Value = DateTime.Now;
                    #endregion
                    break;
            }
        }

        /// <summary>
        /// Пост-обработка процедуры импорта
        /// </summary>
        /// <param name="Mode">Режим работы</param>
        public void Load_PostProcess(string Mode)
        {
            using (FbConnection conn = new FbConnection(getConnectionString()))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    Log.Write($"Ошибка подключения (Load_PostProcess): {ex.Message}");
                    return;
                }

                using (FbTransaction tr = conn.BeginTransaction())
                {
                    using (FbCommand cmd = new FbCommand())
                    {
                        switch (Mode)
                        {
                            case Modes.GetPrih:
                                cmd.CommandText = "SP$CRON_MAKE_6001";
                                break;

                            case Modes.GetOrder:
                                cmd.CommandText = "SP$CRON_ORDERS";
                                break;

                            default:
                                return;
                        }
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;
                        cmd.Transaction = tr;
                        try
                        {
                            cmd.Prepare();
                            cmd.ExecuteScalar();
                            tr.Commit();
                        }
                        catch (Exception ex)
                        {
                            Log.Write($"Ошибка (Load_PostProcess): {ex.Message}");
                            tr.Rollback();
                        }
                    }
                }
            }
        }
        #endregion
        /// <summary>
        /// Фиксация документа
        /// </summary>
        private void ConfirmDoc(FbTransaction tr, int did, int concept)
        {
            using (FbCommand cmd_post = new FbCommand())
            {
                cmd_post.CommandType = CommandType.Text;
                cmd_post.CommandText = "execute procedure cron$confirm_doc(@did, @concept)";
                cmd_post.Connection = tr.Connection;
                cmd_post.Transaction = tr;

                cmd_post.Parameters.Add("did", FbDbType.Integer).Value = did;
                cmd_post.Parameters.Add("concept", FbDbType.Integer).Value = concept;
                cmd_post.Prepare();
                cmd_post.ExecuteScalar();
            }
        }

        //------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------
        
        #region Формирование и отправка дефектуры от ПрофитМед
        /// <summary>
        /// Софрмировать и отправить дефектуру
        /// </summary>
        /// <param name="ftp"></param>
        public List<string> CreateDefFiles()
        {
            List<string> res = new List<string>();
            using (FbConnection conn = new FbConnection(getConnectionString()))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    Log.Write($"Ошибка подключения (CreateDefFiles): {ex.Message}");
                    return res;
                }

                try
                {
                    string fieldName = "date";
                    string answerFile = CreateAnswerBodyBySingleParam<Def>(
                        conn.BeginTransaction(),
                        6054,
                        $"select * from sp$cron_confirm_6054_md(@{fieldName})",
                        fieldName,
                        FbDbType.Date,
                        DateTime.Now.Date);

                    if (!string.IsNullOrEmpty(answerFile))
                        res.Add(answerFile);
                }
                catch (Exception ex)
                {
                    Log.Write(String.Format("Ошибка (CreateDefFiles): Не удалось получить список заказов :: {0}", ex.Message));
                    res = new List<string>();
                }
            }

            return res;
        }
        #endregion

        //------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------

        #region УНИВЕРСАЛЬНАЯ ПРОЦЕДУРА ФОРМИРОВАНИЯ DBF
        /// <summary>
        /// Софрмировать DBF файлы и вернуть список ПУТЕЙ на локальном хранилище
        /// </summary>
        /// <param name="action">Метод формирующий файл/ы на основе параметров isAnswer и isMd</param>
        /// <param name="concept">Концепт документов</param>
        /// <param name="isAnswer">Нужен ответ по TW4 (возможно без маркированных позиций, как отдельный файл)</param>
        /// <param name="isMd">Нужен ответ по маркированным позициям (отдельный файл)</param>
        /// <returns>Список полных ПУТЕЙ к сформированным файлам на локальном хранилище</returns>
        public List<string> CreateAnswerFiles<TAnswer, TMd>(int concept, bool isAnswer, bool isMd)
            where TAnswer : IDbfStruct, new()
            where TMd : IDbfStruct, new()
        {
            List<string> res = new List<string>();

            var stack = new StackTrace().GetFrames();
            string methodName = (stack?.Length > 1) 
                ? stack[1].GetMethod().Name 
                : "CreateAnswerFiles";

            using (FbConnection conn = new FbConnection(getConnectionString()))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    Log.Write($"Ошибка подключения ({methodName}): {ex.Message}");
                    return res;
                }

                try
                {
                    using (FbCommand cmd = new FbCommand())
                    {
                        string fdid = "did";

                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = $"select {fdid} from cron$get_doc(@concept)";
                        //cmd.CommandText = $"select 143668246 as {fdid} from rdb$database"; // Режим тестирования
                        cmd.Parameters.Add("concept", FbDbType.Integer).Value = concept;
                        cmd.Connection = conn;

                        using (FbDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                if (dr[fdid].Equals(DBNull.Value) || dr[fdid] == null)
                                    continue;

                                int did = Convert.ToInt32(dr[fdid]);
                                Console.WriteLine($"did: {did}");

                                try
                                {
                                    List<string> bodyRes = CreateAnswerBody<TAnswer, TMd>(conn, concept, did, isAnswer, isMd);
                                    res.AddRange(bodyRes);
                                }
                                catch(Exception iex)
                                {
                                    Log.Write($"Ошибка ({methodName}): Не удалось обработать did = {did} :: {iex.Message}");
                                }
                            }
                            dr.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write($"Ошибка ({methodName}): Не удалось получить список документов :: {ex.Message}");
                    res = new List<string>();
                }
            }

            return res;
        }

        /// <summary>
        /// Софрмировать DBF файлы и вернуть список ПУТЕЙ на локальном хранилище
        /// </summary>
        /// <param name="action">Метод формирующий файл/ы на основе параметров isAnswer и isMd</param>
        /// <param name="did">DID документа</param>
        /// <param name="concept">Концепт документов</param>
        /// <param name="isAnswer">Нужен ответ по TW4 (возможно без маркированных позиций, как отдельный файл)</param>
        /// <param name="isMd">Нужен ответ по маркированным позициям (отдельный файл)</param>
        /// <returns>Список полных ПУТЕЙ к сформированным файлам на локальном хранилище</returns>
        public List<string> CreateAnswerFilesByDID<TAnswer, TMd>(int did, int concept, bool isAnswer, bool isMd)
            where TAnswer : IDbfStruct, new()
            where TMd : IDbfStruct, new()
        {
            List<string> res = new List<string>();

            var stack = new StackTrace().GetFrames();
            string methodName = (stack?.Length > 1)
                ? stack[1].GetMethod().Name
                : "CreateAnswerFilesByDID";

            using (FbConnection conn = new FbConnection(getConnectionString()))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    Log.Write($"Ошибка подключения ({methodName}): {ex.Message}");
                    return res;
                }

                try
                {
                    Console.WriteLine($"did: {did}");

                    List<string> bodyRes = CreateAnswerBody<TAnswer, TMd>(conn, concept, did, isAnswer, isMd);
                    res.AddRange(bodyRes);
                }
                catch (Exception iex)
                {
                    Log.Write($"Ошибка ({methodName}): Не удалось обработать did = {did} :: {iex.Message}");
                }
            }

            return res;
        }

        private List<string> CreateAnswerBody<TAnswer, TMd>(FbConnection conn, int concept, int did, bool isAnswer, bool isMd)
            where TAnswer : IDbfStruct, new()
            where TMd : IDbfStruct, new()
        {
            List<string> res = new List<string>();
            using (FbTransaction tr = conn.BeginTransaction())
            {
                try
                {
                    string fdid = "did";
                    string query = "";
                    string answerFile = "";

                    if (isAnswer)
                    {
                        Console.WriteLine("Выполняю часть isAnswer...");
                        query = $"select * from sp$cron_confirm_{concept}(@{fdid})";
                        answerFile = CreateAnswerBodyBySingleParam<TAnswer>(tr, concept, query, fdid, FbDbType.Integer, did);
                        if (!string.IsNullOrEmpty(answerFile))
                            res.Add(answerFile);
                        Console.WriteLine("Выполнена часть isAnswer");
                    }
                    if (isMd)
                    {
                        Console.WriteLine("Выполняю часть isMd...");
                        query = $"select * from sp$cron_confirm_{concept}_md(@{fdid})";
                        answerFile = CreateAnswerBodyBySingleParam<TMd>(tr, concept, query, fdid, FbDbType.Integer, did);
                        if (!string.IsNullOrEmpty(answerFile))
                            res.Add(answerFile);
                        Console.WriteLine("Выполнена часть isMd");
                    }
                    if (isAnswer || isMd)
                    {
                        try
                        {
                            Console.WriteLine("Фиксирую факт обработки");
                            ConfirmDoc(tr, did, concept);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Не удалось зафиксировать факт ответа :: {ex.Message}");
                        }
                    }

                    // Подкатываем транзакцию
                    tr.Commit();
                }
                catch (Exception ex)
                {
                    tr.Rollback();
                    Log.Write($"(!) Не удалось обработать did = {did} по причине: {ex.Message}");
                }
            }
            return res;
        }

        private string CreateAnswerBodyBySingleParam<T>(FbTransaction tr, int concept, string query, string fieldName, FbDbType fieldType, object fieldValue)
            where T : IDbfStruct, new()
        {
            T dbf = new T();

            //-----------------------------------------------------------
            // Получаем данные
            //-----------------------------------------------------------
            try
            {
                using (FbCommand cmd_did = new FbCommand())
                {
                    cmd_did.CommandType = CommandType.Text;
                    cmd_did.CommandText = query;
                    cmd_did.Parameters.Add(fieldName, fieldType).Value = fieldValue;
                    cmd_did.Connection = tr.Connection;
                    cmd_did.Transaction = tr;

                    using (FbDataReader dr_did = cmd_did.ExecuteReader())
                    {
                        while (dr_did.Read())
                        {
                            dbf.AddItem(dr_did);
                        }
                        dr_did.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось [получить]/[записать в объект] информацию :: {ex.Message}");
            }

            //-----------------------------------------------------------
            // Формируем файл ответа
            //-----------------------------------------------------------
            try
            {
                dbf.CreateDbf();
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось создать файл ответа :: {ex.Message}");
            }

            return dbf.GetAnswerFile();
        }
        #endregion

        //------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------

        #region Формирование прайс-листов
        public string CreateOstatki(bool OstatkiNew = false)
        {
            try
            {
                Ostatki o = new Ostatki();

                using (FbConnection conn = new FbConnection(getConnectionString()))
                {
                    conn.Open();

                    using (FbCommand cmd = new FbCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = ((OstatkiNew)
                                ? "select * from SP$CRON_MAKE_PRICE_52569"
                                : "select * from SP$CRON_MAKE_PRICE");
                        cmd.Connection = conn;

                        // 1. Получаем информацию по остаткам
                        FbDataAdapter da = new FbDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);                        

                        // 2. Формируем файл
                        if (OstatkiNew)
                            o.CreateOstatkiNewDbf(dt);
                        else
                            o.CreateOstatkiDbf(dt);
                    }
                }

                return o.FileName;
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("(!) Ошибка при формировании {0} по причине: {1}", 
                    ((OstatkiNew) ? "ostatki_new" : "ostatki"),
                    ex.Message));
                return "";
            }
        }
        #endregion
        #region Формирование Adres_delivery.dbf
        public string CreateAdresDelivery()
        {
            try
            {
                AdresDelivery o = new AdresDelivery();

                using (FbConnection conn = new FbConnection(getConnectionString()))
                {
                    conn.Open();

                    using (FbCommand cmd = new FbCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT kvu_name, client_id, orgname, inn, code, code_pm, adres, zone_name, priority, is_delivery, numb_reg_cln FROM sp$get_adress_kvu(61209758)";
                        cmd.Connection = conn;

                        // 1. Получаем информацию по остаткам
                        using (FbDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                if (dr["IS_DELIVERY"].ToString().Trim().ToUpper() != "ДА")
                                    continue;

                                o.AddItem(dr);
                            }
                            dr.Close();
                        }

                        // 2. Формируем файл
                        o.CreateDbf();
                    }
                }

                return o.GetAnswerFile();
            }
            catch (Exception ex)
            {
                Log.Write(String.Format("(!) Ошибка при формировании {0} по причине: {1}",
                    "Adres_delivery.dbf",
                    ex.Message));
                return "";
            }
        }
        #endregion
        #region Загрузка резервов
        /// <summary>
        /// Постановка резерва
        /// </summary>
        /// <param name="rez"></param>
        /// <param name="ftp"></param>
        public void Rez(Rez rez, FtpCron ftp)
        {
            using (FbConnection conn = new FbConnection(getConnectionString()))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    Log.Write($"Ошибка подключения (Rez): {ex.Message}");
                    return;
                }

                using (FbTransaction tr = conn.BeginTransaction())
                {
                    using (FbCommand cmd = new FbCommand())
                    {
                        try
                        {
                            int execRows = 0;

                            //---------------------------------------------------------------------------------------------------------------------
                            // 1. Заливаем файл в нашу таблицу

                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = @"insert into CRON$RESERVE_TABLE (IDGES, IDP, KOL, RESERVEID, NZAK, N) values(@IDGES, @IDP, @KOL, @RESERVEID, @NZAK, @N);;";
                            cmd.Prepare();

                            int n = 0;
                            foreach (Rez.Item item in rez.Items)
                            {
                                n++;
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@IDGES", FbDbType.Integer).Value = Int32.Parse(item.IDGES);
                                cmd.Parameters.Add("@IDP", FbDbType.Integer).Value = Int32.Parse(item.IDP);
                                cmd.Parameters.Add("@KOL", FbDbType.Integer).Value = Int32.Parse(item.KOL);
                                cmd.Parameters.Add("@RESERVEID", FbDbType.VarChar, 80).Value = item.ID_RESERV;
                                cmd.Parameters.Add("@NZAK", FbDbType.Integer).Value = Int32.Parse(item.ID_RESERV);
                                cmd.Parameters.Add("@N", FbDbType.Integer).Value = n;

                                execRows = cmd.ExecuteNonQuery();
                            }

                            //---------------------------------------------------------------------------------------------------------------------
                            // 2. Записываем в очередь

                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = "insert into CRON$QUEUE_RESERV (RESERVEID, NDOC, FLAG) values (@RESERVEID, @NDOC, -1);";
                            cmd.Prepare();
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@RESERVEID", FbDbType.VarChar, 80).Value = rez.Items[0].ID_RESERV;
                            cmd.Parameters.Add("@NDOC", FbDbType.Integer).Value = Int32.Parse(rez.Items[0].ID_RESERV);

                            execRows = cmd.ExecuteNonQuery();

                            //---------------------------------------------------------------------------------------------------------------------
                            // 3. Создаем резерв в модуле приема заказов

                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = "execute procedure SP$CRON_MAKE_6951 (@RESERVEID, -@RESERVEID);";
                            cmd.Prepare();
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@RESERVEID", FbDbType.Integer).Value = Int32.Parse(rez.Items[0].ID_RESERV);

                            execRows = cmd.ExecuteNonQuery();

                            //---------------------------------------------------------------------------------------------------------------------
                            // 4. Формируем файл ответа

                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = "select idges, idp, kol, 0 as ages, '' as adr, '' as urlico, reserveid from CRON$RESERVE_TABLE where RESERVEID = @RESERVEID;";
                            cmd.Prepare();
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@RESERVEID", FbDbType.VarChar, 80);
                            cmd.Parameters[0].Value = rez.Items[0].ID_RESERV;

                            FbDataReader dr = cmd.ExecuteReader();
                            while (dr.Read())
                            {
                                string[] q = new string[dr.FieldCount]; // +1 служебное
                                                                        // Для каждой записи, выводим каждое значение поля
                                for (int i = 0; i < dr.FieldCount; i++)
                                    q[i] = dr.GetString(i);

                                rez.AnswerItems.Add(new Rez.Item { IDGES = q[0], IDP = q[1], KOL = q[2], AGES = q[3], ADR = q[4], URLICO = q[5], ID_RESERV = q[6] });
                            }

                            rez.CreateDbf();

                            //---------------------------------------------------------------------------------------------------------------------
                            // 5. Отправляем файл ответа на FTP

                            try
                            {
                                ftp.ToRezAnswer(rez.AnswerDir + rez.GetAnswerFile());
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Ошибка! {0}", ex.Message);
                            }

                            //---------------------------------------------------------------------------------------------------------------------
                            // 6. Отмечаем резерв

                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = "update CRON$RESERVE_TABLE set flag = 0 where RESERVEID = @RESERVEID;";
                            cmd.Prepare();
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@RESERVEID", FbDbType.VarChar, 80).Value = rez.Items[0].ID_RESERV;

                            execRows = cmd.ExecuteNonQuery();

                            //---------------------------------------------------------------------------------------------------------------------
                            // Подкатываем транзакцию и закрываем соединение
                            tr.Commit();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Ошибка! {0}", ex.Message);
                            tr.Rollback();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Снятие с резерва
        /// </summary>
        /// <param name="IdReserve"></param>
        public void SnRez(int IdReserve)
        {
            using (FbConnection conn = new FbConnection(getConnectionString()))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    Log.Write($"Ошибка подключения (SnRez): {ex.Message}");
                    return;
                }

                using (FbTransaction tr = conn.BeginTransaction())
                {
                    using (FbCommand cmd = new FbCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "delete from doc where dint2 = @idrezerv and dconcept = 6951 and did8 = 61155823 and did1 = 61197863;";
                        cmd.Connection = conn;
                        cmd.Transaction = tr;
                        cmd.Parameters.Add("@idrezerv", FbDbType.Integer).Value = IdReserve;

                        try
                        {
                            int execRows = cmd.ExecuteNonQuery();

                            // Подкатываем транзакцию и закрываем соединение
                            tr.Commit();
                        }
                        catch (Exception ex)
                        {
                            tr.Rollback();
                            Log.Write($"Ошибка (SnRez): {ex.Message}");
                        }
                    }
                }
            }
        }
        #endregion
    }
}
