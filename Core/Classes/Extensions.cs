using System;
using System.Data;
using FirebirdSql.Data.FirebirdClient;
using System.Text;
using System.Globalization;


namespace Extensions
{
    public static class ExtensionsClass
    {
        /// <summary>
        /// Получает параметр по имени
        /// </summary>
        /// <param name="sql">FbCommand</param>
        /// <param name="name">Наименование параметра (регистр не важен)</param>
        /// <returns>FbParameter</returns>
        public static FbParameter ParamByName(this FbCommand sql, string name)
        {
            for (int i = 0; i < sql.Parameters.Count; i++)
                if (sql.Parameters[i].ParameterName.ToLower() == name.ToLower())
                    return sql.Parameters[i];

            return null;
        }

        /// <summary>
        /// Получает параметр по имени
        /// </summary>
        /// <param name="Parameters">Список параметров</param>
        /// <param name="name">Наименование параметра (регистр не важен)</param>
        /// <returns></returns>
        public static FbParameter GetByName(this FbParameterCollection Parameters, string name)
        {
            for (int i = 0; i < Parameters.Count; i++)
                if (Parameters[i].ParameterName.ToLower() == name.ToLower())
                    return Parameters[i];

            return null;
        }

        /// <summary>
        /// Получает значение ячейки по имени поля
        /// </summary>
        /// <param name="row">Строка данных</param>
        /// <param name="name">Наименование поля (регистр не важен)</param>
        /// <returns>object</returns>
        public static object FldByName(this DataRow row, string name)
        {
            for (int i = 0; i < row.Table.Columns.Count; i++)
                if (row.Table.Columns[i].ColumnName.ToLower() == name.ToLower())
                    return row.ItemArray[i];

            return null;
        }

        /// <summary>
        /// Получает значение ячейки по имени поля (Текст по формату 'value')
        /// </summary>
        /// <param name="row">Строка данных</param>
        /// <param name="name">Наименование поля (регистр не важен)</param>
        /// <returns>object</returns>
        public static string FldSf(this DataRow row, string name)
        {
            for (int i = 0; i < row.Table.Columns.Count; i++)
                if (row.Table.Columns[i].ColumnName.ToLower() == name.ToLower())
                    return "'" + row.ItemArray[i].ToString().Trim(' ').Replace("'", "''") + "'";

            return "";
        }

        /// <summary>
        /// Получение значения первой строки набора данных по имени ячейки
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object FldByName(this DataSet data, string name)
        {
            int col = data.Tables[0].Columns.IndexOf(name);
            return data.Tables[0].Rows[0].ItemArray[col];
        }

        /// <summary>
        /// Изменение значения ячейки в первой строке набора данных (не запись в базу!!!)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="colName"></param>
        /// <param name="value"></param>
        public static void SetFieldByName(this DataSet data, string colName, string newValue)
        {
            int col = data.Tables[0].Columns.IndexOf(colName);
            Type dt = data.Tables[0].Columns[col].DataType;
            data.Tables[0].Rows[0].SetField(col, ((!String.IsNullOrEmpty(newValue)) ? Convert.ChangeType(newValue, dt) : null));
        }

        /// <summary>
        /// Получаем индекс колонки
        /// </summary>
        /// <param name="cols"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetIndexColumn(this DataColumnCollection cols, string name)
        {
            for (int i = 0; i < cols.Count; i++)
                if (cols[i].ColumnName.ToLower().Trim() == name.ToLower().Trim())
                    return i;
            return -1;
        }

        public static string AnsiToOem(this string text)
        {
            Encoding ansi = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
            Encoding oem = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
            return oem.GetString(ansi.GetBytes(text));
        }

        public static string OemToAnsi(this string text)
        {
            Encoding ansi = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
            Encoding oem = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
            return ansi.GetString(oem.GetBytes(text));
        }

        public static string FixFilePath(this string s)
        {
            return s.Replace("\\\\", "\\");
        }
    }
}