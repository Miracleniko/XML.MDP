using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Log;
using XML.Core;

namespace XML.XCode.DataAccessLayer;

/// <summary>SqlServer元数据</summary>
internal class SqlServerMetaData : RemoteDbMetaData
{
    public SqlServerMetaData() => Types = _DataTypes;

    #region 属性
    ///// <summary>是否SQL2005</summary>
    //public Boolean IsSQL2005 { get { return (Database as SqlServer).IsSQL2005; } }

    public Version Version => (Database as SqlServer).Version;

    ///// <summary>0级类型</summary>
    //public String Level0type { get { return IsSQL2005 ? "SCHEMA" : "USER"; } }
    #endregion

    #region 构架
    /// <summary>取得所有表构架</summary>
    /// <returns></returns>
    protected override List<IDataTable> OnGetTables(String[] names)
    {
        #region 查表说明、字段信息、索引信息
        var session = Database.CreateSession();

        //一次性把所有的表说明查出来
        DataTable DescriptionTable = null;

        try
        {
            var sql = "select b.name n, a.value v from sys.extended_properties a inner join sysobjects b on a.major_id=b.id and a.minor_id=0 and a.name = 'MS_Description'";
            DescriptionTable = session.Query(sql).Tables[0];
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }

        var dt = GetSchema(_.Tables, null);
        if (dt == null || dt.Rows == null || dt.Rows.Count <= 0) return null;

        try
        {
            AllFields = session.Query(SchemaSql).Tables[0];
            AllIndexes = session.Query(IndexSql).Tables[0];
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }
        #endregion

        // 列出用户表
        var rows = dt.Select($"(TABLE_TYPE='BASE TABLE' Or TABLE_TYPE='VIEW') AND TABLE_NAME<>'Sysdiagrams'");
        if (rows == null || rows.Length <= 0) return null;

        var list = GetTables(rows, names);
        if (list == null || list.Count <= 0) return list;

        // 修正备注
        foreach (var item in list)
        {
            var drs = DescriptionTable?.Select("n='" + item.TableName + "'");
            item.Description = drs == null || drs.Length <= 0 ? "" : drs[0][1].ToString();
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

        var dt = GetSchema(_.Tables, null);
        if (dt?.Rows == null || dt.Rows.Count <= 0) return list;

        // 默认列出所有字段
        var rows = dt.Select($"(TABLE_TYPE='BASE TABLE' Or TABLE_TYPE='VIEW') AND TABLE_NAME<>'Sysdiagrams'");
        foreach (var dr in rows)
        {
            list.Add(GetDataRowValue<String>(dr, _.TalbeName));
        }

        return list;
    }

    private DataTable AllFields = null;
    private DataTable AllIndexes = null;

    protected override void FixField(IDataColumn field, DataRow dr)
    {
        base.FixField(field, dr);

        var rows = AllFields?.Select("表名='" + field.Table.TableName + "' And 字段名='" + field.ColumnName + "'", null);
        if (rows != null && rows.Length > 0)
        {
            var dr2 = rows[0];

            field.Identity = GetDataRowValue<Boolean>(dr2, "标识");
            field.PrimaryKey = GetDataRowValue<Boolean>(dr2, "主键");
            //field.NumOfByte = GetDataRowValue<Int32>(dr2, "占用字节数");
            field.Description = GetDataRowValue<String>(dr2, "字段说明");
            field.Precision = GetDataRowValue<Int32>(dr2, "精度");
            field.Scale = GetDataRowValue<Int32>(dr2, "小数位数");
        }
    }

    protected override List<IDataIndex> GetIndexes(IDataTable table, DataTable _indexes, DataTable _indexColumns)
    {
        var list = base.GetIndexes(table, _indexes, _indexColumns);
        if (list != null && list.Count > 0)
        {
            foreach (var item in list)
            {
                var drs = AllIndexes?.Select("name='" + item.Name + "'");
                if (drs != null && drs.Length > 0)
                {
                    item.Unique = GetDataRowValue<Boolean>(drs[0], "is_unique");
                    item.PrimaryKey = GetDataRowValue<Boolean>(drs[0], "is_primary_key");
                }
            }
        }
        return list;
    }

    public override String CreateTableSQL(IDataTable table)
    {
        var sql = base.CreateTableSQL(table);

        var pks = table.PrimaryKeys;
        if (String.IsNullOrEmpty(sql) || pks == null || pks.Length < 2) return sql;

        // 处理多主键
        sql += "; " + Environment.NewLine;
        sql += $"Alter Table {FormatName(table)} Add Constraint PK_{table.TableName} Primary Key Clustered({pks.Join(",", FormatName)})";
        return sql;
    }

    public override String FieldClause(IDataColumn field, Boolean onlyDefine)
    {
        if (!String.IsNullOrEmpty(field.RawType) && field.RawType.Contains("char(-1)"))
        {
            //if (IsSQL2005)
            field.RawType = field.RawType.Replace("char(-1)", "char(MAX)");
            //else
            //    field.RawType = field.RawType.Replace("char(-1)", "char(" + (Int32.MaxValue / 2) + ")");
        }

        //chenqi 2017-3-28
        //增加处理decimal类型精度和小数位数处理
        //此处只针对Sql server进行处理
        //严格来说，应该修改的地方是
        if (!field.RawType.IsNullOrEmpty() && field.RawType.StartsWithIgnoreCase("decimal"))
        {
            field.RawType = $"decimal({field.Precision},{field.Scale})";
        }

        return base.FieldClause(field, onlyDefine);
    }

    protected override String GetFieldConstraints(IDataColumn field, Boolean onlyDefine)
    {
        // 非定义时（修改字段），主键字段没有约束
        if (!onlyDefine && field.PrimaryKey) return null;

        var str = base.GetFieldConstraints(field, onlyDefine);

        // 非定义时，自增字段没有约束
        if (onlyDefine && field.Identity) str = " IDENTITY(1,1)" + str;

        return str;
    }

    //protected override String GetFormatParam(IDataColumn field, DataRow dr)
    //{
    //    var str = base.GetFormatParam(field, dr);
    //    if (String.IsNullOrEmpty(str)) return str;

    //    // 这个主要来自于float，因为无法取得其精度
    //    if (str == "(0)") return null;
    //    return str;
    //}

    //protected override String GetFormatParamItem(IDataColumn field, DataRow dr, String item)
    //{
    //    var pi = base.GetFormatParamItem(field, dr, item);
    //    if (field.DataType == typeof(String) && pi == "-1" && IsSQL2005) return "MAX";
    //    return pi;
    //}
    #endregion

    #region 取得字段信息的SQL模版
    private String _SchemaSql = "";
    /// <summary>构架SQL</summary>
    public virtual String SchemaSql
    {
        get
        {
            if (String.IsNullOrEmpty(_SchemaSql))
            {
                var sb = new StringBuilder();
                sb.Append("SELECT ");
                sb.Append("表名=d.name,");
                sb.Append("字段序号=a.colorder,");
                sb.Append("字段名=a.name,");
                sb.Append("标识=case when COLUMNPROPERTY( a.id,a.name,'IsIdentity')=1 then Convert(Bit,1) else Convert(Bit,0) end,");
                sb.Append("主键=case when exists(SELECT 1 FROM sysobjects where xtype='PK' and name in (");
                sb.Append("SELECT name FROM sysindexes WHERE id = a.id AND indid in(");
                sb.Append("SELECT indid FROM sysindexkeys WHERE id = a.id AND colid=a.colid");
                sb.Append("))) then Convert(Bit,1) else Convert(Bit,0) end,");
                sb.Append("类型=b.name,");
                sb.Append("占用字节数=a.length,");
                sb.Append("长度=COLUMNPROPERTY(a.id,a.name,'PRECISION'),");
                sb.Append("小数位数=isnull(COLUMNPROPERTY(a.id,a.name,'Scale'),0),");
                sb.Append("允许空=case when a.isnullable=1 then Convert(Bit,1)else Convert(Bit,0) end,");
                sb.Append("默认值=isnull(e.text,''),");
                sb.Append("字段说明=isnull(g.[value],'')");
                sb.Append("FROM syscolumns a ");
                sb.Append("left join systypes b on a.xtype=b.xusertype ");
                sb.Append("inner join sysobjects d on a.id=d.id  and d.xtype='U' ");
                sb.Append("left join syscomments e on a.cdefault=e.id ");
                //if (IsSQL2005)
                //{
                sb.Append("left join sys.extended_properties g on a.id=g.major_id and a.colid=g.minor_id and g.name = 'MS_Description'  ");
                //}
                //else
                //{
                //    sb.Append("left join sysproperties g on a.id=g.id and a.colid=g.smallid  ");
                //}
                sb.Append("order by a.id,a.colorder");
                _SchemaSql = sb.ToString();
            }
            return _SchemaSql;
        }
    }

    private String _IndexSql;
    public virtual String IndexSql
    {
        get
        {
            if (_IndexSql == null)
            {
                //if (IsSQL2005)
                _IndexSql = "select ind.* from sys.indexes ind inner join sys.objects obj on ind.object_id = obj.object_id where obj.type='u'";
                //else
                //    _IndexSql = "select IndexProperty(obj.id, ind.name,'IsUnique') as is_unique, ObjectProperty(object_id(ind.name),'IsPrimaryKey') as is_primary_key,ind.* from sysindexes ind inner join sysobjects obj on ind.id = obj.id where obj.type='u'";
            }
            return _IndexSql;
        }
    }

    //private readonly String _DescriptionSql2000 = "select b.name n, a.value v from sysproperties a inner join sysobjects b on a.id=b.id where a.smallid=0";
    //private readonly String _DescriptionSql2005 = "select b.name n, a.value v from sys.extended_properties a inner join sysobjects b on a.major_id=b.id and a.minor_id=0 and a.name = 'MS_Description'";
    ///// <summary>取表说明SQL</summary>
    //public virtual String DescriptionSql { get { return IsSQL2005 ? _DescriptionSql2005 : _DescriptionSql2000; } }
    #endregion

    #region 数据定义
    public override Object SetSchema(DDLSchema schema, params Object[] values)
    {
        var dbname = "";
        var file = "";
        var recoverDir = "";
        switch (schema)
        {
            case DDLSchema.BackupDatabase:
                if (values != null)
                {
                    if (values.Length > 0)
                        dbname = values[0] as String;
                    if (values.Length > 1)
                        file = values[1] as String;
                }
                return Backup(dbname, file, false);
            case DDLSchema.RestoreDatabase:
                if (values != null)
                {
                    if (values.Length > 0)
                        file = values[0] as String;
                    if (values.Length > 1)
                        recoverDir = values[1] as String;
                }
                return Restore(file, recoverDir, true);
            default:
                break;
        }
        return base.SetSchema(schema, values);
    }

    public override String CreateDatabaseSQL(String dbname, String file)
    {
        var dp = (Database as SqlServer).DataPath;

        if (String.IsNullOrEmpty(file))
        {
            if (String.IsNullOrEmpty(dp)) return $"CREATE DATABASE {Database.FormatName(dbname)}";

            file = dbname + ".mdf";
        }

        if (!Path.IsPathRooted(file))
        {
            if (!String.IsNullOrEmpty(dp)) file = Path.Combine(dp, file);

            if (!Path.IsPathRooted(file)) file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
        }
        if (String.IsNullOrEmpty(Path.GetExtension(file))) file += ".mdf";
        file = new FileInfo(file).FullName;

        var logfile = Path.ChangeExtension(file, ".ldf");
        logfile = new FileInfo(logfile).FullName;

        var dir = Path.GetDirectoryName(file);
        if (!String.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var sb = new StringBuilder();

        sb.AppendFormat("CREATE DATABASE {0} ON  PRIMARY", Database.FormatName(dbname));
        sb.AppendLine();
        sb.AppendFormat(@"( NAME = N'{0}', FILENAME = N'{1}', SIZE = 10 , MAXSIZE = UNLIMITED, FILEGROWTH = 10%)", dbname, file);
        sb.AppendLine();
        sb.Append("LOG ON ");
        sb.AppendLine();
        sb.AppendFormat(@"( NAME = N'{0}_Log', FILENAME = N'{1}', SIZE = 10 , MAXSIZE = UNLIMITED, FILEGROWTH = 10%)", dbname, logfile);
        sb.AppendLine();

        return sb.ToString();
    }

    public override String DatabaseExistSQL(String dbname) => $"SELECT * FROM sysdatabases WHERE name = N'{dbname}'";

    /// <summary>使用数据架构确定数据库是否存在，因为使用系统视图可能没有权限</summary>
    /// <param name="dbname"></param>
    /// <returns></returns>
    protected override Boolean DatabaseExist(String dbname)
    {
        var dt = GetSchema(_.Databases, new String[] { dbname });
        return dt != null && dt.Rows != null && dt.Rows.Count > 0;
    }

    //protected override Boolean DropDatabase(String databaseName)
    //{
    //    //return base.DropDatabase(databaseName);

    //    // SQL语句片段，断开该数据库所有链接
    //    var sb = new StringBuilder();
    //    sb.AppendLine("use master");
    //    sb.AppendLine(";");
    //    sb.AppendLine("declare   @spid   varchar(20),@dbname   varchar(20)");
    //    sb.AppendLine("declare   #spid   cursor   for");
    //    sb.AppendFormat("select   spid=cast(spid   as   varchar(20))   from   master..sysprocesses   where   dbid=db_id('{0}')", databaseName);
    //    sb.AppendLine();
    //    sb.AppendLine("open   #spid");
    //    sb.AppendLine("fetch   next   from   #spid   into   @spid");
    //    sb.AppendLine("while   @@fetch_status=0");
    //    sb.AppendLine("begin");
    //    sb.AppendLine("exec('kill   '+@spid)");
    //    sb.AppendLine("fetch   next   from   #spid   into   @spid");
    //    sb.AppendLine("end");
    //    sb.AppendLine("close   #spid");
    //    sb.AppendLine("deallocate   #spid");

    //    var count = 0;
    //    var session = Database.CreateSession();
    //    try
    //    {
    //        count = session.Execute(sb.ToString());
    //    }
    //    catch (Exception ex) { XTrace.WriteException(ex); }
    //    return session.Execute(String.Format("Drop Database {0}", FormatName(databaseName))) > 0;
    //}

    /// <summary>备份文件到目标文件</summary>
    /// <param name="dbname"></param>
    /// <param name="bakfile"></param>
    /// <param name="compressed"></param>
    public override String Backup(String dbname, String bakfile, Boolean compressed)
    {

        var name = dbname;
        if (name.IsNullOrEmpty())
        {
            name = Database.DatabaseName;
        }

        var bf = bakfile;
        if (bf.IsNullOrEmpty())
        {
            var ext = Path.GetExtension(bakfile);
            if (ext.IsNullOrEmpty()) ext = ".bak";

            if (compressed)
                bf = $"{name}{ext}";
            else
                bf = $"{name}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
        }
        if (!Path.IsPathRooted(bf)) bf = XML.Core.Setting.Current.BackupPath.CombinePath(bf).GetBasePath();

        bf = bf.EnsureDirectory(true);

        WriteLog("{0}备份SqlServer数据库 {1} 到 {2}", Database.ConnName, name, bf);

        var sw = Stopwatch.StartNew();

        // 删除已有文件
        if (File.Exists(bf)) File.Delete(bf);

        base.Database.CreateSession().Execute($"USE master;BACKUP DATABASE {name} TO disk = '{bf}'");

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

    /// <summary>还原备份文件到目标数据库</summary>
    /// <param name="bakfile"></param>
    /// <param name="recoverDir"></param>
    /// <param name="replace"></param>
    /// <param name="compressed"></param>
    public String Restore(String bakfile, String recoverDir, Boolean replace = true, Boolean compressed = false)
    {
        var session = base.Database.CreateSession();
        var result = "";
        if (compressed)
        {
            return result;
        }
        if (bakfile.IsNullOrEmpty())
        {
            return result;
        }

        if (recoverDir.IsNullOrEmpty())
        {
            var sql = "select filename from sysfiles";
            var dt = session.Query(sql).Tables[0];
            if (dt.Rows.Count < 1)
            {
                return result;
            }
            else
            {
                recoverDir = Path.GetDirectoryName(dt.Rows[0][0].ToString());
            }
        }


        WriteLog("{0}还原SqlServer数据库 备份文件为{1}", Database.ConnName, bakfile);

        var sw = Stopwatch.StartNew();


        var headInfo = session.Query($"RESTORE HEADERONLY FROM DISK = '{bakfile}'").Tables[0];
        var fileInfo = session.Query($"RESTORE FILELISTONLY from disk= N'{bakfile}'").Tables[0];
        if (headInfo.Rows.Count < 1)
        {
            return result;
        }
        else
        {
            var databaseName = headInfo.Rows[0]["DatabaseName"];
            var dataName = fileInfo.Rows[0]["LogicalName"];
            var logName = fileInfo.Rows[1]["LogicalName"];
            var stopConnect = $"ALTER DATABASE {databaseName} SET OFFLINE WITH ROLLBACK IMMEDIATE";
            var restorSql = $@"RESTORE DATABASE {databaseName} from disk= N'{bakfile}' 
                WITH NOUNLOAD,
                {(replace ? "REPLACE," : "")}
                    MOVE '{dataName}' TO '{Path.Combine(recoverDir, String.Concat(databaseName, ".mdf"))}',
                    MOVE '{logName}' TO '{Path.Combine(recoverDir, String.Concat(databaseName, ".ldf"))}';";
            session.Execute(stopConnect);
            session.Execute(restorSql);
            result = "ok";
        }

        sw.Stop();
        WriteLog("还原完成，耗时{0:n0}ms", sw.ElapsedMilliseconds);

        return result;
    }

    public override String TableExistSQL(IDataTable table) => $"select * from sysobjects where xtype='U' and name='{table.TableName}'";

    /// <summary>使用数据架构确定数据表是否存在，因为使用系统视图可能没有权限</summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public Boolean TableExist(IDataTable table)
    {
        var dt = GetSchema(_.Tables, new String[] { null, null, table.TableName, null });
        return dt != null && dt.Rows != null && dt.Rows.Count > 0;
    }

    protected override String RenameTable(String tableName, String tempTableName)
    {
        if (Version.Major >= 8)
            return $"EXECUTE sp_rename N'{tableName}', N'{tempTableName}', 'OBJECT' ";
        else
            return base.RenameTable(tableName, tempTableName);
    }

    protected override String ReBuildTable(IDataTable entitytable, IDataTable dbtable)
    {
        var sql = base.ReBuildTable(entitytable, dbtable);
        if (String.IsNullOrEmpty(sql)) return sql;

        // 特殊处理带标识列的表，需要增加SET IDENTITY_INSERT
        if (!entitytable.Columns.Any(e => e.Identity)) return sql;

        var tableName = Database.FormatName(entitytable);
        var ss = sql.Split("; " + Environment.NewLine);
        for (var i = 0; i < ss.Length; i++)
        {
            if (ss[i].StartsWithIgnoreCase("Insert Into"))
            {
                ss[i] = $"SET IDENTITY_INSERT {tableName} ON;{ss[i]};SET IDENTITY_INSERT {tableName} OFF";
                break;
            }
        }
        return String.Join("; " + Environment.NewLine, ss);
    }

    public override String AddTableDescriptionSQL(IDataTable table) => $"EXEC dbo.sp_addextendedproperty @name=N'MS_Description', @value=N'{table.Description}' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{table.TableName}'";

    public override String DropTableDescriptionSQL(IDataTable table) => $"EXEC dbo.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{table.TableName}'";

    public override String AddColumnSQL(IDataColumn field) => $"Alter Table {FormatName(field.Table)} Add {FieldClause(field, true)}";

    public override String AlterColumnSQL(IDataColumn field, IDataColumn oldfield)
    {
        // 创建为自增，重建表
        if (field.Identity && !oldfield.Identity)
        {
            //return DropColumnSQL(oldfield) + ";" + Environment.NewLine + AddColumnSQL(field);
            return ReBuildTable(field.Table, oldfield.Table);
        }
        // 类型改变，必须重建表
        if (IsColumnTypeChanged(field, oldfield)) return ReBuildTable(field.Table, oldfield.Table);

        var sql = $"Alter Table {FormatName(field.Table)} Alter Column {FieldClause(field, false)}";
        var pk = DeletePrimaryKeySQL(field);
        if (field.PrimaryKey)
        {
            // 如果没有主键删除脚本，表明没有主键
            //if (String.IsNullOrEmpty(pk))
            if (!oldfield.PrimaryKey)
            {
                // 增加主键约束
                pk = $"Alter Table {FormatName(field.Table)} ADD CONSTRAINT PK_{FormatName(field.Table)} PRIMARY KEY {(field.Identity ? "CLUSTERED" : "")}({FormatName(field)}) ON [PRIMARY]";
                sql += ";" + Environment.NewLine + pk;
            }
        }
        else
        {
            // 字段声明没有主键，但是主键实际存在，则删除主键
            //if (!String.IsNullOrEmpty(pk))
            if (oldfield.PrimaryKey)
            {
                sql += ";" + Environment.NewLine + pk;
            }
        }

        //// 需要提前删除相关默认值
        //if (oldfield.Default != null)
        //{
        //    var df = DropDefaultSQL(oldfield);
        //    if (!String.IsNullOrEmpty(df))
        //    {
        //        sql = df + ";" + Environment.NewLine + sql;

        //        // 如果还有默认值，加上
        //        if (field.Default != null)
        //        {
        //            df = AddDefaultSQLWithNoCheck(field);
        //            if (!String.IsNullOrEmpty(df)) sql += ";" + Environment.NewLine + df;
        //        }
        //    }
        //}
        // 需要提前删除相关索引
        foreach (var di in oldfield.Table.Indexes)
        {
            // 如果包含该字段
            if (di.Columns.Contains(oldfield.ColumnName, StringComparer.OrdinalIgnoreCase))
            {
                var dis = DropIndexSQL(di);
                if (!String.IsNullOrEmpty(dis)) sql = dis + ";" + Environment.NewLine + sql;
            }
        }
        // 如果还有索引，则加上
        foreach (var di in field.Table.Indexes)
        {
            // 如果包含该字段
            if (di.Columns.Contains(field.ColumnName, StringComparer.OrdinalIgnoreCase))
            {
                var cis = CreateIndexSQL(di);
                if (!String.IsNullOrEmpty(cis)) sql += ";" + Environment.NewLine + cis;
            }
        }

        return sql;
    }

    public override String DropIndexSQL(IDataIndex index) => $"Drop Index {FormatName(index.Table)}.{index.Name}";

    public override String DropColumnSQL(IDataColumn field)
    {
        ////删除默认值
        //String sql = DropDefaultSQL(field);
        //if (!String.IsNullOrEmpty(sql)) sql += ";" + Environment.NewLine;

        //删除主键
        var sql = DeletePrimaryKeySQL(field);
        if (!String.IsNullOrEmpty(sql)) sql += ";" + Environment.NewLine;

        sql += base.DropColumnSQL(field);
        return sql;
    }

    public override String AddColumnDescriptionSQL(IDataColumn field)
    {
        var sql = $"EXEC dbo.sp_addextendedproperty @name=N'MS_Description', @value=N'{field.Description}' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{field.Table.TableName}', @level2type=N'COLUMN',@level2name=N'{field.ColumnName}'";
        return sql;
    }

    public override String DropColumnDescriptionSQL(IDataColumn field) => $"EXEC dbo.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{field.Table.TableName}', @level2type=N'COLUMN',@level2name=N'{field.ColumnName}'";

    private String DeletePrimaryKeySQL(IDataColumn field)
    {
        if (!field.PrimaryKey) return String.Empty;

        var dis = field.Table.Indexes;
        if (dis == null || dis.Count <= 0) return String.Empty;

        var di = dis.FirstOrDefault(e => e.Columns.Any(x => x.EqualIgnoreCase(field.ColumnName, field.Name)));
        if (di == null) return String.Empty;

        return $"Alter Table {FormatName(field.Table)} Drop CONSTRAINT {di.Name}";
    }

    public override String DropDatabaseSQL(String dbname)
    {
        var sb = new StringBuilder();
        sb.AppendLine("use master");
        sb.AppendLine(";");
        sb.AppendLine("declare   @spid   varchar(20),@dbname   varchar(20)");
        sb.AppendLine("declare   #spid   cursor   for");
        sb.AppendFormat("select   spid=cast(spid   as   varchar(20))   from   master..sysprocesses   where   dbid=db_id('{0}')", dbname);
        sb.AppendLine();
        sb.AppendLine("open   #spid");
        sb.AppendLine("fetch   next   from   #spid   into   @spid");
        sb.AppendLine("while   @@fetch_status=0");
        sb.AppendLine("begin");
        sb.AppendLine("exec('kill   '+@spid)");
        sb.AppendLine("fetch   next   from   #spid   into   @spid");
        sb.AppendLine("end");
        sb.AppendLine("close   #spid");
        sb.AppendLine("deallocate   #spid");
        sb.AppendLine(";");
        sb.AppendFormat("Drop Database {0}", Database.FormatName(dbname));
        return sb.ToString();
    }

    #endregion

    /// <summary>数据类型映射</summary>
    private static readonly Dictionary<Type, String[]> _DataTypes = new()
    {
        { typeof(Byte[]), new String[] { "binary({0})", "image", "varbinary({0})", "timestamp" } },
        //{ typeof(DateTimeOffset), new String[] { "datetimeoffset({0})" } },
        { typeof(Guid), new String[] { "uniqueidentifier" } },
        //{ typeof(Object), new String[] { "sql_variant" } },
        //{ typeof(TimeSpan), new String[] { "time({0})" } },
        { typeof(Boolean), new String[] { "bit" } },
        { typeof(Byte), new String[] { "tinyint" } },
        { typeof(Int16), new String[] { "smallint" } },
        { typeof(Int32), new String[] { "int" } },
        { typeof(Int64), new String[] { "bigint" } },
        { typeof(Single), new String[] { "real" } },
        { typeof(Double), new String[] { "float" } },
        { typeof(Decimal), new String[] { "money", "decimal({0}, {1})", "numeric({0}, {1})", "smallmoney" } },
        { typeof(DateTime), new String[] { "datetime", "smalldatetime", "datetime2({0})", "date" } },
        { typeof(String), new String[] { "nvarchar({0})", "ntext", "text", "varchar({0})", "char({0})", "nchar({0})", "xml" } }
    };

    #region 辅助函数
    /// <summary>除去字符串两端成对出现的符号</summary>
    /// <param name="str"></param>
    /// <param name="prefix"></param>
    /// <param name="suffix"></param>
    /// <returns></returns>
    public static String Trim(String str, String prefix, String suffix)
    {
        while (!String.IsNullOrEmpty(str))
        {
            if (!str.StartsWith(prefix)) return str;
            if (!str.EndsWith(suffix)) return str;

            str = str[prefix.Length..^suffix.Length];
        }
        return str;
    }
    #endregion
}