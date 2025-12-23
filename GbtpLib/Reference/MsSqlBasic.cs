using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MsSqlProcessor.MsSql
{
    public static class MssqlBasic
    {
        private static string _connectionString = "server=ubizit.kr,61433;uid=BRM_GBTP;pwd=BRM_GBTP;database=BRM_GBTP";

        public static void SetConnectionString(string server, string port, string uid, string pwd, string database)
        {
            _connectionString = $"server={server},{port};uid={uid};pwd={pwd};database={database}";
        }

        public static SqlConnection Open()
        {
            try
            {
                var connection = new SqlConnection(_connectionString);
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB Open Error] {ex.Message}");
                return null;
            }
        }

        public static void Close(SqlConnection connection)
        {
            if (connection == null) return;

            try
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB Close Error] {ex.Message}");
            }
        }

        public static List<T> Select<T>(string mainTableName, MssqlSelectQueryBuilder queryBuilder) where T : new()
        {
            List<T> resultList = new List<T>();

            using (var connection = Open())
            {
                if (connection == null)
                    return resultList;

                try
                {
                    string query = queryBuilder.BuildSelectQuery(mainTableName);
                    List<SqlParameter> parameters = queryBuilder.GetParameters();

                    using (var command = new SqlCommand(query, connection))
                    {
                        if (parameters != null && parameters.Count > 0)
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            var properties = typeof(T).GetProperties();

                            while (reader.Read())
                            {
                                T item = new T();

                                foreach (var prop in properties)
                                {
                                    if (!reader.HasColumn(prop.Name))
                                        continue;

                                    object value = reader[prop.Name];

                                    if (value == DBNull.Value)
                                        continue;

                                    Type propType = prop.PropertyType;

                                    if (propType.IsPrimitive || propType == typeof(string) || propType == typeof(decimal) || propType == typeof(DateTime) || propType == typeof(DateTime?) || propType.IsEnum)
                                    {
                                        if (propType == typeof(DateTime?) && value is DateTime dt)
                                            prop.SetValue(item, dt);
                                        else if (propType.IsEnum)
                                            prop.SetValue(item, Enum.Parse(propType, value.ToString()));
                                        else
                                            prop.SetValue(item, Convert.ChangeType(value, propType));
                                    }
                                    else if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>))
                                    {
                                        string json = value.ToString();
                                        if (!string.IsNullOrWhiteSpace(json))
                                        {
                                            var deserialized = JsonConvert.DeserializeObject(json, propType);
                                            prop.SetValue(item, deserialized);
                                        }
                                        else
                                        {
                                            prop.SetValue(item, Activator.CreateInstance(propType)); // 빈 리스트로 초기화
                                        }
                                    }
                                    else
                                    {
                                        prop.SetValue(item, Convert.ChangeType(value, prop.PropertyType));
                                    }
                                }

                                resultList.Add(item);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DB Select Error] {ex.Message}");
                    resultList = new List<T>();
                }
            }

            return resultList;
        }

        public static int Insert<T>(MssqlInsertQueryBuilder insertBuilder)
        {
            using (var connection = Open())
            {
                if (connection == null)
                    return -1;

                try
                {
                    string query = insertBuilder.BuildInsertQuery();
                    var parameters = insertBuilder.GetParameters();

                    using (var command = new SqlCommand(query, connection))
                    {
                        if (parameters.Any())
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                        }

                        // 기본적으로 ExecuteNonQuery 사용
                        int affectedRows = command.ExecuteNonQuery();
                        return affectedRows;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Insert Error] {ex.Message}");
                    return -1;
                }
            }
        }

        public static int Update<T>(MssqlUpdateQueryBuilder updateBuilder)
        {
            using (var connection = Open())
            {
                if (connection == null)
                    return -1;

                try
                {
                    string query = updateBuilder.BuildUpdateQuery();
                    var parameters = updateBuilder.GetParameters();

                    using (var command = new SqlCommand(query, connection))
                    {
                        if (parameters != null && parameters.Count > 0)
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                        }

                        int affectedRows = command.ExecuteNonQuery();
                        return affectedRows; // 변경된 행 수 반환
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Update Error] {ex.Message}");
                    return -1;
                }
            }
        }

        public static int Delete<T>(MssqlDeleteQueryBuilder deleteBuilder)
        {
            using (var connection = Open())
            {
                if (connection == null)
                    return -1;

                try
                {
                    string query = deleteBuilder.BuildDeleteQuery();
                    var parameters = deleteBuilder.GetParameters();

                    using (var command = new SqlCommand(query, connection))
                    {
                        if (parameters != null && parameters.Count > 0)
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                        }

                        int affectedRows = command.ExecuteNonQuery();
                        return affectedRows; // 삭제된 행 수 반환
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Delete Error] {ex.Message}");
                    return -1;
                }
            }
        }

        public static bool ExecuteStoredProcedure(string procedureName, Dictionary<string, object> parameters)
        {
            using (var connection = Open())
            {
                if (connection == null)
                    return false;

                try
                {
                    using (var command = new SqlCommand(procedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            foreach (var kvp in parameters)
                            {
                                command.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                            }
                        }

                        command.ExecuteNonQuery();

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ExecuteStoredProcedure Error] {ex.Message}");
                    return false;
                }
            }
        }
    }

    public class MssqlSelectQueryBuilder
    {
        private List<string> _selectColumns = new List<string>();
        private List<string> _whereConditions = new List<string>();
        private List<SqlParameter> _whereParameters = new List<SqlParameter>();
        private List<string> _joinConditions = new List<string>();

        public void Clear()
        {
            _selectColumns.Clear();
            _whereConditions.Clear();
            _whereParameters.Clear();
            _joinConditions.Clear();
        }

        public void AddAllColumns<T>()
        {
            string tableName = typeof(T).Name;

            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                _selectColumns.Add($"[{tableName}].[{prop.Name}]");
            }
        }

        public void AddSelectColumns<T>(params string[] propertyNames)
        {
            string tableName = typeof(T).Name;

            if (propertyNames == null || propertyNames.Length == 0)
            {
                foreach (var prop in typeof(T).GetProperties())
                {
                    _selectColumns.Add($"[{tableName}].[{prop.Name}]");
                }
            }
            else
            {
                foreach (var name in propertyNames)
                {
                    _selectColumns.Add($"[{tableName}].[{name}]");
                }
            }
        }

        public void AddWhere(string tableName, string columnName, object value)
        {
            string paramName = $"@param_{_whereParameters.Count}";
            _whereConditions.Add($"[{tableName}].[{columnName}] = {paramName}");
            _whereParameters.Add(new SqlParameter(paramName, value ?? DBNull.Value));
        }

        public void AddWhereCondition_IsNotNull(string tableName, string columnName)
        {
            _whereConditions.Add($"[{tableName}].[{columnName}] IS NOT NULL");
        }

        public void AddLeftJoin(string leftTable, string leftColumn, string rightTable, string rightColumn)
        {
            _joinConditions.Add($"LEFT JOIN [{rightTable}] ON [{leftTable}].[{leftColumn}] = [{rightTable}].[{rightColumn}]");
        }

        public string BuildSelectQuery(string mainTable)
        {
            if (_selectColumns.Count == 0)
                throw new InvalidOperationException("No SELECT columns specified. Use AddSelectColumns<T>() first.");

            StringBuilder builder = new StringBuilder();

            builder.AppendLine("SELECT " + string.Join(", ", _selectColumns));
            builder.AppendLine($"FROM [{mainTable}]");

            foreach (var join in _joinConditions)
            {
                builder.AppendLine(join);
            }

            if (_whereConditions.Count > 0)
            {
                builder.AppendLine("WHERE " + string.Join(" AND ", _whereConditions));
            }

            return builder.ToString();
        }

        public List<SqlParameter> GetParameters()
        {
            return _whereParameters;
        }
    }

    public class MssqlInsertQueryBuilder
    {
        private readonly string _tableName;
        private readonly List<string> _columns = new List<string>();
        private readonly List<string> _values = new List<string>();
        private readonly List<SqlParameter> _parameters = new List<SqlParameter>();

        public MssqlInsertQueryBuilder(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));

            _tableName = tableName;
        }

        public void AddColumn(string columnName, object value)
        {
            string paramName = $"@param_{_parameters.Count}";
            _columns.Add($"[{columnName}]");
            _values.Add(paramName);

            object dbValue;

            if (value == null)
            {
                dbValue = DBNull.Value;
            }
            else if (value.GetType().IsEnum)
            {
                dbValue = value.ToString(); // enum → string
            }
            else if (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(List<>))
            {
                dbValue = JsonConvert.SerializeObject(value); // list → json string
            }
            else
            {
                dbValue = value;
            }

            _parameters.Add(new SqlParameter(paramName, dbValue));
        }

        public void AddAllColumnsFrom<T>(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            foreach (var prop in typeof(T).GetProperties())
            {
                object value = prop.GetValue(entity);

                AddColumn(prop.Name, value);
            }
        }

        public string BuildInsertQuery()
        {
            if (!_columns.Any())
                throw new InvalidOperationException("No columns specified for INSERT.");

            var builder = new StringBuilder();
            builder.Append($"INSERT INTO [{_tableName}] ");
            builder.Append($"({string.Join(", ", _columns)}) ");
            builder.Append($"VALUES ({string.Join(", ", _values)});");

            return builder.ToString();
        }

        public List<SqlParameter> GetParameters() => _parameters;
    }

    public class MssqlUpdateQueryBuilder
    {
        private readonly string _tableName;

        private readonly List<string> _setClauses = new List<string>();
        private readonly List<string> _whereClauses = new List<string>();
        private readonly List<SqlParameter> _parameters = new List<SqlParameter>();

        public MssqlUpdateQueryBuilder(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));

            _tableName = tableName;
        }

        public void AddSet(string columnName, object value)
        {
            string paramName = GetNextParameterName("param_set");
            _setClauses.Add($"[{columnName}] = {paramName}");
            _parameters.Add(new SqlParameter(paramName, ConvertToDbValue(value)));
        }

        public void AddWhere(string columnName, object value)
        {
            string paramName = GetNextParameterName("param_where");
            _whereClauses.Add($"[{columnName}] = {paramName}");
            _parameters.Add(new SqlParameter(paramName, ConvertToDbValue(value)));
        }

        public string BuildUpdateQuery()
        {
            if (_setClauses.Count == 0)
                throw new InvalidOperationException("SET clause is required for an UPDATE query.");

            var builder = new StringBuilder();

            builder.AppendLine($"UPDATE [{_tableName}]");
            builder.AppendLine("SET " + string.Join(", ", _setClauses));

            if (_whereClauses.Count > 0)
            {
                builder.AppendLine("WHERE " + string.Join(" AND ", _whereClauses));
            }

            return builder.ToString();
        }

        public List<SqlParameter> GetParameters() => _parameters;

        private string GetNextParameterName(string prefix)
        {
            return $"@{prefix}_{_parameters.Count}";
        }

        private object ConvertToDbValue(object value)
        {
            if (value == null)
                return DBNull.Value;

            var type = value.GetType();

            if (type.IsEnum)
                return value.ToString();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return JsonConvert.SerializeObject(value);

            return value;
        }
    }

    public class MssqlDeleteQueryBuilder
    {
        private readonly string _tableName;
        private readonly List<string> _whereConditions = new List<string>();
        private readonly List<SqlParameter> _parameters = new List<SqlParameter>();

        public MssqlDeleteQueryBuilder(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));

            _tableName = tableName;
        }

        public void AddWhere(string columnName, object value)
        {
            string paramName = $"@param_{_parameters.Count}";

            object dbValue;
            if (value == null)
            {
                dbValue = DBNull.Value;
            }
            else if (value.GetType().IsEnum)
            {
                dbValue = value.ToString(); // Enum → 문자열로 저장
            }
            else if (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(List<>))
            {
                dbValue = JsonConvert.SerializeObject(value); // List → JSON 문자열로 저장
            }
            else
            {
                dbValue = value;
            }

            _whereConditions.Add($"[{columnName}] = {paramName}");
            _parameters.Add(new SqlParameter(paramName, dbValue));
        }

        public string BuildDeleteQuery()
        {
            if (!_whereConditions.Any())
                throw new InvalidOperationException("Delete requires at least one WHERE condition to prevent full table deletion.");

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"DELETE FROM [{_tableName}]");

            builder.AppendLine("WHERE " + string.Join(" AND ", _whereConditions));

            return builder.ToString();
        }

        public List<SqlParameter> GetParameters()
        {
            return _parameters;
        }
    }

    public static class SqlDataReaderExtensions
    {
        public static bool HasColumn(this SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static class MssqlProcedure
    {
        public static string GetCode_BatteryState(string COMM_CD_NM)
        {
            try
            {
                var selectQueryBuilder = new MssqlSelectQueryBuilder();
                selectQueryBuilder.AddSelectColumns<MST_CODE>(
                    new string[]
                    {
                        nameof(MST_CODE.COMM_CD),
                    });
                selectQueryBuilder.AddWhere(nameof(MST_CODE), nameof(MST_CODE.COMM_CD_GROUP), "STS003");
                selectQueryBuilder.AddWhere(nameof(MST_CODE), nameof(MST_CODE.COMM_CD_NM), COMM_CD_NM);
                var mstCodeList = MssqlBasic.Select<MST_CODE>(nameof(MST_CODE), selectQueryBuilder);
                var mstCode = mstCodeList.First();

                return mstCode.COMM_CD;
            }
            catch
            {
                Debug.WriteLine($"배터리 상태 가져오기 실패. (Battery State : {COMM_CD_NM})");

                return string.Empty;
            }
        }

        public static string GetCode_IncomeDivision(string COMM_CD_NM)
        {
            try
            {
                var selectQueryBuilder = new MssqlSelectQueryBuilder();
                selectQueryBuilder.AddSelectColumns<MST_CODE>(
                    new string[]
                    {
                        nameof(MST_CODE.COMM_CD),
                    });
                selectQueryBuilder.AddWhere(nameof(MST_CODE), nameof(MST_CODE.COMM_CD_GROUP), "INV001");
                selectQueryBuilder.AddWhere(nameof(MST_CODE), nameof(MST_CODE.COMM_CD_NM), COMM_CD_NM);
                var mstCodeList = MssqlBasic.Select<MST_CODE>(nameof(MST_CODE), selectQueryBuilder);
                var mstCode = mstCodeList.First();

                return mstCode.COMM_CD;
            }
            catch
            {
                Debug.WriteLine($"Error: GetCode_IncomeDivision (IncomeDivision : {COMM_CD_NM})");

                return string.Empty;
            }
        }
    }

    public enum EIF_CMD
    {
        AA0, AA1, AA2, AA3, AA4, AA5, HH3, HH4,
        EE1, EE2, EE3, EE4, EE5, EE6, EE7, EE8,
        GG1,
        UNUSED,
    }

    public enum EIF_FLAG
    {
        C, Y,
    }

    public enum EYN_FLAG
    {
        Y, N,
    }

    public enum EInspectionResult
    {
        None,
        Pass,
        Fail,
    }

    public class MST_USER_INFO
    {
        public string SITE_CD { get; set; } = string.Empty;
        public string USER_ID { get; set; } = string.Empty;
        public string USER_GROUP_CD { get; set; } = string.Empty;
        public string USER_NM { get; set; } = string.Empty;
        public string DEPT_CD { get; set; } = string.Empty;
        public string PWD { get; set; } = string.Empty;
        public string ENTRY_DAT { get; set; } = string.Empty;
        public string RETIRED_DAT { get; set; } = string.Empty;
        public string SEX { get; set; } = string.Empty;
        public int LIST_ORDER { get; set; } = 0;
        public string USE_YN { get; set; } = string.Empty;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class QLT_BTR_INOUT_INSP
    {
        public string SITE_CD { get; set; } = string.Empty;
        public string FACT_CD { get; set; } = string.Empty;
        public string PRCS_CD { get; set; } = string.Empty;
        public string MC_CD { get; set; } = string.Empty;
        public string INSP_KIND_GROUP_CD { get; set; } = string.Empty;
        public string INSP_KIND_CD { get; set; } = string.Empty;
        public string LBL_ID { get; set; } = string.Empty;
        public string INSP_SEQ { get; set; } = string.Empty;
        public string INSP_VAL { get; set; } = string.Empty;
        public EInspectionResult INSP_RESULT { get; set; } = EInspectionResult.None;
        public DateTime? INSP_STA_DT { get; set; } = null;
        public DateTime? INSP_END_DT { get; set; } = null;
        public string NOTE { get; set; } = string.Empty;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class QLT_BTR_INSP
    {
        public string SITE_CD { get; set; } = string.Empty;
        public string INSP_KIND_CD { get; set; } = string.Empty;
        public string LBL_ID { get; set; } = string.Empty;
        public string INSP_SEQ { get; set; } = string.Empty;
        public string INSP_GRD { get; set; } = string.Empty;
        public string BTR_DIAG_STS { get; set; } = string.Empty;
    }

    public class ITF_CMD_DATA
    {
        public string IF_UID { get; set; } = string.Empty;
        public string IF_DATE { get; set; } = string.Empty;
        public EIF_CMD CMD_CD { get; set; } = EIF_CMD.UNUSED;
        public string DATA1 { get; set; } = string.Empty;
        public string DATA2 { get; set; } = string.Empty;
        public string DATA3 { get; set; } = string.Empty;
        public string DATA4 { get; set; } = string.Empty;
        public string DATA5 { get; set; } = string.Empty;
        public string DATA6 { get; set; } = string.Empty;
        public string DATA7 { get; set; } = string.Empty;
        public string DATA8 { get; set; } = string.Empty;
        public string DATA9 { get; set; } = string.Empty;
        public string DATA10 { get; set; } = string.Empty;
        public EIF_FLAG IF_FLAG { get; set; } = EIF_FLAG.C;
        public DateTime? REQ_TIME { get; set; } = null;
        public DateTime? RES_TIME { get; set; } = null;
        public string REQ_SYS { get; set; } = string.Empty;
        public string RES_SYS { get; set; } = string.Empty;
    }

    public class MST_CAR_MAKE
    {
        public string CAR_MAKE_CD { get; set; } = string.Empty;
        public string CAR_MAKE_NM { get; set; } = string.Empty;
        public int LIST_ORDER { get; set; } = int.MaxValue;
        public EYN_FLAG USE_YN { get; set; } = EYN_FLAG.N;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class MST_CAR
    {
        public string CAR_MAKE_CD { get; set; } = string.Empty;
        public string CAR_CD { get; set; } = string.Empty;
        public string CAR_NM { get; set; } = string.Empty;
        public int LIST_ORDER { get; set; } = int.MaxValue;
        public EYN_FLAG USE_YN { get; set; } = EYN_FLAG.N;
        public string RELS_YEAR { get; set; } = string.Empty;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class MST_BTR_TYPE
    {
        public int BTR_TYPE_NO { get; set; } = int.MaxValue;
        public string CAR_MAKE_CD { get; set; } = string.Empty;
        public string CAR_CD { get; set; } = string.Empty;
        public string BTR_MAKE_CD { get; set; } = string.Empty;
        public string BTR_TYPE_SLT_CD { get; set; } = string.Empty;
        public string BTR_TYPE_CD { get; set; } = string.Empty;
        public string BTR_TYPE_NM { get; set; } = string.Empty;
        public string CAR_RELS_YEAR { get; set; } = string.Empty;
        public string PACK_MDLE_CD { get; set; } = string.Empty;
        public double CAPA_VALUE { get; set; } = 0.0;
        public double NOMINL_VALUE { get; set; } = 0.0;
        public double CHARGE_VALUE { get; set; } = 0.0;
        public double DISCRG_VALUE { get; set; } = 0.0;
        public double VOLT_MAX_VALUE { get; set; } = 0.0;
        public double VOLT_MIN_VALUE { get; set; } = 0.0;
        public double ACIR_MAX_VALUE { get; set; } = 0.0;
        public double DCIR_MAX_VALUE { get; set; } = 0.0;
        public double INSUL_MIN_VALUE { get; set; } = 0.0;
        public string CELL_QTY { get; set; } = string.Empty;
        public string BTR_TYPE_NM_CELL { get; set; } = string.Empty;
        public string CONN_TYPE { get; set; } = string.Empty;
        public string IMAGE_NM { get; set; } = string.Empty;
        public BitmapImage IMAGE_FILE { get; set; } = null;
        public int LIST_ORDER { get; set; } = int.MaxValue;
        public EYN_FLAG USE_YN { get; set; } = EYN_FLAG.N;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class MST_BTR_MAKE
    {
        public string BTR_MAKE_CD { get; set; } = string.Empty;
        public string BTR_MAKE_NM { get; set; } = string.Empty;
        public int LIST_ORDER { get; set; } = int.MaxValue;
        public EYN_FLAG USE_YN { get; set; } = EYN_FLAG.N;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class MST_BTR
    {
        public string LBL_ID { get; set; } = string.Empty;
        public int BTR_TYPE_NO { get; set; } = int.MaxValue;
        public string PACK_MDLE_CD { get; set; } = string.Empty;
        public string SITE_CD { get; set; } = string.Empty;
        public string COLT_DAT { get; set; } = string.Empty;
        public string COLT_RESN { get; set; } = string.Empty;
        public string MUFT_DAT { get; set; } = string.Empty;
        public int MILE { get; set; } = 0;
        public int VER { get; set; } = 0;
        public string NOTE { get; set; } = string.Empty;
        public EYN_FLAG PRT_YN { get; set; } = EYN_FLAG.N;
        public string BTR_STS { get; set; } = "01";
        public EYN_FLAG STO_INSP_FLAG { get; set; } = EYN_FLAG.N;
        public DateTime? STO_INSP_END_DTM { get; set; } = null;
        public EYN_FLAG ENE_INSP_FLAG { get; set; } = EYN_FLAG.N;
        public int ENE_INSP_CNT { get; set; } = 0;
        public DateTime? ENE_INSP_END_DTM { get; set; } = null;
        public EYN_FLAG DIG_INSP_FLAG { get; set; } = EYN_FLAG.N;
        public DateTime? DIG_INSP_END_DTM { get; set; } = null;
        public EYN_FLAG EMG_OUT_FLAG { get; set; } = EYN_FLAG.N;
        public string EMG_OUT_NOTE { get; set; } = string.Empty;
        public EYN_FLAG USE_YN { get; set; } = EYN_FLAG.N;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class MST_CODE_GROUP
    {
        public string COMM_CD_GROUP { get; set; } = string.Empty;
        public string COMM_CD_GROUP_NM { get; set; } = string.Empty;
        public string COMM_CD_GROUP_NM_LANG1 { get; set; } = string.Empty;
        public string COMM_CD_GROUP_NM_LANG2 { get; set; } = string.Empty;
        public string COMM_CD_GROUP_NM_LANG3 { get; set; } = string.Empty;
        public int LIST_ORDER { get; set; } = int.MaxValue;
        public EYN_FLAG USE_YN { get; set; } = EYN_FLAG.N;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class MST_CODE
    {
        public string COMM_CD_GROUP { get; set; } = string.Empty;
        public string COMM_CD { get; set; } = string.Empty;
        public string COMM_CD_NM { get; set; } = string.Empty;
        public string COMM_CD_NM_LANG1 { get; set; } = string.Empty;
        public string COMM_CD_NM_LANG2 { get; set; } = string.Empty;
        public string COMM_CD_NM_LANG3 { get; set; } = string.Empty;
        public int LIST_ORDER { get; set; } = int.MaxValue;
        public string REMARK { get; set; } = string.Empty;
        public EYN_FLAG USE_YN { get; set; } = EYN_FLAG.N;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class ITF_SYS_CON_CHECK
    {
        public string SYS_CD { get; set; } = "IMS";
        public int CON_FLAG { get; set; } = 0;
        public DateTime? RES_TIME { get; set; } = null;
    }

    public class MST_SITE
    {
        public string SITE_CD { get; set; } = string.Empty;
        public string SITE_NM { get; set; } = string.Empty;
        public int LIST_ORDER { get; set; } = int.MaxValue;
        public EYN_FLAG USE_YN { get; set; } = EYN_FLAG.N;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class MST_FACTORY
    {
        public string SITE_CD { get; set; } = string.Empty;
        public string FACT_CD { get; set; } = string.Empty;
        public string FACT_NM { get; set; } = string.Empty;
        public int LIST_ORDER { get; set; } = int.MaxValue;
        public EYN_FLAG USE_YN { get; set; } = EYN_FLAG.N;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class MST_PROCESS
    {
        public string SITE_CD { get; set; } = string.Empty;
        public string FACT_CD { get; set; } = string.Empty;
        public string PRCS_CD { get; set; } = string.Empty;
        public string PRCS_NM { get; set; } = string.Empty;
        public int LIST_ORDER { get; set; } = int.MaxValue;
        public EYN_FLAG USE_YN { get; set; } = EYN_FLAG.N;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class MST_MACHINE
    {
        public string SITE_CD { get; set; } = string.Empty;
        public string FACT_CD { get; set; } = string.Empty;
        public string MC_CD { get; set; } = string.Empty;
        public string MC_NM { get; set; } = string.Empty;
        public string MC_TYPE { get; set; } = string.Empty;
        public string POP_CD { get; set; } = string.Empty;
        public int LIST_ORDER { get; set; } = int.MaxValue;
        public EYN_FLAG USE_YN { get; set; } = EYN_FLAG.N;
        public string NOTE { get; set; } = string.Empty;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class MST_INSP_KIND_GROUP
    {
        public string INSP_KIND_GROUP_CD { get; set; } = string.Empty;
        public string INSP_KIND_GROUP_NM { get; set; } = string.Empty;
        public int LIST_ORDER { get; set; } = int.MaxValue;
        public EYN_FLAG USE_YN { get; set; } = EYN_FLAG.N;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class MST_INSP_KIND
    {
        public string INSP_KIND_GROUP_CD { get; set; } = string.Empty;
        public string INSP_KIND_CD { get; set; } = string.Empty;
        public string INSP_KIND_NM { get; set; } = string.Empty;
        public int LIST_ORDER { get; set; } = int.MaxValue;
        public EYN_FLAG USE_YN { get; set; } = EYN_FLAG.N;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class MST_WAREHOUSE
    {
        public string SITE_CD { get; set; } = string.Empty;
        public string FACT_CD { get; set; } = string.Empty;
        public string WH_CD { get; set; } = string.Empty;
        public string WH_NM { get; set; } = string.Empty;
        public string WH_TYPE { get; set; } = string.Empty;
        public string WH_DIV { get; set; } = string.Empty;
        public string WH_SVR_LNK { get; set; } = string.Empty;
        public EYN_FLAG USE_YN { get; set; } = EYN_FLAG.N;
        public string REG_ID { get; set; } = string.Empty;
        public DateTime? REG_DTM { get; set; } = null;
        public string MOD_ID { get; set; } = string.Empty;
        public DateTime? MOD_DTM { get; set; } = null;
    }

    public class INV_WAREHOUSE
    {
        public string SITE_CD { get; set; } = string.Empty;
        public string FACT_CD { get; set; } = string.Empty;
        public string WH_CD { get; set; } = string.Empty;
        public string ROW { get; set; } = string.Empty;
        public string COL { get; set; } = string.Empty;
        public string LVL { get; set; } = string.Empty;
        public string LBL_ID { get; set; } = string.Empty;
        public string LOAD_GRD { get; set; } = string.Empty;
        public string STORE_DIV { get; set; } = string.Empty;
        public string STS { get; set; } = string.Empty;
        public string NOTE { get; set; } = string.Empty;
    }
}
