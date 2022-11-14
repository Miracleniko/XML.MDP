using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using XML.Core.Collections;
using XML.Core.Log;
using XML.Core.Stub;
using XML.Core;
using XML.Core.Reflection;
using XML.XCode.Configuration;
using XML.XCode.DataAccessLayer;

namespace XML.XCode.Membership;

/// <summary>菜单</summary>
[Serializable]
[DataObject]
[Description("菜单")]
[BindIndex("IX_Menu_Name", false, "Name")]
[BindIndex("IU_Menu_ParentID_Name", true, "ParentID,Name")]
[BindTable("Menu", Description = "菜单", ConnName = "Membership", DbType = DatabaseType.None)]
public partial class Menu
{
    #region 属性
    private Int32 _ID;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, true, false, 0)]
    [BindColumn("ID", "编号", "")]
    public Int32 ID { get => _ID; set { if (OnPropertyChanging("ID", value)) { _ID = value; OnPropertyChanged("ID"); } } }

    private String _Name;
    /// <summary>名称</summary>
    [DisplayName("名称")]
    [Description("名称")]
    [DataObjectField(false, false, false, 50)]
    [BindColumn("Name", "名称", "", Master = true)]
    public String Name { get => _Name; set { if (OnPropertyChanging("Name", value)) { _Name = value; OnPropertyChanged("Name"); } } }

    private String _DisplayName;
    /// <summary>显示名</summary>
    [DisplayName("显示名")]
    [Description("显示名")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("DisplayName", "显示名", "")]
    public String DisplayName { get => _DisplayName; set { if (OnPropertyChanging("DisplayName", value)) { _DisplayName = value; OnPropertyChanged("DisplayName"); } } }

    private String _FullName;
    /// <summary>全名</summary>
    [DisplayName("全名")]
    [Description("全名")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("FullName", "全名", "")]
    public String FullName { get => _FullName; set { if (OnPropertyChanging("FullName", value)) { _FullName = value; OnPropertyChanged("FullName"); } } }

    private Int32 _ParentID;
    /// <summary>父编号</summary>
    [DisplayName("父编号")]
    [Description("父编号")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("ParentID", "父编号", "")]
    public Int32 ParentID { get => _ParentID; set { if (OnPropertyChanging("ParentID", value)) { _ParentID = value; OnPropertyChanged("ParentID"); } } }

    private String _Url;
    /// <summary>链接</summary>
    [DisplayName("链接")]
    [Description("链接")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("Url", "链接", "")]
    public String Url { get => _Url; set { if (OnPropertyChanging("Url", value)) { _Url = value; OnPropertyChanged("Url"); } } }

    private Int32 _Sort;
    /// <summary>排序</summary>
    [DisplayName("排序")]
    [Description("排序")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Sort", "排序", "")]
    public Int32 Sort { get => _Sort; set { if (OnPropertyChanging("Sort", value)) { _Sort = value; OnPropertyChanged("Sort"); } } }

    private String _Icon;
    /// <summary>图标</summary>
    [DisplayName("图标")]
    [Description("图标")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Icon", "图标", "")]
    public String Icon { get => _Icon; set { if (OnPropertyChanging("Icon", value)) { _Icon = value; OnPropertyChanged("Icon"); } } }

    private Boolean _Visible;
    /// <summary>可见</summary>
    [DisplayName("可见")]
    [Description("可见")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Visible", "可见", "")]
    public Boolean Visible { get => _Visible; set { if (OnPropertyChanging("Visible", value)) { _Visible = value; OnPropertyChanged("Visible"); } } }

    private Boolean _Necessary;
    /// <summary>必要。必要的菜单，必须至少有角色拥有这些权限，如果没有则自动授权给系统角色</summary>
    [DisplayName("必要")]
    [Description("必要。必要的菜单，必须至少有角色拥有这些权限，如果没有则自动授权给系统角色")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Necessary", "必要。必要的菜单，必须至少有角色拥有这些权限，如果没有则自动授权给系统角色", "")]
    public Boolean Necessary { get => _Necessary; set { if (OnPropertyChanging("Necessary", value)) { _Necessary = value; OnPropertyChanged("Necessary"); } } }

    private Boolean _NewWindow;
    /// <summary>新窗口。新窗口打开链接</summary>
    [DisplayName("新窗口")]
    [Description("新窗口。新窗口打开链接")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("NewWindow", "新窗口。新窗口打开链接", "")]
    public Boolean NewWindow { get => _NewWindow; set { if (OnPropertyChanging("NewWindow", value)) { _NewWindow = value; OnPropertyChanged("NewWindow"); } } }

    private String _Permission;
    /// <summary>权限子项。逗号分隔，每个权限子项名值竖线分隔</summary>
    [DisplayName("权限子项")]
    [Description("权限子项。逗号分隔，每个权限子项名值竖线分隔")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("Permission", "权限子项。逗号分隔，每个权限子项名值竖线分隔", "")]
    public String Permission { get => _Permission; set { if (OnPropertyChanging("Permission", value)) { _Permission = value; OnPropertyChanged("Permission"); } } }

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
    /// <summary>创建时间</summary>
    [Category("扩展")]
    [DisplayName("创建时间")]
    [Description("创建时间")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("CreateTime", "创建时间", "")]
    public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging("CreateTime", value)) { _CreateTime = value; OnPropertyChanged("CreateTime"); } } }

    private String _UpdateUser;
    /// <summary>更新者</summary>
    [Category("扩展")]
    [DisplayName("更新者")]
    [Description("更新者")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("UpdateUser", "更新者", "")]
    public String UpdateUser { get => _UpdateUser; set { if (OnPropertyChanging("UpdateUser", value)) { _UpdateUser = value; OnPropertyChanged("UpdateUser"); } } }

    private Int32 _UpdateUserID;
    /// <summary>更新用户</summary>
    [Category("扩展")]
    [DisplayName("更新用户")]
    [Description("更新用户")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("UpdateUserID", "更新用户", "")]
    public Int32 UpdateUserID { get => _UpdateUserID; set { if (OnPropertyChanging("UpdateUserID", value)) { _UpdateUserID = value; OnPropertyChanged("UpdateUserID"); } } }

    private String _UpdateIP;
    /// <summary>更新地址</summary>
    [Category("扩展")]
    [DisplayName("更新地址")]
    [Description("更新地址")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("UpdateIP", "更新地址", "")]
    public String UpdateIP { get => _UpdateIP; set { if (OnPropertyChanging("UpdateIP", value)) { _UpdateIP = value; OnPropertyChanged("UpdateIP"); } } }

    private DateTime _UpdateTime;
    /// <summary>更新时间</summary>
    [Category("扩展")]
    [DisplayName("更新时间")]
    [Description("更新时间")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("UpdateTime", "更新时间", "")]
    public DateTime UpdateTime { get => _UpdateTime; set { if (OnPropertyChanging("UpdateTime", value)) { _UpdateTime = value; OnPropertyChanged("UpdateTime"); } } }

    private String _Remark;
    /// <summary>备注</summary>
    [Category("扩展")]
    [DisplayName("备注")]
    [Description("备注")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("Remark", "备注", "")]
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
                case "Name": return _Name;
                case "DisplayName": return _DisplayName;
                case "FullName": return _FullName;
                case "ParentID": return _ParentID;
                case "Url": return _Url;
                case "Sort": return _Sort;
                case "Icon": return _Icon;
                case "Visible": return _Visible;
                case "Necessary": return _Necessary;
                case "NewWindow": return _NewWindow;
                case "Permission": return _Permission;
                case "Ex1": return _Ex1;
                case "Ex2": return _Ex2;
                case "Ex3": return _Ex3;
                case "Ex4": return _Ex4;
                case "Ex5": return _Ex5;
                case "Ex6": return _Ex6;
                case "CreateUser": return _CreateUser;
                case "CreateUserID": return _CreateUserID;
                case "CreateIP": return _CreateIP;
                case "CreateTime": return _CreateTime;
                case "UpdateUser": return _UpdateUser;
                case "UpdateUserID": return _UpdateUserID;
                case "UpdateIP": return _UpdateIP;
                case "UpdateTime": return _UpdateTime;
                case "Remark": return _Remark;
                default: return base[name];
            }
        }
        set
        {
            switch (name)
            {
                case "ID": _ID = value.ToInt(); break;
                case "Name": _Name = Convert.ToString(value); break;
                case "DisplayName": _DisplayName = Convert.ToString(value); break;
                case "FullName": _FullName = Convert.ToString(value); break;
                case "ParentID": _ParentID = value.ToInt(); break;
                case "Url": _Url = Convert.ToString(value); break;
                case "Sort": _Sort = value.ToInt(); break;
                case "Icon": _Icon = Convert.ToString(value); break;
                case "Visible": _Visible = value.ToBoolean(); break;
                case "Necessary": _Necessary = value.ToBoolean(); break;
                case "NewWindow": _NewWindow = value.ToBoolean(); break;
                case "Permission": _Permission = Convert.ToString(value); break;
                case "Ex1": _Ex1 = value.ToInt(); break;
                case "Ex2": _Ex2 = value.ToInt(); break;
                case "Ex3": _Ex3 = value.ToDouble(); break;
                case "Ex4": _Ex4 = Convert.ToString(value); break;
                case "Ex5": _Ex5 = Convert.ToString(value); break;
                case "Ex6": _Ex6 = Convert.ToString(value); break;
                case "CreateUser": _CreateUser = Convert.ToString(value); break;
                case "CreateUserID": _CreateUserID = value.ToInt(); break;
                case "CreateIP": _CreateIP = Convert.ToString(value); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
                case "UpdateUser": _UpdateUser = Convert.ToString(value); break;
                case "UpdateUserID": _UpdateUserID = value.ToInt(); break;
                case "UpdateIP": _UpdateIP = Convert.ToString(value); break;
                case "UpdateTime": _UpdateTime = value.ToDateTime(); break;
                case "Remark": _Remark = Convert.ToString(value); break;
                default: base[name] = value; break;
            }
        }
    }
    #endregion

    #region 字段名
    /// <summary>取得菜单字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field ID = FindByName("ID");

        /// <summary>名称</summary>
        public static readonly Field Name = FindByName("Name");

        /// <summary>显示名</summary>
        public static readonly Field DisplayName = FindByName("DisplayName");

        /// <summary>全名</summary>
        public static readonly Field FullName = FindByName("FullName");

        /// <summary>父编号</summary>
        public static readonly Field ParentID = FindByName("ParentID");

        /// <summary>链接</summary>
        public static readonly Field Url = FindByName("Url");

        /// <summary>排序</summary>
        public static readonly Field Sort = FindByName("Sort");

        /// <summary>图标</summary>
        public static readonly Field Icon = FindByName("Icon");

        /// <summary>可见</summary>
        public static readonly Field Visible = FindByName("Visible");

        /// <summary>必要。必要的菜单，必须至少有角色拥有这些权限，如果没有则自动授权给系统角色</summary>
        public static readonly Field Necessary = FindByName("Necessary");

        /// <summary>新窗口。新窗口打开链接</summary>
        public static readonly Field NewWindow = FindByName("NewWindow");

        /// <summary>权限子项。逗号分隔，每个权限子项名值竖线分隔</summary>
        public static readonly Field Permission = FindByName("Permission");

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

        /// <summary>创建者</summary>
        public static readonly Field CreateUser = FindByName("CreateUser");

        /// <summary>创建用户</summary>
        public static readonly Field CreateUserID = FindByName("CreateUserID");

        /// <summary>创建地址</summary>
        public static readonly Field CreateIP = FindByName("CreateIP");

        /// <summary>创建时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

        /// <summary>更新者</summary>
        public static readonly Field UpdateUser = FindByName("UpdateUser");

        /// <summary>更新用户</summary>
        public static readonly Field UpdateUserID = FindByName("UpdateUserID");

        /// <summary>更新地址</summary>
        public static readonly Field UpdateIP = FindByName("UpdateIP");

        /// <summary>更新时间</summary>
        public static readonly Field UpdateTime = FindByName("UpdateTime");

        /// <summary>备注</summary>
        public static readonly Field Remark = FindByName("Remark");

        static Field FindByName(String name) => Meta.Table.FindByName(name);
    }

    /// <summary>取得菜单字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String ID = "ID";

        /// <summary>名称</summary>
        public const String Name = "Name";

        /// <summary>显示名</summary>
        public const String DisplayName = "DisplayName";

        /// <summary>全名</summary>
        public const String FullName = "FullName";

        /// <summary>父编号</summary>
        public const String ParentID = "ParentID";

        /// <summary>链接</summary>
        public const String Url = "Url";

        /// <summary>排序</summary>
        public const String Sort = "Sort";

        /// <summary>图标</summary>
        public const String Icon = "Icon";

        /// <summary>可见</summary>
        public const String Visible = "Visible";

        /// <summary>必要。必要的菜单，必须至少有角色拥有这些权限，如果没有则自动授权给系统角色</summary>
        public const String Necessary = "Necessary";

        /// <summary>新窗口。新窗口打开链接</summary>
        public const String NewWindow = "NewWindow";

        /// <summary>权限子项。逗号分隔，每个权限子项名值竖线分隔</summary>
        public const String Permission = "Permission";

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

        /// <summary>创建者</summary>
        public const String CreateUser = "CreateUser";

        /// <summary>创建用户</summary>
        public const String CreateUserID = "CreateUserID";

        /// <summary>创建地址</summary>
        public const String CreateIP = "CreateIP";

        /// <summary>创建时间</summary>
        public const String CreateTime = "CreateTime";

        /// <summary>更新者</summary>
        public const String UpdateUser = "UpdateUser";

        /// <summary>更新用户</summary>
        public const String UpdateUserID = "UpdateUserID";

        /// <summary>更新地址</summary>
        public const String UpdateIP = "UpdateIP";

        /// <summary>更新时间</summary>
        public const String UpdateTime = "UpdateTime";

        /// <summary>备注</summary>
        public const String Remark = "Remark";
    }
    #endregion
}

/// <summary>菜单</summary>
[EntityFactory(typeof(MenuFactory))]
public partial class Menu : EntityTree<Menu>, IMenu
{
    #region 对象操作
    static Menu()
    {
        // 引发内部
        new Menu();

        //EntityFactory.Register(typeof(Menu), new MenuFactory());

        //ObjectContainer.Current.AutoRegister<IMenuFactory, MenuFactory>();

        Meta.Modules.Add<UserModule>();
        Meta.Modules.Add<TimeModule>();
        Meta.Modules.Add<IPModule>();
    }

    /// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
    /// <param name="isNew">是否新数据</param>
    public override void Valid(Boolean isNew)
    {
        if (String.IsNullOrEmpty(Name)) throw new ArgumentNullException(__.Name, _.Name.DisplayName + "不能为空！");

        base.Valid(isNew);

        if (Icon == "&#xe63f;") Icon = null;

        SavePermission();
    }

    /// <summary>已重载。调用Save时写日志，而调用Insert和Update时不写日志</summary>
    /// <returns></returns>
    public override Int32 Save()
    {
        // 先处理一次，否则可能因为别的字段没有修改而没有脏数据
        SavePermission();

        //if (Icon.IsNullOrWhiteSpace()) Icon = "&#xe63f;";

        // 更改日志保存顺序，先保存才能获取到id
        var action = "添加";
        var isNew = IsNullKey;
        if (!isNew)
        {
            // 没有修改时不写日志
            if (!HasDirty) return 0;

            action = "修改";

            // 必须提前写修改日志，否则修改后脏数据失效，保存的日志为空
            LogProvider.Provider.WriteLog(action, this);
        }

        var result = base.Save();

        if (isNew) LogProvider.Provider.WriteLog(action, this);

        return result;
    }

    /// <summary>删除。</summary>
    /// <returns></returns>
    protected override Int32 OnDelete()
    {
        var err = "";
        try
        {
            // 递归删除子菜单
            var rs = 0;
            using var ts = Meta.CreateTrans();
            rs += base.OnDelete();

            var ms = Childs;
            if (ms != null && ms.Count > 0)
            {
                foreach (var item in ms)
                {
                    rs += item.Delete();
                }
            }

            ts.Commit();

            return rs;
        }
        catch (Exception ex)
        {
            err = ex.Message;
            throw;
        }
        finally
        {
            LogProvider.Provider.WriteLog("删除", this, err);
        }
    }

    /// <summary>加载权限字典</summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        // 构造权限字典
        LoadPermission();
    }

    /// <summary>如果Permission被修改，则重新加载</summary>
    /// <param name="fieldName"></param>
    protected override void OnPropertyChanged(String fieldName)
    {
        base.OnPropertyChanged(fieldName);

        if (fieldName == __.Permission) LoadPermission();
    }
    #endregion

    #region 扩展属性
    /// <summary></summary>
    public String Url2 => Url?.Replace("~", "");

    /// <summary>父菜单名</summary>
    public virtual String ParentMenuName { get => Parent?.Name; set { } }

    /// <summary>必要的菜单。必须至少有角色拥有这些权限，如果没有则自动授权给系统角色</summary>
    internal static Int32[] Necessaries
    {
        get
        {
            // 找出所有的必要菜单，如果没有，则表示全部都是必要
            var list = FindAllWithCache();
            var list2 = list.Where(e => e.Necessary).ToList();
            if (list2.Count > 0) list = list2;

            return list.Select(e => e.ID).ToArray();
        }
    }

    /// <summary>友好名称。优先显示名</summary>
    public String FriendName => DisplayName.IsNullOrWhiteSpace() ? Name : DisplayName;
    #endregion

    #region 扩展查询
    /// <summary>根据编号查找</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Menu FindByID(Int32 id)
    {
        if (id <= 0) return null;

        return Meta.Cache.Find(e => e.ID == id);
    }

    /// <summary>根据名字查找</summary>
    /// <param name="name">名称</param>
    /// <returns></returns>
    public static Menu FindByName(String name) => Meta.Cache.Find(e => e.Name.EqualIgnoreCase(name));

    /// <summary>根据全名查找</summary>
    /// <param name="name">全名</param>
    /// <returns></returns>
    public static Menu FindByFullName(String name) => Meta.Cache.Find(e => e.FullName.EqualIgnoreCase(name));

    /// <summary>根据Url查找</summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static Menu FindByUrl(String url) => Meta.Cache.Find(e => e.Url.EqualIgnoreCase(url));

    /// <summary>根据名字查找，支持路径查找</summary>
    /// <param name="name">名称</param>
    /// <returns></returns>
    public static Menu FindForName(String name)
    {
        var entity = FindByName(name);
        if (entity != null) return entity;

        return Root.FindByPath(name, _.Name, _.DisplayName);
    }

    /// <summary>查找指定菜单的子菜单</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static List<Menu> FindAllByParentID(Int32 id) => Meta.Cache.FindAll(e => e.ParentID == id).OrderByDescending(e => e.Sort).ThenBy(e => e.ID).ToList();

    /// <summary>取得当前角色的子菜单，有权限、可显示、排序</summary>
    /// <param name="filters"></param>
    /// <param name="inclInvisible">包含不可见菜单</param>
    /// <returns></returns>
    public IList<IMenu> GetSubMenus(Int32[] filters, Boolean inclInvisible = false)
    {
        var list = Childs;
        if (list == null || list.Count <= 0) return new List<IMenu>();

        if (!inclInvisible) list = list.Where(e => e.Visible).ToList();
        if (list == null || list.Count <= 0) return new List<IMenu>();

        return list.Where(e => filters.Contains(e.ID)).Cast<IMenu>().ToList();
    }
    #endregion

    #region 扩展操作
    /// <summary>添加子菜单</summary>
    /// <param name="name"></param>
    /// <param name="displayName"></param>
    /// <param name="fullName"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    public IMenu Add(String name, String displayName, String fullName, String url)
    {
        var entity = new Menu
        {
            Name = name,
            DisplayName = displayName,
            FullName = fullName,
            Url = url,
            ParentID = ID,

            Visible = ID == 0 || displayName != null
        };

        entity.Save();

        return entity;
    }
    #endregion

    #region 扩展权限
    /// <summary>可选权限子项</summary>
    [XmlIgnore, ScriptIgnore, IgnoreDataMember]
    public Dictionary<Int32, String> Permissions { get; set; } = new Dictionary<Int32, String>();

    private void LoadPermission()
    {
        Permissions.Clear();
        if (String.IsNullOrEmpty(Permission)) return;

        var dic = Permission.SplitAsDictionary("#", ",");
        foreach (var item in dic)
        {
            var resid = item.Key.ToInt();
            Permissions[resid] = item.Value;
        }
    }

    private void SavePermission()
    {
        // 不能这样子直接清空，因为可能没有任何改变，而这么做会两次改变脏数据，让系统以为有改变
        //Permission = null;
        if (Permissions.Count <= 0)
        {
            //Permission = null;
            SetItem(__.Permission, null);
            return;
        }

        var sb = Pool.StringBuilder.Get();
        // 根据资源按照从小到大排序一下
        foreach (var item in Permissions.OrderBy(e => e.Key))
        {
            if (sb.Length > 0) sb.Append(',');
            sb.AppendFormat("{0}#{1}", item.Key, item.Value);
        }
        SetItem(__.Permission, sb.Put(true));
    }
    #endregion

    #region 日志
    ///// <summary>写日志</summary>
    ///// <param name="action">操作</param>
    ///// <param name="remark">备注</param>
    //public static void WriteLog(String action, String remark) => LogProvider.Provider.WriteLog(typeof(Menu), action, remark);
    #endregion

    #region 辅助
    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override String ToString()
    {
        var path = GetFullPath(true, "\\", e => e.FriendName);
        if (!path.IsNullOrEmpty()) return path;

        return FriendName;
    }
    #endregion

    #region IMenu 成员
    /// <summary>取得全路径的实体，由上向下排序</summary>
    /// <param name="includeSelf">是否包含自己</param>
    /// <param name="separator">分隔符</param>
    /// <param name="func">回调</param>
    /// <returns></returns>
    String IMenu.GetFullPath(Boolean includeSelf, String separator, Func<IMenu, String> func)
    {
        Func<Menu, String> d = null;
        if (func != null) d = item => func(item);

        return GetFullPath(includeSelf, separator, d);
    }

    //IMenu IMenu.Add(String name, String displayName, String fullName, String url) => Add(name, displayName, fullName, url);

    /// <summary>父菜单</summary>
    IMenu IMenu.Parent => Parent;

    /// <summary>子菜单</summary>
    IList<IMenu> IMenu.Childs => Childs.OfType<IMenu>().ToList();

    /// <summary>子孙菜单</summary>
    IList<IMenu> IMenu.AllChilds => AllChilds.OfType<IMenu>().ToList();

    /// <summary>根据层次路径查找</summary>
    /// <param name="path">层次路径</param>
    /// <returns></returns>
    IMenu IMenu.FindByPath(String path) => FindByPath(path, _.Name, _.DisplayName);
    #endregion

    #region 菜单工厂
    /// <summary>菜单工厂</summary>
    public class MenuFactory : DefaultEntityFactory, IMenuFactory
    {
        #region IMenuFactory 成员
        IMenu IMenuFactory.Root => Root;

        /// <summary>根据编号找到菜单</summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IMenu IMenuFactory.FindByID(Int32 id) => FindByID(id);

        /// <summary>根据Url找到菜单</summary>
        /// <param name="url"></param>
        /// <returns></returns>
        IMenu IMenuFactory.FindByUrl(String url) => FindByUrl(url);

        /// <summary>根据全名找到菜单</summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        IMenu IMenuFactory.FindByFullName(String fullName) => FindByFullName(fullName);

        /// <summary>获取指定菜单下，当前用户有权访问的子菜单。</summary>
        /// <param name="menuid"></param>
        /// <param name="user"></param>
        /// <param name="inclInvisible">是否包含不可见菜单</param>
        /// <returns></returns>
        IList<IMenu> IMenuFactory.GetMySubMenus(Int32 menuid, IUser user, Boolean inclInvisible)
        {
            var factory = this as IMenuFactory;
            var root = factory.Root;

            // 当前用户
            //var user = ManageProvider.Provider.Current as IUser;
            var rs = user?.Roles;
            if (rs == null || rs.Length == 0) return new List<IMenu>();

            IMenu menu = null;

            // 找到菜单
            if (menuid > 0) menu = FindByID(menuid);

            if (menu == null)
            {
                menu = root;
                if (menu == null || menu.Childs == null || menu.Childs.Count <= 0) return new List<IMenu>();
            }

            return menu.GetSubMenus(rs.SelectMany(e => e.Resources).ToArray(), inclInvisible);
        }

        /// <summary>扫描命名空间下的控制器并添加为菜单</summary>
        /// <param name="rootName">根菜单名称，所有菜单附属在其下</param>
        /// <param name="asm">要扫描的程序集</param>
        /// <param name="nameSpace">要扫描的命名空间</param>
        /// <returns></returns>
        public virtual IList<IMenu> ScanController(String rootName, Assembly asm, String nameSpace)
        {
            var list = new List<IMenu>();
            var mf = this as IMenuFactory;

            // 所有控制器
            var types = asm.GetTypes().Where(e => e.Name.EndsWith("Controller") && e.Namespace == nameSpace).ToList();
            if (types.Count == 0) return list;

            // 如果根菜单不存在，则添加
            var r = Root as IMenu;
            var root = mf.FindByFullName(nameSpace);
            if (root == null) root = r.FindByPath(rootName);
            //if (root == null) root = r.Childs.FirstOrDefault(e => e.Name.EqualIgnoreCase(rootName));
            //if (root == null) root = r.Childs.FirstOrDefault(e => e.Url.EqualIgnoreCase("~/" + rootName));
            if (root == null)
            {
                root = r.Add(rootName, null, nameSpace, "~/" + rootName);
                list.Add(root);
            }
            if (root.FullName != nameSpace)
            {
                root.FullName = nameSpace;
                (root as IEntity).Save();
            }

            var ms = new List<IMenu>();

            // 遍历该程序集所有类型
            foreach (var type in types)
            {
                var name = type.Name.TrimEnd("Controller");
                var url = root.Url;
                var node = root;

                // 添加Controller
                var controller = node.FindByPath(name);
                if (controller == null)
                {
                    url += "/" + name;
                    controller = FindByUrl(url);
                    if (controller == null)
                    {
                        // DisplayName特性作为中文名
                        controller = node.Add(name, type.GetDisplayName(), type.FullName, url);

                        //list.Add(controller);
                    }
                }
                if (controller.FullName.IsNullOrEmpty()) controller.FullName = type.FullName;
                if (controller.Remark.IsNullOrEmpty()) controller.Remark = type.GetDescription();

                ms.Add(controller);
                list.Add(controller);

                // 反射调用控制器的方法来获取动作
                var func = type.GetMethodEx("ScanActionMenu");
                if (func == null) continue;

                // 由于控制器使用IOC，无法直接实例化控制器，需要给各个参数传入空
                var ctor = type.GetConstructors()?.FirstOrDefault();
                var ctrl = ctor.Invoke(new Object[ctor.GetParameters().Length]);
                //var ctrl = type.CreateInstance();

                var acts = func.As<Func<IMenu, IDictionary<MethodInfo, Int32>>>(ctrl).Invoke(controller);
                if (acts == null || acts.Count == 0) continue;

                // 可选权限子项
                controller.Permissions.Clear();

                // 添加该类型下的所有Action作为可选权限子项
                foreach (var item in acts)
                {
                    var method = item.Key;

                    var dn = method.GetDisplayName();
                    if (!dn.IsNullOrEmpty()) dn = dn.Replace("{type}", (controller as Menu)?.FriendName);

                    var pmName = !dn.IsNullOrEmpty() ? dn : method.Name;
                    if (item.Value <= (Int32)PermissionFlags.Delete) pmName = ((PermissionFlags)item.Value).GetDescription();
                    controller.Permissions[item.Value] = pmName;
                }

                // 排序
                if (controller.Sort == 0)
                {
                    var pi = type.GetPropertyEx("MenuOrder");
                    if (pi != null) controller.Sort = pi.GetValue(null).ToInt();
                }
            }

            for (var i = 0; i < ms.Count; i++)
            {
                (ms[i] as IEntity).Save();
            }

            // 如果新增了菜单，需要检查权限
            if (list.Count > 0)
            {
                ThreadPool.QueueUserWorkItem(s =>
                {
                    try
                    {
                        XTrace.WriteLine("新增了菜单，需要检查权限");
                        var fact = ManageProvider.GetFactory<IRole>();
                        fact.EntityType.Invoke("CheckRole");
                    }
                    catch (Exception ex)
                    {
                        XTrace.WriteException(ex);
                    }
                });
            }

            return list;
        }
        #endregion
    }
    #endregion
}