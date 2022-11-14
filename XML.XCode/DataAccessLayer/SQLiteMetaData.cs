using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Reflection;
using XML.Core;

namespace XML.XCode.DataAccessLayer;

/// <summary>SQLite元数据</summary>
internal class SQLiteMetaData : FileDbMetaData
{
    public SQLiteMetaData() => Types = _DataTypes;

    #region 数据类型
    protected override List<KeyValuePair<Type, Type>> FieldTypeMaps
    {
        get
        {
            if (_FieldTypeMaps == null)
            {
                var list = base.FieldTypeMaps;
                // SQLite自增字段有时是Int64，需要到Int32的映射
                if (!list.Any(e => e.Key == typeof(Int64) && e.Value == typeof(Int32)))
                    list.Add(new KeyValuePair<Type, Type>(typeof(Int64), typeof(Int32)));
            }
            return base.FieldTypeMaps;
        }
    }

    /// <summary>数据类型映射</summary>
    private static readonly Dictionary<Type, String[]> _DataTypes = new()
    {
        { typeof(Byte[]), new String[] { "binary", "varbinary", "blob", "image", "general", "oleobject" } },
        { typeof(Guid), new String[] { "uniqueidentifier", "guid" } },
        { typeof(Boolean), new String[] { "bit", "yesno", "logical", "bool", "boolean" } },
        { typeof(Byte), new String[] { "tinyint" } },
        { typeof(Int16), new String[] { "smallint" } },
        { typeof(Int32), new String[] { "int" } },
        { typeof(Int64), new String[] { "integer", "counter", "autoincrement", "identity", "long", "bigint" } },
        { typeof(Single), new String[] { "single" } },
        { typeof(Double), new String[] { "real", "float", "double" } },
        { typeof(Decimal), new String[] { "decimal", "money", "currency", "numeric" } },
        { typeof(DateTime), new String[] { "datetime", "smalldate", "timestamp", "date", "time" } },
        { typeof(String), new String[] { "nvarchar({0})", "ntext", "varchar({0})", "memo({0})", "longtext({0})", "note({0})", "text({0})", "string({0})", "char({0})", "char({0})" } }
    };
    #endregion

    #region 构架
    protected override List<IDataTable> OnGetTables(String[] names)
    {
        // 特殊处理内存数据库
        if ((Database as SQLite).IsMemoryDatabase) return memoryTables.Where(t => names.Contains(t.TableName)).ToList();

        //var dt = GetSchema(_.Tables, null);
        //if (dt?.Rows == null || dt.Rows.Count <= 0) return null;

        //// 默认列出所有字段
        //var rows = dt.Select("TABLE_TYPE='table'");
        //if (rows == null || rows.Length <= 0) return null;

        //return GetTables(rows, names);

        var list = new List<IDataTable>();
        var ss = Database.CreateSession();

        var sql = "select * from sqlite_master";
        var ds = ss.Query(sql, null);
        if (ds.Rows.Count == 0) return list;

        var hs = new HashSet<String>(names ?? new String[0], StringComparer.OrdinalIgnoreCase);

        var dts = Select(ds, "type", "table");
        var dis = Select(ds, "type", "index");

        for (var dr = 0; dr < dts.Rows.Count; dr++)
        {
            var name = dts.Get<String>(dr, "tbl_name");
            if (hs.Count > 0 && !hs.Contains(name)) continue;

            var table = DAL.CreateTable();
            table.TableName = name;
            table.DbType = Database.Type;

            sql = dts.Get<String>(dr, "sql");
            var p1 = sql.IndexOf('(');
            var p2 = sql.LastIndexOf(')');
            if (p1 < 0 || p2 < 0) continue;
            sql = sql.Substring(p1 + 1, p2 - p1 - 1);
            if (sql.IsNullOrEmpty()) continue;

            var sqls = sql.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            #region 字段
            //GetTbFields(table);
            foreach (var line in sqls)
            {
                if (line.IsNullOrEmpty() || line.StartsWithIgnoreCase("CREATE")) continue;

                // 处理外键设置
                if (line.Contains("CONSTRAINT") && line.Contains("FOREIGN KEY")) continue;

                var fs = line.Trim().Split(' ');
                var field = table.CreateColumn();

                field.ColumnName = fs[0].TrimStart('[', '"').TrimEnd(']', '"');

                if (line.Contains("AUTOINCREMENT")) field.Identity = true;
                if (line.Contains("Primary Key")) field.PrimaryKey = true;

                if (line.Contains("NOT NULL"))
                    field.Nullable = false;
                else if (line.Contains(" NULL "))
                    field.Nullable = true;

                field.RawType = fs.Length > 1 ? fs[1] : "nvarchar(50)";
                field.Length = field.RawType.Substring("(", ")").ToInt();
                field.DataType = GetDataType(field.RawType);

                // SQLite的字段长度、精度等，都是由类型决定，固定值

                // 如果数据库里面是integer或者autoincrement，识别类型是Int64，又是自增，则改为Int32，保持与大多数数据库的兼容
                if (field.Identity && field.DataType == typeof(Int64) && field.RawType.EqualIgnoreCase("integer", "autoincrement"))
                {
                    field.DataType = typeof(Int32);
                }

                if (field.DataType == null)
                {
                    if (field.RawType.StartsWithIgnoreCase("varchar2", "nvarchar2")) field.DataType = typeof(String);
                }

                field.Fix();

                table.Columns.Add(field);
            }
            #endregion

            #region 索引
            var dis2 = Select(dis, "tbl_name", name);
            for (var i = 0; dis2?.Rows != null && i < dis2.Rows.Count; i++)
            {
                var di = table.CreateIndex();
                di.Name = dis2.Get<String>(i, "name");

                sql = dis2.Get<String>(i, "sql");
                if (sql.IsNullOrEmpty()) continue;

                if (sql.Contains(" UNIQUE ")) di.Unique = true;

                di.Columns = sql.Substring("(", ")").Split(',').Select(e => e.Trim().Trim('[', '"', ']')).ToArray();

                table.Indexes.Add(di);
            }
            #endregion

            //FixTable(table, dr, data);

            // 修正关系数据
            table.Fix();

            list.Add(table);
        }

        return list;
    }

    /// <summary>
    /// 快速取得所有表名
    /// </summary>
    /// <returns></returns>
    public override IList<String> GetTableNames()
    {
        var list = new List<String>();

        var sql = "select * from sqlite_master";
        var dt = base.Database.CreateSession().Query(sql, null);
        if (dt.Rows.Count == 0) return list;

        // 所有表
        foreach (var dr in dt)
        {
            var name = dr["tbl_name"] + "";
            if (!name.IsNullOrEmpty()) list.Add(name);
        }

        return list;
    }

    /// <summary>
    /// 获取表字段 zhangy 2018年10月23日 15:30:43
    /// </summary>
    /// <param name="table"></param>
    public void GetTbFields(IDataTable table)
    {
        var sql = $"PRAGMA table_info({table.TableName})";

        var ss = Database.CreateSession();
        var ds = ss.Query(sql, null);
        if (ds.Rows.Count == 0) return;

        foreach (var row in ds.Rows)
        {
            var field = table.CreateColumn();
            field.ColumnName = row[1].ToString().Replace(" ", "");
            field.RawType = row[2].ToString().Replace(" ", "");//去除所有空格
            field.Nullable = row[3].ToInt() != 1;
            field.PrimaryKey = row[5].ToInt() == 1;

            field.DataType = GetDataType(field.RawType);
            if (field.DataType == null)
            {
                if (field.RawType.StartsWithIgnoreCase("varchar2", "nvarchar2")) field.DataType = typeof(String);
            }
            field.Fix();
            table.Columns.Add(field);
        }
    }

    protected override String GetFieldType(IDataColumn field)
    {
        var typeName = base.GetFieldType(field);

        // 自增字段必须是integer
        // 云飞扬2017-07-19 修改为也支持长整型转成integer
        if (field.Identity && typeName.Contains("int")) return "integer";
        //云飞扬 2017-07-05
        //因为SQLite的text长度比较小，这里设置为默认值
        if (typeName.Contains("text")) return "text";
        return typeName;
    }

    protected override String GetFieldConstraints(IDataColumn field, Boolean onlyDefine)
    {
        // SQLite要求自增必须是主键
        if (field.Identity && !field.PrimaryKey)
        {
            // 取消所有主键
            field.Table.Columns.ForEach(dc => dc.PrimaryKey = false);

            // 自增字段作为主键
            field.PrimaryKey = true;
        }

        var str = base.GetFieldConstraints(field, onlyDefine);

        if (field.Identity) str += " AUTOINCREMENT";

        // 给字符串字段加上忽略大小写，否则admin和Admin是查不出来的
        if (field.DataType == typeof(String)) str += " COLLATE NOCASE";

        return str;
    }
    #endregion

    #region 数据定义
    public override Object SetSchema(DDLSchema schema, params Object[] values)
    {
        switch (schema)
        {
            case DDLSchema.BackupDatabase:
                var dbname = FileName;
                if (!dbname.IsNullOrEmpty()) dbname = Path.GetFileNameWithoutExtension(dbname);
                var file = "";
                if (values != null && values.Length > 0) file = values[0] as String;
                return Backup(dbname, file, false);

            default:
                break;
        }
        return base.SetSchema(schema, values);
    }

    protected override void CreateDatabase()
    {
        if (!(Database as SQLite).IsMemoryDatabase) base.CreateDatabase();
    }

    protected override void DropDatabase()
    {
        if (!(Database as SQLite).IsMemoryDatabase) base.DropDatabase();
    }

    /// <summary>备份文件到目标文件</summary>
    /// <param name="dbname"></param>
    /// <param name="bakfile"></param>
    /// <param name="compressed"></param>
    public override String Backup(String dbname, String bakfile, Boolean compressed)
    {
        var dbfile = FileName;

        // 备份文件
        var bf = bakfile;
        if (bf.IsNullOrEmpty())
        {
            var name = dbname;
            if (name.IsNullOrEmpty()) name = Path.GetFileNameWithoutExtension(dbfile);

            var ext = Path.GetExtension(dbfile);
            if (ext.IsNullOrEmpty()) ext = ".db";

            if (compressed)
                bf = $"{name}{ext}";
            else
                bf = $"{name}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
        }
        if (!Path.IsPathRooted(bf)) bf = XML.Core.Setting.Current.BackupPath.CombinePath(bf).GetBasePath();

        bf = bf.EnsureDirectory(true);

        WriteLog("{0}备份SQLite数据库 {1} 到 {2}", Database.ConnName, dbfile, bf);

        var sw = Stopwatch.StartNew();

        // 删除已有文件
        if (File.Exists(bf)) File.Delete(bf);

        using (var conn = Database.Factory.CreateConnection())
        using (var conn2 = Database.OpenConnection())
        {
            conn.ConnectionString = $"Data Source={bf}";
            conn.Open();

            //conn2.ConnectionString = Database.ConnectionString;
            //conn2.Open();

            //var method = conn.GetType().GetMethodEx("BackupDatabase");
            // 借助BackupDatabase函数可以实现任意两个SQLite之间倒数据，包括内存数据库
            conn2.Invoke("BackupDatabase", conn, "main", "main", -1, null, 0);
        }

        // 压缩
        WriteLog("备份文件大小：{0:n0}字节", bf.AsFile().Length);
        if (compressed)
        {
            var zipfile = Path.ChangeExtension(bf, "zip");
            if (bakfile.IsNullOrEmpty()) zipfile = zipfile.TrimEnd(".zip") + $"_{DateTime.Now:yyyyMMddHHmmss}.zip";

            var fi = bf.AsFile();
            fi.Compress(zipfile);
            WriteLog("压缩后大小：{0:n0}字节，{1}", zipfile.AsFile().Length, zipfile);

            // 删除未备份
            File.Delete(bf);

            bf = zipfile;
        }

        sw.Stop();
        WriteLog("备份完成，耗时{0:n0}ms", sw.ElapsedMilliseconds);

        return bf;
    }

    public override String CreateIndexSQL(IDataIndex index)
    {
        var sb = new StringBuilder(32 + index.Columns.Length * 20);
        if (index.Unique)
            sb.Append("Create Unique Index ");
        else
            sb.Append("Create Index ");

        // SQLite索引优先采用自带索引名
        if (!String.IsNullOrEmpty(index.Name) && index.Name.Contains(index.Table.TableName))
            sb.Append(index.Name);
        else
        {
            // SQLite中不同表的索引名也不能相同
            sb.Append("IX_");
            sb.Append(FormatName(index.Table, false));
            foreach (var item in index.Columns)
            {
                sb.AppendFormat("_{0}", item);
            }
        }

        var dcs = index.Table.GetColumns(index.Columns);
        sb.AppendFormat(" On {0} ({1})", FormatName(index.Table), dcs.Join(", ", FormatName));

        return sb.ToString();
    }

    /// <summary>删除索引方法</summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public override String DropIndexSQL(IDataIndex index) => $"Drop Index {index.Name}";

    protected override String CheckColumnsChange(IDataTable entitytable, IDataTable dbtable, Boolean onlySql, Boolean noDelete)
    {
        foreach (var item in entitytable.Columns)
        {
            // 自增字段必须是主键
            if (item.Identity && !item.PrimaryKey)
            {
                // 取消所有主键
                item.Table.Columns.ForEach(dc => dc.PrimaryKey = false);

                // 自增字段作为主键
                item.PrimaryKey = true;
                break;
            }
        }

        // 把onlySql设为true，让基类只产生语句而不执行
        var sql = base.CheckColumnsChange(entitytable, dbtable, true, false);
        if (sql.IsNullOrEmpty()) return sql;

        // 只有修改字段、删除字段需要重建表
        if (!sql.Contains("Alter Column") && !sql.Contains("Drop Column"))
        {
            if (onlySql) return sql;

            Database.CreateSession().Execute(sql);

            return null;
        }

        var sql2 = sql;

        sql = ReBuildTable(entitytable, dbtable);
        if (sql.IsNullOrEmpty() || onlySql) return sql;

        // 输出日志，说明重建表的理由
        WriteLog("SQLite需要重建表，因无法执行：{0}", sql2);

        var flag = true;
        // 如果设定不允许删
        if (noDelete)
        {
            // 看看有没有数据库里面有而实体库里没有的
            foreach (var item in dbtable.Columns)
            {
                var dc = entitytable.GetColumn(item.ColumnName);
                if (dc == null)
                {
                    flag = false;
                    break;
                }
            }
        }
        if (!flag) return sql;

        //Database.CreateSession().Execute(sql);
        // 拆分为多行执行，避免数据库被锁定
        var sqls = sql.Split("; " + Environment.NewLine);
        var session = Database.CreateSession();
        session.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            foreach (var item in sqls)
            {
                session.Execute(item);
            }
            session.Commit();
        }
        catch
        {
            session.Rollback();
            throw;
        }

        return null;
    }
    #endregion

    #region 表和字段备注
    /// <summary>添加描述</summary>
    /// <remarks>返回Empty，告诉反向工程，该数据库类型不支持该功能，请不要输出日志</remarks>
    /// <param name="table"></param>
    /// <returns></returns>
    public override String AddTableDescriptionSQL(IDataTable table) => String.Empty;

    public override String DropTableDescriptionSQL(IDataTable table) => String.Empty;

    public override String AddColumnDescriptionSQL(IDataColumn field) => String.Empty;

    public override String DropColumnDescriptionSQL(IDataColumn field) => String.Empty;
    #endregion

    #region 反向工程
    private readonly List<IDataTable> memoryTables = new();
    /// <summary>已重载。因为内存数据库无法检测到架构，不知道表是否已存在，所以需要自己维护</summary>
    /// <param name="entitytable"></param>
    /// <param name="dbtable"></param>
    /// <param name="mode"></param>
    protected override void CheckTable(IDataTable entitytable, IDataTable dbtable, Migration mode)
    {
        if (dbtable == null && (Database as SQLite).IsMemoryDatabase)
        {
            if (memoryTables.Any(t => t.TableName.EqualIgnoreCase(entitytable.TableName))) return;

            memoryTables.Add(entitytable);
        }

        base.CheckTable(entitytable, dbtable, mode);
    }

    public override String CompactDatabaseSQL() => "VACUUM";

    public override Int32 CompactDatabase() => Database.CreateSession().Execute("VACUUM");
    #endregion
}