﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using XML.Core.Data;
using XML.Core;
using XML.XCode.Cache;
using XML.XCode.Configuration;
using XML.XCode.DataAccessLayer;

namespace XML.XCode.Membership;

/// <summary>日志</summary>
[Serializable]
[DataObject]
[Description("日志")]
[BindIndex("IX_Log_Action_Category_ID", false, "Action,Category,ID")]
[BindIndex("IX_Log_Category_LinkID_ID", false, "Category,LinkID,ID")]
[BindIndex("IX_Log_CreateUserID_ID", false, "CreateUserID,ID")]
[BindTable("Log", Description = "日志", ConnName = "Log", DbType = DatabaseType.None)]
public partial class Log
{
    #region 属性
    private Int64 _ID;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, false, false, 0)]
    [BindColumn("ID", "编号", "")]
    public Int64 ID { get => _ID; set { if (OnPropertyChanging("ID", value)) { _ID = value; OnPropertyChanged("ID"); } } }

    private String _Category;
    /// <summary>类别</summary>
    [DisplayName("类别")]
    [Description("类别")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Category", "类别", "")]
    public String Category { get => _Category; set { if (OnPropertyChanging("Category", value)) { _Category = value; OnPropertyChanged("Category"); } } }

    private String _Action;
    /// <summary>操作</summary>
    [DisplayName("操作")]
    [Description("操作")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Action", "操作", "")]
    public String Action { get => _Action; set { if (OnPropertyChanging("Action", value)) { _Action = value; OnPropertyChanged("Action"); } } }

    private Int32 _LinkID;
    /// <summary>链接</summary>
    [DisplayName("链接")]
    [Description("链接")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("LinkID", "链接", "")]
    public Int32 LinkID { get => _LinkID; set { if (OnPropertyChanging("LinkID", value)) { _LinkID = value; OnPropertyChanged("LinkID"); } } }

    private Boolean _Success;
    /// <summary>成功</summary>
    [DisplayName("成功")]
    [Description("成功")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Success", "成功", "")]
    public Boolean Success { get => _Success; set { if (OnPropertyChanging("Success", value)) { _Success = value; OnPropertyChanged("Success"); } } }

    private String _UserName;
    /// <summary>用户名</summary>
    [DisplayName("用户名")]
    [Description("用户名")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("UserName", "用户名", "")]
    public String UserName { get => _UserName; set { if (OnPropertyChanging("UserName", value)) { _UserName = value; OnPropertyChanged("UserName"); } } }

    private Int32 _Ex1;
    /// <summary>扩展1</summary>
    [Category("扩展")]
    [DisplayName("扩展1")]
    [Description("扩展1")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Ex1", "扩展1", "")]
    public Int32 Ex1 { get => _Ex1; set { if (OnPropertyChanging("Ex1", value)) { _Ex1 = value; OnPropertyChanged("Ex1"); } } }

    private Int32 _Ex2;
    /// <summary>扩展2</summary>
    [Category("扩展")]
    [DisplayName("扩展2")]
    [Description("扩展2")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Ex2", "扩展2", "")]
    public Int32 Ex2 { get => _Ex2; set { if (OnPropertyChanging("Ex2", value)) { _Ex2 = value; OnPropertyChanged("Ex2"); } } }

    private Double _Ex3;
    /// <summary>扩展3</summary>
    [Category("扩展")]
    [DisplayName("扩展3")]
    [Description("扩展3")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Ex3", "扩展3", "")]
    public Double Ex3 { get => _Ex3; set { if (OnPropertyChanging("Ex3", value)) { _Ex3 = value; OnPropertyChanged("Ex3"); } } }

    private String _Ex4;
    /// <summary>扩展4</summary>
    [Category("扩展")]
    [DisplayName("扩展4")]
    [Description("扩展4")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Ex4", "扩展4", "")]
    public String Ex4 { get => _Ex4; set { if (OnPropertyChanging("Ex4", value)) { _Ex4 = value; OnPropertyChanged("Ex4"); } } }

    private String _Ex5;
    /// <summary>扩展5</summary>
    [Category("扩展")]
    [DisplayName("扩展5")]
    [Description("扩展5")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Ex5", "扩展5", "")]
    public String Ex5 { get => _Ex5; set { if (OnPropertyChanging("Ex5", value)) { _Ex5 = value; OnPropertyChanged("Ex5"); } } }

    private String _Ex6;
    /// <summary>扩展6</summary>
    [Category("扩展")]
    [DisplayName("扩展6")]
    [Description("扩展6")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Ex6", "扩展6", "")]
    public String Ex6 { get => _Ex6; set { if (OnPropertyChanging("Ex6", value)) { _Ex6 = value; OnPropertyChanged("Ex6"); } } }

    private String _TraceId;
    /// <summary>性能追踪。用于APM性能追踪定位，还原该事件的调用链</summary>
    [DisplayName("性能追踪")]
    [Description("性能追踪。用于APM性能追踪定位，还原该事件的调用链")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("TraceId", "性能追踪。用于APM性能追踪定位，还原该事件的调用链", "")]
    public String TraceId { get => _TraceId; set { if (OnPropertyChanging("TraceId", value)) { _TraceId = value; OnPropertyChanged("TraceId"); } } }

    private String _CreateUser;
    /// <summary>创建者</summary>
    [Category("扩展")]
    [DisplayName("创建者")]
    [Description("创建者")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("CreateUser", "创建者", "")]
    public String CreateUser { get => _CreateUser; set { if (OnPropertyChanging("CreateUser", value)) { _CreateUser = value; OnPropertyChanged("CreateUser"); } } }

    private Int32 _CreateUserID;
    /// <summary>创建用户</summary>
    [Category("扩展")]
    [DisplayName("创建用户")]
    [Description("创建用户")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("CreateUserID", "创建用户", "")]
    public Int32 CreateUserID { get => _CreateUserID; set { if (OnPropertyChanging("CreateUserID", value)) { _CreateUserID = value; OnPropertyChanged("CreateUserID"); } } }

    private String _CreateIP;
    /// <summary>创建地址</summary>
    [Category("扩展")]
    [DisplayName("创建地址")]
    [Description("创建地址")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("CreateIP", "创建地址", "")]
    public String CreateIP { get => _CreateIP; set { if (OnPropertyChanging("CreateIP", value)) { _CreateIP = value; OnPropertyChanged("CreateIP"); } } }

    private DateTime _CreateTime;
    /// <summary>时间</summary>
    [DisplayName("时间")]
    [Description("时间")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("CreateTime", "时间", "")]
    public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging("CreateTime", value)) { _CreateTime = value; OnPropertyChanged("CreateTime"); } } }

    private String _Remark;
    /// <summary>详细信息</summary>
    [DisplayName("详细信息")]
    [Description("详细信息")]
    [DataObjectField(false, false, true, 2000)]
    [BindColumn("Remark", "详细信息", "")]
    public String Remark { get => _Remark; set { if (OnPropertyChanging("Remark", value)) { _Remark = value; OnPropertyChanged("Remark"); } } }
    #endregion

    #region 获取/设置 字段值
    /// <summary>获取/设置 字段值</summary>
    /// <param name="name">字段名</param>
    /// <returns></returns>
    public override Object this[String name]
    {
        get
        {
            switch (name)
            {
                case "ID": return _ID;
                case "Category": return _Category;
                case "Action": return _Action;
                case "LinkID": return _LinkID;
                case "Success": return _Success;
                case "UserName": return _UserName;
                case "Ex1": return _Ex1;
                case "Ex2": return _Ex2;
                case "Ex3": return _Ex3;
                case "Ex4": return _Ex4;
                case "Ex5": return _Ex5;
                case "Ex6": return _Ex6;
                case "TraceId": return _TraceId;
                case "CreateUser": return _CreateUser;
                case "CreateUserID": return _CreateUserID;
                case "CreateIP": return _CreateIP;
                case "CreateTime": return _CreateTime;
                case "Remark": return _Remark;
                default: return base[name];
            }
        }
        set
        {
            switch (name)
            {
                case "ID": _ID = value.ToLong(); break;
                case "Category": _Category = Convert.ToString(value); break;
                case "Action": _Action = Convert.ToString(value); break;
                case "LinkID": _LinkID = value.ToInt(); break;
                case "Success": _Success = value.ToBoolean(); break;
                case "UserName": _UserName = Convert.ToString(value); break;
                case "Ex1": _Ex1 = value.ToInt(); break;
                case "Ex2": _Ex2 = value.ToInt(); break;
                case "Ex3": _Ex3 = value.ToDouble(); break;
                case "Ex4": _Ex4 = Convert.ToString(value); break;
                case "Ex5": _Ex5 = Convert.ToString(value); break;
                case "Ex6": _Ex6 = Convert.ToString(value); break;
                case "TraceId": _TraceId = Convert.ToString(value); break;
                case "CreateUser": _CreateUser = Convert.ToString(value); break;
                case "CreateUserID": _CreateUserID = value.ToInt(); break;
                case "CreateIP": _CreateIP = Convert.ToString(value); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
                case "Remark": _Remark = Convert.ToString(value); break;
                default: base[name] = value; break;
            }
        }
    }
    #endregion

    #region 字段名
    /// <summary>取得日志字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field ID = FindByName("ID");

        /// <summary>类别</summary>
        public static readonly Field Category = FindByName("Category");

        /// <summary>操作</summary>
        public static readonly Field Action = FindByName("Action");

        /// <summary>链接</summary>
        public static readonly Field LinkID = FindByName("LinkID");

        /// <summary>成功</summary>
        public static readonly Field Success = FindByName("Success");

        /// <summary>用户名</summary>
        public static readonly Field UserName = FindByName("UserName");

        /// <summary>扩展1</summary>
        public static readonly Field Ex1 = FindByName("Ex1");

        /// <summary>扩展2</summary>
        public static readonly Field Ex2 = FindByName("Ex2");

        /// <summary>扩展3</summary>
        public static readonly Field Ex3 = FindByName("Ex3");

        /// <summary>扩展4</summary>
        public static readonly Field Ex4 = FindByName("Ex4");

        /// <summary>扩展5</summary>
        public static readonly Field Ex5 = FindByName("Ex5");

        /// <summary>扩展6</summary>
        public static readonly Field Ex6 = FindByName("Ex6");

        /// <summary>性能追踪。用于APM性能追踪定位，还原该事件的调用链</summary>
        public static readonly Field TraceId = FindByName("TraceId");

        /// <summary>创建者</summary>
        public static readonly Field CreateUser = FindByName("CreateUser");

        /// <summary>创建用户</summary>
        public static readonly Field CreateUserID = FindByName("CreateUserID");

        /// <summary>创建地址</summary>
        public static readonly Field CreateIP = FindByName("CreateIP");

        /// <summary>时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

        /// <summary>详细信息</summary>
        public static readonly Field Remark = FindByName("Remark");

        static Field FindByName(String name) => Meta.Table.FindByName(name);
    }

    /// <summary>取得日志字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String ID = "ID";

        /// <summary>类别</summary>
        public const String Category = "Category";

        /// <summary>操作</summary>
        public const String Action = "Action";

        /// <summary>链接</summary>
        public const String LinkID = "LinkID";

        /// <summary>成功</summary>
        public const String Success = "Success";

        /// <summary>用户名</summary>
        public const String UserName = "UserName";

        /// <summary>扩展1</summary>
        public const String Ex1 = "Ex1";

        /// <summary>扩展2</summary>
        public const String Ex2 = "Ex2";

        /// <summary>扩展3</summary>
        public const String Ex3 = "Ex3";

        /// <summary>扩展4</summary>
        public const String Ex4 = "Ex4";

        /// <summary>扩展5</summary>
        public const String Ex5 = "Ex5";

        /// <summary>扩展6</summary>
        public const String Ex6 = "Ex6";

        /// <summary>性能追踪。用于APM性能追踪定位，还原该事件的调用链</summary>
        public const String TraceId = "TraceId";

        /// <summary>创建者</summary>
        public const String CreateUser = "CreateUser";

        /// <summary>创建用户</summary>
        public const String CreateUserID = "CreateUserID";

        /// <summary>创建地址</summary>
        public const String CreateIP = "CreateIP";

        /// <summary>时间</summary>
        public const String CreateTime = "CreateTime";

        /// <summary>详细信息</summary>
        public const String Remark = "Remark";
    }
    #endregion
}

/// <summary>日志</summary>
public partial class Log : Entity<Log>
{
    #region 对象操作
    static Log()
    {
        Meta.Table.DataTable.InsertOnly = true;
        //Meta.Factory.FullInsert = false;

        Meta.Modules.Add<TimeModule>();
        Meta.Modules.Add<UserModule>();
        Meta.Modules.Add<IPModule>();
        Meta.Modules.Add<TraceModule>();

#if !DEBUG
            // 关闭SQL日志
            ThreadPool.QueueUserWorkItem(s => { Meta.Session.Dal.Db.ShowSQL = false; });
#endif
    }

    /// <summary>已重载。记录当前管理员</summary>
    /// <param name="isNew"></param>
    public override void Valid(Boolean isNew)
    {
        if (isNew)
        {
            // 自动设置当前登录用户
            if (!IsDirty(__.UserName)) UserName = ManageProvider.Provider?.Current + "";
        }

        // 处理过长的备注
        var len = _.Remark.Length;
        if (len > 0 && !Remark.IsNullOrEmpty() && Remark.Length > len) Remark = Remark[..len];

        len = _.UserName.Length;
        if (len > 0 && !UserName.IsNullOrEmpty() && UserName.Length > len) UserName = UserName[..len];

        base.Valid(isNew);

        // 时间
        if (isNew && CreateTime.Year < 2000 && !IsDirty(__.CreateTime)) CreateTime = DateTime.Now;
    }

    /// <summary></summary>
    /// <returns></returns>
    protected override Int32 OnUpdate() => throw new Exception("禁止修改日志！");

    /// <summary></summary>
    /// <returns></returns>
    protected override Int32 OnDelete() => throw new Exception("禁止删除日志！");
    #endregion

    #region 扩展属性
    #endregion

    #region 扩展查询
    /// <summary>查询</summary>
    /// <param name="key"></param>
    /// <param name="userid"></param>
    /// <param name="category"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    [Obsolete]
    public static IList<Log> Search(String key, Int32 userid, String category, DateTime start, DateTime end, PageParameter p)
    {
        var exp = new WhereExpression();
        //if (!key.IsNullOrEmpty()) exp &= (_.Action == key | _.Remark.Contains(key));
        if (!category.IsNullOrEmpty() && category != "全部") exp &= _.Category == category;
        if (userid > 0) exp &= _.CreateUserID == userid;

        // 主键带有时间戳
        var snow = Meta.Factory.Snow;
        if (snow != null)
            exp &= _.ID.Between(start, end, snow);
        else
            exp &= _.CreateTime.Between(start, end);

        // 先精确查询，再模糊
        if (!key.IsNullOrEmpty())
        {
            var list = FindAll(exp & _.Action == key, p);
            if (list.Count > 0) return list;

            exp &= _.Action.Contains(key) | _.Remark.Contains(key);
        }

        return FindAll(exp, p);
    }

    /// <summary>查询</summary>
    /// <param name="category"></param>
    /// <param name="action"></param>
    /// <param name="success"></param>
    /// <param name="userid"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="key"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    [Obsolete]
    public static IList<Log> Search(String category, String action, Boolean? success, Int32 userid, DateTime start, DateTime end, String key, PageParameter p)
    {
        var exp = new WhereExpression();

        if (!category.IsNullOrEmpty() && category != "全部") exp &= _.Category == category;
        if (!action.IsNullOrEmpty() && action != "全部") exp &= _.Action == action;
        if (success != null) exp &= _.Success == success;
        if (userid > 0) exp &= _.CreateUserID == userid;

        // 主键带有时间戳
        var snow = Meta.Factory.Snow;
        if (snow != null)
            exp &= _.ID.Between(start, end, snow);
        else
            exp &= _.CreateTime.Between(start, end);

        if (!key.IsNullOrEmpty()) exp &= _.Remark.Contains(key);

        return FindAll(exp, p);
    }

    /// <summary>查询</summary>
    /// <param name="category"></param>
    /// <param name="action"></param>
    /// <param name="linkId"></param>
    /// <param name="success"></param>
    /// <param name="userid"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="key"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IList<Log> Search(String category, String action, Int32 linkId, Boolean? success, Int32 userid, DateTime start, DateTime end, String key, PageParameter p)
    {
        var exp = new WhereExpression();

        if (!category.IsNullOrEmpty() && category != "全部") exp &= _.Category == category;
        if (!action.IsNullOrEmpty() && action != "全部") exp &= _.Action == action;
        if (linkId > 0) exp &= _.LinkID == linkId;
        if (success != null) exp &= _.Success == success;
        if (userid > 0) exp &= _.CreateUserID == userid;

        // 主键带有时间戳
        var snow = Meta.Factory.Snow;
        if (snow != null)
            exp &= _.ID.Between(start, end, snow);
        else
            exp &= _.CreateTime.Between(start, end);

        if (!key.IsNullOrEmpty()) exp &= _.Remark.Contains(key);

        return FindAll(exp, p);
    }
    #endregion

    #region 扩展操作
    // Select Count(ID) as ID,Category From Log Where CreateTime>'2020-01-24 00:00:00' Group By Category Order By ID Desc limit 20
    static readonly FieldCache<Log> CategoryCache = new(__.Category)
    {
        Where = _.CreateTime > DateTime.Today.AddDays(-30) & Expression.Empty
    };

    /// <summary>获取所有类别名称，最近30天</summary>
    /// <returns></returns>
    public static IDictionary<String, String> FindAllCategoryName() => CategoryCache.FindAllName();

    static readonly FieldCache<Log> ActionCache = new(__.Action)
    {
        Where = _.CreateTime > DateTime.Today.AddDays(-30) & Expression.Empty
    };

    /// <summary>获取所有操作名称，最近30天</summary>
    /// <returns></returns>
    public static IDictionary<String, String> FindAllActionName() => ActionCache.FindAllName();
    #endregion

    #region 业务
    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override String ToString() => $"{Category} {Action} {UserName} {CreateTime:yyyy-MM-dd HH:mm:ss} {Remark}";
    #endregion
}